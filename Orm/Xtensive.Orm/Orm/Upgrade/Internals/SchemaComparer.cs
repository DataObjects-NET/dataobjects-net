// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Ivan Galkin
// Created:    2009.05.01

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Orm.Model;
using Xtensive.Orm.Upgrade.Model;
using Xtensive.Reflection;

namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// Compares storage models.
  /// </summary>
  internal static class SchemaComparer
  {
    private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

    /// <summary>
    /// Compares <paramref name="sourceSchema"/> and <paramref name="targetSchema"/>.
    /// </summary>
    /// <param name="sourceSchema">The source schema.</param>
    /// <param name="targetSchema">The target schema.</param>
    /// <param name="schemaHints">The upgrade hints.</param>
    /// <param name="upgradeHints"><see cref="UpgradeHint"/>s to be applied.</param>
    /// <param name="schemaUpgradeMode">A <see cref="SchemaUpgradeMode"/> being used.</param>
    /// <param name="model">A <see cref="DomainModel"/> of a storage.</param>
    /// <param name="briefExceptionFormat">Indicates whether brief or full exception format should be used.</param>
    /// <param name="upgradeStage">A current <see cref="UpgradeStage"/>.</param>
    /// <returns>Comparison result.</returns>
    public static SchemaComparisonResult Compare(
      StorageModel sourceSchema, StorageModel targetSchema, 
      HintSet schemaHints, IEnumerable<UpgradeHint> upgradeHints,
      SchemaUpgradeMode schemaUpgradeMode, DomainModel model,
      bool briefExceptionFormat, UpgradeStage upgradeStage)
    {
      if (schemaHints==null)
        schemaHints = new HintSet(sourceSchema, targetSchema);

      var comparer = new Comparer();
      var difference = comparer.Compare(sourceSchema, targetSchema, schemaHints);
      var actions = GetUpgradeActions(comparer, difference, schemaHints);
      var actionList = actions.Flatten().ToList();
      var comparisonStatus = GetComparisonStatus(actionList, schemaUpgradeMode);
      var unsafeActions = GetUnsafeActions(actionList, upgradeHints);
      var columnTypeChangeActions = actionList.OfType<PropertyChangeAction>().Where(IsTypeChangeAction).ToList();

      if (schemaUpgradeMode!=SchemaUpgradeMode.ValidateLegacy)
        return new SchemaComparisonResult(
          comparisonStatus, columnTypeChangeActions.Count > 0, null,
          schemaHints, difference, actions, unsafeActions);

      // Legacy comparison

      var systemTablesSequence = model.Types.Where(type => type.IsSystem).Select(type => type.MappingName);
      var systemTables = new HashSet<string>(systemTablesSequence, Comparer);

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
        schemaHints, difference, actions, unsafeActions);
    }

    private static ActionSequence GetUpgradeActions(Comparer comparer, Difference difference, HintSet hints)
    {
      var actions = difference!=null
        ? new Upgrader().GetUpgradeSequence(difference, hints, comparer)
        : Enumerable.Empty<NodeAction>();
      return new ActionSequence {actions};
    }

    private static IList<NodeAction> GetUnsafeActions(ICollection<NodeAction> actions, IEnumerable<UpgradeHint> hints)
    {
      var unsafeActions = new List<NodeAction>();

      GetUnsafeColumnTypeChanges(actions, hints, unsafeActions);
      GetUnsafeColumnRemovals(actions, hints, unsafeActions);
      GetUnsafeTableRemovals(actions, hints, unsafeActions);
      GetUnsafeDataActions(actions, hints, unsafeActions);

      return unsafeActions;
    }

    private static void GetUnsafeColumnTypeChanges(
      IEnumerable<NodeAction> actions, IEnumerable<UpgradeHint> hints, ICollection<NodeAction> output)
    {
       var columnsWithHints = hints.OfType<ChangeFieldTypeHint>().SelectMany(hint => hint.AffectedColumns);

      // The following code will not work because HintGenerator generates node paths
      // inconsistent with RemoveFieldHint:

      // GetUnsafeColumnActions(actions, columnsWithHints, IsUnsafeTypeChangeAction, output);

      var safeColumns = new HashSet<string>(columnsWithHints, Comparer);

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
      var safeColumns = new HashSet<string>(columnsWithHints, Comparer);
      var tableMapping = new Dictionary<string, string>(Comparer);
      var reverseTableMapping = new Dictionary<string, string>(Comparer);

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

        var tablePath = $"Tables/{pathItems[1]}";
        var columnName = pathItems[3];

        string originalTablePath;

        if (reverseTableMapping.TryGetValue(tablePath, out originalTablePath))
          columnPath = $"{originalTablePath}/Columns/{columnName}";

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
        .Where(a => !safeTableRemovals.Contains(a.Path, Comparer))
        .ForEach(output.Add);
    }

    private static void GetUnsafeDataActions(IEnumerable<NodeAction> actions, IEnumerable<UpgradeHint> hints, ICollection<NodeAction> output)
    {
      GetCrossHierarchicalMovements(actions, output);
      GetTableRecreateDataLossActions(actions, output);
    }

    private static void GetCrossHierarchicalMovements(IEnumerable<NodeAction> actions, ICollection<NodeAction> output)
    {
      (from action in actions.OfType<DataAction>()
        let deleteDataHint = action.DataHint as DeleteDataHint
        where deleteDataHint!=null && deleteDataHint.PostCopy
        select action).ForEach(output.Add);
    }

    private static void GetTableRecreateDataLossActions(IEnumerable<NodeAction> actions, ICollection<NodeAction> output)
    {
      actions.OfType<DataAction>()
        .Select(da => new {
          DataAction = da,
          Difference = da.Difference as NodeDifference,
          DeleteDataHint = da.DataHint as DeleteDataHint
        })
        .Where(a => a.DeleteDataHint != null && a.Difference != null && a.Difference.MovementInfo.HasFlag(MovementInfo.Removed | MovementInfo.Created))
        .Select(a => a.DataAction)
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
        : targetType => targetType.In(WellKnownUpgradeTypes.TableInfo, WellKnownUpgradeTypes.StorageColumnInfo);
      var hasCreateActions = actions
        .OfType<CreateNodeAction>()
        .Select(action => action.Difference.Target.GetType())
        .Any(filter);
      var hasRemoveActions = actions
        .OfType<RemoveNodeAction>()
        .Select(action => action.Difference.Source.GetType())
        .Any(sourceType => sourceType.In(WellKnownUpgradeTypes.TableInfo, WellKnownUpgradeTypes.StorageColumnInfo));

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
  