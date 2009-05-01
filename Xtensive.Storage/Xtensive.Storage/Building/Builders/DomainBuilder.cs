// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using System;
using System.Globalization;
using System.Reflection;
using Microsoft.Practices.Unity.Configuration;
using Xtensive.Core;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Reflection;
using Xtensive.Modelling.Comparison;
using Xtensive.PluginManager;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Upgrade;
using Activator=System.Activator;

namespace Xtensive.Storage.Building.Builders
{
  /// <summary>
  /// Utility class for <see cref="Storage"/> building.
  /// </summary>
  public static class DomainBuilder
  {
    private static readonly PluginManager<ProviderAttribute> pluginManager =
      new PluginManager<ProviderAttribute>(typeof (HandlerFactory), AppDomain.CurrentDomain.BaseDirectory);

    /// <summary>
    /// Builds the domain.
    /// </summary>
    /// <param name="configuration">The domain configuration.</param>
    /// <param name="schemaUpgradeMode">The schema upgrade mode.</param>
    /// <returns>Built domain.</returns>
    public static Domain BuildDomain(DomainConfiguration configuration, SchemaUpgradeMode schemaUpgradeMode)
    {
      return BuildDomain(configuration, schemaUpgradeMode, () => { }, type => true);
    }

    /// <summary>
    /// Builds the domain.
    /// </summary>
    /// <param name="configuration">The domain configuration.</param>
    /// <param name="schemaUpgradeMode">The schema upgrade mode.</param>
    /// <param name="upgradeHandler">The method that can process storage data when domain is built.</param>
    /// <returns>Built domain.</returns>
    public static Domain BuildDomain(DomainConfiguration configuration, SchemaUpgradeMode schemaUpgradeMode, Action upgradeHandler)
    {
      return BuildDomain(configuration, schemaUpgradeMode, upgradeHandler, type => true);
    }

    /// <summary>
    /// Builds the domain.
    /// </summary>
    /// <param name="configuration">The domain configuration.</param>
    /// <param name="schemaUpgradeMode">The schema upgrade mode.</param>
    /// <param name="upgradeHandler">The method that can process storage data when domain is built.</param>
    /// <param name="typeFilter">The persistent type filter.</param>
    /// <returns>Built domain.</returns>
    public static Domain BuildDomain(DomainConfiguration configuration, 
      SchemaUpgradeMode schemaUpgradeMode, Action upgradeHandler, Func<Type, bool> typeFilter)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");
      ArgumentValidator.EnsureArgumentNotNull(upgradeHandler, "upgradeHandler");
      ArgumentValidator.EnsureArgumentNotNull(typeFilter, "typeFilter");

      if (!configuration.IsLocked)
        configuration.Lock(true);

      Validate(configuration);

      var context = new BuildingContext(configuration) {
        TypeFilter = typeFilter
      };

      using (LogTemplate<Log>.InfoRegion(Strings.LogBuildingX, typeof (Domain).GetShortName())) {
        using (new BuildingScope(context)) {
          try {
            CreateDomain();
            ConfigureServiceContainer();
            CreateHandlerFactory();
            CreateDomainHandler();

            CreateNameBuilder();
            CreateDomainHandler();
            BuildModel();

            using (context.Domain.OpenSystemSession()) {
              context.SystemSessionHandler = Session.Current.Handler;
              using (var transactionScope = Transaction.Open()) {
                context.Domain.Handler.OnSystemSessionOpen();
                SynchronizeSchema(schemaUpgradeMode);
                context.Domain.Handler.BuildMapping();
                CreateGenerators();
                TypeIdBuilder.BuildTypeIds();
                upgradeHandler.Invoke();
                transactionScope.Complete();
              }
            }
          }
          catch (DomainBuilderException e) {
            context.RegisterError(e);
          }
          context.EnsureBuildSucceed();
        }
      }
      return context.Domain;
    }

    private static void ConfigureServiceContainer()
    {
      var context = BuildingContext.Current;
      if (context.Configuration.Services==null)
        return;
      foreach (UnityTypeElement typeElement in context.Configuration.Services)
        typeElement.Configure(BuildingContext.Current.Domain.ServiceContainer);
    }

