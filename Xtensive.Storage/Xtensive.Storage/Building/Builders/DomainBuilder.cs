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
using Xtensive.Modelling.Actions;
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
        var thisAssembly = typeof (DomainBuilder).Assembly;
        Assembly providerAssembly;
        if (thisAssembly.GetName().Name==WellKnown.MergedAssemblyName) {
          GetProviderAssemblyName(protocol); // Just to validate a protocol
          providerAssembly = thisAssembly;
        }
        else
          providerAssembly = AssemblyHelper.LoadExtensionAssembly(GetProviderAssemblyName(protocol));
        
        // Creating the provider
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

    private static string GetProviderAssemblyName(string providerName)
    {
      switch (providerName) {
      case WellKnown.Protocol.Memory:
        return WellKnown.ProviderAssembly.Indexing;
      case WellKnown.Protocol.SqlServer:
      case WellKnown.Protocol.PostgreSql:
      case WellKnown.Protocol.Oracle:
        return WellKnown.ProviderAssembly.Sql;
      default:
        throw new NotSupportedException(
          string.Format(Strings.ExProtocolXIsNotSupportedUseOneOfTheFollowingY, providerName, WellKnown.Protocol.All));
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
        var localKeyGenerators = BuildingContext.Current.Domain.LocalKeyGenerators;
        var generatorFactory = handlerAccessor.HandlerFactory.CreateHandler<KeyGeneratorFactory>();
        var localGeneratorFactory = new LocalKeyGeneratorFactory();
        foreach (var keyProviderInfo in BuildingContext.Current.Model.KeyProviders) {
          KeyGenerator keyGenerator;
          if (keyProviderInfo.KeyGeneratorType==null)
            continue;
          if (keyProviderInfo.KeyGeneratorType == typeof(KeyGenerator)) {
            keyGenerator = generatorFactory.CreateGenerator(keyProviderInfo);
            var localKeyGenerator = localGeneratorFactory.CreateGenerator(keyProviderInfo);
            localKeyGenerator.Initialize();
            localKeyGenerators.Register(keyProviderInfo, keyGenerator);
          }
          else
            keyGenerator = (KeyGenerator)Activator.CreateInstance(keyProviderInfo.KeyGeneratorType, new object[] { keyProviderInfo });
          keyGenerator.Initialize();
          keyGenerators.Register(keyProviderInfo, keyGenerator);
        }
        keyGenerators.Lock();
        localKeyGenerators.Lock();
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
          result = SchemaComparer.Compare(extractedSchema, emptySchema, null, false, context.Model);
          if (result.Status!=SchemaComparisonStatus.Equal || result.HasTypeChanges) {
            if (Log.IsLogged(LogEventTypes.Info))
              Log.Info(Strings.LogClearingComparisonResultX, result);
            upgradeHandler.UpgradeSchema(result.UpgradeActions, extractedSchema, emptySchema);
            extractedSchema = upgradeHandler.GetExtractedSchema();
            hints = null; // Must re-bind them
          }
        }

        result = SchemaComparer.Compare(extractedSchema, targetSchema, hints, 
          schemaUpgradeMode==SchemaUpgradeMode.ValidateLegacy, context.Model);
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
            result.Status!=SchemaComparisonStatus.TargetIsSubset)
            throw new SchemaSynchronizationException(
              Strings.ExExtractedSchemaIsNotCompatibleWithTheTargetSchema);
          break;
        case SchemaUpgradeMode.Recreate:
        case SchemaUpgradeMode.Perform:
            upgradeHandler.UpgradeSchema(result.UpgradeActions, extractedSchema, targetSchema);
          break;
        case SchemaUpgradeMode.PerformSafely:
          var firstBreakingAction = result.BreakingActions.FirstOrDefault();
          if (firstBreakingAction!=null)
            throw new SchemaSynchronizationException(
              string.Format(Strings.ExCannotUpgradeSchemaSafely, GetErrorMessage(firstBreakingAction)));
            upgradeHandler.UpgradeSchema(result.UpgradeActions, extractedSchema, targetSchema);
          break;
        case SchemaUpgradeMode.ValidateLegacy:
          firstBreakingAction = result.BreakingActions.FirstOrDefault();
          if (!result.IsCompatible)
            throw new SchemaSynchronizationException(string.Format("Legacy schema is not compatible ({0}).", firstBreakingAction != null ? GetErrorMessage(firstBreakingAction) : string.Empty));
          break;
        default:
          throw new ArgumentOutOfRangeException("schemaUpgradeMode");
        }
      }
    }

    private static string GetErrorMessage(NodeAction unsafeAction)
    {
      var path = unsafeAction.Path;
      if (unsafeAction is PropertyChangeAction) {
        return string.Format(Strings.CantChangeTypeOfColumnX, path);
      }
      if (unsafeAction is RemoveNodeAction) {
        var source = ((NodeDifference) unsafeAction.Difference).Source;
        if (source is TableInfo)
          return string.Format(Strings.CantRemoveTableX, source.Path);
        if (source is ColumnInfo)
          return string.Format(Strings.CantRemoveColumnX, source.Path);
      }
      if (unsafeAction is CreateNodeAction) {
        var target = ((NodeDifference) unsafeAction.Difference).Target;
        if (target is TableInfo)
          return string.Format("Can't find table {0}", target.Path);
        if (target is ColumnInfo)
          return string.Format("Can't find column {0}", target.Path);
      }
      return string.Empty;
    }
  }
}
