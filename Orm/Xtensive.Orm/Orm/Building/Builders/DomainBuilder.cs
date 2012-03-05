// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xtensive.Core;
using Xtensive.Diagnostics;
using Xtensive.IoC;
using Xtensive.Modelling.Comparison;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade.Model;
using Xtensive.Reflection;
using Xtensive.Sql;
using DomainHandler = Xtensive.Orm.Providers.DomainHandler;
using HandlerFactory = Xtensive.Orm.Providers.HandlerFactory;
using SchemaUpgradeHandler = Xtensive.Orm.Providers.SchemaUpgradeHandler;
using Tuple = Xtensive.Tuples.Tuple;
using UpgradeContext = Xtensive.Orm.Upgrade.UpgradeContext;

namespace Xtensive.Orm.Building.Builders
{
  /// <summary>
  /// Utility class for <see cref="Domain"/> building.
  /// </summary>
  internal sealed class DomainBuilder
  {
    private readonly BuildingContext context;
    private ProviderFactory providerFactory;

    /// <summary>
    /// Builds the domain.
    /// </summary>
    /// <param name="configuration">The domain configuration.</param>
    /// <param name="builderConfiguration">The builder configuration.</param>
    /// <returns>Built domain.</returns>
    public static Domain BuildDomain(
      DomainConfiguration configuration, DomainBuilderConfiguration builderConfiguration)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");
      ArgumentValidator.EnsureArgumentNotNull(builderConfiguration, "builderConfiguration");

      if (!configuration.IsLocked)
        configuration.Lock(true);

      var context = new BuildingContext(configuration, builderConfiguration);
      using (Log.InfoRegion(Strings.LogBuildingX, typeof (Domain).GetShortName())) {
        using (new BuildingScope(context)) {
          new DomainBuilder(context).Run();
        }
      }

      return context.Domain;
    }

    private void Run()
    {
      CreateProviderFactory();
      CreateDomain();
      CreateHandlerFactory();
      CreateHandlers();
      CreateServices();
      BuildModel();
      CreateKeyGenerators();
      ConfigureServices();
      PerformUpgradeActions();
    }

