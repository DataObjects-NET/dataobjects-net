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
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.PluginManager;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Upgrade;
using Activator=System.Activator;
using UpgradeContext=Xtensive.Modelling.Comparison.UpgradeContext;

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
      return BuildDomain(configuration, new DomainBuilderConfiguration(schemaUpgradeMode));
    }

    /// <summary>
    /// Builds the domain.
    /// </summary>
    /// <param name="configuration">The domain configuration.</param>
    /// <param name="builderConfiguration">The builder configuration.</param>
    /// <returns>Built domain.</returns>
    public static Domain BuildDomain(DomainConfiguration configuration, 
      DomainBuilderConfiguration builderConfiguration)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");
      ArgumentValidator.EnsureArgumentNotNull(builderConfiguration, "builderConfiguration");

      if (!configuration.IsLocked)
        configuration.Lock(true);

      Validate(configuration);

      var context = new BuildingContext(configuration) {
        BuilderConfiguration = builderConfiguration
      };

      using (Log.InfoRegion(Strings.LogBuildingX, typeof (Domain).GetShortName())) {
        using (new BuildingScope(context)) {
          CreateDomain();
          ConfigureServiceContainer();
          CreateHandlerFactory();
          CreateDomainHandler();

          CreateNameBuilder();
          CreateDomainHandler();
          BuildModel();

          using (context.Domain.OpenSession(SessionType.System)) {
            context.SystemSessionHandler = Session.Current.Handler;
            using (var transactionScope = Transaction.Open()) {
              context.Domain.Handler.InitializeFirstSession();
              SynchronizeSchema(builderConfiguration.SchemaUpgradeMode);
              context.Domain.Handler.BuildMapping();
              CreateGenerators();
              TypeIdBuilder.BuildTypeIds();
              if (builderConfiguration.UpgradeHandler!=null)
                builderConfiguration.UpgradeHandler.Invoke();
              transactionScope.Complete();
            }
          }
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
          string.Format(Strings.ExTypeXMustHavePublicInstanceParameterlessConstructorInOrderToBeUsedAsStorageDefinitionBuilder, type.GetFullName()));

      if (!typeof (IDomainBuilder).IsAssignableFrom(type))
        throw new DomainBuilderException(
          string.Format(CultureInfo.CurrentCulture,
            Strings.ExTypeXDoesNotImplementYInterface, type.GetFullName(), typeof (IDomainBuilder).GetFullName()));
    }

    #endregion


    private static void CreateDomain()
    {
      using (Log.InfoRegion(Strings.LogCreatingX, typeof (Domain).GetShortName())) {
        var domain = new Domain(BuildingContext.Current.Configuration);
        BuildingContext.Current.Domain = domain;
      }
    }

    /// <exception cref="DomainBuilderException">Something went wrong.</exception>
    private static void CreateHandlerFactory()
    {
      using (Log.InfoRegion(Strings.LogCreatingX, typeof (HandlerFactory).GetShortName())) {
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
      using (Log.InfoRegion(Strings.LogCreatingX, typeof (NameBuilder).GetShortName())) {
        var handlerAccessor = BuildingContext.Current.Domain.Handlers;
        handlerAccessor.NameBuilder = handlerAccessor.HandlerFactory.CreateHandler<NameBuilder>();
        handlerAccessor.NameBuilder.Initialize(handlerAccessor.Domain.Configuration.NamingConvention);
      }
    }

    private static void CreateDomainHandler()
    {
      using (Log.InfoRegion(Strings.LogCreatingX, typeof (DomainHandler).GetShortName())) {
        var handlerAccessor = BuildingContext.Current.Domain.Handlers;
        handlerAccessor.DomainHandler = handlerAccessor.HandlerFactory.CreateHandler<DomainHandler>();
        handlerAccessor.DomainHandler.Initialize();
      }
    }

    private static void BuildModel()
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.Model)) {
        ModelBuilder.Build();
        var domain = BuildingContext.Current.Domain;
        domain.Model = BuildingContext.Current.Model;
      }
    }

    private static void CreateGenerators()
    {
      using (Log.InfoRegion(Strings.LogCreatingX, Strings.Generators)) {
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
      var domain = context.Domain;
      using (Log.InfoRegion(Strings.LogSynchronizingSchemaInXMode, schemaUpgradeMode)) {
        var upgradeHandler = context.HandlerFactory.CreateHandler<SchemaUpgradeHandler>();
        
        // Target schema 
        var targetSchema = upgradeHandler.GetTargetSchema();
        domain.Schema = ((StorageInfo) targetSchema.Clone(null, StorageInfo.DefaultName));
        domain.Schema.Lock();
        // Dumping target schema
        Log.Info(Strings.LogTargetSchema);
        if (Log.IsLogged(LogEventTypes.Info))
          targetSchema.Dump();
        
        // Extracted schema 
        var extractedSchema = upgradeHandler.GetExtractedSchema();
        domain.ExtractedSchema = ((StorageInfo) extractedSchema.Clone(null, StorageInfo.DefaultName));
        domain.ExtractedSchema.Lock();
        // Dumping extracted schema
        Log.Info(Strings.LogExtractedSchema);
        if (Log.IsLogged(LogEventTypes.Info))
          extractedSchema.Dump();

        // Hints
        HintSet hints = null;
        if (context.BuilderConfiguration.SchemaReadyHandler!=null)
          hints = context.BuilderConfiguration.SchemaReadyHandler.Invoke(extractedSchema, targetSchema);
        SchemaComparisonResult result;

        // Let's clear the schema if mode is Recreate
        if (schemaUpgradeMode==SchemaUpgradeMode.Recreate) {
          var emptySchema = new StorageInfo();
          result = SchemaComparer.Compare(extractedSchema, emptySchema, null);
          if (result.Status!=SchemaComparisonStatus.Equal) {
            if (Log.IsLogged(LogEventTypes.Info))
              Log.Info(Strings.LogClearingComparisonResultX, result);
            upgradeHandler.UpgradeSchema(result.UpgradeActions, extractedSchema, emptySchema);
            extractedSchema = upgradeHandler.GetExtractedSchema();
            hints = null; // Must re-bind them
          }
        }

        result = SchemaComparer.Compare(extractedSchema, targetSchema, hints);
        if (Log.IsLogged(LogEventTypes.Info))
          Log.Info(Strings.LogComparisonResultX, result);
        if (context.BuilderConfiguration.UpgradeActionsReadyHandler!=null)
          context.BuilderConfiguration.UpgradeActionsReadyHandler.Invoke(
            (NodeDifference) result.Difference, result.UpgradeActions);

        switch (schemaUpgradeMode) {
        case SchemaUpgradeMode.ValidateExact:
          if (result.Status!=SchemaComparisonStatus.Equal)
            throw new SchemaSynchronizationException(
              Strings.ExExtractedSchemaIsNotEqualToTheTargetSchema);
          break;
        case SchemaUpgradeMode.ValidateCompatible:
          if (result.Status!=SchemaComparisonStatus.Equal &&
            result.Status!=SchemaComparisonStatus.TargetIsSubset)
            throw new SchemaSynchronizationException(
              Strings.ExExtractedSchemaIsNotCompatibleWithTheTargetSchema);
          break;
        case SchemaUpgradeMode.Recreate:
        case SchemaUpgradeMode.Perform:
            upgradeHandler.UpgradeSchema(result.UpgradeActions, extractedSchema, targetSchema);
          break;
        case SchemaUpgradeMode.PerformSafely:
          if((result.Status != SchemaComparisonStatus.Equal 
            && result.Status != SchemaComparisonStatus.TargetIsSuperset)
            || !result.CanUpgradeTypesSafely)
            throw new SchemaSynchronizationException(Strings.ExCannotUpgradeSchemaSafely);
          upgradeHandler.UpgradeSchema(result.UpgradeActions, extractedSchema, targetSchema);
          break;
        default:
          throw new ArgumentOutOfRangeException("schemaUpgradeMode");
        }
      }
    }
  }
}
