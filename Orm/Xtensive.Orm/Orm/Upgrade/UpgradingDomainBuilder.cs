// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.23

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.IoC;
using Xtensive.Modelling.Comparison;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Logging;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade.Model;
using Xtensive.Reflection;
using Xtensive.Sql;

namespace Xtensive.Orm.Upgrade
{
  internal sealed class UpgradingDomainBuilder
  {
    private readonly UpgradeContext context;
    private readonly DomainUpgradeMode upgradeMode;

    private FutureResult<SqlWorkerResult> workerResult;

    public static Domain Build(DomainConfiguration configuration)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");

      if (configuration.ConnectionInfo==null)
        throw new ArgumentException(Strings.ExConnectionInfoIsMissing, "configuration");

      if (!configuration.IsLocked)
        configuration.Lock();

      LogManager.Default.AutoInitialize();

      var context = new UpgradeContext(configuration);

      using (context.Activate())
      using (context.Services) {
        return new UpgradingDomainBuilder(context).Run();
      }
    }

    public static StorageNode BuildNode(Domain parentDomain, NodeConfiguration nodeConfiguration)
    {
      ArgumentValidator.EnsureArgumentNotNull(parentDomain, "parentDomain");
      ArgumentValidator.EnsureArgumentNotNull(nodeConfiguration, "nodeConfiguration");

      nodeConfiguration.Validate(parentDomain.Configuration);
      if (!nodeConfiguration.IsLocked)
        nodeConfiguration.Lock();

      var context = new UpgradeContext(parentDomain, nodeConfiguration);

      using (context.Activate())
      using (context.Services) {
        new UpgradingDomainBuilder(context).Run();
        return context.StorageNode;
      }
    }

    private Domain Run()
    {
      BuildServices();
      OnPrepare();

      var domain = upgradeMode.IsMultistage() ? BuildMultistageDomain() : BuildSingleStageDomain();

      OnComplete(domain);
      CompleteUpgradeTransaction();
      context.Services.ClearTemporaryResources();

      return domain;
    }

    private void CompleteUpgradeTransaction()
    {
      var connection = context.Services.Connection;
      var driver = context.Services.StorageDriver;

      if (connection.ActiveTransaction==null)
        return;

      try {
        driver.CommitTransaction(null, connection);
      }
      catch {
        driver.RollbackTransaction(null, connection);
        throw;
      }
    }

    private FutureResult<T> CreateResult<T>(T value)
    {
      return new ValueFutureResult<T>(value);
    }

    private FutureResult<T> CreateResult<T>(Func<T> action)
    {
      if (context.Configuration.BuildInParallel)
        return new AsyncFutureResult<T>(action, UpgradeLog.Instance);
      return new SynchronousFutureResult<T>(action);
    }

    private Domain BuildMultistageDomain()
    {
      Domain finalDomain;
      using (StartSqlWorker()) {
        var finalDomainResult = context.ParentDomain!=null
          ? CreateResult(context.ParentDomain)
          : CreateResult(CreateBuilder(UpgradeStage.Final));
        using (finalDomainResult) {
          OnConfigureUpgradeDomain();
          using (var upgradeDomain = CreateBuilder(UpgradeStage.Upgrading).Invoke()) {
            CompleteSqlWorker();
            PerformUpgrade(upgradeDomain, UpgradeStage.Upgrading);
          }
          finalDomain = finalDomainResult.Get();
        }
      }
      PerformUpgrade(finalDomain, UpgradeStage.Final);
      return finalDomain;
    }

    private Domain BuildSingleStageDomain()
    {
      using (StartSqlWorker()) {
        var domain = context.ParentDomain ?? CreateBuilder(UpgradeStage.Final).Invoke();
        CompleteSqlWorker();
        PerformUpgrade(domain, UpgradeStage.Final);
        return domain;
      }
    }

    private void BuildServices()
    {
      var services = context.Services;
      var configuration = context.Configuration;

      services.Configuration = configuration;
      services.IndexFilterCompiler = new PartialIndexFilterCompiler();

      if (context.ParentDomain==null) {
        var descriptor = ProviderDescriptor.Get(configuration.ConnectionInfo.Provider);
        var driverFactory = (SqlDriverFactory) Activator.CreateInstance(descriptor.DriverFactory);
        var handlerFactory = (HandlerFactory) Activator.CreateInstance(descriptor.HandlerFactory);
        var driver = StorageDriver.Create(driverFactory, configuration);
        services.HandlerFactory = handlerFactory;
        services.StorageDriver = driver;
        services.NameBuilder = new NameBuilder(configuration, driver.ProviderInfo);
      }
      else {
        var handlers = context.ParentDomain.Handlers;
        services.HandlerFactory = handlers.Factory;
        services.StorageDriver = handlers.StorageDriver;
        services.NameBuilder = handlers.NameBuilder;
      }

      CreateConnection(services);
      var defaultSchemaInfo = services.StorageDriver.GetDefaultSchema(services.Connection);
      services.MappingResolver = MappingResolver.Create(configuration, context.NodeConfiguration, defaultSchemaInfo);
      BuildExternalServices(services, configuration);
      services.Lock();

      context.TypeIdProvider = new TypeIdProvider(context);
    }

