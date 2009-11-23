// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.01

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Storage.Indexing.Model;
using Xtensive.Storage.Upgrade;
using UpgradeContext=Xtensive.Storage.Upgrade.UpgradeContext;

namespace Xtensive.Storage.Building
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
    public static SchemaComparisonResult Compare(StorageInfo sourceSchema,
      StorageInfo targetSchema, HintSet hints)
    {
      if (hints == null)
        hints = new HintSet(sourceSchema, targetSchema);
      var comparer = new Comparer();
      var difference = comparer.Compare(sourceSchema, targetSchema, hints);
      var actions = new ActionSequence() {
        difference==null 
        ? EnumerableUtils<NodeAction>.Empty 
        : new Upgrader().GetUpgradeSequence(difference, hints, comparer)
      };
      var status = GetComparisonStatus(actions);
      var unsafeActions = GetUnsafeActions(actions);
      var hasTypeChanges = GetTypeChangeActions(actions).Any();
      var hasCreateDataNodeActions = actions
        .OfType<CreateNodeAction>()
        .Any(action => action.Difference.Target is TableInfo
          || action.Difference.Target is ColumnInfo);
      var isCompatible = !hasCreateDataNodeActions && !hasTypeChanges;

      return new SchemaComparisonResult(status, hints, difference, actions, hasTypeChanges, unsafeActions, isCompatible);
    }

    private static IList<NodeAction> GetUnsafeActions(ActionSequence upgradeActions)
    {
      var unsafeActions = new List<NodeAction>();
      
      // Unsafe type changes
      var typeChangeAction = GetTypeChangeActions(upgradeActions);
      var columnsWithHint = UpgradeContext.Demand().Hints
        .OfType<ChangeFieldTypeHint>()
        .SelectMany(hint => hint.AffectedColumns)
        .ToHashSet();
      typeChangeAction
        .Where(action => !TypeConversionVerifier.CanConvertSafely(
          action.Difference.Source as TypeInfo, action.Difference.Target as TypeInfo))
        .Where(action1 => !columnsWithHint.Contains(action1.Path))
        .Apply(unsafeActions.Add);

      // Unsafe column removes
      var columnActions = GetColumnActions(upgradeActions).ToList();
      columnsWithHint = UpgradeContext.Demand().Hints
        .OfType<RemoveFieldHint>()
        .SelectMany(hint => hint.AffectedColumns)
        .ToHashSet();
      columnActions
        .OfType<RemoveNodeAction>()
        .Where(action => !columnsWithHint.Contains(action.Path))
        .Apply(unsafeActions.Add);
      
      // Unsafe type removes
      var tableActions = GetTableActions(upgradeActions);
      var tableWithHints = UpgradeContext.Demand().Hints
        .OfType<RemoveTypeHint>()
        .SelectMany(hint => hint.AffectedTables)
        .ToHashSet();
      tableActions
        .OfType<RemoveNodeAction>()
        .Where(action => !tableWithHints.Contains(action.Path))
        .Apply(unsafeActions.Add);

      return unsafeActions;
    }

    private static IEnumerable<PropertyChangeAction> GetTypeChangeActions(ActionSequence upgradeActions)
    {
      return upgradeActions
        .Flatten()
        .OfType<PropertyChangeAction>()
        .Where(action =>
          action.Properties.ContainsKey("Type")
            && action.Difference.Parent.Source is ColumnInfo
            && action.Difference.Parent.Target is ColumnInfo);
    }

    private static IEnumerable<NodeAction> GetColumnActions(ActionSequence upgradeActions)
    {
      Func<NodeAction, bool> isColumnAction = (action) => {
        var diff = action.Difference as NodeDifference;
        if (diff==null)
          return false;
        var item = diff.Source ?? diff.Target;
        return item is ColumnInfo;
      };

      return upgradeActions
        .Flatten()
        .OfType<NodeAction>()
        .Where(isColumnAction);
    }

    private static IEnumerable<NodeAction> GetTableActions(ActionSequence upgradeActions)
    {
      Func<NodeAction, bool> isTableAction = (action) => {
        var diff = action.Difference as NodeDifference;
        if (diff==null)
          return false;
        var item = diff.Source ?? diff.Target;
        return item is TableInfo;
      };

      return upgradeActions
        .Flatten()
        .OfType<NodeAction>()
        .Where(isTableAction);
    }

    private static SchemaComparisonStatus GetComparisonStatus(ActionSequence upgradeActions)
    {
      var actions = upgradeActions.Flatten().ToList();
      
      var hasCreateActions = actions
        .OfType<CreateNodeAction>()
        .Any();
      var hasRemoveActions = actions
        .OfType<RemoveNodeAction>()
        .Any(action => action.Difference.Source is TableInfo
          || action.Difference.Source is ColumnInfo);
      

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
  