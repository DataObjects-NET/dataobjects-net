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
      if (hints==null)
        hints = new HintSet(sourceSchema, targetSchema);

      var comparer = new Comparer();
      var difference = comparer.Compare(sourceSchema, targetSchema, hints);
      var actions = GetUpgradeActions(comparer, difference, hints);
      var actionList = actions.Flatten().ToList();
      var comparisonStatus = GetComparisonStatus(actionList, schemaUpgradeMode);
      var unsafeActions = GetUnsafeActions(actionList);
      var columnTypeChangeActions = actionList.OfType<PropertyChangeAction>().Where(IsTypeChangeAction).ToList();

      if (schemaUpgradeMode!=SchemaUpgradeMode.ValidateLegacy)
        return new SchemaComparisonResult(
          comparisonStatus, columnTypeChangeActions.Count > 0, null,
          hints, difference, actions, unsafeActions);

      // Legacy comparison

      var systemTablesSequence = model.Types.Where(type => type.IsSystem).Select(type => type.MappingName);
      var systemTables = new HashSet<string>(systemTablesSequence, StringComparer.OrdinalIgnoreCase);

      var createTableActions = actionList
        .OfType<CreateNodeAction>()
        .Where(
          action => {
            var table = action.Difference.Target as TableInfo;
            return table!=null && !systemTables.Contains(table.Name);
          })
        .ToList();

      var createColumnActions = actionList
        .OfType<CreateNodeAction>()
        .Where(
          action => {
            var column = action.Difference.Target as StorageColumnInfo;
            return column!=null && !systemTables.Contains(column.Parent.Name);
          })
        .ToList();

      columnTypeChangeActions = columnTypeChangeActions
        .Where(
          action => {
            var sourceType = action.Difference.Source as StorageTypeInfo;
            var targetType = action.Difference.Target as StorageTypeInfo;
            return sourceType==null || targetType==null || sourceType.IsTypeUndefined
              || sourceType.Type.ToNullable()!=targetType.Type.ToNullable();
          })
        .ToList();

      var isCompatibleInLegacyMode =
        createTableActions.Count==0
        && createColumnActions.Count==0
        && columnTypeChangeActions.Count==0;

      if (briefExceptionFormat)
        unsafeActions =
          createTableActions.Cast<NodeAction>()
            .Concat(createColumnActions.Cast<NodeAction>())
            .Concat(columnTypeChangeActions.Cast<NodeAction>())
            .ToList();

      return new SchemaComparisonResult(
        comparisonStatus, columnTypeChangeActions.Count > 0, isCompatibleInLegacyMode,
        hints, difference, actions, unsafeActions);
    }

    private static ActionSequence GetUpgradeActions(Comparer comparer, Difference difference, HintSet hints)
    {
      var actions = difference!=null
        ? new Upgrader().GetUpgradeSequence(difference, hints, comparer)
        : EnumerableUtils<NodeAction>.Empty;
      return new ActionSequence {actions};
    }

    private static IList<NodeAction> GetUnsafeActions(ICollection<NodeAction> actions)
    {
      var unsafeActions = new List<NodeAction>();
      var upgradeContext = UpgradeContext.Demand();
      var hints = upgradeContext.Hints;
      
      GetUnsafeColumnTypeChanges(actions, hints, unsafeActions);
      GetUnsafeColumnRemovals(actions, hints, unsafeActions);
      GetUnsafeTableRemovals(actions, hints, unsafeActions);

      return unsafeActions;
    }

    private static void GetUnsafeColumnTypeChanges(
      IEnumerable<NodeAction> actions, IEnumerable<UpgradeHint> hints, ICollection<NodeAction> output)
    {
       var columnsWithHints = hints.OfType<ChangeFieldTypeHint>().SelectMany(hint => hint.AffectedColumns);

      // The following code will not work because HintGenerator generates node paths
      // inconsistent with RemoveFieldHint:

      // GetUnsafeColumnActions(actions, columnsWithHints, IsUnsafeTypeChangeAction, output);

      var safeColumns = new HashSet<string>(columnsWithHints, StringComparer.OrdinalIgnoreCase);

      var unsafeActions = actions
        .Where(IsUnsafeTypeChangeAction)
        .Where(action => !safeColumns.Contains(action.Path));

      foreach (var action in unsafeActions)
        output.Add(action);
    }

    private static void GetUnsafeColumnRemovals(
      IEnumerable<NodeAction> actions, IEnumerable<UpgradeHint> hints, ICollection<NodeAction> output)
    {
      var columnsWithHints = hints.OfType<RemoveFieldHint>().SelectMany(hint => hint.AffectedColumns);
      GetUnsafeColumnActions(actions, columnsWithHints, a => IsColumnAction(a) && a is RemoveNodeAction, output);
    }

    private static void GetUnsafeColumnActions(
      IEnumerable<NodeAction> actions, IEnumerable<string> columnsWithHints,
      Func<NodeAction, bool> unsafeActionFilter, ICollection<NodeAction> output)
    {
      var safeColumns = new HashSet<string>(columnsWithHints, StringComparer.OrdinalIgnoreCase);
      var tableMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
      var reverseTableMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

      foreach (var action in actions) {
        if (IsTableAction(action)) {
          var moveAction = action as MoveNodeAction;
          if (moveAction==null)
            continue;
          string originalPath;
          if (reverseTableMapping.TryGetValue(moveAction.Path, out originalPath)) {
            tableMapping.Remove(originalPath);
            reverseTableMapping.Remove(moveAction.Path);
            tableMapping.Add(originalPath, moveAction.NewPath);
            reverseTableMapping.Add(moveAction.NewPath, originalPath);
          }
          else {
            tableMapping.Add(moveAction.Path, moveAction.NewPath);
            reverseTableMapping.Add(moveAction.NewPath, moveAction.Path);
          }
          continue;
        }

        if (!unsafeActionFilter.Invoke(action))
          continue;

        var columnPath = action.Path;

        // Adjust column path if table was renamed

        var pathItems = columnPath.Split('/');
        if (pathItems.Length!=4) {
          // Should not happen, but need to know if this happens
          output.Add(action);
          continue;
        }

        var tablePath = string.Format("Tables/{0}", pathItems[1]);
        var columnName = pathItems[3];

        string originalTablePath;

        if (reverseTableMapping.TryGetValue(tablePath, out originalTablePath))
          columnPath = string.Format("{0}/Columns/{1}", originalTablePath, columnName);

        if (!safeColumns.Contains(columnPath))
          output.Add(action);
      }
    }

    private static void GetUnsafeTableRemovals(IEnumerable<NodeAction> actions, IEnumerable<UpgradeHint> hints, ICollection<NodeAction> output)
    {
      var safeTableRemovals = hints
        .OfType<RemoveTypeHint>()
        .SelectMany(hint => hint.AffectedTables)
        .ToHashSet();

      actions
        .OfType<RemoveNodeAction>()
        .Where(IsTableAction)
        .Where(a => !safeTableRemovals.Contains(a.Path, StringComparer.OrdinalIgnoreCase))
        .ForEach(output.Add);
    }

    private static bool IsTypeChangeAction(PropertyChangeAction action)
    {
      return action.Properties.ContainsKey("Type")
        && action.Difference.Parent.Source is StorageColumnInfo
        && action.Difference.Parent.Target is StorageColumnInfo;
    }

    private static bool IsUnsafeTypeChangeAction(NodeAction action)
    {
      var propertyChangeAction = action as PropertyChangeAction;
      if (propertyChangeAction==null)
        return false;

      if (!IsTypeChangeAction(propertyChangeAction))
        return false;

      return !TypeConversionVerifier.CanConvertSafely(
        (StorageTypeInfo) propertyChangeAction.Difference.Source,
        (StorageTypeInfo) propertyChangeAction.Difference.Target);
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
  