// Copyright (C) 2003-2010 Xtensive LLC.
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
using Xtensive.Helpers;
using Xtensive.Internals.DocTemplates;
using Xtensive.Threading;
using Xtensive.Resources;

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
    private static ThreadSafeCached<HintSet> cachedEmpty = 
      ThreadSafeCached<HintSet>.Create(new object());

    /// <summary>
    /// Gets the empty <see cref="HintSet"/>.
    /// </summary>
    public static HintSet Empty {
      get {
        return cachedEmpty.GetValue(() => {
          var hs = new HintSet();
          hs.Lock(true);
          return hs;
        });
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
    long ICountable.Count {
      get { return Count; }
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
          
          if (!hintMap.ContainsKey(node))
            hintMap.Add(node, new Dictionary<Type, object>());
          var nodeHintMap = hintMap[node];
          var hintType = hint.GetType();
          
          if (!nodeHintMap.ContainsKey(hintType))
            nodeHintMap.Add(hintType, null);
          
          var hintOrList = nodeHintMap[hintType];
          if (hintOrList==null)
            nodeHintMap[hintType] = hint;
          else {
            var list = hintOrList as List<Hint>;
            if (list==null) {
              var oldHint = (Hint) hintOrList;
              nodeHintMap[hintType] = new List<Hint>(new[] {oldHint, hint});
            }
            else
              list.Add(hint);
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
    public THint GetHint<THint>(Node node)
      where THint : Hint
    {
      ArgumentValidator.EnsureArgumentNotNull(node, "node");

      if (!hintMap.ContainsKey(node))
        hintMap.Add(node, new Dictionary<Type, object>());
      var nodeHintMap = hintMap.GetValueOrDefault(node);
      if (nodeHintMap==null)
        return null;
      var hintType = typeof(THint);
      var hintOrList = nodeHintMap.GetValueOrDefault(hintType);
      if (hintOrList==null)
        return null;
      var hint = hintOrList as THint;
      if (hint!=null)
        return hint;
      throw new InvalidOperationException(Strings.ExMultipleHintsFound);
    }

    /// <inheritdoc/>
    public THint[] GetHints<THint>(Node node)
      where THint : Hint
    {
      ArgumentValidator.EnsureArgumentNotNull(node, "node");

      if (!hintMap.ContainsKey(node))
        hintMap.Add(node, new Dictionary<Type, object>());
      var nodeHintMap = hintMap.GetValueOrDefault(node);
      if (nodeHintMap==null)
        return ArrayUtils<THint>.EmptyArray;
      var hintType = typeof (THint);
      var hintOrList = nodeHintMap.GetValueOrDefault(hintType);
      if (hintOrList==null)
        return ArrayUtils<THint>.EmptyArray;
      var hint = hintOrList as THint;
      if (hint!=null)
        return new[] {hint};
      return ((List<Hint>) hintOrList).Cast<THint>().ToArray();
    }

    /// <summary>
    /// Determines whether there are any hints associated with the specified.
    /// </summary>
    /// <param name="node">The node to check.</param>
    /// <returns>
    /// <see langword="true"/> if the specified node has associated hints; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool HasHints(Node node)
    {
      ArgumentValidator.EnsureArgumentNotNull(node, "node");

      if (!hintMap.ContainsKey(node))
        hintMap.Add(node, new Dictionary<Type, object>());
      var nodeHintMap = hintMap.GetValueOrDefault(node);
      if (nodeHintMap==null)
        return false;

      return nodeHintMap.Values.Count > 0;
    }

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
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
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