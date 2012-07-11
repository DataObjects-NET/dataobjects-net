// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.01

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Orm.Upgrade.Model;
using Xtensive.Orm.Model;
using Xtensive.Orm.Upgrade;
using UpgradeContext=Xtensive.Orm.Upgrade.UpgradeContext;
using UpgradeStage=Xtensive.Modelling.Comparison.UpgradeStage;

namespace Xtensive.Orm.Building
{
  /// <summary>
  /// Compares storage models.
  /// </summary>
  internal static class SchemaComparer
  {
    /// <summary>
    /// Compares <paramref name="sourceSchema"/> and <paramref name="targetSchema"/>.
    /// </summary>
    /// <param name="sourceSchema">The source schema.</param>
    /// <param name="targetSchema">The target schema.</param>
    /// <param name="hints">The upgrade hints.</param>
    /// <returns>Comparison result.</returns>
    public static SchemaComparisonResult Compare(StorageModel sourceSchema, StorageModel targetSchema, 
      HintSet hints, SchemaUpgradeMode schemaUpgradeMode, DomainModel model, bool briefExceptionFormat)
    {
      if (hints == null)
        hints = new HintSet(sourceSchema, targetSchema);

      // Nothing must be done in Skip mode
      if (schemaUpgradeMode==SchemaUpgradeMode.Skip)
        return new SchemaComparisonResult(
          SchemaComparisonStatus.NotEqual, 
          false, 
          null, 
          hints, 
          null, 
          new ActionSequence(), 
          new List<NodeAction>());

      var comparer = new Comparer();
      var difference = comparer.Compare(sourceSchema, targetSchema, hints);
      var actions = new ActionSequence() {
        difference==null 
        ? EnumerableUtils<NodeAction>.Empty 
        : new Upgrader().GetUpgradeSequence(difference, hints, comparer)
      };

      var actionList = actions.Flatten().ToList().AsReadOnly();
      var comparisonStatus = GetComparisonStatus(actionList, schemaUpgradeMode);
      var unsafeActions = GetUnsafeActions(actionList);
      var columnTypeChangeActions = GetTypeChangeActions(actions);
      
      if (schemaUpgradeMode!=SchemaUpgradeMode.ValidateLegacy)
        return new SchemaComparisonResult(
          comparisonStatus, 
          columnTypeChangeActions.Any(), 
          null, 
          hints, 
          difference, 
          actions, 
          unsafeActions);

      var systemTables = model.Types
        .Where(type => type.IsSystem)
        .Select(type => type.MappingName)
        .ToHashSet();
      
      var createTableActions = actions.Flatten()
        .OfType<CreateNodeAction>()
        .Where(action => {
          var table = action.Difference.Target as TableInfo;
          return table!=null && !systemTables.Contains(table.Name, StringComparer.OrdinalIgnoreCase);
        }).ToList();
      var createColumnActions = actions.Flatten()
        .OfType<CreateNodeAction>()
        .Where(action => {
          var column = action.Difference.Target as StorageColumnInfo;
          return column!=null && !systemTables.Contains(column.Parent.Name, StringComparer.OrdinalIgnoreCase);
        }).ToList();
      columnTypeChangeActions = columnTypeChangeActions.Where(action => {
        var sourceType = action.Difference.Source as StorageTypeInfo;
        var targetType = action.Difference.Target as StorageTypeInfo;
        return sourceType==null 
          || targetType==null
          || sourceType.IsTypeUndefined 
          || sourceType.Type.ToNullable()!=targetType.Type.ToNullable();
      });

      var isCompatibleInLegacyMode = 
        !createTableActions.Any() && 
        !createColumnActions.Any() && 
        !columnTypeChangeActions.Any();

      if (briefExceptionFormat)
        unsafeActions =
          createTableActions.Cast<NodeAction>()
            .Concat(createColumnActions.Cast<NodeAction>())
            .Concat(columnTypeChangeActions.Cast<NodeAction>())
            .ToList();

      return new SchemaComparisonResult(
        comparisonStatus, 
        columnTypeChangeActions.Any(), 
        isCompatibleInLegacyMode, 
        hints, 
        difference, 
        actions, 
        unsafeActions);
    }

