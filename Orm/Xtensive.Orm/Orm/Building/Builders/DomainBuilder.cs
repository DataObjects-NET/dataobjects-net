// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using System;
using System.Collections.Generic;
using System.Linq;
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
using DomainHandler = Xtensive.Orm.Providers.DomainHandler;
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
      CreateHandlers();
      BuildModel();
      CreateServices();
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
      using (Log.InfoRegion(Strings.LogCreatingX, typeof (ProviderDescriptor).GetShortName())) {
        ProviderDescriptor.Get(context.Configuration.ConnectionInfo.Provider);
      }
    }

    private void CreateDomain()
    {
      using (Log.InfoRegion(Strings.LogCreatingX, typeof (Domain).GetShortName())) {
        context.Domain = new Domain(context.Configuration);
      }
    }

    private void CreateHandlers()
    {
      var configuration = context.Domain.Configuration;
      var handlers = context.Domain.Handlers;
      var upgradeContext = context.BuilderConfiguration.UpgradeContext;

      using (Log.InfoRegion(Strings.LogCreatingX, typeof (DomainHandler).GetShortName())) {
        // NameBuilder
        handlers.NameBuilder = upgradeContext.NameBuilder;

        // SchemaResolver
        handlers.SchemaResolver = upgradeContext.SchemaResolver;

        // StorageDriver
        handlers.StorageDriver = upgradeContext.TemplateDriver.CreateNew(context.Domain);

        // GeneratorQueryBuilder
        handlers.SequenceQueryBuilder = new SequenceQueryBuilder(handlers.StorageDriver);

        // HandlerFactory
        handlers.Factory = upgradeContext.HandlerFactory;

        // DomainHandler
        handlers.DomainHandler = handlers.CreateAndInitialize<DomainHandler>();
        handlers.DomainHandler.CreateHandlers();

        // SchemaUpgradeHandler
        handlers.SchemaUpgradeHandler = handlers.CreateAndInitialize<SchemaUpgradeHandler>();
      }
    }

    private void CreateServices()
    {
      using (Log.InfoRegion(Strings.LogCreatingX, typeof (IServiceContainer).GetShortName())) {
        var domain = context.Domain;
        var configuration = domain.Configuration;
        var serviceContainerType = configuration.ServiceContainerType ?? typeof (ServiceContainer);
        var registrations = CreateServiceRegistrations(configuration)
          .Concat(KeyGeneratorFactory.GetRegistrations(context));
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
        var generators = domain.KeyGenerators;
        var initialized = new HashSet<IKeyGenerator>();
        var keysToProcess = domain.Model.Hierarchies
          .Select(h => h.Key)
          .Where(k => k.GeneratorKind!=KeyGeneratorKind.None);
        foreach (var keyInfo in keysToProcess) {
          var generator = domain.Services.Demand<IKeyGenerator>(keyInfo.GeneratorName);
          generators.Register(keyInfo, generator);
          if (keyInfo.IsFirstAmongSimilarKeys)
            generator.Initialize(context.Domain, keyInfo.TupleDescriptor);
          var temporaryGenerator = domain.Services.Get<ITemporaryKeyGenerator>(keyInfo.GeneratorName);
          if (temporaryGenerator==null)
            continue; // Temporary key generators are optional
          generators.RegisterTemporary(keyInfo, temporaryGenerator);
          if (keyInfo.IsFirstAmongSimilarKeys)
            temporaryGenerator.Initialize(context.Domain, keyInfo.TupleDescriptor);
        }
        generators.Lock();
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
        var schemas = BuildSchemas(domain, upgradeHandler);
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
          var emptySchema = upgradeHandler.GetEmptyStorageModel();
          result = SchemaComparer.Compare(extractedSchema, emptySchema, null, schemaUpgradeMode, context.Model);
          if (result.SchemaComparisonStatus!=SchemaComparisonStatus.Equal || result.HasColumnTypeChanges) {
            if (Log.IsLogged(LogEventTypes.Info))
              Log.Info(Strings.LogClearingComparisonResultX, result);
            upgradeHandler.UpgradeSchema(result.UpgradeActions, extractedSchema, emptySchema);
            upgradeHandler.ClearExtractedSchemaCache();
            extractedSchema = upgradeHandler.GetExtractedSchema();
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

    private static Pair<StorageModel, StorageModel> BuildSchemas(Domain domain, SchemaUpgradeHandler upgradeHandler)
    {
      var extractedModel = upgradeHandler.GetExtractedSchema();
      var targetModel = upgradeHandler.GetTargetSchema();

      if (Log.IsLogged(LogEventTypes.Info)) {
        Log.Info(Strings.LogExtractedSchema);
        extractedModel.Dump();

        Log.Info(Strings.LogTargetSchema);
        targetModel.Dump();
      }

      domain.StorageModel = targetModel;

      return new Pair<StorageModel, StorageModel>(extractedModel, targetModel);
    }

    private static IEnumerable<ServiceRegistration> CreateServiceRegistrations(DomainConfiguration configuration)
    {
      return configuration.Types.DomainServices.SelectMany(ServiceRegistration.CreateAll);
    }

    // Constructors

    private DomainBuilder(BuildingContext context)
    {
      this.context = context;
    }
  }
}
