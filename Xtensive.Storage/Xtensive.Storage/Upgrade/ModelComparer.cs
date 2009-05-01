// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.01

using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Storage.Indexing.Model;

namespace Xtensive.Storage.Upgrade
{
  /// <summary>
  /// Compare indexing storage models.
  /// </summary>
  public static class ModelComparer
  {
    /// <summary>
    /// Compare models.
    /// </summary>
    /// <param name="sourceModel">The source model.</param>
    /// <param name="targetModel">The target model.</param>
    /// <param name="upgradeHints">The hints.</param>
    /// <returns>Comparison result.</returns>
    public static ModelComparisonResult Compare(StorageInfo sourceModel,
      StorageInfo targetModel, HintSet upgradeHints)
    {
      if (upgradeHints==null)
        upgradeHints = new HintSet(sourceModel, targetModel);
      var comparer = new Comparer();
      var difference = comparer.Compare(sourceModel, targetModel, upgradeHints);
      var actions = new ActionSequence() {
        difference==null 
        ? EnumerableUtils<NodeAction>.Empty 
        : new Upgrader().GetUpgradeSequence(difference, upgradeHints, comparer)
      };
      return new ModelComparisonResult(difference, actions, upgradeHints, 
        GetStorageConformity(actions, upgradeHints));
    }

    private static SchemaComparisonStatus GetStorageConformity(ActionSequence upgradeActions, HintSet hints)
    {
      var hasRemoveActions = upgradeActions
        .OfType<RemoveNodeAction>()
        .Any();
      var hasCreateActions = upgradeActions
        .OfType<CreateNodeAction>()
        .Any();

      if (hasCreateActions && hasRemoveActions)
        return SchemaComparisonStatus.NotEqual;
      if (hasCreateActions)
        return SchemaComparisonStatus.Subset;
      if (hasRemoveActions)
        return SchemaComparisonStatus.Superset;
      return SchemaComparisonStatus.Equal;
    }

  }
}