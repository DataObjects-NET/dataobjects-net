// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.07

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison.Hints;
using System.Linq;

namespace Xtensive.Modelling.Comparison
{
  /// <summary>
  /// <see cref="IUpgrader"/> implementation.
  /// </summary>
  public class Upgrader : IUpgrader
  {
    [ThreadStatic]
    private static Upgrader current;
    private Cached<IEnumerable<NodeAction>> cachedActions;

    /// <summary>
    /// Gets the current comparer.
    /// </summary>
    public static Upgrader Current {
      get { return current; }
    }

    #region IUpgrader properties

    /// <inheritdoc/>
    public HintSet Hints { get; private set; }

    /// <inheritdoc/>
    public Difference Difference { get; private set; }

    /// <inheritdoc/>
    public IEnumerable<NodeAction> Actions {
      get {
        return cachedActions.GetValue(
          _this => {
            var previous = current;
            current = this;
            try {
              return _this.Visit(Difference);
            }
            finally {
              current = previous;
            }
          },
          this);
      }
    }

    #endregion

    /// <summary>
    /// Visitor dispatcher.
    /// </summary>
    /// <param name="difference">The difference to visit.</param>
    /// <returns>
    /// A sequence of actions that must be performed to upgrade
    /// from <see cref="IDifference.Source"/> of the specified
    /// difference to its <see cref="IDifference.Target"/>.
    /// </returns>
    protected IEnumerable<NodeAction> Visit(Difference difference)
    {
      if (difference==null)
        return EnumerableUtils<NodeAction>.Empty;
      if (difference is NodeDifference)
        return VisitNodeDifference((NodeDifference) difference);
      if (difference is NodeCollectionDifference)
        return VisitNodeCollectionDifference((NodeCollectionDifference) difference);
      if (difference is ValueDifference)
        return VisitValueDifference((ValueDifference) difference);
      return VisitUnknownDifference(difference);
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
    protected virtual IEnumerable<NodeAction> VisitNodeDifference(NodeDifference difference)
    {
      var sequence = new List<NodeAction>();
      var source = difference.Source;
      var target = difference.Target;

      // Processing movement
      if (difference.MovementInfo.IsRemoved) {
        sequence.Add(
          new RemoveNodeAction() {Path = (source ?? target).Path});
        if (source!=null)
          return sequence;
      }

      if (difference.MovementInfo.IsCreated) {
        var cna = new CreateNodeAction()
          {
            Path = target.Parent==null ? string.Empty : target.Parent.Path,
            Type = target.GetType(),
            Name = target.Name,
          };
        if (target.Nesting.IsNestedToCollection) {
          var collection = (NodeCollection) target.Nesting.PropertyValue;
          cna.AfterPath = target.Index==0 ? collection.Path : collection[target.Index - 1].Path;
        }
        sequence.Add(cna);
      }
      else if (!difference.MovementInfo.IsUnchanged) {
        sequence.Add(new MoveNodeAction()
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
        sequence.AddRange(Visit(pair.Value));

      return sequence;
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
    protected virtual IEnumerable<NodeAction> VisitNodeCollectionDifference(NodeCollectionDifference difference)
    {
      var sequence = new List<NodeAction>();
      foreach (var newDifference in difference.ItemChanges)
        sequence.AddRange(Visit(newDifference));
      return sequence;
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
    protected virtual IEnumerable<NodeAction> VisitValueDifference(ValueDifference difference)
    {
      var targetNode = ((NodeDifference) difference.Parent).Target;
      var pca = new PropertyChangeAction() {Path = targetNode.Path};
      pca.Properties.Add(
        ((IHasPropertyChanges) difference.Parent).PropertyChanges
          .Where(kv => kv.Value==difference)
          .Select(kv => kv.Key)
          .First(), 
        PathNodeReference.Get(difference.Target));

      return EnumerableUtils<NodeAction>.One(pca);
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
    protected virtual IEnumerable<NodeAction> VisitUnknownDifference(Difference difference)
    {
      throw new NotSupportedException();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="difference">The difference.</param>
    /// <param name="hints">The hints.</param>
    /// <exception cref="ArgumentOutOfRangeException"><c>hints.SourceModel</c> or <c>hints.TargetModel</c>
    /// is out of range.</exception>
    public Upgrader(Difference difference, HintSet hints)
    {
      ArgumentValidator.EnsureArgumentNotNull(difference, "difference");
      ArgumentValidator.EnsureArgumentNotNull(hints, "hints");
      var sourceModel = (IModel) difference.Source;
      var targetModel = (IModel) difference.Target;
      if (hints.SourceModel!=sourceModel)
        throw new ArgumentOutOfRangeException("hints.SourceModel");
      if (hints.TargetModel!=targetModel)
        throw new ArgumentOutOfRangeException("hints.TargetModel");
      Difference = difference;
      Hints = hints;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="difference">The difference.</param>
    public Upgrader(Difference difference)
      : this(difference, new HintSet((IModel) difference.Source, (IModel) difference.Target))
    {
    }
  }
}