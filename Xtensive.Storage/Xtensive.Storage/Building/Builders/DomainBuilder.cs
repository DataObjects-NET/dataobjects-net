// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.IoC;
using Xtensive.Core.Reflection;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Resources;
using Activator = System.Activator;
using UpgradeContext = Xtensive.Storage.Upgrade.UpgradeContext;

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

      var upgradeContext = UpgradeContext.Demand();
      using (Log.InfoRegion(Strings.LogBuildingX, typeof (Domain).GetShortName())) {
        using (new BuildingScope(context)) {
          CreateDomain();
          CreateHandlerFactory();
          CreateDomainHandler();
          CreateNameBuilder();
          CreateServices();
          BuildModel();
          CreateKeyGenerators();

          using (Session.Open(context.Domain, SessionType.System)) {
            context.SystemSessionHandler = Session.Demand().Handler;
            try {
              upgradeContext.TransactionScope = Transaction.Open();
              SynchronizeSchema(builderConfiguration.SchemaUpgradeMode);
              context.Domain.Handler.BuildMapping();
              TypeIdBuilder.BuildTypeIds();
              BuildTypeLevelCaches();
              if (builderConfiguration.UpgradeHandler!=null)
                builderConfiguration.UpgradeHandler.Invoke();
              upgradeContext.TransactionScope.Complete();
            }
            finally {
              upgradeContext.TransactionScope.DisposeSafely();
            }
          }
        }
      }
      return context.Domain;
    }

    private static void CreateDomain()
    {
      using (Log.InfoRegion(Strings.LogCreatingX, typeof (Domain).GetShortName())) {
        var domain = new Domain(BuildingContext.Demand().Configuration);
        BuildingContext.Demand().Domain = domain;
      }
    }

    /// <exception cref="DomainBuilderException">Something went wrong.</exception>
    private static void CreateHandlerFactory()
    {
      using (Log.InfoRegion(Strings.LogCreatingX, typeof (HandlerFactory).GetShortName())) {
        string protocol = BuildingContext.Demand().Configuration.ConnectionInfo.Provider;
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
        handlerFactory.Domain = BuildingContext.Demand().Domain;
        handlerFactory.Initialize();
        var handlerAccessor = BuildingContext.Demand().Domain.Handlers;
        handlerAccessor.HandlerFactory = handlerFactory;
      }
    }

    private static string GetProviderAssemblyName(string providerName)
    {
      switch (providerName) {
      case WellKnown.Provider.Memory:
        return WellKnown.ProviderAssembly.Indexing;
      case WellKnown.Provider.SqlServer:
      case WellKnown.Provider.SqlServerCe:
      case WellKnown.Provider.PostgreSql:
      case WellKnown.Provider.Oracle:
        return WellKnown.ProviderAssembly.Sql;
      default:
        throw new NotSupportedException(
          string.Format(Strings.ExProviderXIsNotSupportedUseOneOfTheFollowingY, providerName, WellKnown.Provider.All));
      }
    }

    private static void CreateDomainHandler()
    {
      using (Log.InfoRegion(Strings.LogCreatingX, typeof (DomainHandler).GetShortName())) {
        var handlerAccessor = BuildingContext.Demand().Domain.Handlers;
        handlerAccessor.DomainHandler = handlerAccessor.HandlerFactory.CreateHandler<DomainHandler>();
        handlerAccessor.DomainHandler.Initialize();
      }
    }

    private static void CreateNameBuilder()
    {
      using (Log.InfoRegion(Strings.LogCreatingX, typeof (NameBuilder).GetShortName())) {
        var handlerAccessor = BuildingContext.Demand().Domain.Handlers;
        handlerAccessor.NameBuilder = handlerAccessor.HandlerFactory.CreateHandler<NameBuilder>();
        handlerAccessor.NameBuilder.Initialize(handlerAccessor.Domain.Configuration.NamingConvention);
      }
    }

    internal static IEnumerable<ServiceRegistration> CreateServiceRegistrations(
      DomainConfiguration configuration)
    {
      return from type in configuration.Types.DomainServices
      from registration in ServiceRegistration.CreateAll(type)
      select registration;
    }

    internal static IEnumerable<ServiceRegistration> CreateKeyGeneratorRegistrations(
      DomainConfiguration configuration)
    {
      var keyGeneratorRegistrations = (
        from type in configuration.Types.KeyGenerators
        from registration in ServiceRegistration.CreateAll(type)
        select registration
        ).ToList();
      var defaultKeyGeneratorRegistrations = (
        from type in KeyGenerator.SupportedKeyFieldTypes
        let registration = new ServiceRegistration(typeof (KeyGenerator), type.GetShortName(),
          typeof(CachingKeyGenerator<>).MakeGenericType(type), true)
        where !keyGeneratorRegistrations.Any(r => r.Type==registration.Type && r.Name==registration.Name)
        select registration
        ).ToList();
      return keyGeneratorRegistrations.Concat(defaultKeyGeneratorRegistrations);
    }

    internal static IEnumerable<ServiceRegistration> CreateQueryPreprocessorRegistrations(
      DomainConfiguration configuration)
    {
      return from type in configuration.Types.QueryPreprocessors
      from registration in ServiceRegistration.CreateAll(type)
      select registration;
    }

    private static void CreateServices()
    {
      using (Log.InfoRegion(Strings.LogCreatingX, typeof (IServiceContainer).GetShortName())) {
        var domain = BuildingContext.Demand().Domain;
        var configuration = domain.Configuration;
        var serviceContainerType = configuration.ServiceContainerType ?? typeof (ServiceContainer);
        domain.Services =
          ServiceContainer.Create(typeof (ServiceContainer),
            CreateServiceRegistrations(configuration)
            .Concat(CreateKeyGeneratorRegistrations(configuration))
            .Concat(CreateQueryPreprocessorRegistrations(configuration)),
            ServiceContainer.Create(serviceContainerType, domain.Handler.CreateBaseServices()));
      }
    }

    private static void BuildModel()
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.Model)) {
        var context = BuildingContext.Demand();
        var domain = context.Domain;
        ModelBuilder.Run();
        domain.Model = context.Model;
        domain.Model.Lock(true);
      }
    }

    private static void CreateKeyGenerators()
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.KeyGenerators)) {
        var context = BuildingContext.Demand();
        var domain = context.Domain;
        foreach (var keyInfo in domain.Model.Hierarchies.Select(h => h.Key)) {
          KeyGenerator generator = null;
          if (keyInfo.GeneratorType!=null) {
            generator = (KeyGenerator) domain.Services.Demand(
              keyInfo.GeneratorType, keyInfo.GeneratorName);
            if (!generator.IsInitialized) {
              generator.Initialize(domain.Handlers, keyInfo);
            }
          }
          // So non-exisitng (==null) generators are added as well!
          domain.KeyGenerators.Add(keyInfo, generator);
        }
      }
    }

    private static void BuildTypeLevelCaches()
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.CachedTypeInfo)) {
        var context = BuildingContext.Demand();
        var domain = context.Domain;
        foreach (var typeInfo in domain.Model.Types)
          if (typeInfo.TypeId!=Model.TypeInfo.NoTypeId)
            domain.TypeLevelCaches.Add(typeInfo.TypeId, new TypeLevelCache(typeInfo));
      }
    }

    /// <exception cref="SchemaSynchronizationException">Extracted schema is incompatible 
    /// with the target schema in specified <paramref name="schemaUpgradeMode"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><c>schemaUpgradeMode</c> is out of range.</exception>
    private static void SynchronizeSchema(SchemaUpgradeMode schemaUpgradeMode)
    {
      var context = BuildingContext.Demand();
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
          result = SchemaComparer.Compare(extractedSchema, emptySchema, null, schemaUpgradeMode, context.Model);
          if (result.Status!=SchemaComparisonStatus.Equal || result.HasTypeChanges) {
            if (Log.IsLogged(LogEventTypes.Info))
              Log.Info(Strings.LogClearingComparisonResultX, result);
            upgradeHandler.UpgradeSchema(result.UpgradeActions, extractedSchema, emptySchema);
            extractedSchema = upgradeHandler.GetExtractedSchema();
            hints = null; // Must re-bind them
          }
        }

        result = SchemaComparer.Compare(extractedSchema, targetSchema, hints, schemaUpgradeMode, context.Model);
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
