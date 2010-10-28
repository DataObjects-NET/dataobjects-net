// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Threading;
using Xtensive.Core;
using Xtensive.Diagnostics;
using Xtensive.IoC;
using Xtensive.Reflection;
using Xtensive.Tuples;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Orm.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Model;
using Xtensive.Storage.Providers;
using Xtensive.Orm.Resources;
using Activator = System.Activator;
using UpgradeContext = Xtensive.Orm.Upgrade.UpgradeContext;
using Tuple=Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Building.Builders
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
          CreateHandlers();
          CreateServices();
          BuildModel();
          CreateKeyGenerators();

          using (var session = context.Domain.OpenSession(SessionType.System))
          using (session.Activate()) {
            context.SystemSessionHandler = session.Handler;
            try {
              upgradeContext.TransactionScope = session.OpenTransaction();
              SynchronizeSchema(builderConfiguration.SchemaUpgradeMode);
              context.Domain.Handler.BuildMapping();
              if (builderConfiguration.UpgradeHandler!=null)
                // We don't build TypeIds here 
                // leaving this job for SystemUpgradeHandler
                builderConfiguration.UpgradeHandler.Invoke();
              else
                TypeIdBuilder.BuildTypeIds(false);
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
        GetProviderAssemblyName(protocol); // Just to validate a protocol
        var providerAssembly = thisAssembly;
        
        // Creating the provider
        var handlerProviderType = providerAssembly.GetTypes()
          .Where(type => type.IsPublicNonAbstractInheritorOf(typeof (HandlerFactory)))
          .Where(type => type.IsDefined(typeof (ProviderAttribute), false))
          .Where(type => type.GetAttributes<ProviderAttribute>()
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

    private static void CreateHandlers()
    {
      var handlers = BuildingContext.Demand().Domain.Handlers;
      var factory = handlers.HandlerFactory;
      using (Log.InfoRegion(Strings.LogCreatingX, typeof (DomainHandler).GetShortName())) {
        // DomainHandler
        handlers.DomainHandler = factory.CreateHandler<DomainHandler>();
        handlers.DomainHandler.Initialize();
        // NameBuilder
        handlers.NameBuilder = factory.CreateHandler<NameBuilder>();
        handlers.NameBuilder.Initialize(handlers.Domain.Configuration.NamingConvention);
        // SchemaUpgradeHandler
        handlers.SchemaUpgradeHandler = factory.CreateHandler<SchemaUpgradeHandler>();
        handlers.SchemaUpgradeHandler.Initialize();
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
        var model = context.Model;
        model.Lock(true);
        domain.Model = model;

        // Starting background process caching the Tuples related to model
        Func<bool> backgroundCacher = () => {
          var processedHierarchies = new HashSet<HierarchyInfo>();
          foreach (var type in model.Types) {
            try {
              var ignored1 = type.TuplePrototype;
              var hierarchy = type.Hierarchy;
              if (hierarchy==null) // It's Structure
                continue;
              if (!processedHierarchies.Contains(hierarchy)) {
                var key = hierarchy.Key;
                if (key!=null && key.TupleDescriptor!=null) {
                  var ignored2 = Tuple.Create(key.TupleDescriptor);
                }
              }
            }
            catch {
              // We supress everything here.
            }
          }
          return true;
        };
        backgroundCacher.InvokeAsync();
      }
    }

    private static void CreateKeyGenerators()
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.KeyGenerators)) {
        var context = BuildingContext.Demand();
        var domain = context.Domain;
        var keyGenerators = domain.KeyGenerators;
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
          keyGenerators.Add(keyInfo, generator);
        }

        // Starting background process invoking KeyGenerator.Prepare methods
        Func<bool> backgroundCacher = () => {
          foreach (var pair in keyGenerators) {
            try {
              var keyGenerator = pair.Value;
              if (keyGenerator!=null)
                keyGenerator.Prepare();
            }
            catch {
              // We supress everything here.
            }
          }
          return true;
        };
        backgroundCacher.InvokeAsync();
      }
    }

    /// <exception cref="SchemaSynchronizationException">Extracted schema is incompatible 
    /// with the target schema in specified <paramref name="schemaUpgradeMode"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><c>schemaUpgradeMode</c> is out of range.</exception>
    private static void SynchronizeSchema(SchemaUpgradeMode schemaUpgradeMode)
    {
      var context = BuildingContext.Demand();
      var domain = context.Domain;
      var upgradeHandler = domain.Handlers.SchemaUpgradeHandler;

      using (Log.InfoRegion(Strings.LogSynchronizingSchemaInXMode, schemaUpgradeMode)) {
        var schemas = BuildSchemasAsync(domain, upgradeHandler);
        var extractedSchema = schemas.First;
        var targetSchema = schemas.Second;

        // Hints
        HintSet hints = null;
        if (context.BuilderConfiguration.SchemaReadyHandler!=null)
          hints = context.BuilderConfiguration.SchemaReadyHandler.Invoke(extractedSchema, targetSchema);
        var upgradeContext = UpgradeContext.Current;
        if (upgradeContext!=null)
          foreach (var pair in upgradeContext.UpgradeHandlers)
            pair.Value.OnSchemaReady();

        if (schemaUpgradeMode==SchemaUpgradeMode.Skip)
          return; // Skipping comparison completely

        SchemaComparisonResult result;
        // Let's clear the schema if mode is Recreate
        if (schemaUpgradeMode==SchemaUpgradeMode.Recreate) {
          var emptySchema = new StorageInfo();
          result = SchemaComparer.Compare(extractedSchema, emptySchema, null, schemaUpgradeMode, context.Model);
          if (result.SchemaComparisonStatus!=SchemaComparisonStatus.Equal || result.HasColumnTypeChanges) {
            if (Log.IsLogged(LogEventTypes.Info))
              Log.Info(Strings.LogClearingComparisonResultX, result);
            upgradeHandler.UpgradeSchema(result.UpgradeActions, extractedSchema, emptySchema);
            upgradeHandler.ClearExtractedSchemaCache();
            extractedSchema = upgradeHandler.GetExtractedSchemaProvider().Invoke();
            hints = null; // Must re-bind them
          }
        }

        result = SchemaComparer.Compare(extractedSchema, targetSchema, hints, schemaUpgradeMode, context.Model);
        if (!schemaUpgradeMode.In(SchemaUpgradeMode.Skip, SchemaUpgradeMode.ValidateCompatible, SchemaUpgradeMode.Recreate))
          Upgrade.Log.Info(result.ToString());
        if (Log.IsLogged(LogEventTypes.Info))
          Log.Info(Strings.LogComparisonResultX, result);
        if (context.BuilderConfiguration.UpgradeActionsReadyHandler!=null)
          context.BuilderConfiguration.UpgradeActionsReadyHandler.Invoke(
            (NodeDifference) result.Difference, result.UpgradeActions);

        switch (schemaUpgradeMode) {
        case SchemaUpgradeMode.ValidateExact:
          if (result.SchemaComparisonStatus!=SchemaComparisonStatus.Equal || result.HasColumnTypeChanges)
            throw new SchemaSynchronizationException(
              Strings.ExExtractedSchemaIsNotEqualToTheTargetSchema_DetailsX.FormatWith(result));
          break;
        case SchemaUpgradeMode.ValidateCompatible:
          if (result.SchemaComparisonStatus!=SchemaComparisonStatus.Equal && result.SchemaComparisonStatus!=SchemaComparisonStatus.TargetIsSubset)
            throw new SchemaSynchronizationException(
              Strings.ExExtractedSchemaIsNotCompatibleWithTheTargetSchema_DetailsX.FormatWith(result));
          break;
        case SchemaUpgradeMode.PerformSafely:
          if (result.HasUnsafeActions)
            throw new SchemaSynchronizationException(
              Strings.ExCanNotUpgradeSchemaSafely_DetailsX.FormatWith(result));
          goto case SchemaUpgradeMode.Perform;
        case SchemaUpgradeMode.Recreate:
        case SchemaUpgradeMode.Perform:
          upgradeHandler.UpgradeSchema(result.UpgradeActions, extractedSchema, targetSchema);
          if (result.UpgradeActions.Any())
            upgradeHandler.ClearExtractedSchemaCache();
          break;
        case SchemaUpgradeMode.ValidateLegacy:
          if (result.IsCompatibleInLegacyMode!=true)
            throw new SchemaSynchronizationException(
              Strings.ExLegacySchemaIsNotCompatible_DetailsX.FormatWith(result));
          break;
        default:
          throw new ArgumentOutOfRangeException("schemaUpgradeMode");
        }
      }
    }

    private static Pair<StorageInfo, StorageInfo> BuildSchemasAsync(Domain domain, SchemaUpgradeHandler upgradeHandler)
    {
      var extractedSchema = upgradeHandler.GetExtractedSchemaProvider().InvokeAsync();
      var targetSchema = upgradeHandler.GetTargetSchemaProvider().InvokeAsync();

      Func<Func<StorageInfo>, Pair<StorageInfo,StorageInfo>> cloner = schemaProvider => {
        var origin = schemaProvider.Invoke();
        origin.Lock();
        var clone = (StorageInfo) origin.Clone(null, StorageInfo.DefaultName);
        return new Pair<StorageInfo, StorageInfo>(origin, clone);
      };

      var extractedSchemaCloner = cloner.InvokeAsync(extractedSchema);
      var targetSchemaCloner = cloner.InvokeAsync(targetSchema);
      
      var extractedSchemas = extractedSchemaCloner.Invoke();
      var targetSchemas = targetSchemaCloner.Invoke();

      domain.ExtractedSchema = extractedSchemas.First; // Assigning locked schema
      if (Log.IsLogged(LogEventTypes.Info)) {
        Log.Info(Strings.LogExtractedSchema);
        domain.ExtractedSchema.Dump();
      }
      domain.Schema = targetSchemas.First; // Assigning locked schema
      if (Log.IsLogged(LogEventTypes.Info)) {
        Log.Info(Strings.LogTargetSchema);
        domain.Schema.Dump();
      }
      Thread.MemoryBarrier();

      // Returning unlocked clones
      return new Pair<StorageInfo, StorageInfo>(extractedSchemas.Second, targetSchemas.Second);
    }
  }
}