    private void PerformUpgradeActions()
    {
      var upgradeContext = UpgradeContext.Demand();
      var builderConfiguration = context.BuilderConfiguration;
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
            TypeIdBuilder.BuildTypeIds(context, false);
          upgradeContext.TransactionScope.Complete();
        }
        finally {
          upgradeContext.TransactionScope.DisposeSafely();
        }
      }
    }

    private void CreateProviderFactory()
    {
      using (Log.InfoRegion(Strings.LogCreatingX, typeof (ProviderFactory).GetShortName())) {
        providerFactory = ProviderFactory.Get(context.Configuration.ConnectionInfo.Provider);
      }
    }

    private void CreateDomain()
    {
      using (Log.InfoRegion(Strings.LogCreatingX, typeof (Domain).GetShortName())) {
        context.Domain = new Domain(context.Configuration);
      }
    }

    private void CreateHandlerFactory()
    {
      using (Log.InfoRegion(Strings.LogCreatingX, typeof (HandlerFactory).GetShortName())) {
        var handlers = context.Domain.Handlers;
        var handlerFactory = providerFactory.CreateHandlerFactory();
        handlerFactory.Initialize(handlers);
        handlers.Factory = handlerFactory;
      }
    }

    private void CreateHandlers()
    {
      var handlers = context.Domain.Handlers;
      var factory = handlers.Factory;
      var configuration = context.Domain.Configuration;

      using (Log.InfoRegion(Strings.LogCreatingX, typeof (DomainHandler).GetShortName())) {
        // StorageDriver
        var storageDriver = new StorageDriver(providerFactory.CreateDriverFactory(), context.Domain);
        handlers.StorageDriver = storageDriver;
        // DomainHandler
        var domainHandler = factory.CreateHandler<DomainHandler>();
        handlers.DomainHandler = domainHandler;
        domainHandler.Initialize();
        // NameBuilder
        handlers.NameBuilder = new NameBuilder(configuration, storageDriver.ProviderInfo);
        // SchemaUpgradeHandler
        var schemaUpgradeHandler = factory.CreateHandler<SchemaUpgradeHandler>();
        handlers.SchemaUpgradeHandler = schemaUpgradeHandler;
        schemaUpgradeHandler.Initialize();
      }
    }

    private void CreateServices()
    {
      using (Log.InfoRegion(Strings.LogCreatingX, typeof (IServiceContainer).GetShortName())) {
        var domain = context.Domain;
        var configuration = domain.Configuration;
        var serviceContainerType = configuration.ServiceContainerType ?? typeof (ServiceContainer);
        var registrations = CreateServiceRegistrations(configuration)
          .Concat(CreateKeyGeneratorRegistrations(configuration));
        var baseServiceContainer = domain.Handler.CreateBaseServices();
        domain.Services = ServiceContainer.Create(
          typeof (ServiceContainer), registrations, ServiceContainer.Create(serviceContainerType, baseServiceContainer));
      }
    }

    private void BuildModel()
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.Model)) {
        var domain = context.Domain;
        ModelBuilder.Run(context);
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

    private void CreateKeyGenerators()
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.KeyGenerators)) {
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

    private void ConfigureServices()
    {
      context.Domain.Handler.ConfigureServices();
    }

    /// <exception cref="SchemaSynchronizationException">Extracted schema is incompatible 
    /// with the target schema in specified <paramref name="schemaUpgradeMode"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><c>schemaUpgradeMode</c> is out of range.</exception>
    private void SynchronizeSchema(SchemaUpgradeMode schemaUpgradeMode)
    {
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
          var emptySchema = new StorageModel();
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
        var shouldDumpSchema = !schemaUpgradeMode
          .In(SchemaUpgradeMode.Skip, SchemaUpgradeMode.ValidateCompatible, SchemaUpgradeMode.Recreate);
        if (shouldDumpSchema)
          Upgrade.Log.Info(result.ToString());
        if (Log.IsLogged(LogEventTypes.Info))
          Log.Info(Strings.LogComparisonResultX, result);
        if (context.BuilderConfiguration.UpgradeActionsReadyHandler!=null)
          context.BuilderConfiguration.UpgradeActionsReadyHandler.Invoke(
            (NodeDifference) result.Difference, result.UpgradeActions);

        switch (schemaUpgradeMode) {
          case SchemaUpgradeMode.ValidateExact:
            if (result.SchemaComparisonStatus!=SchemaComparisonStatus.Equal || result.HasColumnTypeChanges)
              throw new SchemaSynchronizationException(result);
            break;
          case SchemaUpgradeMode.ValidateCompatible:
            if (result.SchemaComparisonStatus!=SchemaComparisonStatus.Equal
              && result.SchemaComparisonStatus!=SchemaComparisonStatus.TargetIsSubset)
              throw new SchemaSynchronizationException(result);
            break;
          case SchemaUpgradeMode.PerformSafely:
            if (result.HasUnsafeActions)
              throw new SchemaSynchronizationException(result);
            goto case SchemaUpgradeMode.Perform;
          case SchemaUpgradeMode.Recreate:
          case SchemaUpgradeMode.Perform:
            upgradeHandler.UpgradeSchema(result.UpgradeActions, extractedSchema, targetSchema);
            if (result.UpgradeActions.Any())
              upgradeHandler.ClearExtractedSchemaCache();
            break;
          case SchemaUpgradeMode.ValidateLegacy:
            if (result.IsCompatibleInLegacyMode!=true)
              throw new SchemaSynchronizationException(result);
            break;
          default:
            throw new ArgumentOutOfRangeException("schemaUpgradeMode");
        }
      }
    }

    private static Pair<StorageModel, StorageModel> BuildSchemasAsync(Domain domain, SchemaUpgradeHandler upgradeHandler)
    {
      var extractedSchema = upgradeHandler.GetExtractedSchemaProvider().InvokeAsync();
      var targetSchema = upgradeHandler.GetTargetSchemaProvider().InvokeAsync();

      Func<Func<StorageModel>, Pair<StorageModel, StorageModel>> cloner = schemaProvider => {
        var origin = schemaProvider.Invoke();
        origin.Lock();
        var clone = (StorageModel) origin.Clone(null, StorageModel.DefaultName);
        return new Pair<StorageModel, StorageModel>(origin, clone);
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
      return new Pair<StorageModel, StorageModel>(extractedSchemas.Second, targetSchemas.Second);
    }

    private static IEnumerable<ServiceRegistration> CreateServiceRegistrations(DomainConfiguration configuration)
    {
      return configuration.Types.DomainServices.SelectMany(ServiceRegistration.CreateAll);
    }

    public static IEnumerable<ServiceRegistration> CreateKeyGeneratorRegistrations(
      DomainConfiguration configuration)
    {
      var userRegistrations = configuration.Types.KeyGenerators
        .SelectMany(ServiceRegistration.CreateAll)
        .ToList();

      var standardRegistrations =
        KeyGenerator.SupportedKeyFieldTypes
          .Select(type => new {type, reg = GetStandardKeyGenerator(type)})
          .Where(item => !userRegistrations.Any(r => r.Type==item.reg.Type && r.Name==item.reg.Name))
          .Select(item => item.reg)
          .ToList();

      var allRegistrations = userRegistrations.Concat(standardRegistrations);

      if (!configuration.IsMultidatabase)
        return allRegistrations;

      // If we are in multidatabase mode key generators will have database specific suffixes
      // We need to handle it by building a cross product between all key generators and all databases.
      // TODO: handle user's per-database registrations
      // They should have more priority than user's database-agnostic key generators
      // and standard key generators (which are always database-agnostic).

      var databases = configuration.MappingRules
        .Select(rule => rule.Database)
        .Where(db => !string.IsNullOrEmpty(db))
        .Concat(Enumerable.Repeat(configuration.DefaultDatabase, 1))
        .ToHashSet();

      return allRegistrations.SelectMany(_ => databases, AddKeyGeneratorLocation);
    }

    private static ServiceRegistration AddKeyGeneratorLocation(ServiceRegistration originalRegistration, string database)
    {
      return new ServiceRegistration(
        originalRegistration.Type,
        NameBuilder.BuildKeyGeneratorName(originalRegistration.Name, database),
        originalRegistration.MappedType,
        originalRegistration.Singleton);
    }

    private static ServiceRegistration GetStandardKeyGenerator(Type keyType)
    {
      return new ServiceRegistration(
        typeof (KeyGenerator),
        keyType.GetShortName(),
        typeof (CachingKeyGenerator<>).MakeGenericType(keyType),
        true);
    }

    // Constructors

    private DomainBuilder(BuildingContext context)
    {
      this.context = context;
    }
  }
}
