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
using Xtensive.Orm.Providers;
using Xtensive.Reflection;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Orm.Building;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Upgrade.Model;
using Xtensive.Sorting;
using Xtensive.Sql;
using ModelTypeInfo = Xtensive.Orm.Model.TypeInfo;

namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// Builds domain in extended modes.
  /// </summary>
  public static class UpgradingDomainBuilder
  {
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
        BuildEssentialHandlers(context);
        BuildServices(context);
        BuildModules(context);
        BuildUpgradeHandlers(context);

        // 1st Domain
        try {
          BuildStageDomain(context, UpgradeStage.Initializing).DisposeSafely();
        }
        catch (Exception e) {
          if (GetInnermostException(e) is SchemaSynchronizationException) {
            if (context.SchemaUpgradeActions.OfType<RemoveNodeAction>().Any())
              throw; // There must be no any removes to proceed further 
                     // (i.e. schema should be clean)
          }
          else
            throw;
        }

        // 2nd Domain
        BuildStageDomain(context, UpgradeStage.Upgrading).DisposeSafely();

        // 3rd Domain
        var domain = BuildStageDomain(context, UpgradeStage.Final);
        foreach (var module in context.Modules)
          module.OnBuilt(domain);

        return domain;
      }
    }

    private static void BuildEssentialHandlers(UpgradeContext context)
    {
      var configuration = context.OriginalConfiguration;

      var descriptor = ProviderDescriptor.Get(configuration.ConnectionInfo.Provider);

      var driverFactory = (SqlDriverFactory) Activator.CreateInstance(descriptor.DriverFactory);
      var handlerFactory = (HandlerFactory) Activator.CreateInstance(descriptor.HandlerFactory);

      context.HandlerFactory = handlerFactory;
      context.TemplateDriver = StorageDriver.Create(driverFactory, configuration);
      context.NameBuilder = new NameBuilder(configuration, context.TemplateDriver.ProviderInfo);
      context.SchemaResolver = SchemaResolver.Get(configuration);
    }

    private static void BuildServices(UpgradeContext context)
    {
      var configuration = context.OriginalConfiguration;

      var handlers = configuration.Types.UpgradeHandlers
        .Select(type => new ServiceRegistration(typeof (IUpgradeHandler), type, false));

      var modules = configuration.Types.Modules
        .Select(type => new ServiceRegistration(typeof (IModule), type, false));

      var allRegistrations = handlers.Concat(modules);

      var baseServices = new ServiceContainer(new List<ServiceRegistration> {
        new ServiceRegistration(typeof (DomainConfiguration), configuration),
        new ServiceRegistration(typeof (UpgradeContext), context),
      });

      var serviceContainerType = configuration.ServiceContainerType ?? typeof (ServiceContainer);
      context.Services = ServiceContainer.Create(typeof (ServiceContainer), allRegistrations,
        ServiceContainer.Create(serviceContainerType, baseServices));
    }

    /// <exception cref="DomainBuilderException">More then one enabled handler is provided for some assembly.</exception>
    private static void BuildUpgradeHandlers(UpgradeContext context)
    {
      // Getting user handlers
      var userHandlers =
        from handler in context.Services.GetAll<IUpgradeHandler>()
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
        context.OriginalConfiguration.Types.PersistentTypes
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
      context.UpgradeHandlers = 
        new ReadOnlyDictionary<Assembly, IUpgradeHandler>(handlers);
      context.OrderedUpgradeHandlers = 
        new ReadOnlyList<IUpgradeHandler>(sortedHandlers.ToList());
    }

    private static void BuildModules(UpgradeContext context)
    {
      context.Modules = new ReadOnlyList<IModule>(context.Services.GetAll<IModule>().ToList());
    }

    /// <exception cref="ArgumentOutOfRangeException"><c>context.Stage</c> is out of range.</exception>
    private static Domain BuildStageDomain(UpgradeContext context, UpgradeStage stage)
    {
      var configuration = context.Configuration = context.OriginalConfiguration.Clone();
      context.Stage = stage;
      // Raising "Before upgrade" event
      foreach (var handler in context.OrderedUpgradeHandlers)
        handler.OnBeforeStage();

      var schemaUpgradeMode = GetUpgradeMode(stage, configuration.UpgradeMode);
      if (schemaUpgradeMode==null)
        return null;

      var builderConfiguration = CreateBuilderConfiguration(context, schemaUpgradeMode.Value);
      return DomainBuilder.BuildDomain(configuration, builderConfiguration);
    }

    private static DomainBuilderConfiguration CreateBuilderConfiguration(UpgradeContext context, SchemaUpgradeMode schemaUpgradeMode)
    {
      return new DomainBuilderConfiguration(schemaUpgradeMode, context) {
        TypeFilter = type => {
          var assembly = type.Assembly;
          var handlers = context.UpgradeHandlers;
          return handlers.ContainsKey(assembly)
            && DomainTypeRegistry.IsPersistentType(type)
            && handlers[assembly].IsTypeAvailable(type, context.Stage);
        },
        FieldFilter = field => {
          var assembly = field.DeclaringType.Assembly;
          var handlers = context.UpgradeHandlers;
          return handlers.ContainsKey(assembly)
            && handlers[assembly].IsFieldAvailable(field, context.Stage);
        },
        SchemaReadyHandler = (extractedSchema, targetSchema) => {
          context.SchemaHints = new HintSet(extractedSchema, targetSchema);
          if (context.Stage==UpgradeStage.Upgrading)
            BuildSchemaHints(extractedSchema, targetSchema);
          return context.SchemaHints;
        },
        UpgradeActionsReadyHandler = (schemaDifference, schemaUpgradeActions) => {
          context.SchemaDifference = schemaDifference;
          context.SchemaUpgradeActions = schemaUpgradeActions;
        },
        UpgradeHandler = () => {
          foreach (var handler in context.OrderedUpgradeHandlers)
            handler.OnStage();
        },
        TypeIdProvider = (type => ProvideTypeId(context, type)),
      };
    }

    private static Exception GetInnermostException(Exception exception)
    {
      ArgumentValidator.EnsureArgumentNotNull(exception, "exception");
      var current = exception;
      while (current.InnerException!=null)
        current = current.InnerException;
      return current;
    }

    private static void BuildSchemaHints(StorageModel extractedSchema, StorageModel targetSchema)
    {
      var context = UpgradeContext.Demand();
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

    private static int ProvideTypeId(UpgradeContext context, Type type)
    {
      var typeId = ModelTypeInfo.NoTypeId;
      var oldModel = context.ExtractedDomainModel;
      if (oldModel==null && context.ExtractedTypeMap==null)
        return typeId;

      // type has been renamed?
      var fullName = type.GetFullName();
      var renamer = context.Hints.OfType<RenameTypeHint>()
        .SingleOrDefault(hint => hint.NewType.GetFullName()==fullName);
      if (renamer!=null) {
        if (context.ExtractedTypeMap.TryGetValue(renamer.OldType, out typeId))
          return typeId;
        if (oldModel!=null)
          return oldModel.Types.Single(t => t.UnderlyingType==renamer.OldType).TypeId;
      }
      // type has been preserved
      if (context.ExtractedTypeMap.TryGetValue(fullName, out typeId))
        return typeId;
      if (oldModel!=null) {
        var oldType = oldModel.Types
          .SingleOrDefault(t => t.UnderlyingType==fullName);
        if (oldType!=null)
          return oldType.TypeId;
      }
      return ModelTypeInfo.NoTypeId;
    }

    private static SchemaUpgradeMode GetUpgradingStageUpgradeMode(DomainUpgradeMode upgradeMode)
    {
      switch (upgradeMode) {
        case DomainUpgradeMode.PerformSafely:
          return SchemaUpgradeMode.PerformSafely;
        case DomainUpgradeMode.Perform:
          return SchemaUpgradeMode.Perform;
        default:
          throw new ArgumentOutOfRangeException("upgradeMode");
      }
    }

    private static SchemaUpgradeMode GetFinalStageUpgradeMode(DomainUpgradeMode upgradeMode)
    {
      switch (upgradeMode) {
        case DomainUpgradeMode.Skip:
        case DomainUpgradeMode.LegacySkip:
          return SchemaUpgradeMode.Skip;
        case DomainUpgradeMode.Validate:
          return SchemaUpgradeMode.ValidateExact;
        case DomainUpgradeMode.LegacyValidate:
          return SchemaUpgradeMode.ValidateLegacy;
        case DomainUpgradeMode.Recreate:
          return SchemaUpgradeMode.Recreate;
        case DomainUpgradeMode.Perform:
        case DomainUpgradeMode.PerformSafely:
          // We need Perform here because after Upgrading stage
          // there may be some recycled columns/tables.
          // Perform will wipe them out.
          return SchemaUpgradeMode.Perform;
        default:
          throw new ArgumentOutOfRangeException("upgradeMode");
      }
    }

    private static SchemaUpgradeMode? GetUpgradeMode(UpgradeStage stage, DomainUpgradeMode upgradeMode)
    {
      switch (stage) {
        case UpgradeStage.Initializing:
          return upgradeMode.RequiresInitializingStage()
            ? SchemaUpgradeMode.ValidateCompatible
            : (SchemaUpgradeMode?) null;
        case UpgradeStage.Upgrading:
          return upgradeMode.RequiresUpgradingStage() 
            ? GetUpgradingStageUpgradeMode(upgradeMode)
            : (SchemaUpgradeMode?) null;
        case UpgradeStage.Final:
          return GetFinalStageUpgradeMode(upgradeMode);
        default:
          throw new ArgumentOutOfRangeException("context.Stage");
      }      
    }
  }
}