    private void CreateConnection(UpgradeServiceAccessor serviceAccessor)
    {
      var driver = serviceAccessor.StorageDriver;
      var connection = driver.CreateConnection(null);

      driver.ApplyNodeConfiguration(connection, context.NodeConfiguration);

      try {
        driver.OpenConnection(null, connection);
        driver.BeginTransaction(null, connection, null);
      }
      catch {
        connection.Dispose();
        throw;
      }

      if (driver.ProviderInfo.Supports(ProviderFeatures.SingleConnection))
        serviceAccessor.RegisterTemporaryResource(connection);
      else
        serviceAccessor.RegisterResource(connection);

      serviceAccessor.Connection = connection;
    }

    private void BuildExternalServices(UpgradeServiceAccessor serviceAccessor, DomainConfiguration configuration)
    {
      var standardRegistrations = new[] {
        new ServiceRegistration(typeof (DomainConfiguration), configuration),
        new ServiceRegistration(typeof (UpgradeContext), context)
      };

      var modules = configuration.Types.Modules
        .Select(type => new ServiceRegistration(typeof (IModule), type, false));
      var handlers = configuration.Types.UpgradeHandlers
        .Select(type => new ServiceRegistration(typeof (IUpgradeHandler), type, false));

      var registrations = standardRegistrations.Concat(modules).Concat(handlers);
      var serviceContainer = new ServiceContainer(registrations);
      serviceAccessor.RegisterResource(serviceContainer);

      BuildModules(serviceAccessor, serviceContainer);
      BuildUpgradeHandlers(serviceAccessor, serviceContainer);
    }

    private static void BuildModules(UpgradeServiceAccessor serviceAccessor, IServiceContainer serviceContainer)
    {
      serviceAccessor.Modules = new ReadOnlyList<IModule>(serviceContainer.GetAll<IModule>().ToList());
    }

    private static void BuildUpgradeHandlers(UpgradeServiceAccessor serviceAccessor, IServiceContainer serviceContainer)
    {
      // Getting user handlers
      var userHandlers =
        from handler in serviceContainer.GetAll<IUpgradeHandler>()
        let assembly = handler.Assembly ?? handler.GetType().Assembly
        where handler.IsEnabled
        group handler by assembly;

      // Adding user handlers
      var handlers = new Dictionary<Assembly, IUpgradeHandler>();
      foreach (var group in userHandlers) {
        var candidates = group.ToList();
        if (candidates.Count > 1) {
          throw new DomainBuilderException(
            string.Format(Strings.ExMoreThanOneEnabledXIsProvidedForAssemblyY, typeof (IUpgradeHandler).GetShortName(), @group.Key));
        }
        handlers.Add(group.Key, candidates[0]);
      }

      // Adding default handlers
      var assembliesWithUserHandlers = handlers.Select(pair => pair.Key);
      var assembliesWithoutUserHandler = 
        serviceAccessor.Configuration.Types.PersistentTypes
          .Select(type => type.Assembly)
          .Distinct()
          .Except(assembliesWithUserHandlers);

      foreach (var assembly in assembliesWithoutUserHandler) {
        var handler = new UpgradeHandler(assembly);
        handlers.Add(assembly, handler);
      }

      // Building a list of handlers sorted by dependencies of their assemblies
      var dependencies = handlers.Keys.ToDictionary(
        assembly => assembly,
        assembly => assembly.GetReferencedAssemblies().Select(assemblyName => assemblyName.ToString()).ToHashSet());
      var sortedHandlers = handlers
        .SortTopologically((a0, a1) => dependencies[a1.Key].Contains(a0.Key.GetName().ToString()))
        .Select(pair => pair.Value);

      // Storing the result
      serviceAccessor.UpgradeHandlers = 
        new ReadOnlyDictionary<Assembly, IUpgradeHandler>(handlers);
      serviceAccessor.OrderedUpgradeHandlers = 
        new ReadOnlyList<IUpgradeHandler>(sortedHandlers.ToList());
    }

