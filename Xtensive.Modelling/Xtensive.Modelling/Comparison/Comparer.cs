// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.07

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Disposing;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Modelling.Comparison.Hints;

namespace Xtensive.Modelling.Comparison
{
  /// <summary>
  /// Abstract base class for <see cref="IComparer"/> implementation.
  /// </summary>
  public abstract class Comparer : IComparer
  {
    [ThreadStatic]
    private static Comparer current;
    private Cached<Difference> cachedDifference;
    private Difference currentDifference;

    /// <summary>
    /// Gets the current comparer.
    /// </summary>
    public static Comparer Current {
      get { return current; }
    }
    
    /// <summary>
    /// Gets the current difference.
    /// </summary>
    public Difference CurrentDifference {
      get { return currentDifference; }
    }

    /// <summary>
    /// Gets a value indicating whether reference comparison is performed now.
    /// </summary>
    protected bool IsReferenceComparison { get; private set; }

    /// <summary>
    /// Opens reference comparison region.
    /// </summary>
    /// <returns>A disposable object closing the region.</returns>
    protected IDisposable OpenReferenceComparisonRegion()
    {
      bool oldValue = IsReferenceComparison;
      IsReferenceComparison = true;
      return new Disposable<Comparer, bool>(this, oldValue,
        (isDisposing, _this, _OldValue) => _this.IsReferenceComparison = oldValue);
    }

    #region IComparer<T> properties

    /// <inheritdoc/>
    public IModel Source { get; private set; }

    /// <inheritdoc/>
    public IModel Target { get; private set; }

    /// <inheritdoc/>
    public HintSet Hints { get; private set; }

