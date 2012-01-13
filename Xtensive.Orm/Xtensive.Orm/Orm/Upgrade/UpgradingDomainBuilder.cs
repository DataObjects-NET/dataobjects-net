// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.23

using System;
using System.Linq;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Orm.Building;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Upgrade.Model;
using Xtensive.Orm.Resources;
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
      configuration.Lock();
      var context = new UpgradeContext(configuration);
      using (context.Activate()) {
        // 1st Domain
        try {
          BuildStageDomain(UpgradeStage.Initializing).DisposeSafely();
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
        BuildStageDomain(UpgradeStage.Upgrading).DisposeSafely();

        // 3rd Domain
        var domain = BuildStageDomain(UpgradeStage.Final);
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

      var schemaUpgradeMode = GetUpgradeMode(stage, configuration.UpgradeMode);
      if (schemaUpgradeMode==null)
        return null;
      return DomainBuilder.BuildDomain(
        configuration, CreateBuilderConfiguration(schemaUpgradeMode.Value));
    }

    private static DomainBuilderConfiguration CreateBuilderConfiguration(SchemaUpgradeMode schemaUpgradeMode)
    {
      var context = UpgradeContext.Current;
      return new DomainBuilderConfiguration(schemaUpgradeMode, context.Modules, context.Services) {
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
      var newModel = Domain.Demand().Model;
      var hintGenerator = new HintGenerator(oldModel, newModel, extractedSchema);
      var hints = hintGenerator.GenerateHints(context.Hints);
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