    /// <exception cref="ArgumentOutOfRangeException"><c>context.Stage</c> is out of range.</exception>
    private void PerformUpgrade(Domain domain, UpgradeStage stage)
    {
      context.Stage = stage;

      OnBeforeStage();

      using (var session = domain.OpenSession(SessionType.System))
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var upgrader = new SchemaUpgrader(context, session);
        var extractor = new SchemaExtractor(context, session);
        SynchronizeSchema(domain, upgrader, extractor, GetUpgradeMode(stage));
        var storageNode = BuildStorageNode(domain, extractor);
        session.SetStorageNode(storageNode);
        OnStage(session);
        if (stage==UpgradeStage.Final)
          CleanUpKeyGenerators(session);
        transaction.Complete();
      }
    }

    private StorageNode BuildStorageNode(Domain domain, SchemaExtractor extractor)
    {
      var modelMapping = ModelMappingBuilder.Build(
        domain.Handlers, extractor.GetSqlSchema(),
        context.Services.MappingResolver, context.UpgradeMode.IsLegacy());
      var result = new StorageNode(context.NodeConfiguration, modelMapping, new TypeIdRegistry());

      // Register default storage node immediately,
      // non-default nodes are registered in NodeManager after everything completes successfully.
      if (result.Id==WellKnown.DefaultNodeId)
        domain.Handlers.StorageNodeRegistry.Add(result);

      context.StorageNode = result;
      return result;
    }

    private Func<Domain> CreateBuilder(UpgradeStage stage)
    {
      var configuration = new DomainBuilderConfiguration {
        DomainConfiguration = context.Configuration,
        Stage = stage,
        Services = context.Services,
        ModelFilter = new StageModelFilter(context.UpgradeHandlers, stage),
        UpgradeContextCookie = context.Cookie,
        RecycledDefinitions = context.RecycledDefinitions
      };

      configuration.Lock();
      Func<DomainBuilderConfiguration, Domain> builder = DomainBuilder.Run;
      return builder.Bind(configuration);
    }

    private HintSet GetSchemaHints(StorageModel extractedSchema, StorageModel targetSchema)
    {
      context.SchemaHints = new HintSet(extractedSchema, targetSchema);
      if (context.Stage==UpgradeStage.Upgrading)
        BuildSchemaHints(extractedSchema);
      return context.SchemaHints;
    }

    private void BuildSchemaHints(StorageModel extractedSchema)
    {
      var oldModel = context.ExtractedDomainModel;
      if (oldModel==null)
        return;
      var handlers = Domain.Demand().Handlers;
      var hintGenerator = new HintGenerator(handlers, context.Services.MappingResolver, oldModel, extractedSchema, context.Hints, context.TypesMovementsAutoDetection);
      var hints = hintGenerator.Run();
      context.UpgradedTypesMapping = hints.UpgradedTypesMapping;
      context.Hints.Clear();
      foreach (var modelHint in hints.ModelHints)
        context.Hints.Add(modelHint);
      foreach (var schemaHint in hints.SchemaHints) {
        try {
          context.SchemaHints.Add(schemaHint);
        }
        catch (Exception error) {
          UpgradeLog.Warning(Strings.LogFailedToAddSchemaHintXErrorY, schemaHint, error);
        }
      }
    }

    private void SynchronizeSchema(
      Domain domain, SchemaUpgrader upgrader, SchemaExtractor extractor, SchemaUpgradeMode schemaUpgradeMode)
    {
      using (UpgradeLog.InfoRegion(Strings.LogSynchronizingSchemaInXMode, schemaUpgradeMode)) {
        StorageModel targetSchema = null;
        if (schemaUpgradeMode==SchemaUpgradeMode.Skip) {
          if (context.StorageNode==null) {
            //If we build main domain we should log target model.
            //Log of Storage Node target model is not necessary
            //because storage target model exactly the same.
            targetSchema = GetTargetModel(domain);
            context.TargetStorageModel = targetSchema;
            if (UpgradeLog.IsLogged(LogLevel.Info)) {
              UpgradeLog.Info(Strings.LogTargetSchema);
              targetSchema.Dump();
            }
          }
          OnSchemaReady();
          return; // Skipping comparison completely
        }
        
        targetSchema = GetTargetModel(domain);
        context.TargetStorageModel = targetSchema;

        var extractedSchema = extractor.GetSchema();

        if (UpgradeLog.IsLogged(LogLevel.Info)) {
          UpgradeLog.Info(Strings.LogExtractedSchema);
          extractedSchema.Dump();
          UpgradeLog.Info(Strings.LogTargetSchema);
          targetSchema.Dump();
        }

        // Hints
        var hints = GetSchemaHints(extractedSchema, targetSchema);
        OnSchemaReady();

        var breifExceptionFormat = domain.Configuration.SchemaSyncExceptionFormat==SchemaSyncExceptionFormat.Brief;
        var result = SchemaComparer.Compare(extractedSchema, targetSchema,
          hints, context.Hints, schemaUpgradeMode, domain.Model, breifExceptionFormat);
        var shouldDumpSchema = !schemaUpgradeMode.In(
          SchemaUpgradeMode.Skip, SchemaUpgradeMode.ValidateCompatible, SchemaUpgradeMode.Recreate);
        if (shouldDumpSchema && UpgradeLog.IsLogged(LogLevel.Info))
          UpgradeLog.Info(result.ToString());

        if (UpgradeLog.IsLogged(LogLevel.Info))
          UpgradeLog.Info(Strings.LogComparisonResultX, result);

        context.SchemaDifference = (NodeDifference) result.Difference;
        context.SchemaUpgradeActions = result.UpgradeActions;

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
            upgrader.UpgradeSchema(extractor.GetSqlSchema(), extractedSchema, targetSchema, result.UpgradeActions);
            if (result.UpgradeActions.Any())
              extractor.ClearCache();
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

    private void OnConfigureUpgradeDomain()
    {
      foreach (var handler in context.OrderedUpgradeHandlers)
        handler.OnConfigureUpgradeDomain();
    }

    private void OnSchemaReady()
    {
      foreach (var handler in context.OrderedUpgradeHandlers)
        handler.OnSchemaReady();
    }

    private void OnPrepare()
    {
      foreach (var handler in context.OrderedUpgradeHandlers)
        handler.OnPrepare();
    }

    private IDisposable StartSqlWorker()
    {
      var result = CreateResult(SqlWorker.Create(context.Services, upgradeMode.GetSqlWorkerTask()));
      workerResult = result;
      return result;
    }

    private void CompleteSqlWorker()
    {
      if (!workerResult.IsAvailable)
        return;
      var result = workerResult.Get();
      context.Metadata = result.Metadata;
      context.ExtractedSqlModelCache = result.Schema;
    }

    private void OnBeforeStage()
    {
      foreach (var handler in context.OrderedUpgradeHandlers)
        handler.OnBeforeStage();
    }

    private void OnStage(Session session)
    {
      context.Session = session;
      try {
        foreach (var handler in context.OrderedUpgradeHandlers)
          handler.OnStage();
      }
      finally {
        context.Session = null;
      }
    }

    private static void CleanUpKeyGenerators(Session session)
    {
      // User might generate some entities in OnStage() method
      // Since upgrade connection was used for key generators
      // we need to clean up key generator tables here.

      var sequenceAccessor = session.Services.Demand<IStorageSequenceAccessor>();
      var keyGenerators = session.Domain.Model.Hierarchies
        .Select(h => h.Key.Sequence)
        .Where(s => s!=null)
        .Distinct();

      sequenceAccessor.CleanUp(keyGenerators, session);
    }

    private void OnComplete(Domain domain)
    {
      foreach (var handler in context.OrderedUpgradeHandlers)
        handler.OnComplete(domain);

      foreach (var module in context.Modules)
        module.OnBuilt(domain);
    }

    private StorageModel GetTargetModel(Domain domain)
    {
      var indexFilterCompiler = context.Services.IndexFilterCompiler;
      var converter = new DomainModelConverter(domain.Handlers, context.TypeIdProvider, indexFilterCompiler, context.Services.MappingResolver) {
        BuildForeignKeys = context.Configuration.Supports(ForeignKeyMode.Reference),
        BuildHierarchyForeignKeys = context.Configuration.Supports(ForeignKeyMode.Hierarchy)
      };
      return converter.Run();
    }

    private SchemaUpgradeMode GetUpgradeMode(UpgradeStage stage)
    {
      switch (stage) {
      case UpgradeStage.Upgrading:
        return upgradeMode.GetUpgradingStageUpgradeMode();
      case UpgradeStage.Final:
        return upgradeMode.GetFinalStageUpgradeMode();
      default:
        throw new ArgumentOutOfRangeException("stage");
      }
    }

    // Constructors

    private UpgradingDomainBuilder(UpgradeContext context)
    {
      this.context = context;

      upgradeMode = context.UpgradeMode;
    }
  }
}