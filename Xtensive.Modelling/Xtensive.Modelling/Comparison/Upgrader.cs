// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.07

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Disposing;
using Xtensive.Core.Serialization.Binary;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Modelling.Resources;
using System.Linq;
using Xtensive.Core.Reflection;

namespace Xtensive.Modelling.Comparison
{
  /// <summary>
  /// <see cref="IUpgrader"/> implementation.
  /// </summary>
  public class Upgrader : IUpgrader
  {
    [ThreadStatic]
    private static Upgrader current;

    #region Properties: Current, Context, Difference, Hints, XxxModel, Actions

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
      using (NullActionHandler.Instance.Activate()) {
        try {
          var result = Visit(Difference);
          if (comparer!=null) {
            var diff = comparer.Compare(CurrentModel, TargetModel);
            if (diff!=null) {
              Log.InfoRegion(Strings.LogAutomaticUpgradeSequenceValidation);
              Log.Info(Strings.LogValidationFailed);
              Log.Info(Strings.LogItemFormat, Strings.Difference);
              Log.Info("{0}", diff);
              Log.Info(Strings.LogItemFormat+"\r\n{1}", Strings.UpgradeSequence, 
                new ActionSequence() {result});
              Log.Info(Strings.LogItemFormat, Strings.ExpectedTargetModel);
              TargetModel.Dump();
              Log.Info(Strings.LogItemFormat, Strings.ActualTargetModel);
              CurrentModel.Dump();
              throw new InvalidOperationException(Strings.ExUpgradeSequenceValidationFailure);
            }
          }
          return new ReadOnlyList<NodeAction>(result.Actions, true);
        }
        finally {
          current = previous;
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
    protected GroupingNodeAction Visit(Difference difference)
    {
      if (difference==null)
        return null;
      UpgradeContext ctx = null;
      using (CreateContext().Activate()) {
        ctx = Context;
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
      return ctx.Actions;
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
      var any = target ?? source;
      bool mustClone = Context.MustClone;

      using (OpenActionGroup((target ?? source).Name)) {
        // Processing movement
        if (mustClone) {
          bool isSourceRemoved = false;
          if ((difference.MovementInfo & MovementInfo.Removed)!=0) {
            AddAction(UpgradeActionType.PreCondition, 
              new RemoveNodeAction() {
                Path = source.Path
              });
            isSourceRemoved = true;
          }
          if (difference.HasChanges) {
            if ((difference.MovementInfo & MovementInfo.Created)==0)
              if (!isSourceRemoved || source.Path!=target.Path)
                AddAction(UpgradeActionType.PreCondition, 
                  new RemoveNodeAction() {
                    Path = target.Path
                  });
            var action = new CloneNodeAction() {
              Source = target,
              Path = target.Parent==null ? string.Empty : target.Parent.Path,
              Name = target.Name,
              Index = target.Nesting.IsNestedToCollection ? (int?) target.Index : null
            };
            AddAction(UpgradeActionType.PostCondition, action);
          }
        }
        else {
          if ((difference.MovementInfo & MovementInfo.Removed)!=0) {
            AddAction(UpgradeActionType.PreCondition, 
              new RemoveNodeAction() {
                Path = source.Path
              });
            if (target==null)
              return;
          }
          if ((difference.MovementInfo & MovementInfo.Created)!=0) {
            var action = new CreateNodeAction() {
              Path = target.Parent==null ? string.Empty : target.Parent.Path,
              Type = target.GetType(),
              Name = target.Name,
              Index = target.Nesting.IsNestedToCollection ? (int?) target.Index : null
            };
            AddAction(UpgradeActionType.PostCondition, action);
          }
          else if ((difference.MovementInfo & MovementInfo.Changed)!=0) {
            AddAction(UpgradeActionType.PostCondition,
              new MoveNodeAction() {
                Path = source.Path,
                Parent = target.Parent==null ? string.Empty : target.Parent.Path,
                Name = target.Name,
                Index = target.Index,
                NewPath = target.Path
              });
          }
        }

        // Processing property changes
        foreach (var pair in difference.PropertyChanges) {
          var accessor = any.PropertyAccessors[pair.Key];
          if (!mustClone || accessor.IgnoreInCloning)
            using (CreateContext().Activate()) {
              Context.Property = pair.Key;
              Context.MustClone = accessor.IsCloningRoot;
              Context.DependencyRootType = accessor.DependencyRootType;
              Visit(pair.Value);
            }
        }
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
      using (OpenActionGroup("+" + Context.Property)) {
        foreach (var newDifference in difference.ItemChanges)
          Visit(newDifference);
      }
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
      AddAction(UpgradeActionType.PostCondition, action);
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
    /// Creates the new action group.
    /// </summary>
    /// <param name="comment">The action group comment.</param>
    /// <returns>A disposable deactivating the group.</returns>
    protected IDisposable OpenActionGroup(string comment)
    {
      var oldActions = new {Context.Actions, PreActions = Context.PreConditions, PostActions = Context.PostConditions};
      Context.Actions = new GroupingNodeAction {
        Comment = comment
      };
      Context.PreConditions = new GroupingNodeAction {
        Comment = "*PreConditions"
      };
      Context.PostConditions = new GroupingNodeAction {
        Comment = "*PostConditions"
      };
      try {
        return new Disposable( 
          (isDisposing) => {
            var postConditions = Context.PostConditions;
            var actions = Context.GetMergeActions();
            Context.Actions = oldActions.Actions;
            Context.PreConditions = oldActions.PreActions;
            Context.PostConditions = oldActions.PostActions;
            if (actions!=null && actions.Actions.Count!=0)
              Context.Actions.Actions.Add(actions);
            postConditions.Execute(CurrentModel);
          });
      }
      catch {
        Context.Actions = oldActions.Actions;
        Context.PreConditions = oldActions.PreActions;
        Context.PostConditions = oldActions.PostActions;
        throw;
      }
    }

    /// <summary>
    /// Appends the specified action to the action sequence that is building now.
    /// </summary>
    /// <param name="actionType">Type of the action.</param>
    /// <param name="action">The action to append.</param>
    /// <exception cref="ArgumentOutOfRangeException"><c>actionType</c> is out of range.</exception>
    /// <exception cref="InvalidOperationException">Invalid <c>Context.DependencyRootType</c>.</exception>
    protected void AddAction(UpgradeActionType actionType, NodeAction action)
    {
      var dependencyRootType = Context.DependencyRootType;
      if (dependencyRootType==null || actionType==UpgradeActionType.Regular) {
        Context.Actions.Actions.Add(action);
        action.Execute(CurrentModel);
      }
      else {
        foreach (var ctx in EnumerableUtils.Unfold(Context, c => c.Parent)) {
          var difference = ctx.Difference;
          var any = difference.Target ?? difference.Source;
          var type = any==null ? typeof (object) : any.GetType();
          if (type==dependencyRootType) {
            switch (actionType) {
            case UpgradeActionType.PreCondition:
              ctx.PreConditions.Actions.Add(action);
              action.Execute(CurrentModel);
              break;
            case UpgradeActionType.PostCondition:
              ctx.PostConditions.Actions.Add(action);
              break;
            default:
              throw new ArgumentOutOfRangeException("actionType");
            }
            return;
          }
        }
        throw new InvalidOperationException(string.Format(
          Strings.ExDifferenceRelatedToXTypeIsNotFoundOnTheUpgradeContextStack, 
          dependencyRootType.GetShortName()));
      }
    }

    #endregion
    }
  }