    #region ValidateXxx methods

    private static void Validate(DomainConfiguration configuration)
    {
      if (configuration.Builders.Count > 0)
        foreach (Type type in configuration.Builders)
          ValidateBuilder(type);
    }

    /// <exception cref="DomainBuilderException">Something went wrong.</exception>
    private static void ValidateBuilder(Type type)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");

      ConstructorInfo constructor =
        type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null);

      if (constructor==null)
        throw new DomainBuilderException(
          string.Format(Strings.ExTypeXMustHavePublicInstanceParameterlessConstructorInOrderToBeUsedAsStorageDefinitionBuilder, type.FullName));

      if (!typeof (IDomainBuilder).IsAssignableFrom(type))
        throw new DomainBuilderException(
          string.Format(CultureInfo.CurrentCulture,
            Strings.ExTypeXDoesNotImplementYInterface, type.FullName, typeof (IDomainBuilder).FullName));
    }

    #endregion


    private static void CreateDomain()
    {
      using (LogTemplate<Log>.InfoRegion(Strings.LogCreatingX, typeof (Domain).GetShortName())) {
        var domain = new Domain(BuildingContext.Current.Configuration);
        BuildingContext.Current.Domain = domain;
      }
    }

    /// <exception cref="DomainBuilderException">Something went wrong.</exception>
    private static void CreateHandlerFactory()
    {
      using (LogTemplate<Log>.InfoRegion(Strings.LogCreatingX, typeof (HandlerFactory).GetShortName())) {
        string protocol = BuildingContext.Current.Configuration.ConnectionInfo.Protocol;
        Type handlerProviderType;
        lock (pluginManager) {
          handlerProviderType = pluginManager[new ProviderAttribute(protocol)];
          if (handlerProviderType==null)
            throw new DomainBuilderException(
              string.Format(Strings.ExStorageProviderNotFound,
                protocol,
                Environment.CurrentDirectory));
        }
        var handlerFactory = (HandlerFactory) Activator.CreateInstance(handlerProviderType, new object[] {BuildingContext.Current.Domain});
        var handlerAccessor = BuildingContext.Current.Domain.Handlers;
        handlerAccessor.HandlerFactory = handlerFactory;
      }
    }

    private static void CreateNameBuilder()
    {
      using (LogTemplate<Log>.InfoRegion(Strings.LogCreatingX, typeof (NameBuilder).GetShortName())) {
        var handlerAccessor = BuildingContext.Current.Domain.Handlers;
        handlerAccessor.NameBuilder = handlerAccessor.HandlerFactory.CreateHandler<NameBuilder>();
        handlerAccessor.NameBuilder.Initialize(handlerAccessor.Domain.Configuration.NamingConvention);
      }
    }

    private static void CreateDomainHandler()
    {
      using (LogTemplate<Log>.InfoRegion(Strings.LogCreatingX, typeof (DomainHandler).GetShortName())) {
        var handlerAccessor = BuildingContext.Current.Domain.Handlers;
        handlerAccessor.DomainHandler = handlerAccessor.HandlerFactory.CreateHandler<DomainHandler>();
        handlerAccessor.DomainHandler.Initialize();
      }
    }

    private static void BuildModel()
    {
      using (LogTemplate<Log>.InfoRegion(Strings.LogBuildingX, Strings.Model)) {
        ModelBuilder.Build();
        var domain = BuildingContext.Current.Domain;
        domain.Model = BuildingContext.Current.Model;
      }
    }

    private static void CreateGenerators()
    {
      using (LogTemplate<Log>.InfoRegion(Strings.LogCreatingX, Strings.Generators)) {
        var handlerAccessor = BuildingContext.Current.Domain.Handlers;
        var keyGenerators = BuildingContext.Current.Domain.KeyGenerators;
        var generatorFactory = handlerAccessor.HandlerFactory.CreateHandler<KeyGeneratorFactory>();
        foreach (var generatorInfo in BuildingContext.Current.Model.Generators) {
          KeyGenerator keyGenerator;
          if (generatorInfo.KeyGeneratorType==null)
            continue;
          if (generatorInfo.KeyGeneratorType==typeof (KeyGenerator))
            keyGenerator = generatorFactory.CreateGenerator(generatorInfo);
          else
            keyGenerator = (KeyGenerator) Activator.CreateInstance(generatorInfo.KeyGeneratorType, new object[] {generatorInfo});
          keyGenerator.Initialize();
          keyGenerators.Register(generatorInfo, keyGenerator);
        }
        keyGenerators.Lock();
      }
    }

    /// <exception cref="SchemaSynchronizationException">Extracted schema is incompatible 
    /// with the target schema in specified <paramref name="schemaUpgradeMode"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><c>schemaUpgradeMode</c> is out of range.</exception>
    private static void SynchronizeSchema(SchemaUpgradeMode schemaUpgradeMode)
    {
      var context = BuildingContext.Current;
      using (Log.InfoRegion(Strings.LogSynchronizingSchemaInXMode, schemaUpgradeMode)) {
        var upgradeHandler = context.HandlerFactory.CreateHandler<SchemaUpgradeHandler>();
        var targetSchema = upgradeHandler.GetTargetSchema();
        Log.Info(Strings.LogTargetSchema);
        if (Log.IsLogged(LogEventTypes.Info))
          targetSchema.Dump();
        var extractedSchema = upgradeHandler.GetExtractedSchema();
        Log.Info(Strings.LogExtractedSchema);
        if (Log.IsLogged(LogEventTypes.Info))
          extractedSchema.Dump();
        UpgradingDomainBuilder.OnSchemasReady(extractedSchema, targetSchema);
        SchemaComparisonResult result;

        // Let's clear the schema if mode is Recreate
        if (schemaUpgradeMode==SchemaUpgradeMode.Recreate) {
          var emptySchema = new StorageInfo();
          result = SchemaComparer.Compare(extractedSchema, emptySchema, null);
          if (result.Status!=SchemaComparisonStatus.Equal) {
            if (Log.IsLogged(LogEventTypes.Info))
              Log.Info(Strings.LogClearingComparisonResultX, result);
            upgradeHandler.UpgradeSchema(result.UpgradeActions, emptySchema);
            extractedSchema = upgradeHandler.GetExtractedSchema();
          }
        }

        result = SchemaComparer.Compare(extractedSchema, targetSchema, null);
        if (Log.IsLogged(LogEventTypes.Info))
          Log.Info(Strings.LogComparisonResultX, result);
        UpgradingDomainBuilder.OnUpgradeActionsReady((NodeDifference) result.Difference, result.UpgradeActions);

        switch (schemaUpgradeMode) {
        case SchemaUpgradeMode.Recreate:
        case SchemaUpgradeMode.Upgrade:
          upgradeHandler.UpgradeSchema(result.UpgradeActions, targetSchema);
          break;
        case SchemaUpgradeMode.ValidateExact:
          if (result.Status!=SchemaComparisonStatus.Equal)
            throw new SchemaSynchronizationException(
              Strings.ExExtractedSchemaIsNotEqualToTheTargetSchema);
          upgradeHandler.UpgradeSchema(result.UpgradeActions, targetSchema);
          break;
        case SchemaUpgradeMode.ValidateCompatible:
          if (result.Status!=SchemaComparisonStatus.Equal &&
            result.Status!=SchemaComparisonStatus.TargetIsSubset)
            throw new SchemaSynchronizationException(
              Strings.ExExtractedSchemaIsNotCompatibleWithTheTargetSchema);
          upgradeHandler.UpgradeSchema(result.UpgradeActions, targetSchema);
          break;
        case SchemaUpgradeMode.UpgradeSafely:
          if (result.Status!=SchemaComparisonStatus.Equal &&
            result.Status!=SchemaComparisonStatus.TargetIsSuperset)
            throw new SchemaSynchronizationException(Strings.ExCanNotUpgradeSchemaSafely);
          upgradeHandler.UpgradeSchema(result.UpgradeActions, targetSchema);
          break;
        default:
          throw new ArgumentOutOfRangeException("schemaUpgradeMode");
        }
      }
    }
  }
}