    /// <inheritdoc/>
    public Difference Difference {
      get {
        return cachedDifference.GetValue(
          _this => {
            var previous = current;
            current = this;
            try {
              return _this.Visit(_this.Source, _this.Target);
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
    /// <param name="source">The source.</param>
    /// <param name="target">The target.</param>
    /// <returns>Difference between <paramref name="source"/> 
    /// and <paramref name="target"/> objects.
    /// <see langword="null" />, if they're equal.</returns>
    protected Difference Visit(object source, object target)
    {
      return Visit(GetCommonBase(source, target), source, target);
    }

    /// <summary>
    /// Visitor dispatcher.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="source">The source.</param>
    /// <param name="target">The target.</param>
    /// <returns>Difference between <paramref name="source"/> 
    /// and <paramref name="target"/> objects.
    /// <see langword="null" />, if they're equal.</returns>
    protected virtual Difference Visit(Type type, object source, object target)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");
      if (typeof(Node).IsAssignableFrom(type))
        return VisitNode((Node) source, (Node) target);
      if (typeof(NodeCollection).IsAssignableFrom(type))
        return VisitNodeCollection((NodeCollection) source, (NodeCollection) target);
      return VisitUnknown((object) source, (object) target);
    }

    /// <summary>
    /// Visits specified <see cref="Node"/> objects.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="target">The target.</param>
    /// <returns>Difference between <paramref name="source"/> 
    /// and <paramref name="target"/> objects.
    /// <see langword="null" />, if they're equal.</returns>
    protected virtual NodeDifference VisitNode(Node source, Node target)
    {
      var difference = (NodeDifference) CurrentDifference;
      var propertyName = difference.PropertyName;

      // Filling MovementInfo
      var mi = new MovementInfo();
      if (source==null) { // => target!=null
        mi.IsCreated = true;
        if (difference.Parent!=null) {
          var parentSource = difference.Parent.Source as Node;
          if (parentSource!=null && parentSource.GetProperty(propertyName)!=null)
            mi.IsRemoved = true; // = recreated
        }
      }
      else if (target==null)
        mi.IsRemoved = true;
      else {
        // both source!=null && target!=null
        if (!(source is IUnnamedNode) && source.Name!=target.Name) {
          var renameHint = Hints.GetHint<RenameHint>(source);
          if (renameHint==null || renameHint.TargetPath!=target.Path)
            mi.IsNameChanged = true;
        }
        mi.IsIndexChanged = source.Index!=target.Index; // TODO: Fix this!
        var collection = target.Nesting.PropertyValue as NodeCollection;
        if (collection!=null && (collection is IUnorderedNodeCollection))
          mi.IsIndexChanged = false;
        var nd = difference.GetNearestParent<NodeDifference>();
        if (nd!=null) {
          if (source.Parent!=nd.Source || target.Parent!=nd.Target)
            mi.IsParentChanged = true; // TODO: Fix this!
          var ndmi = nd.MovementInfo;
          if (ndmi!=null)
            mi.IsAnyParentChanged = ndmi.IsAnyParentChanged | mi.IsParentChanged;
        }
      }
      difference.MovementInfo = mi;
      bool bMoved = !mi.IsUnchanged;
      if (!difference.IsNestedPropertyDifference && bMoved)
        using (difference.Parent.Activate())
          return new PropertyValueDifference(propertyName, source, target);

      // Comparing properties
      if (!mi.IsRemoved || mi.IsCreated) {
        foreach (var pair in source.PropertyAccessors) {
          var newPropertyName = pair.Key;
          var accessor = pair.Value;
          if (accessor.IgnoreInComparison)
            continue;

          object sourceValue = (source==null || !accessor.HasGetter) ?
            accessor.Default : accessor.Getter(source);
          object targetValue = (target==null || !accessor.HasGetter) ?
            accessor.Default : accessor.Getter(target);
          if (sourceValue==null) {
            if (targetValue==null)
              continue; // Both are null
            var dTargetValue = targetValue as IDifferentiable;
            if (dTargetValue!=null) {
              var d = dTargetValue.GetDifferenceWith(sourceValue, newPropertyName, true);
              if (d!=null)
                difference.PropertyChanges.Add(newPropertyName, d);
            }
            else
              difference.PropertyChanges.Add(newPropertyName,
                new PropertyValueDifference(newPropertyName, sourceValue, targetValue));
            continue;
          }
          var dSourceValue = sourceValue as IDifferentiable;
          if (dSourceValue!=null) {
            var d = dSourceValue.GetDifferenceWith(targetValue, newPropertyName, false);
            if (d!=null)
              difference.PropertyChanges.Add(newPropertyName, d);
          }
          else if (!Equals(sourceValue, targetValue))
            difference.PropertyChanges.Add(newPropertyName,
              new PropertyValueDifference(newPropertyName, sourceValue, targetValue));
        }
      }

      bool bChanged = bMoved || (difference.PropertyChanges.Count > 0);
      if (!bChanged)
        return null;
      if (!difference.IsNestedPropertyDifference)
        using (difference.Parent.Activate())
          return new PropertyValueDifference(propertyName, source, target);
      return difference;
    }

    /// <summary>
    /// Visits specified <see cref="NodeCollection"/> objects.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="target">The target.</param>
    /// <returns>Difference between <paramref name="source"/> 
    /// and <paramref name="target"/> objects.
    /// <see langword="null" />, if they're equal.</returns>
    protected virtual NodeCollectionDifference VisitNodeCollection(NodeCollection source, NodeCollection target)
    {
      var difference = (NodeCollectionDifference) CurrentDifference;
      string propertyName = difference.PropertyName;

      if (source.Count==0 && target.Count==0)
        return null;
      var someItems = source.Count!=0 ? source : target;
      var someItem = someItems.Cast<Node>().First();

      Func<Node, Pair<Node,object>> keyExtractor = n => new Pair<Node, object>(n,n.Name);
      if (someItem is IUnnamedNode) {
        if (someItem is INodeReference)
          keyExtractor = n => {
            var referredNode = ((INodeReference) n).Value;
            return new Pair<Node, object>(n, referredNode==null ? null : referredNode.Path);
          };
        else
          keyExtractor = n => new Pair<Node, object>(n, n.Index);
      }

      var sourceKeyMap = new Dictionary<object, Node>();
      foreach (var pair in source.Cast<Node>().Select(keyExtractor))
        sourceKeyMap.Add(pair.Second, pair.First);

      var targetKeyMap = new Dictionary<object, Node>();
      foreach (var pair in target.Cast<Node>().Select(keyExtractor))
        targetKeyMap.Add(pair.Second, pair.First);

      var sourceKeys = source.Cast<Node>().Select(n => keyExtractor(n).Second);
      var targetKeys = target.Cast<Node>().Select(n => keyExtractor(n).Second);
      var commonKeys = sourceKeys.Intersect(targetKeys);

      // Comparing source only items
      foreach (var key in sourceKeys.Except(commonKeys)) {
        var item = sourceKeyMap[key];
        var d = (NodeDifference) Visit(item, null, propertyName);
        if (d!=null)
          difference.ItemChanges.Add(item.Name, d);
      }

      // Comparing common items
      foreach (var key in commonKeys) {
        var item = sourceKeyMap[key];
        var d = (NodeDifference) Visit(item, targetKeyMap[key], propertyName);
        if (d!=null)
          difference.ItemChanges.Add(item.Name, d);
      }

      // Comparing target only items
      foreach (var key in targetKeys.Except(commonKeys)) {
        var item = targetKeyMap[key];
        var d = (NodeDifference) Visit(null, item, propertyName);
        if (d!=null)
          difference.ItemChanges.Add(item.Name, d);
      }

      return (difference.ItemChanges.Count != 0) ? difference : null;
    }

    /// <summary>
    /// Visits specified objects.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="target">The target.</param>
    /// <returns>Difference between <paramref name="source"/> 
    /// and <paramref name="target"/> objects.
    /// <see langword="null" />, if they're equal.</returns>
    protected virtual Difference VisitUnknown(object source, object target)
    {
      throw new NotImplementedException();
    }

    #region Helper methods

    /// <summary>
    /// Gets the highest common base type.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <param name="target">The target object.</param>
    /// <returns>The highest common base type.</returns>
    protected Type GetCommonBase(object source, object target)
    {
      var sourceAncestors = GetAncestors(source==null ? typeof (object) : source.GetType());
      var targetAncestors = GetAncestors(target==null ? typeof (object) : target.GetType());
      var commonBase = typeof (object);
      for (int i = 0; i < sourceAncestors.Count; i++) {
        var ancestor = sourceAncestors[i];
        if (ancestor!=targetAncestors[i])
          break;
        commonBase = ancestor;
      }
      return commonBase;
    }

    private List<Type> GetAncestors(Type type)
    {
      var list = new List<Type>();
      while (type!=typeof(object)) {
        list.Insert(0, type);
        type = type.BaseType;
      }
      list.Insert(0, typeof(object));
      return list;
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="target">The target.</param>
    public Comparer(IModel source, IModel target)
    {
      Source = source;
      Target = target;
      Hints = new HintSet(source, target);
    }
  }
}