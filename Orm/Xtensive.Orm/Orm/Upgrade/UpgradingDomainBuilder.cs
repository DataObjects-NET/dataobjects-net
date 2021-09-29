// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Kofman
// Created:    2009.04.23

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.IoC;
using Xtensive.Modelling.Comparison;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Logging;
using Xtensive.Orm.Model;
using Xtensive.Orm.Model.Stored;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade.Internals;
using Xtensive.Orm.Upgrade.Internals.Interfaces;
using Xtensive.Orm.Upgrade.Model;
using Xtensive.Reflection;
using Xtensive.Sql;
using Xtensive.Sql.Info;

namespace Xtensive.Orm.Upgrade
{
  internal sealed class UpgradingDomainBuilder
  {
    private readonly UpgradeContext context;
    private readonly DomainUpgradeMode upgradeMode;

    private DefaultSchemaInfo defaultSchemaInfo;
    private FutureResult<SqlWorkerResult> workerResult;

    public static Domain Build(DomainConfiguration configuration)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, nameof(configuration));

      if (configuration.ConnectionInfo==null) {
        throw new ArgumentException(Strings.ExConnectionInfoIsMissing, nameof(configuration));
      }

      if (!configuration.IsLocked) {
        configuration.Lock();
      }

      LogManager.Default.AutoInitialize();

      var context = new UpgradeContext(configuration);

