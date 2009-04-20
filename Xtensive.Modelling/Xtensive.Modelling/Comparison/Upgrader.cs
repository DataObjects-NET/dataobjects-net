// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.07

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Serialization.Binary;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Modelling.Resources;

namespace Xtensive.Modelling.Comparison
{
  /// <summary>
  /// <see cref="IUpgrader"/> implementation.
  /// </summary>
  public class Upgrader : IUpgrader
  {
    [ThreadStatic]
    private static Upgrader current;
    private List<NodeAction> actions;

    #region Properties: Current, Context, Difference, Hints, XxxModel

    /// <summary>
    /// Gets the current comparer.
    /// </summary>
    public static Upgrader Current {
      get { return current; }
    }

    /// <summary>
    /// Gets the current upgrade context.
    /// </summary>
    protected internal UpgradeContext Context { get; internal set; }

    /// <summary>
    /// Gets the difference that is currently processed.
    /// </summary>
    protected Difference Difference { get; private set; }

    /// <summary>
    /// Gets the comparison and upgrade hints.
    /// </summary>
    protected HintSet Hints { get; private set; }

    /// <summary>
    /// Gets the source model to compare.
    /// </summary>
    protected IModel SourceModel { get; private set; }

    /// <summary>
    /// Gets the target model to compare.
    /// </summary>
    protected IModel TargetModel { get; private set; }

    /// <summary>
    /// Gets the current model.
    /// </summary>
    protected IModel CurrentModel { get; private set; }

    #endregion

