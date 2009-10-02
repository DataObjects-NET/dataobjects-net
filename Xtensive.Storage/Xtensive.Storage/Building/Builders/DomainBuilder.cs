// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using System;
using System.Reflection;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Reflection;
using Xtensive.Modelling.Comparison;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Resources;
using Activator = System.Activator;

namespace Xtensive.Storage.Building.Builders
{
  /// <summary>
  /// Utility class for <see cref="Storage"/> building.
  /// </summary>
  internal static class DomainBuilder
  {
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

      var context = new BuildingContext(configuration) {
        BuilderConfiguration = builderConfiguration
      };

      using (Log.InfoRegion(Strings.LogBuildingX, typeof (Domain).GetShortName())) {
        using (new BuildingScope(context)) {
          CreateDomain();
          CreateHandlerFactory();
          CreateDomainHandler();

          CreateNameBuilder();
          CreateDomainHandler();
          BuildModel();

          using (Session.Open(context.Domain, SessionType.System)) {
            context.SystemSessionHandler = Session.Demand().Handler;
            using (var transactionScope = Transaction.Open()) {
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

    private static void CreateDomain()
    {
      using (Log.InfoRegion(Strings.LogCreatingX, typeof (Domain).GetShortName())) {
        var domain = new Domain(BuildingContext.Current.Configuration,
          BuildingContext.Current.BuilderConfiguration.Modules);
        BuildingContext.Current.Domain = domain;
      }
    }

    /// <exception cref="DomainBuilderException">Something went wrong.</exception>
    private static void CreateHandlerFactory()
    {
      using (Log.InfoRegion(Strings.LogCreatingX, typeof (HandlerFactory).GetShortName())) {
        string protocol = BuildingContext.Current.Configuration.ConnectionInfo.Protocol;
        var thisAssemblyNameObject = typeof(DomainBuilder).Assembly.GetName();
        var thisAssemblyShortName  = thisAssemblyNameObject.Name;
        var thisAssemblyName = thisAssemblyNameObject.FullName;

        // Getting provider assembly name
        string providerAssemblyShortName;
        switch (protocol) {
        case WellKnown.Protocol.Memory:
          providerAssemblyShortName = "Xtensive.Storage.Providers.Index";
          break;
        case WellKnown.Protocol.SqlServer:
        case WellKnown.Protocol.PostgreSql:
        case WellKnown.Protocol.Oracle:
          providerAssemblyShortName = "Xtensive.Storage.Providers.Sql";
          break;
        default:
          throw new NotSupportedException(
            string.Format(Strings.ExProtocolXIsNotSupportedUseOneOfTheFollowingY, protocol, WellKnown.Protocol.All));
        }
        var providerAssemblyName = thisAssemblyName.Replace(thisAssemblyShortName , providerAssemblyShortName);

        // Check for merged assembly
        if (thisAssemblyShortName==WellKnown.MergedAssemblyName)
          providerAssemblyName = thisAssemblyName;

        // Creating the provider
        var providerAssembly = Assembly.Load(providerAssemblyName);
        var handlerProviderType = providerAssembly.GetTypes()
          .Where(type => type.IsPublicNonAbstractInheritorOf(typeof (HandlerFactory)))
          .Where(type => type.IsDefined(typeof (ProviderAttribute), false))
          .Where(type => type.GetAttributes<ProviderAttribute>(AttributeSearchOptions.InheritNone)
            .Any(attribute => attribute.Protocol==protocol))
          .FirstOrDefault();
        if (handlerProviderType==null)
          throw new DomainBuilderException(
            string.Format(Strings.ExStorageProviderNotFound, protocol));

        var handlerFactory = (HandlerFactory) Activator.CreateInstance(handlerProviderType);
        handlerFactory.Domain = BuildingContext.Current.Domain;
        handlerFactory.Initialize();
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
        ModelBuilder.Run();
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
        var upgradeContext = Upgrade.UpgradeContext.Current;
        if (upgradeContext != null)
          foreach (var pair in upgradeContext.UpgradeHandlers)
            pair.Value.OnSchemaReady();

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
          if (result.Status!=SchemaComparisonStatus.Equal || result.HasTypeChanges)
            throw new SchemaSynchronizationException(
              Strings.ExExtractedSchemaIsNotEqualToTheTargetSchema);
          break;
        case SchemaUpgradeMode.ValidateCompatible:
          if (result.Status!=SchemaComparisonStatus.Equal &&
            result.Status!=SchemaComparisonStatus.TargetIsSubset &&
            !result.HasTypeChanges)
            throw new SchemaSynchronizationException(
              Strings.ExExtractedSchemaIsNotCompatibleWithTheTargetSchema);
          break;
        case SchemaUpgradeMode.Recreate:
        case SchemaUpgradeMode.Perform:
            upgradeHandler.UpgradeSchema(result.UpgradeActions, extractedSchema, targetSchema);
          break;
        case SchemaUpgradeMode.PerformSafely:
          if ((result.Status!=SchemaComparisonStatus.Equal 
            && result.Status!=SchemaComparisonStatus.TargetIsSuperset)
            || (result.HasTypeChanges && !result.CanUpgradeTypesSafely))
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