      using (context.Activate())
      using (context.Services) {
        return new UpgradingDomainBuilder(context).Run();
      }
    }

    public static async Task<Domain> BuildAsync(DomainConfiguration configuration, CancellationToken token)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, nameof(configuration));

      if (configuration.ConnectionInfo==null) {
        throw new ArgumentException(Strings.ExConnectionInfoIsMissing, nameof(configuration));
      }

      if (!configuration.IsLocked) {
        configuration.Lock();
      }

      LogManager.Default.AutoInitialize();

      var context = new UpgradeContext(configuration);

      using (context.Activate())
      using (context.Services) {
        return await new UpgradingDomainBuilder(context).RunAsync(token).ConfigureAwait(false);
      }
    }

    public static StorageNode BuildNode(Domain parentDomain, NodeConfiguration nodeConfiguration)
    {
      ArgumentValidator.EnsureArgumentNotNull(parentDomain, nameof(parentDomain));
      ArgumentValidator.EnsureArgumentNotNull(nodeConfiguration, nameof(nodeConfiguration));

      nodeConfiguration.Validate(parentDomain.Configuration);
      if (!nodeConfiguration.IsLocked) {
        nodeConfiguration.Lock();
      }

      var context = new UpgradeContext(parentDomain, nodeConfiguration);

      using (context.Activate())
      using (context.Services) {
        new UpgradingDomainBuilder(context).Run();
        return context.StorageNode;
      }
    }

    public static async Task<StorageNode> BuildNodeAsync(
      Domain parentDomain, NodeConfiguration nodeConfiguration, CancellationToken token)
    {
      ArgumentValidator.EnsureArgumentNotNull(parentDomain, nameof(parentDomain));
      ArgumentValidator.EnsureArgumentNotNull(nodeConfiguration, nameof(nodeConfiguration));

      nodeConfiguration.Validate(parentDomain.Configuration);
      if (!nodeConfiguration.IsLocked) {
        nodeConfiguration.Lock();
      }

      var context = new UpgradeContext(parentDomain, nodeConfiguration);

      using (context.Activate())
      using (context.Services) {
        await new UpgradingDomainBuilder(context).RunAsync(token).ConfigureAwait(false);
        return context.StorageNode;
      }
    }

    private Domain Run()
    {
      BuildServices(false).GetAwaiter().GetResult();
      OnPrepare();

      var domain = upgradeMode.IsMultistage() ? BuildMultistageDomain() : BuildSingleStageDomain();

      OnComplete(domain);
      CompleteUpgradeTransaction();
      context.Services.ClearTemporaryResources();

      return domain;
    }

    private async Task<Domain> RunAsync(CancellationToken token = default)
    {
      await BuildServices(true, token).ConfigureAwait(false);
      await OnPrepareAsync(token).ConfigureAwait(false);

      var domain = upgradeMode.IsMultistage()
        ? await BuildMultistageDomainAsync(token).ConfigureAwait(false)
        : await BuildSingleStageDomainAsync(token).ConfigureAwait(false);

      await OnCompleteAsync(domain, token).ConfigureAwait(false);
      await CompleteUpgradeTransactionAsync(token).ConfigureAwait(false);
      context.Services.ClearTemporaryResources();

      return domain;
    }

    private void CompleteUpgradeTransaction()
    {
      var connection = context.Services.Connection;
      var driver = context.Services.StorageDriver;

      if (connection.ActiveTransaction == null) {
        return;
      }

      try {
        driver.CommitTransaction(null, connection);
      }
      catch {
        driver.RollbackTransaction(null, connection);
        throw;
      }
    }

    private async ValueTask CompleteUpgradeTransactionAsync(CancellationToken token)
    {
      var connection = context.Services.Connection;
      var driver = context.Services.StorageDriver;

      if (connection.ActiveTransaction == null) {
        return;
      }

      try {
        await driver.CommitTransactionAsync(null, connection, token);
      }
      catch {
        await driver.RollbackTransactionAsync(null, connection, token);
        throw;
      }
    }

    private Domain BuildMultistageDomain()
    {
      Domain finalDomain;
      using (StartSqlWorker()) {
        var domainBuilder = CreateDomainBuilder(UpgradeStage.Final);
        using (var finalDomainResult = CreateResult(domainBuilder)) {
          OnConfigureUpgradeDomain();
          using (var upgradeDomain = CreateDomainBuilder(UpgradeStage.Upgrading).Invoke()) {
            CompleteSqlWorker();
            PerformUpgrade(upgradeDomain, UpgradeStage.Upgrading);
          }
          finalDomain = finalDomainResult.Get();
        }
      }
      PerformUpgrade(finalDomain, UpgradeStage.Final);
      return finalDomain;
    }

    private async Task<Domain> BuildMultistageDomainAsync(CancellationToken token)
    {
      Domain finalDomain;
      var sqlAsyncWorker = StartSqlAsyncWorker(token);
      await using (sqlAsyncWorker.ConfigureAwait(false)) {
        var domainBuilder = CreateDomainBuilder(UpgradeStage.Final);
        var finalDomainResult = CreateResult(domainBuilder);
        await using (finalDomainResult.ConfigureAwait(false)) {
          OnConfigureUpgradeDomain();
          using (var upgradeDomain = CreateDomainBuilder(UpgradeStage.Upgrading).Invoke()) {
            await CompleteSqlWorkerAsync().ConfigureAwait(false);
            await PerformUpgradeAsync(upgradeDomain, UpgradeStage.Upgrading, token).ConfigureAwait(false);
          }
          finalDomain = await finalDomainResult.GetAsync().ConfigureAwait(false);
        }
      }
      await PerformUpgradeAsync(finalDomain, UpgradeStage.Final, token).ConfigureAwait(false);
      return finalDomain;
    }

    private Domain BuildSingleStageDomain()
    {
      using (StartSqlWorker()) {
        var domain = CreateDomainBuilder(UpgradeStage.Final).Invoke();
        CompleteSqlWorker();
        PerformUpgrade(domain, UpgradeStage.Final);
        return domain;
      }
    }

    private async Task<Domain> BuildSingleStageDomainAsync(CancellationToken token)
    {
      var sqlAsyncWorker = StartSqlAsyncWorker(token);
      await using (sqlAsyncWorker.ConfigureAwait(false)) {
        var domain = CreateDomainBuilder(UpgradeStage.Final).Invoke();
        await CompleteSqlWorkerAsync().ConfigureAwait(false);
        await PerformUpgradeAsync(domain, UpgradeStage.Final, token).ConfigureAwait(false);
        return domain;
      }
    }

    private async ValueTask BuildServices(bool isAsync, CancellationToken token = default)
    {
      var services = context.Services;
      var configuration = context.Configuration;

      services.Configuration = configuration;
      services.IndexFilterCompiler = new PartialIndexFilterCompiler();

      if (context.ParentDomain==null) {
        var descriptor = ProviderDescriptor.Get(configuration.ConnectionInfo.Provider);
        var driverFactory = (SqlDriverFactory) Activator.CreateInstance(descriptor.DriverFactory);
        var handlerFactory = (HandlerFactory) Activator.CreateInstance(descriptor.HandlerFactory);
        var driver = isAsync
          ? await StorageDriver.CreateAsync(driverFactory, configuration, token).ConfigureAwait(false)
          : StorageDriver.Create(driverFactory, configuration);
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

      await CreateConnection(services, isAsync, token).ConfigureAwait(false);
      context.DefaultSchemaInfo = defaultSchemaInfo = isAsync
        ? await services.StorageDriver.GetDefaultSchemaAsync(services.Connection, token).ConfigureAwait(false)
        : services.StorageDriver.GetDefaultSchema(services.Connection);
      services.MappingResolver = MappingResolver.Create(configuration, context.NodeConfiguration, defaultSchemaInfo);
      BuildExternalServices(services, configuration);
      services.Lock();

      context.TypeIdProvider = new TypeIdProvider(context);
    }

    private async ValueTask CreateConnection(
      UpgradeServiceAccessor serviceAccessor, bool isAsync, CancellationToken token = default)
    {
      var driver = serviceAccessor.StorageDriver;
      var connection = driver.CreateConnection(null);

      driver.ApplyNodeConfiguration(connection, context.NodeConfiguration);

      try {
        if (isAsync) {
          await driver.OpenConnectionAsync(null, connection, token).ConfigureAwait(false);
          await driver.BeginTransactionAsync(null, connection, null, token).ConfigureAwait(false);
        }
        else {
          driver.OpenConnection(null, connection);
          driver.BeginTransaction(null, connection, null);
        }
      }
      catch {
        if (isAsync) {
          await connection.DisposeAsync().ConfigureAwait(false);
        }
        else {
          connection.Dispose();
        }
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
      var ftCatalogResolvers = configuration.Types.FullTextCatalogResolvers
        .Select(type => new ServiceRegistration(typeof (IFullTextCatalogNameBuilder), type, false));

      var registrations = standardRegistrations.Concat(modules).Concat(handlers).Concat(ftCatalogResolvers);
      var serviceContainer = new ServiceContainer(registrations);
      serviceAccessor.RegisterResource(serviceContainer);

      BuildModules(serviceAccessor, serviceContainer);
      BuildUpgradeHandlers(serviceAccessor, serviceContainer);
      BuildFullTextCatalogResolver(serviceAccessor, serviceContainer);
    }

    private static void BuildModules(UpgradeServiceAccessor serviceAccessor, IServiceContainer serviceContainer)
    {
      serviceAccessor.Modules = serviceContainer.GetAll<IModule>().ToList().AsReadOnly();
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
      serviceAccessor.UpgradeHandlers = new ReadOnlyDictionary<Assembly, IUpgradeHandler>(handlers);
      serviceAccessor.OrderedUpgradeHandlers = sortedHandlers.ToList().AsReadOnly();
    }

    private static void BuildFullTextCatalogResolver(UpgradeServiceAccessor serviceAccessor, IServiceContainer serviceContainer)
    {
      //Getting user resolvers
      var candidates = from r in serviceContainer.GetAll<IFullTextCatalogNameBuilder>()
        let assembly = r.GetType().Assembly
        where r.IsEnabled && assembly!=typeof (IFullTextCatalogNameBuilder).Assembly
        select r;

      var userResolversCount = candidates.Count();
      if (userResolversCount > 1)
        throw new DomainBuilderException(string.Format(Strings.ExMoreThanOneEnabledXIsProvided, typeof (IFullTextCatalogNameBuilder).GetShortName()));

      var resolver = (userResolversCount==0)
        ? new FullTextCatalogNameBuilder()
        : candidates.First();

      //storing resolver
      serviceAccessor.FulltextCatalogNameBuilder = resolver;
    }

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
        var storageNode = BuildStorageNode(domain, extractor.GetSqlSchema());
        session.SetStorageNode(storageNode);
        OnStage(session);
        transaction.Complete();
      }
    }

    private async Task PerformUpgradeAsync(Domain domain, UpgradeStage stage, CancellationToken token)
    {
      context.Stage = stage;

      await OnBeforeStageAsync(token).ConfigureAwait(false);

      var session = await domain.OpenSessionAsync(SessionType.System, token).ConfigureAwait(false);
      await using (session.ConfigureAwait(false)) {
        using (session.Activate()) {
          var transaction = session.OpenTransaction();
          await using (transaction.ConfigureAwait(false)) {
            var upgrader = new SchemaUpgrader(context, session);
            var extractor = new SchemaExtractor(context, session);
            await SynchronizeSchemaAsync(domain, upgrader, extractor, GetUpgradeMode(stage), token).ConfigureAwait(false);
            var storageNode = BuildStorageNode(domain, await extractor.GetSqlSchemaAsync(token).ConfigureAwait(false));
            session.SetStorageNode(storageNode);
            await OnStageAsync(session, token).ConfigureAwait(false);
            transaction.Complete();
          }
        }
      }
    }

    private StorageNode BuildStorageNode(Domain domain, SchemaExtractionResult extractedSchema)
    {
      var schemaExtractionResult = GetRealExtractionResult(extractedSchema);
      context.ExtractedSqlModelCache = schemaExtractionResult;

      var modelMapping = ModelMappingBuilder.Build(
        domain.Handlers, schemaExtractionResult,
        context.Services.MappingResolver, context.NodeConfiguration, context.UpgradeMode.IsLegacy());
      var result = new StorageNode(domain, context.NodeConfiguration, modelMapping, new TypeIdRegistry());

      // Register default storage node immediately,
      // non-default nodes are registered in NodeManager after everything completes successfully.
      if (result.Id==WellKnown.DefaultNodeId) {
        _ = domain.Handlers.StorageNodeRegistry.Add(result);
      }

      context.StorageNode = result;
      return result;
    }

    private SchemaExtractionResult GetRealExtractionResult(SchemaExtractionResult baseSchemaExtractionResult)
    {
      if (!context.Configuration.ShareStorageSchemaOverNodes)
        return baseSchemaExtractionResult;
      // Skip mode is a special case. Real extraction result is generated by
      // a builder in the SynchronizeSchema() method.
      if (context.NodeConfiguration.UpgradeMode == DomainUpgradeMode.Skip)
        return baseSchemaExtractionResult;

      //for default node
      if (context.ParentDomain == null) {
        if (context.Stage==UpgradeStage.Final)
          return baseSchemaExtractionResult.MakeShared();
        return baseSchemaExtractionResult;
      }
      //for additional nodes
      if (context.Stage==UpgradeStage.Final) {
        var schemaExtractionResult = new SchemaExtractionResult();
        var defaultNode = context.ParentDomain.StorageNodeManager.GetNode(WellKnown.DefaultNodeId);
        defaultNode.Mapping.GetAllSchemaNodes()
          .Select(node => node.Schema.Catalog)
          .Distinct()
          .ForEach(schemaExtractionResult.Catalogs.Add);
        return schemaExtractionResult.MakeShared();
      }
      return baseSchemaExtractionResult;
    }

    private Func<Domain> CreateDomainBuilder(UpgradeStage stage)
    {
      if (stage == UpgradeStage.Final && context.ParentDomain != null) {
        return () => context.ParentDomain;
      }

      var configuration = CreateDomainBuilderConfiguration(stage);
      return ((Func<DomainBuilderConfiguration, Domain>) DomainBuilder.Run).Bind(configuration);
    }

    private DomainBuilderConfiguration CreateDomainBuilderConfiguration(UpgradeStage stage)
    {
      var configuration = new DomainBuilderConfiguration {
        DomainConfiguration = context.Configuration,
        Stage = stage,
        Services = context.Services,
        ModelFilter = new StageModelFilter(context.UpgradeHandlers, stage),
        UpgradeContextCookie = context.Cookie,
        RecycledDefinitions = context.RecycledDefinitions,
        DefaultSchemaInfo = defaultSchemaInfo
      };

      configuration.Lock();
      return configuration;
    }

    private void BuildSchemaHints(StorageModel extractedSchema, UpgradeHintsProcessingResult result, StoredDomainModel currentDomainModel)
    {
      var oldModel = context.ExtractedDomainModel;
      if (oldModel==null)
        return;
      var handlers = Domain.Demand().Handlers;
      // It's important to use same StoredDomainModel of current domain
      // in both UpgradeHintsProcessor and HintGenerator instances.
      var hintGenerator = new HintGenerator(result, handlers, context.Services.MappingResolver, extractedSchema, currentDomainModel, oldModel);
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
          if (context.ParentDomain==null) {
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
          var builder = ExtractedModelBuilderFactory.GetBuilder(context);
          context.ExtractedSqlModelCache = builder.Run();
          OnSchemaReady();
          return; // Skipping comparison completely
        }

        var extractedSchema = extractor.GetSchema();

        // Hints
        var triplet = BuildTargetModelAndHints(extractedSchema);
        var hintProcessingResult = triplet.Item3;
        targetSchema = triplet.Item1;
        context.TargetStorageModel = targetSchema;
        var hints = triplet.Item2;
        if (UpgradeLog.IsLogged(LogLevel.Info))
        {
          UpgradeLog.Info(Strings.LogExtractedSchema);
          extractedSchema.Dump();
          UpgradeLog.Info(Strings.LogTargetSchema);
          targetSchema.Dump();
        }
        OnSchemaReady();

        var breifExceptionFormat = domain.Configuration.SchemaSyncExceptionFormat==SchemaSyncExceptionFormat.Brief;
        var result = SchemaComparer.Compare(extractedSchema, targetSchema,
          hints, context.Hints, schemaUpgradeMode, domain.Model, breifExceptionFormat, context.Stage);
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
            if (!hintProcessingResult.AreAllTypesMapped() && hintProcessingResult.SuspiciousTypes.Any())
              throw new SchemaSynchronizationException(Strings.ExExtractedAndTargetSchemasAreEqualButThereAreChangesInTypeIdentifiersSet);
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

    private async Task SynchronizeSchemaAsync(
      Domain domain, SchemaUpgrader upgrader, SchemaExtractor extractor, SchemaUpgradeMode schemaUpgradeMode, CancellationToken token)
    {
      using (UpgradeLog.InfoRegion(Strings.LogSynchronizingSchemaInXMode, schemaUpgradeMode)) {
        StorageModel targetSchema = null;
        if (schemaUpgradeMode==SchemaUpgradeMode.Skip) {
          if (context.ParentDomain==null) {
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
          var builder = ExtractedModelBuilderFactory.GetBuilder(context);
          context.ExtractedSqlModelCache = builder.Run();
          await OnSchemaReadyAsync(token).ConfigureAwait(false);
          return; // Skipping comparison completely
        }

        var extractedSchema = await extractor.GetSchemaAsync(token).ConfigureAwait(false);

        // Hints
        var triplet = BuildTargetModelAndHints(extractedSchema);
        var hintProcessingResult = triplet.Item3;
        targetSchema = triplet.Item1;
        context.TargetStorageModel = targetSchema;
        var hints = triplet.Item2;
        if (UpgradeLog.IsLogged(LogLevel.Info))
        {
          UpgradeLog.Info(Strings.LogExtractedSchema);
          extractedSchema.Dump();
          UpgradeLog.Info(Strings.LogTargetSchema);
          targetSchema.Dump();
        }
        await OnSchemaReadyAsync(token).ConfigureAwait(false);

        var briefExceptionFormat = domain.Configuration.SchemaSyncExceptionFormat==SchemaSyncExceptionFormat.Brief;
        var result = SchemaComparer.Compare(extractedSchema, targetSchema,
          hints, context.Hints, schemaUpgradeMode, domain.Model, briefExceptionFormat, context.Stage);
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
            if (!hintProcessingResult.AreAllTypesMapped() && hintProcessingResult.SuspiciousTypes.Any())
              throw new SchemaSynchronizationException(Strings.ExExtractedAndTargetSchemasAreEqualButThereAreChangesInTypeIdentifiersSet);
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
            var extractedSqlSchema = await extractor.GetSqlSchemaAsync(token).ConfigureAwait(false);
            await upgrader.UpgradeSchemaAsync(
              extractedSqlSchema, extractedSchema, targetSchema, result.UpgradeActions, token).ConfigureAwait(false);
            if (result.UpgradeActions.Any())
              extractor.ClearCache();
            break;
          case SchemaUpgradeMode.ValidateLegacy:
            if (result.IsCompatibleInLegacyMode!=true)
              throw new SchemaSynchronizationException(result);
            break;
          default:
            throw new ArgumentOutOfRangeException(nameof(schemaUpgradeMode));
        }
      }
    }

    private (StorageModel, HintSet, UpgradeHintsProcessingResult) BuildTargetModelAndHints(StorageModel extractedSchema)
    {
      var handlers = Domain.Demand().Handlers;
      var currentDomainModel = GetStoredDomainModel(handlers.Domain.Model);
      var hintProcessor = GetHintProcessor(currentDomainModel, extractedSchema, handlers);
      var processedInfo = hintProcessor.Process(context.Hints);
      var targetModel = GetTargetModel(handlers.Domain, processedInfo.ReverseFieldMapping, processedInfo.CurrentModelTypes, extractedSchema);
      context.SchemaHints = new HintSet(extractedSchema, targetModel);
      if (context.Stage == UpgradeStage.Upgrading) {
        BuildSchemaHints(extractedSchema, processedInfo, currentDomainModel);
      }
      return (targetModel, context.SchemaHints, processedInfo);
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

    private async ValueTask OnSchemaReadyAsync(CancellationToken token)
    {
      foreach (var handler in context.OrderedUpgradeHandlers) {
        await handler.OnSchemaReadyAsync(token).ConfigureAwait(false);
      }
    }

    private void OnPrepare()
    {
      foreach (var handler in context.OrderedUpgradeHandlers) {
        handler.OnPrepare();
      }
    }

    private async ValueTask OnPrepareAsync(CancellationToken token)
    {
      foreach (var handler in context.OrderedUpgradeHandlers) {
        await handler.OnPrepareAsync(token).ConfigureAwait(false);
      }
    }

    private FutureResult<T> CreateResult<T>(Func<T> action) =>
      context.Configuration.BuildInParallel
        ? (FutureResult<T>) new AsyncFutureResult<T>(action, UpgradeLog.Instance)
        : new SynchronousFutureResult<T>(action);

    private FutureResult<SqlWorkerResult> StartSqlWorker()
    {
      var result = CreateResult(SqlWorker.Create(context.Services, upgradeMode.GetSqlWorkerTask()));
      workerResult = result;
      return result;
    }

    private FutureResult<SqlWorkerResult> StartSqlAsyncWorker(CancellationToken token)
    {
      var worker = SqlAsyncWorker.Create(context.Services, upgradeMode.GetSqlWorkerTask(), token);
      return workerResult =
        new AsyncFutureResult<SqlWorkerResult>(worker, UpgradeLog.Instance, context.Configuration.BuildInParallel);
    }

    private void CompleteSqlWorker()
    {
      if (!workerResult.IsAvailable) {
        return;
      }

      var result = workerResult.Get();
      context.Metadata = result.Metadata;
      if (result.Schema!=null) {
        context.ExtractedSqlModelCache = result.Schema;
      }
    }

    private async Task CompleteSqlWorkerAsync()
    {
      if (!workerResult.IsAvailable) {
        return;
      }

      var result = await workerResult.GetAsync().ConfigureAwait(false);
      context.Metadata = result.Metadata;
      if (result.Schema!=null) {
        context.ExtractedSqlModelCache = result.Schema;
      }
    }

    private void OnBeforeStage()
    {
      foreach (var handler in context.OrderedUpgradeHandlers) {
        handler.OnBeforeStage();
      }
    }

    private async ValueTask OnBeforeStageAsync(CancellationToken token)
    {
      foreach (var handler in context.OrderedUpgradeHandlers) {
        await handler.OnBeforeStageAsync(token).ConfigureAwait(false);
      }
    }

    private void OnStage(Session session)
    {
      context.Session = session;
      try {
        foreach (var handler in context.OrderedUpgradeHandlers) {
          handler.OnStage();
        }
      }
      finally {
        context.Session = null;
      }
    }

    private async ValueTask OnStageAsync(Session session, CancellationToken token)
    {
      context.Session = session;
      try {
        foreach (var handler in context.OrderedUpgradeHandlers) {
          await handler.OnStageAsync(token).ConfigureAwait(false);
        }
      }
      finally {
        context.Session = null;
      }
    }

    private void OnComplete(Domain domain)
    {
      foreach (var handler in context.OrderedUpgradeHandlers) {
        handler.OnComplete(domain);
      }

      foreach (var module in context.Modules) {
        module.OnBuilt(domain);
      }
    }

    private async ValueTask OnCompleteAsync(Domain domain, CancellationToken token)
    {
      foreach (var handler in context.OrderedUpgradeHandlers) {
        await handler.OnCompleteAsync(domain, token).ConfigureAwait(false);
      }

      foreach (var module in context.Modules) {
        module.OnBuilt(domain);
      }
    }

    private StorageModel GetTargetModel(Domain domain)
    {
      return GetTargetModel(domain, new Dictionary<StoredFieldInfo, StoredFieldInfo>(), new Dictionary<string, StoredTypeInfo>(), null);
    }

    private StorageModel GetTargetModel(Domain domain, Dictionary<StoredFieldInfo, StoredFieldInfo> fieldMapping, Dictionary<string, StoredTypeInfo> currentTypes, StorageModel extractedModel)
    {
      var indexFilterCompiler = context.Services.IndexFilterCompiler;
      var fullTextCatalogResolver = context.Services.FulltextCatalogNameBuilder;
      var mappingResolver = context.Services.MappingResolver;
      var converter = new DomainModelConverter(domain.Handlers, context.TypeIdProvider, indexFilterCompiler, mappingResolver, fullTextCatalogResolver, context.Stage==UpgradeStage.Upgrading) {
        BuildForeignKeys = context.Configuration.Supports(ForeignKeyMode.Reference),
        BuildHierarchyForeignKeys = context.Configuration.Supports(ForeignKeyMode.Hierarchy),
        FieldMapping = fieldMapping,
        CurrentModelTypes = currentTypes,
        StorageModel = extractedModel
      };
      return converter.Run();
    }

    private IUpgradeHintsProcessor GetHintProcessor(StoredDomainModel currentDomainModel, StorageModel extractedSchema, HandlerAccessor handlers)
    {
      var oldModel = context.ExtractedDomainModel;
      var stage = context.Stage;

      if (stage==UpgradeStage.Upgrading && oldModel!=null)
        return new UpgradeHintsProcessor(handlers, context.Services.MappingResolver, currentDomainModel, oldModel, extractedSchema, context.TypesMovementsAutoDetection);
      if (context.UpgradeMode==DomainUpgradeMode.Validate && oldModel!=null)
        return new UpgradeHintsProcessor(handlers, context.Services.MappingResolver, currentDomainModel, oldModel, extractedSchema, false);
      return new NullUpgradeHintsProcessor(currentDomainModel);
    }

    private SchemaUpgradeMode GetUpgradeMode(UpgradeStage stage) =>
      stage switch {
        UpgradeStage.Upgrading => upgradeMode.GetUpgradingStageUpgradeMode(),
        UpgradeStage.Final => upgradeMode.GetFinalStageUpgradeMode(),
        _ => throw new ArgumentOutOfRangeException(nameof(stage))
      };

    private StoredDomainModel GetStoredDomainModel(DomainModel domainModel)
    {
      var storedDomainModel = domainModel.ToStoredModel();
      storedDomainModel.UpdateReferences();
      // Since we support storage nodes, stored domain model and real model of a node
      // must be synchronized. So we must update types' mappings
      storedDomainModel.UpdateMappings(context.NodeConfiguration);
      return storedDomainModel;
    }

    // Constructors

    private UpgradingDomainBuilder(UpgradeContext context)
    {
      this.context = context;

      upgradeMode = context.UpgradeMode;
    }
  }
}