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
using Xtensive.Storage.Model;
using Xtensive.Orm.Model;
using Xtensive.Orm.Upgrade;
using ColumnInfo=Xtensive.Storage.Model.ColumnInfo;
using TypeInfo=Xtensive.Storage.Model.TypeInfo;
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
    public static SchemaComparisonResult Compare(StorageInfo sourceSchema, StorageInfo targetSchema, 
      HintSet hints, SchemaUpgradeMode schemaUpgradeMode, DomainModel model)
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
      var comparisonStatus = GetComparisonStatus(actions, schemaUpgradeMode);
      var unsafeActions = GetUnsafeActions(actions);
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
          return table!=null && !systemTables.Contains(table.Name);
        }).ToList();
      var createColumnActions = actions.Flatten()
        .OfType<CreateNodeAction>()
        .Where(action => {
          var column = action.Difference.Target as ColumnInfo;
          return column!=null && !systemTables.Contains(column.Parent.Name);
        }).ToList();
      columnTypeChangeActions = columnTypeChangeActions.Where(action => {
        var sourceType = action.Difference.Source as TypeInfo;
        var targetType = action.Difference.Target as TypeInfo;
        return sourceType==null 
          || targetType==null
          || sourceType.IsTypeUndefined 
          || sourceType.Type.ToNullable()!=targetType.Type.ToNullable();
      });

      var isCompatibleInLegacyMode = 
        !createTableActions.Any() && 
        !createColumnActions.Any() && 
        !columnTypeChangeActions.Any();
      return new SchemaComparisonResult(
        comparisonStatus, 
        columnTypeChangeActions.Any(), 
        isCompatibleInLegacyMode, 
        hints, 
        difference, 
        actions, 
        unsafeActions);
    }

    private static IList<GroupingNodeAction> GetSystemTableActions(ActionSequence actions, HashSet<string> systemTableNames)
    {
      return actions.OfType<GroupingNodeAction>().Flatten(
        action => action.Actions.OfType<GroupingNodeAction>(), ga => { }, false)
        .Where(action => systemTableNames.Contains(action.Comment)).ToList();
    }
    
    private static IList<NodeAction> GetUnsafeActions(ActionSequence upgradeActions)
    {
      var unsafeActions = new List<NodeAction>();
      var upgradeContext = UpgradeContext.Demand();
      var hints = upgradeContext.Hints;
      
      // Unsafe type changes
      var typeChangeAction = GetTypeChangeActions(upgradeActions);
      var columnsWithHint = hints
        .OfType<ChangeFieldTypeHint>()
        .SelectMany(hint => hint.AffectedColumns)
        .ToHashSet();
      typeChangeAction
        .Where(action => !TypeConversionVerifier.CanConvertSafely(
          action.Difference.Source as TypeInfo, action.Difference.Target as TypeInfo))
        .Where(action1 => !columnsWithHint.Contains(action1.Path))
        .ForEach(unsafeActions.Add);

      // Unsafe column removes
      var columnActions = GetColumnActions(upgradeActions).ToList();
      columnsWithHint = hints
        .OfType<RemoveFieldHint>()
        .SelectMany(hint => hint.AffectedColumns)
        .ToHashSet();
      columnActions
        .OfType<RemoveNodeAction>()
        .Where(action => !columnsWithHint.Contains(action.Path))
        .ForEach(unsafeActions.Add);
      
      // Unsafe type removes
      var tableActions = GetTableActions(upgradeActions);
      var tableWithHints = hints
        .OfType<RemoveTypeHint>()
        .SelectMany(hint => hint.AffectedTables)
        .ToHashSet();
      tableActions
        .OfType<RemoveNodeAction>()
        .Where(action => !tableWithHints.Contains(action.Path))
        .ForEach(unsafeActions.Add);

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

    private static SchemaComparisonStatus GetComparisonStatus(ActionSequence actions, SchemaUpgradeMode schemaUpgradeMode)
    {
      var actionList = actions.Flatten().ToList();
      
      var filter = schemaUpgradeMode != SchemaUpgradeMode.ValidateCompatible
        ? (Func<Type, bool>) (targetType => true)
        : targetType => targetType.In(typeof(TableInfo), typeof(ColumnInfo));
      var hasCreateActions = actionList
        .OfType<CreateNodeAction>()
        .Select(action => action.Difference.Target.GetType())
        .Any(filter);
      var hasRemoveActions = actionList
        .OfType<RemoveNodeAction>()
        .Select(action => action.Difference.Source.GetType())
        .Any(sourceType => sourceType.In(typeof(TableInfo), typeof(ColumnInfo)));
      
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
  