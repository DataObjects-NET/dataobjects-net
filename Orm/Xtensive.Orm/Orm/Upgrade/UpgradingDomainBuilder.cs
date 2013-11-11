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
using Xtensive.Orm.Logging;
using Xtensive.IoC;
using Xtensive.Modelling.Comparison;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade.Model;
using Xtensive.Reflection;
using Xtensive.Sql;
using ModelTypeInfo = Xtensive.Orm.Model.TypeInfo;

namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// Builds domain in extended modes.
  /// </summary>
  public sealed class UpgradingDomainBuilder
  {
    private readonly UpgradeContext context;
    private readonly DomainUpgradeMode upgradeMode;

    private FutureResult<SqlWorkerResult> workerResult;

    /// <summary>
    /// Builds the new <see cref="Domain"/> by the specified configuration.
    /// </summary>
    /// <param name="configuration">The domain configuration.</param>
    /// <returns>Newly created <see cref="Domain"/>.</returns>
    /// <exception cref="ArgumentNullException">Parameter <paramref name="configuration"/> is null.</exception>
    /// <exception cref="DomainBuilderException">At least one error have been occurred 
    /// during storage building process.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><c>configuration.UpgradeMode</c> is out of range.</exception>
    public static Domain Build(DomainConfiguration configuration)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, "configuration");

      if (configuration.ConnectionInfo==null)
        throw new ArgumentNullException("configuration.ConnectionInfo", Strings.ExConnectionInfoIsMissing);

      if (!configuration.IsLocked)
        configuration.Lock();

      var context = new UpgradeContext(configuration);

      using (context.Activate()) {
        try {
          return new UpgradingDomainBuilder(context).Run();
        }
        catch {
          // If we running in shared connection mode
          // connection would not be registered as an upgrade session.
          // This means if error happens connection will leak.
          // To avoid that we dispose it here manually.
          if (context.Services!=null) {
            var connection = context.Services.Connection;
            var driver = context.Services.Driver;
            if (driver!=null && connection!=null && driver.ProviderInfo.Supports(ProviderFeatures.SingleConnection))
              connection.Dispose();
          }
          throw;
        }
        finally {
          context.Services.DisposeSafely();
        }
      }
    }

    private Domain Run()
    {
      Domain domain;

      using (workerResult = OnPrepare()) {
        domain = upgradeMode.IsMultistage() ? BuildMultistageDomain() : BuildSingleStageDomain();
      }

      OnComplete(domain);

      return domain;
    }

    private void CompleteUpgradeTransaction()
    {
      var connection = context.Services.Connection;
      var driver = context.Services.Driver;

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

    private FutureResult<T> CreateResult<T>(Func<T> action)
    {
      return new FutureResult<T>(action, context.Configuration.BuildInParallel);
    }

    private Domain BuildMultistageDomain()
    {
      Domain finalDomain;
      using (var finalDomainResult = CreateResult(CreateBuilder(UpgradeStage.Final))) {
        using (var upgradeDomain = CreateBuilder(UpgradeStage.Upgrading).Invoke()) {
          PerformUpgrade(upgradeDomain, UpgradeStage.Upgrading);
        }
        finalDomain = finalDomainResult.Get();
      }
      PerformUpgrade(finalDomain, UpgradeStage.Final);
      return finalDomain;
    }

    private Domain BuildSingleStageDomain()
    {
      var domain = CreateBuilder(UpgradeStage.Final).Invoke();
      PerformUpgrade(domain, UpgradeStage.Final);
      return domain;
    }

    private void BuildServices()
    {
      var configuration = context.Configuration;
      var descriptor = ProviderDescriptor.Get(configuration.ConnectionInfo.Provider);
      var driverFactory = (SqlDriverFactory) Activator.CreateInstance(descriptor.DriverFactory);
      var handlerFactory = (HandlerFactory) Activator.CreateInstance(descriptor.HandlerFactory);
      var driver = StorageDriver.Create(driverFactory, configuration);

      var serviceAccessor = context.Services = new UpgradeServiceAccessor {
        Configuration = configuration,
        HandlerFactory = handlerFactory,
        Driver = driver,
        NameBuilder = new NameBuilder(configuration, driver.ProviderInfo),
        IndexFilterCompiler = new PartialIndexFilterCompiler(),
        Resolver = MappingResolver.Get(configuration, driver.ProviderInfo),
      };

      BuildExternalServices(serviceAccessor, configuration);
      CreateConnection(serviceAccessor);

      serviceAccessor.Lock();

      context.TypeIdProvider = new TypeIdProvider(context);
    }

    private void CreateConnection(UpgradeServiceAccessor serviceAccessor)
    {
      var driver = serviceAccessor.Driver;
      var connection = driver.CreateConnection(null);

      try {
        driver.OpenConnection(null, connection);
        driver.BeginTransaction(null, connection, null);
      }
      catch {
        connection.Dispose();
        throw;
      }

      if (!driver.ProviderInfo.Supports(ProviderFeatures.SingleConnection))
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
        domain.Handler.BuildMapping(extractor.GetSqlSchema());
        OnStage(session);
        transaction.Complete();
      }
    }


    private Func<Domain> CreateBuilder(UpgradeStage stage)
    {
      if (stage==UpgradeStage.Upgrading)
        foreach (var handler in context.OrderedUpgradeHandlers)
          handler.OnConfigureUpgradeDomain();

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
      var hintGenerator = new HintGenerator(handlers, oldModel, extractedSchema, context.Hints);
      var hints = hintGenerator.Run();
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
        var extractedSchema = extractor.GetSchema();
        var targetSchema = domain.StorageModel = GetTargetModel(domain);

        if (UpgradeLog.IsLogged(LogLevel.Info)) {
          UpgradeLog.Info(Strings.LogExtractedSchema);
          extractedSchema.Dump();
          UpgradeLog.Info(Strings.LogTargetSchema);
          targetSchema.Dump();
        }

        // Hints
        var hints = GetSchemaHints(extractedSchema, targetSchema);
        OnSchemaReady();

        if (schemaUpgradeMode==SchemaUpgradeMode.Skip)
          return; // Skipping comparison completely

        var breifExceptionFormat = domain.Configuration.SchemaSyncExceptionFormat==SchemaSyncExceptionFormat.Brief;
        var result = SchemaComparer.Compare(extractedSchema, targetSchema,
          hints, context.Hints, schemaUpgradeMode, domain.Model, breifExceptionFormat);
        var shouldDumpSchema = !schemaUpgradeMode.In(
          SchemaUpgradeMode.Skip, SchemaUpgradeMode.ValidateCompatible, SchemaUpgradeMode.Recreate);
        if (shouldDumpSchema)
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

    private void OnSchemaReady()
    {
      foreach (var handler in context.OrderedUpgradeHandlers)
        handler.OnSchemaReady();
    }

    private FutureResult<SqlWorkerResult> OnPrepare()
    {
      BuildServices();

      foreach (var handler in context.OrderedUpgradeHandlers)
        handler.OnPrepare();

      return CreateResult(SqlWorker.Create(context.Services, upgradeMode.GetSqlWorkerTask()));
    }

    private void OnBeforeStage()
    {
      if (workerResult.IsAvailable) {
        var result = workerResult.Get();
        context.Metadata = result.Metadata;
        context.ExtractedSqlModelCache = result.Schema;
      }

      foreach (var handler in context.OrderedUpgradeHandlers)
        handler.OnBeforeStage();
    }

    private void OnStage(Session session)
    {
      context.Session = session;
      try {
        foreach (var handler in context.OrderedUpgradeHandlers)
          handler.OnStage();
        if (context.Stage==UpgradeStage.Final)
          CleanUpKeyGenerators(session);
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

      CompleteUpgradeTransaction();
    }

    private StorageModel GetTargetModel(Domain domain)
    {
      var indexFilterCompiler = context.Services.IndexFilterCompiler;
      var converter = new DomainModelConverter(domain.Handlers, context.TypeIdProvider, indexFilterCompiler) {
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

      upgradeMode = context.Configuration.UpgradeMode;
    }
  }
}