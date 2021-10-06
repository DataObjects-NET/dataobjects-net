// Copyright (C) 2009-2021 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.26

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Xtensive.Collections;
using System.Linq;
using Xtensive.Core;

namespace Xtensive.Modelling.Comparison.Hints
{
  /// <summary>
  /// <see cref="Hint"/> set.
  /// </summary>
  [Serializable]
  [DebuggerDisplay("Count = {Count}")]
  public class HintSet : LockableBase,
    IHintSet
  {
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static Lazy<HintSet> cachedEmpty = new Lazy<HintSet>(() => {
      var hs = new HintSet();
      hs.Lock(true);
      return hs;
    });

    /// <summary>
    /// Gets the empty <see cref="HintSet"/>.
    /// </summary>
    public static HintSet Empty {
      get {
        return cachedEmpty.Value;
      }
    }

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    private readonly List<Hint> list = new List<Hint>();
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly HashSet<Hint> set = new HashSet<Hint>();
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Dictionary<Node, Dictionary<Type, object>> hintMap =
      new Dictionary<Node, Dictionary<Type, object>>();

    /// <summary>
    /// Gets the number of elements contained in a collection.
    /// </summary>
    /// <value></value>
    public int Count {
      get { return set.Count; }
    }

    /// <inheritdoc/>
    public IModel SourceModel { get; private set; }

    /// <inheritdoc/>
    public IModel TargetModel { get; private set; }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">One of paths returned by
    /// <see cref="Hint.GetTargets"/> method isn't found.</exception>
    public void Add(Hint hint)
    {
      ArgumentValidator.EnsureArgumentNotNull(hint, "hint");

      if (set.Contains(hint))
        throw new InvalidOperationException(Strings.ExItemAlreadyExists);

      try {
        var targets = hint.GetTargets().ToList();
        var nodes = new List<Node>();
        foreach (var target in targets) {
          Node node;
          if (target.Model==ModelType.Source)
            node = (Node) SourceModel.Resolve(target.Path, true);
          else
            node = (Node) TargetModel.Resolve(target.Path, true);
          nodes.Add(node);

          var nodeHintMap = GetNodeHints(node);
          var hintType = hint.GetType();
          
          if (nodeHintMap.TryGetValue(hintType, out var hintOrList)) {
            if (hintOrList is List<Hint> list) {
              list.Add(hint);
            }
            else {
              nodeHintMap[hintType] = new List<Hint>(new[] { (Hint) hintOrList, hint });
            }
          }
          else {
            nodeHintMap.Add(hintType, hint);
          }
        }
      }
      catch (Exception) {
        var oldHints = list.ToList(); // Cloning  the list
        Clear();
        foreach (var oldHint in oldHints)
          Add(oldHint);
        throw;
      }
      set.Add(hint);
      list.Add(hint);
    }

    /// <inheritdoc/>
    public void Clear()
    {
      set.Clear();
      list.Clear();
      hintMap.Clear();
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Multiple hints found.</exception>
    public THint GetHint<THint>(Node node) where THint : Hint =>
      GetNodeHints(node).GetValueOrDefault(typeof(THint)) switch {
        null => null,
        THint hint => hint,
        _ => throw new InvalidOperationException(Strings.ExMultipleHintsFound)
      };

    /// <inheritdoc/>
    public THint[] GetHints<THint>(Node node) where THint : Hint =>
      GetNodeHints(node).GetValueOrDefault(typeof(THint)) switch {
        null => Array.Empty<THint>(),
        THint hint => new[] { hint },
        var list => ((List<Hint>) list).Cast<THint>().ToArray()
      };

    /// <summary>
    /// Determines whether there are any hints associated with the specified.
    /// </summary>
    /// <param name="node">The node to check.</param>
    /// <returns>
    /// <see langword="true"/> if the specified node has associated hints; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool HasHints(Node node) => GetNodeHints(node).Count > 0;

    public bool HasHints<THint>(Node node) where THint : Hint =>
      GetNodeHints(node).ContainsKey(typeof(THint));

    #region IEnumerable<...> methods

    /// <inheritdoc/>
    public IEnumerator<Hint> GetEnumerator()
    {
      return list.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion

    private Dictionary<Type, object> GetNodeHints(Node node)
    {
      ArgumentValidator.EnsureArgumentNotNull(node, "node");

      if (!hintMap.TryGetValue(node, out var nodeHintMap)) {
        hintMap.Add(node, nodeHintMap = new Dictionary<Type, object>());
      }
      return nodeHintMap;
    }

    #region ILockable methods

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (recursive)
        foreach (var hint in list) {
          var lockable = hint as ILockable;
          if (lockable!=null)
            lockable.Lock(recursive);
        }
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Join(Environment.NewLine, 
        list.Select(hint => hint.ToString()).ToArray());
    }


    // Constructors

    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="sourceModel">The source model.</param>
    /// <param name="targetModel">The target model.</param>
    public HintSet(IModel sourceModel, IModel targetModel)
    {
      ArgumentValidator.EnsureArgumentNotNull(sourceModel, "sourceModel");
      ArgumentValidator.EnsureArgumentNotNull(targetModel, "targetModel");
      SourceModel = sourceModel;
      TargetModel = targetModel;
    }

    private HintSet()
    {
    }
  }
}