    /// <inheritdoc/>
    /// <exception cref="ArgumentOutOfRangeException"><c>hints.SourceModel</c> or <c>hints.TargetModel</c>
    /// is out of range.</exception>
    public ReadOnlyList<NodeAction> GetUpgradeSequence(Difference difference, HintSet hints)
    {
      return GetUpgradeSequence(difference, hints);
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentOutOfRangeException"><c>hints.SourceModel</c> or <c>hints.TargetModel</c>
    /// is out of range.</exception>
    /// <exception cref="InvalidOperationException">Upgrade sequence validation has failed.</exception>
    public ReadOnlyList<NodeAction> GetUpgradeSequence(Difference difference, HintSet hints, Comparer comparer)
    {
      ArgumentValidator.EnsureArgumentNotNull(difference, "difference");
      ArgumentValidator.EnsureArgumentNotNull(hints, "hints");
      SourceModel = (IModel) difference.Source;
      TargetModel = (IModel) difference.Target;
      Hints = hints ?? new HintSet(SourceModel, TargetModel);
      if (Hints.SourceModel!=SourceModel)
        throw new ArgumentOutOfRangeException("hints.SourceModel");
      if (Hints.TargetModel!=TargetModel)
        throw new ArgumentOutOfRangeException("hints.TargetModel");
      CurrentModel = (IModel) LegacyBinarySerializer.Instance.Clone(SourceModel);
      Difference = difference;
      var previous = current;
      current = this;
      actions = new List<NodeAction>();
      using (NullActionHandler.Instance.Activate()) {
        try {
          Visit(Difference);
          if (comparer!=null) {
            var diff = comparer.Compare(CurrentModel, TargetModel);
            if (diff!=null) {
              Log.InfoRegion(Strings.LogAutomaticUpgradeSequenceValidation);
              Log.Info(Strings.LogValidationFailed);
              Log.Info(Strings.LogItemFormat, Strings.Difference);
              Log.Info("{0}", diff);
              Log.Info(Strings.LogItemFormat, Strings.ExpectedTargetModel);
              TargetModel.Dump();
              Log.Info(Strings.LogItemFormat, Strings.ActualTargetModel);
              CurrentModel.Dump();
              throw new InvalidOperationException(Strings.ExUpgradeSequenceValidationFailure);
            }
          }
          return new ReadOnlyList<NodeAction>(actions, true);
        }
        finally {
          current = previous;
          actions = null;
        }
      }
    }

    /// <summary>
    /// Visitor dispatcher.
    /// </summary>
    /// <param name="difference">The difference to visit.</param>
    /// <returns>
    /// A sequence of actions that must be performed to upgrade
    /// from <see cref="IDifference.Source"/> of the specified
    /// difference to its <see cref="IDifference.Target"/>.
    /// </returns>
    protected void Visit(Difference difference)
    {
      if (difference==null)
        return;
      using (CreateContext().Activate()) {
        Context.Difference = difference;
        if (difference is NodeDifference)
          VisitNodeDifference((NodeDifference) difference);
        else if (difference is NodeCollectionDifference)
          VisitNodeCollectionDifference((NodeCollectionDifference) difference);
        else if (difference is ValueDifference)
          VisitValueDifference((ValueDifference) difference);
        else
          VisitUnknownDifference(difference);
      }
    }

    /// <summary>
    /// Visits the node difference.
    /// </summary>
    /// <param name="difference">The difference.</param>
    /// <returns>
    /// A sequence of actions that must be performed to upgrade
    /// from <see cref="IDifference.Source"/> of the specified
    /// difference to its <see cref="IDifference.Target"/>.
    /// </returns>
    protected virtual void VisitNodeDifference(NodeDifference difference)
    {
      var source = difference.Source;
      var target = difference.Target;

      // Processing movement
      if (difference.MovementInfo.IsRemoved) {
        Append(new RemoveNodeAction() {
          Path = (source ?? target).Path
        });
        if (source!=null)
          return;
      }

      if (difference.MovementInfo.IsCreated) {
        var action = new CreateNodeAction()
          {
            Path = target.Parent==null ? string.Empty : target.Parent.Path,
            Type = target.GetType(),
            Name = target.Name,
          };
        if (target.Nesting.IsNestedToCollection) {
          var collection = (NodeCollection) target.Nesting.PropertyValue;
          action.AfterPath = target.Index==0 ? collection.Path : collection[target.Index - 1].Path;
        }
        Append(action);
      }
      else if (!difference.MovementInfo.IsUnchanged) {
        Append(new MoveNodeAction()
          {
            Path = source.Path,
            Parent = target.Parent==null ? string.Empty : target.Parent.Path,
            Name = target.Name,
            Index = target.Index,
            NewPath = target.Path
          });
      }

      // And property changes
      foreach (var pair in difference.PropertyChanges)
        using (CreateContext().Activate()) {
          Context.Property = pair.Key;
          Visit(pair.Value);
        }
    }

    /// <summary>
    /// Visits the node collection difference.
    /// </summary>
    /// <param name="difference">The difference.</param>
    /// <returns>
    /// A sequence of actions that must be performed to upgrade
    /// from <see cref="IDifference.Source"/> of the specified
    /// difference to its <see cref="IDifference.Target"/>.
    /// </returns>
    protected virtual void VisitNodeCollectionDifference(NodeCollectionDifference difference)
    {
      foreach (var newDifference in difference.ItemChanges)
        Visit(newDifference);
    }

    /// <summary>
    /// Visits the value difference.
    /// </summary>
    /// <param name="difference">The difference.</param>
    /// <returns>
    /// A sequence of actions that must be performed to upgrade
    /// from <see cref="IDifference.Source"/> of the specified
    /// difference to its <see cref="IDifference.Target"/>.
    /// </returns>
    protected virtual void VisitValueDifference(ValueDifference difference)
    {
      var parentContext = Context.Parent;
      var targetNode = ((NodeDifference) parentContext.Difference).Target;
      var action = new PropertyChangeAction() {
        Path = targetNode.Path
      };
      action.Properties.Add(parentContext.Property, PathNodeReference.Get(difference.Target));
      Append(action);
    }

    /// <summary>
    /// Visits the unknown difference.
    /// </summary>
    /// <param name="difference">The difference.</param>
    /// <returns>
    /// A sequence of actions that must be performed to upgrade
    /// from <see cref="IDifference.Source"/> of the specified
    /// difference to its <see cref="IDifference.Target"/>.
    /// </returns>
    /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
    protected virtual void VisitUnknownDifference(Difference difference)
    {
      throw new NotSupportedException();
    }

    #region Helper methods

    /// <summary>
    /// Creates new upgrade context.
    /// </summary>
    /// <returns>Newly created <see cref="UpgradeContext"/> instance.</returns>
    protected virtual UpgradeContext CreateContext()
    {
      return new UpgradeContext();
    }

    /// <summary>
    /// Appends the specified action to the action sequence that is building now.
    /// </summary>
    /// <param name="action">The action to append.</param>
    protected void Append(NodeAction action)
    {
      actions.Add(action);
      action.Execute(CurrentModel);
    }

    #endregion
  }
}