    private static IList<NodeAction> GetUnsafeActions(ICollection<NodeAction> actions)
    {
      var unsafeActions = new List<NodeAction>();
      var upgradeContext = UpgradeContext.Demand();
      var hints = upgradeContext.Hints;
      
      // Unsafe type changes
      var typeChangeAction = GetTypeChangeActions(actions);
      var safeColumnTypeChanges = hints
        .OfType<ChangeFieldTypeHint>()
        .SelectMany(hint => hint.AffectedColumns)
        .ToHashSet();

      typeChangeAction
        .Where(a => !TypeConversionVerifier.CanConvertSafely(a.Difference.Source as StorageTypeInfo, a.Difference.Target as StorageTypeInfo))
        .Where(a => !safeColumnTypeChanges.Contains(a.Path, StringComparer.OrdinalIgnoreCase))
        .ForEach(unsafeActions.Add);

      // Unsafe column removes
      var safeColumnRemovals = hints
        .OfType<RemoveFieldHint>()
        .SelectMany(hint => hint.AffectedColumns)
        .ToHashSet();

      actions
        .OfType<RemoveNodeAction>()
        .Where(IsColumnAction)
        .Where(a => !safeColumnRemovals.Contains(a.Path, StringComparer.OrdinalIgnoreCase))
        .ForEach(unsafeActions.Add);
      
      // Unsafe type removes
      var tableWithHints = hints
        .OfType<RemoveTypeHint>()
        .SelectMany(hint => hint.AffectedTables)
        .ToHashSet();

      actions
        .OfType<RemoveNodeAction>()
        .Where(IsTableAction)
        .Where(a => !tableWithHints.Contains(a.Path, StringComparer.OrdinalIgnoreCase))
        .ForEach(unsafeActions.Add);

      return unsafeActions;
    }

    private static IEnumerable<PropertyChangeAction> GetTypeChangeActions(IEnumerable<NodeAction> actions)
    {
      return actions
        .OfType<PropertyChangeAction>()
        .Where(action =>
          action.Properties.ContainsKey("Type")
            && action.Difference.Parent.Source is StorageColumnInfo
            && action.Difference.Parent.Target is StorageColumnInfo);
    }

    private static bool IsTableAction(NodeAction action)
    {
      var diff = action.Difference as NodeDifference;
      if (diff==null)
        return false;
      var item = diff.Source ?? diff.Target;
      return item is TableInfo;
    }

    private static bool IsColumnAction(NodeAction action)
    {
      var diff = action.Difference as NodeDifference;
      if (diff==null)
        return false;
      var item = diff.Source ?? diff.Target;
      return item is StorageColumnInfo;
    }

    private static SchemaComparisonStatus GetComparisonStatus(ICollection<NodeAction> actions, SchemaUpgradeMode schemaUpgradeMode)
    {
      var filter = schemaUpgradeMode!=SchemaUpgradeMode.ValidateCompatible
        ? (Func<Type, bool>) (targetType => true)
        : targetType => targetType.In(typeof (TableInfo), typeof (StorageColumnInfo));
      var hasCreateActions = actions
        .OfType<CreateNodeAction>()
        .Select(action => action.Difference.Target.GetType())
        .Any(filter);
      var hasRemoveActions = actions
        .OfType<RemoveNodeAction>()
        .Select(action => action.Difference.Source.GetType())
        .Any(sourceType => sourceType.In(typeof (TableInfo), typeof (StorageColumnInfo)));

      if (hasCreateActions && hasRemoveActions)
        return SchemaComparisonStatus.NotEqual;
      if (hasCreateActions)
        return SchemaComparisonStatus.TargetIsSuperset;
      if (hasRemoveActions)
        return SchemaComparisonStatus.TargetIsSubset;
      return SchemaComparisonStatus.Equal;
    }
  }
}
  