// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.23

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Disposing;
using Xtensive.Core.Reflection;
using Xtensive.Core.Sorting;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Storage.Building;
using Xtensive.Storage.Building.Builders;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Resources;
using Assembly = System.Reflection.Assembly;
using ModelTypeInfo = Xtensive.Storage.Model.TypeInfo;

namespace Xtensive.Storage.Upgrade
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
      var context = new UpgradeContext();
      using (context.Activate()) {
        context.OriginalConfiguration = configuration;
        context.Stage = UpgradeStage.Validation;
        context.Modules = new ModuleProvider(configuration);
        BuildUpgradeHandlers();
        context.Modules.BuildModules();

        try {
          BuildStageDomain(UpgradeStage.Validation).DisposeSafely();
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
        BuildStageDomain(UpgradeStage.Upgrading).DisposeSafely();
        var result = BuildStageDomain(UpgradeStage.Final);
        NotifyModules(result);
        return result;
      }
    }

    private static void NotifyModules(Domain domain)
    {
      foreach (var module in domain.Modules)
        module.OnBuilt(domain);
    }

    /// <exception cref="ArgumentOutOfRangeException"><c>context.Stage</c> is out of range.</exception>
    private static Domain BuildStageDomain(UpgradeStage stage)
    {
      var context = UpgradeContext.Current;
      var configuration = context.Configuration = context.OriginalConfiguration.Clone();
      context.Stage = stage;
      // Raising "Before upgrade" event
      foreach (var handler in context.OrderedUpgradeHandlers)
        handler.OnBeforeStage();

      var schemaUpgradeMode = SchemaUpgradeMode.Perform;
      switch (stage) {
      case UpgradeStage.Validation:
        if (configuration.UpgradeMode==DomainUpgradeMode.Recreate ||
            configuration.UpgradeMode==DomainUpgradeMode.Validate)
          return null; // Nothing to do in these modes here
        schemaUpgradeMode = SchemaUpgradeMode.ValidateCompatible;
        break;
      case UpgradeStage.Upgrading:
        if (configuration.UpgradeMode==DomainUpgradeMode.Recreate ||
            configuration.UpgradeMode==DomainUpgradeMode.Validate)
          return null; // Nothing to do in these modes here
        if (configuration.UpgradeMode==DomainUpgradeMode.PerformSafely)
          schemaUpgradeMode = SchemaUpgradeMode.PerformSafely;
        break;
      case UpgradeStage.Final:
        if (configuration.UpgradeMode==DomainUpgradeMode.Recreate)
          schemaUpgradeMode = SchemaUpgradeMode.Recreate;
        if (configuration.UpgradeMode==DomainUpgradeMode.Validate)
          schemaUpgradeMode = SchemaUpgradeMode.ValidateExact;
        break;
      default:
        throw new ArgumentOutOfRangeException("context.Stage");
      }
      return DomainBuilder.BuildDomain(configuration, 
        CreateBuilderConfiguration(schemaUpgradeMode));
    }

    private static DomainBuilderConfiguration CreateBuilderConfiguration(SchemaUpgradeMode schemaUpgradeMode)
    {
      var context = UpgradeContext.Current;
      return new DomainBuilderConfiguration(schemaUpgradeMode, context.Modules) {
        TypeFilter = type => {
          var assembly = type.Assembly;
          var handlers = context.UpgradeHandlers;
          return
            handlers.ContainsKey(assembly) && TypeFilteringHelper.IsPersistentType(type)
              && handlers[assembly].IsTypeAvailable(type, context.Stage);
        },
        FieldFilter = field => {
          var assembly = field.DeclaringType.Assembly;
          var handlers = context.UpgradeHandlers;
          return
            handlers.ContainsKey(assembly)
              && handlers[assembly].IsFieldAvailable(field, context.Stage);
        },
        SchemaReadyHandler = (extractedSchema, targetSchema) => {
          context.SchemaHints = null;
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

    private static List<IUpgradeHandler> SortUpgradeHandlers(
      IDictionary<Assembly, IUpgradeHandler> unsortedHandlers)
    {
      var references = (from asm in unsortedHandlers.Keys
      select new {asm, Reference = asm.GetReferencedAssemblies().Select(a => a.ToString())})
        .ToDictionary(a => a.asm, a => a.Reference);
      return TopologicalSorter.Sort(unsortedHandlers,
        (asm0, asm1) => references[asm1.Key].Any(asmName => asmName==asm0.Key.GetName().ToString()))
        .Select(pair => pair.Value).ToList();
    }

    private static Exception GetInnermostException(Exception exception)
    {
      ArgumentValidator.EnsureArgumentNotNull(exception, "exception");
      var current = exception;
      while (current.InnerException!=null)
        current = current.InnerException;
      return current;
    }

    private static void BuildSchemaHints(StorageInfo extractedSchema, StorageInfo targetSchema)
    {
      var context = UpgradeContext.Demand();
      context.SchemaHints = new HintSet(extractedSchema, targetSchema);
      var oldModel = context.ExtractedDomainModel;
      if (oldModel != null) {
        var newModel = Domain.Demand().Model;
        var generatedHints = new HintGenerator(oldModel, newModel)
          .GenerateHints(context.Hints);
        generatedHints.Apply(context.SchemaHints.Add);
      }
    }

    /// <exception cref="DomainBuilderException">More then one enabled handler is provided for some assembly.</exception>
    private static void BuildUpgradeHandlers()
    {
      var context = UpgradeContext.Current;
      var handlers = new Dictionary<Assembly, IUpgradeHandler>();

      var assemblies = (
        from type in context.OriginalConfiguration.Types
        let assembly = type.Assembly
        select assembly).Distinct();

      // Creating user handlers
      var userHandlers =
        from assembly in assemblies
        from type in assembly.GetTypes()
        where 
          (typeof (IUpgradeHandler)).IsAssignableFrom(type)
          && !type.IsAbstract
          && type.IsClass
          && type!=typeof(UpgradeHandler)
        let handler = (IUpgradeHandler) type.Activate(null)
        where handler!=null && handler.IsEnabled
        group handler by assembly;

      // Adding user handlers
      foreach (var group in userHandlers) {
        if (group.Count()>1)
          throw new DomainBuilderException(string.Format(
            Strings.ExMoreThanOneEnabledXIsProvidedForAssemblyY, 
            typeof(IUpgradeHandler).GetShortName(), group.Key));
        handlers.Add(group.Key, group.First());
      }

      context.Modules.SetUpgradeHandlers(handlers.Values);

      // Adding default handlers
      var assembliesWithUserHandlers = userHandlers.Select(g => g.Key);
      var assembliesWithoutUserHandler = assemblies.Except(assembliesWithUserHandlers);

      foreach (var assembly in assembliesWithoutUserHandler) {
        var handler = new UpgradeHandler(assembly);
        handlers.Add(assembly, handler);
      }

      // Storing the result
      context.UpgradeHandlers = 
        new ReadOnlyDictionary<Assembly, IUpgradeHandler>(handlers, false);
      context.OrderedUpgradeHandlers = 
        new ReadOnlyList<IUpgradeHandler>(SortUpgradeHandlers(handlers));
    }

    private static int ProvideTypeId(UpgradeContext context, Type type)
    {
      var oldModel = context.ExtractedDomainModel;
      if (oldModel==null)
        return ModelTypeInfo.NoTypeId;
      // type has been renamed?
      var renamer = context.Hints.OfType<RenameTypeHint>()
        .SingleOrDefault(hint => hint.NewType==type);
      if (renamer != null)
        return oldModel.Types.Single(t => t.UnderlyingType==renamer.OldType).TypeId;
      // type has been preserved
      var oldType = oldModel.Types
        .SingleOrDefault(t => t.UnderlyingType==type.GetFullName());
      if (oldType != null)
        return oldType.TypeId;
      return ModelTypeInfo.NoTypeId;
    }
  }
}