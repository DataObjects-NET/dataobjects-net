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
using Xtensive.Diagnostics;
using Xtensive.IoC;
using Xtensive.Modelling.Comparison;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Upgrade.Model;
using Xtensive.Reflection;
using Xtensive.Sorting;
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
        return new UpgradingDomainBuilder(context).Run();
      }
    }

    private Domain Run()
    {
      BuildServices();

      workerResult = CreateResult(SqlWorker.Create(context.Services, upgradeMode.GetSqlWorkerTask()));

      using (workerResult) {
        var domain = upgradeMode.IsMultistage()
          ? BuildMultistageDomain()
          : BuildSingleStageDomain();
        foreach (var module in context.Modules)
          module.OnBuilt(domain);
        return domain;
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

      var serviceAccessor = new UpgradeServiceAccessor {
        Configuration = configuration,
        HandlerFactory = handlerFactory,
        Driver = driver,
        NameBuilder = new NameBuilder(configuration, driver.ProviderInfo),
        Normalizer = handlerFactory.CreateHandler<PartialIndexFilterNormalizer>(),
        Resolver = MappingResolver.Get(configuration, driver.ProviderInfo),
      };

      var standardRegistrations = new[] {
        new ServiceRegistration(typeof (DomainConfiguration), configuration),
        new ServiceRegistration(typeof (UpgradeContext), context)
      };

      var modules = configuration.Types.Modules
        .Select(type => new ServiceRegistration(typeof (IModule), type, false));

      var handlers = configuration.Types.UpgradeHandlers
        .Select(type => new ServiceRegistration(typeof (IUpgradeHandler), type, false));

      var registrations = standardRegistrations
        .Concat(modules)
        .Concat(handlers);

      using (var serviceContainer = new ServiceContainer(registrations)) {
        serviceAccessor.Modules = new ReadOnlyList<IModule>(serviceContainer.GetAll<IModule>().ToList());
        BuildUpgradeHandlers(serviceAccessor, serviceContainer);
      }

      serviceAccessor.Lock();

      context.Services = serviceAccessor;
      context.TypeIdProvider = new TypeIdProvider(context);
    }

    /// <exception cref="DomainBuilderException">More then one enabled handler is provided for some assembly.</exception>
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
        var handler = group.SingleOrDefault();
        if (handler==null)
          throw new DomainBuilderException(
            Strings.ExMoreThanOneEnabledXIsProvidedForAssemblyY.FormatWith(
              typeof (IUpgradeHandler).GetShortName(), group.Key));
        handlers.Add(group.Key, handler);
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
      var sortedHandlers =
        from pair in 
          TopologicalSorter.Sort(handlers, 
            (a0, a1) => dependencies[a1.Key].Contains(a0.Key.GetName().ToString()))
        select pair.Value;

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
      using (var upgrader = new SchemaUpgrader(context, session)) {
        var extractor = new SchemaExtractor(context, session);
        SynchronizeSchema(domain, upgrader, extractor, GetUpgradeMode(stage));
        domain.Handler.BuildMapping(extractor.GetSqlSchema());
        OnStage();
        upgrader.Complete();
      }
    }

    private Func<Domain> CreateBuilder(UpgradeStage stage)
    {
      var configuration = new DomainBuilderConfiguration {
        DomainConfiguration = context.Configuration,
        Stage = stage,
        Services = context.Services,
        ModelFilter = new StageModelFilter(context.UpgradeHandlers, stage)
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
          Log.Warning(Strings.LogFailedToAddSchemaHintXErrorY, schemaHint, error);
        }
      }
    }

    private void SynchronizeSchema(
      Domain domain, SchemaUpgrader upgrader, SchemaExtractor extractor, SchemaUpgradeMode schemaUpgradeMode)
    {
      using (Log.InfoRegion(Strings.LogSynchronizingSchemaInXMode, schemaUpgradeMode)) {
        var extractedSchema = extractor.GetSchema();
        var targetSchema = domain.StorageModel = GetTargetModel(domain);

        if (Log.IsLogged(LogEventTypes.Info)) {
          Log.Info(Strings.LogExtractedSchema);
          extractedSchema.Dump();
          Log.Info(Strings.LogTargetSchema);
          targetSchema.Dump();
        }

        // Hints
        var hints = GetSchemaHints(extractedSchema, targetSchema);
        foreach (var handler in context.OrderedUpgradeHandlers)
          handler.OnSchemaReady();

        if (schemaUpgradeMode==SchemaUpgradeMode.Skip)
          return; // Skipping comparison completely

        var result = SchemaComparer.Compare(extractedSchema, targetSchema,
          hints, context.Hints, schemaUpgradeMode, domain.Model);
        var shouldDumpSchema = !schemaUpgradeMode.In(
          SchemaUpgradeMode.Skip, SchemaUpgradeMode.ValidateCompatible, SchemaUpgradeMode.Recreate);
        if (shouldDumpSchema)
          Log.Info(result.ToString());

        if (Log.IsLogged(LogEventTypes.Info))
          Log.Info(Strings.LogComparisonResultX, result);

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

    private void OnStage()
    {
      foreach (var handler in context.OrderedUpgradeHandlers)
        handler.OnStage();
    }

    private StorageModel GetTargetModel(Domain domain)
    {
      var normalizer = context.Services.Normalizer;
      var converter = new DomainModelConverter(domain.Handlers, context.TypeIdProvider, normalizer) {
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