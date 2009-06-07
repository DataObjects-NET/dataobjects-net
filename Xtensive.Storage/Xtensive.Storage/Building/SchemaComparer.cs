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
      var status = GetComparisonStatus(hints, actions);
      var canPerformSafely = CanPerform(difference as NodeDifference, true);
      return new SchemaComparisonResult(status, hints, difference, actions, canPerformSafely);
    }

    private static SchemaComparisonStatus GetComparisonStatus(HintSet hints, ActionSequence upgradeActions)
    {
      var actions = upgradeActions.Flatten();
      var hasRemoveActions = actions
        .OfType<RemoveNodeAction>()
        .Any(action => action.Difference.Source is TableInfo
          || action.Difference.Source is ColumnInfo);
      var hasCreateActions = actions
        .OfType<CreateNodeAction>()
        .Any();
      if (hasCreateActions && hasRemoveActions)
        return SchemaComparisonStatus.NotEqual;
      if (hasCreateActions)
        return SchemaComparisonStatus.TargetIsSuperset;
      if (hasRemoveActions)
        return SchemaComparisonStatus.TargetIsSubset;
      return SchemaComparisonStatus.Equal;
    }

    private static bool CanPerform(NodeDifference schemaDifference, bool safely)
    {
      if (schemaDifference == null)
        return true;

      return
        !GetTypeChanges(schemaDifference).Any(triplet =>
          (safely
            ? !TypeConversionVerifier.CanConvertSafely(triplet.Second, triplet.Third)
            : !TypeConversionVerifier.CanConvert(triplet.Second, triplet.Third)));
    }

    private static IEnumerable<Triplet<string, TypeInfo, TypeInfo>> GetTypeChanges(NodeDifference schemaDifference)
    {
      if (schemaDifference == null)
        return Enumerable.Empty<Triplet<string, TypeInfo, TypeInfo>>();

      return GetColumnDifferences(schemaDifference)
        .Where(columnDifference =>
          columnDifference.Source != null
            && columnDifference.Target != null
            && columnDifference.PropertyChanges.ContainsKey("Type"))
        .Select(columnDifference =>
        {
          var typeChange = columnDifference.PropertyChanges["Type"];
          return new Triplet<string, TypeInfo, TypeInfo>(
            columnDifference.Target.Path,
            typeChange.Source as TypeInfo,
            typeChange.Target as TypeInfo);
        });
    }

    private static IEnumerable<NodeDifference> GetColumnDifferences(NodeDifference schemaDifference)
    {
      Func<Difference, IEnumerable<Difference>> itemExtractor =
        diff => {
          if (diff is NodeDifference)
            return ((NodeDifference) diff).PropertyChanges.Values;
          else if (diff is NodeCollectionDifference)
            return ((NodeCollectionDifference) diff).ItemChanges.Cast<Difference>();
          else
            return Enumerable.Empty<Difference>();
        };
      return EnumerableUtils.Flatten(schemaDifference.PropertyChanges.Values, itemExtractor, diff => { }, true)
        .OfType<NodeDifference>().Where(nodeDifference => (nodeDifference.Source ?? nodeDifference.Target) is ColumnInfo);
    }
  }
}