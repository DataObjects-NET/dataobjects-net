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
      configuration.Lock();
      var context = new UpgradeContext(configuration);
      using (context.Activate()) {
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
        var domain = BuildStageDomain(UpgradeStage.Final);
        domain.Model.Lock(true);
        foreach (var module in context.Modules)
          module.OnBuilt(domain);
        return domain;
      }
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
            configuration.UpgradeMode==DomainUpgradeMode.Validate ||
            configuration.UpgradeMode==DomainUpgradeMode.Legacy)
          return null; // Nothing to do in these modes here
        schemaUpgradeMode = SchemaUpgradeMode.ValidateCompatible;
        break;
      case UpgradeStage.Upgrading:
        if (configuration.UpgradeMode==DomainUpgradeMode.Recreate ||
            configuration.UpgradeMode==DomainUpgradeMode.Validate ||
            configuration.UpgradeMode==DomainUpgradeMode.Legacy)
          return null; // Nothing to do in these modes here
        if (configuration.UpgradeMode==DomainUpgradeMode.PerformSafely)
          schemaUpgradeMode = SchemaUpgradeMode.PerformSafely;
        break;
      case UpgradeStage.Final:
        if (configuration.UpgradeMode==DomainUpgradeMode.Recreate)
          schemaUpgradeMode = SchemaUpgradeMode.Recreate;
        if (configuration.UpgradeMode==DomainUpgradeMode.Validate)
          schemaUpgradeMode = SchemaUpgradeMode.ValidateExact;
        if (configuration.UpgradeMode==DomainUpgradeMode.Legacy)
          schemaUpgradeMode = SchemaUpgradeMode.ValidateLegacy;
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

    private static void BuildSchemaHints(StorageInfo extractedSchema, StorageInfo targetSchema)
    {
      var context = UpgradeContext.Demand();
      var oldModel = context.ExtractedDomainModel;
      if (oldModel != null) {
        var newModel = Domain.Demand().Model;
        var hintGenerator = new HintGenerator(oldModel, newModel, extractedSchema);
        var hints = hintGenerator.GenerateHints(context.Hints);
        context.Hints.Clear();
        foreach (var modelHint in hints.ModelHints)
          context.Hints.Add(modelHint);
        foreach (var schemaHint in hints.SchemaHints)
          context.SchemaHints.Add(schemaHint);
      }
    }

    private static int ProvideTypeId(UpgradeContext context, Type type)
    {
      var typeId = ModelTypeInfo.NoTypeId;
      var oldModel = context.ExtractedDomainModel;
      if (oldModel == null && context.ExtractedTypeMap == null)
        return typeId;

      // type has been renamed?
      var fullName = type.GetFullName();
      var renamer = context.Hints.OfType<RenameTypeHint>()
        .SingleOrDefault(hint => hint.NewType.GetFullName()==fullName);
      if (renamer != null) {
        if (context.ExtractedTypeMap.TryGetValue(renamer.OldType, out typeId))
          return typeId;
        if (oldModel != null)
          return oldModel.Types.Single(t => t.UnderlyingType==renamer.OldType).TypeId;
      }
      // type has been preserved
      if (context.ExtractedTypeMap.TryGetValue(fullName, out typeId))
        return typeId;
      if (oldModel != null) {
        var oldType = oldModel.Types
          .SingleOrDefault(t => t.UnderlyingType==fullName);
        if (oldType != null)
          return oldType.TypeId;
      }
      return ModelTypeInfo.NoTypeId;
    }
  }
}