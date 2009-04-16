// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.07

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Disposing;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Modelling.Resources;

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

    /// <summary>
    /// Gets the current comparer.
    /// </summary>
    public static Comparer Current {
      get { return current; }
    }

    /// <summary>
    /// Gets the current comparison context.
    /// </summary>
    protected internal ComparisonContext Context { get; internal set; }

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
      if (typeof (Node).IsAssignableFrom(type))
        return VisitNode((Node) source, (Node) target);
      if (typeof(NodeCollection).IsAssignableFrom(type))
        return VisitNodeCollection((NodeCollection) source, (NodeCollection) target);
      return VisitUnknown(source, target);
    }

    /// <summary>
    /// Visits specified <see cref="Node"/> objects.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="target">The target.</param>
    /// <returns>
    /// Difference between <paramref name="source"/>
    /// and <paramref name="target"/> objects.
    /// <see langword="null"/>, if they're equal.
    /// </returns>
    /// <exception cref="InvalidOperationException">Both source and target are <see langword="null" />.</exception>
    /// <exception cref="NullReferenceException"><see cref="CurrentDifference"/> is not <see cref="NodeDifference"/>.</exception>
    protected virtual Difference VisitNode(Node source, Node target)
    {
      using (TryActivate(source, target, (s,t) => new NodeDifference(s,t))) {
        var context = Context;
        var difference = (NodeDifference) context.Difference;
        if (difference==null)
          throw new NullReferenceException();
        var any = source ?? target;
        if (any==null)
          throw Exceptions.InternalError(Strings.ExBothSourceAndTargetAreNull, Log.Instance);

        // Filling MovementInfo
        var mi = new MovementInfo();
        if (source==null) {
          // => target!=null
          mi.IsCreated = true;
          if (difference.Parent!=null) {
            var parentSource = difference.Parent.Source as Node;
            if (parentSource!=null && context.Property!=null &&
              parentSource.GetProperty(context.Property.Name)!=null)
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
        bool isReferenceComparison = context.IsReferenceComparison;
        if (isReferenceComparison && bMoved)
          return CreateFallbackValueDifference(source, target);

        // Comparing properties
        if (!mi.IsRemoved || mi.IsCreated) {
          foreach (var pair in any.PropertyAccessors) {
            var accessor = pair.Value;
            if (accessor.IgnoreInComparison)
              continue;

            var newProperty = accessor.PropertyInfo;
            using (CreateContext().Activate()) {
              Context.Property = newProperty;
              object newSource = (source==null || !accessor.HasGetter)
                ? accessor.Default : accessor.Getter(source);
              object newTarget = (target==null || !accessor.HasGetter)
                ? accessor.Default : accessor.Getter(target);

              var anyValue = newSource ?? newTarget;
              if (anyValue==null)
                continue; // Both are null

              bool isNewReferenceComparison = false;
              var newAny = anyValue as Node;
              if (newAny!=null) {
                isNewReferenceComparison = true;
                var newAnyParent = newAny.Nesting.Node;
                if (newAnyParent==source || newAnyParent==target)
                  isNewReferenceComparison = false;
              }

              using (isNewReferenceComparison ? CreateContext().Activate() : null) {
                if (isNewReferenceComparison)
                  Context.IsReferenceComparison = isNewReferenceComparison;
                var newDifference = Visit(newSource, newTarget);
                if (newDifference!=null) {
                  difference.PropertyChanges.Add(newProperty.Name, newDifference);
                  if (isReferenceComparison)
                    break; // It's enough to find a single property difference in this case
                }
              }
            }
          }
        }

        bool bChanged = bMoved || (difference.PropertyChanges.Count > 0);
        if (!bChanged)
          return null;
        if (isReferenceComparison)
          return CreateFallbackValueDifference(source, target);
        return difference;
      }
    }

    /// <summary>
    /// Visits specified <see cref="NodeCollection"/> objects.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="target">The target.</param>
    /// <returns>Difference between <paramref name="source"/> 
    /// and <paramref name="target"/> objects.
    /// <see langword="null" />, if they're equal.</returns>
    /// <exception cref="InvalidOperationException">Both source and target are <see langword="null" />.</exception>
    /// <exception cref="NullReferenceException"><see cref="CurrentDifference"/> is not <see cref="NodeCollectionDifference"/>.</exception>
    protected virtual Difference VisitNodeCollection(NodeCollection source, NodeCollection target)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(target, "target");
      using (TryActivate(source, target, (s,t) => new NodeCollectionDifference(s,t))) {
        var context = Context;
        var difference = (NodeCollectionDifference) context.Difference;
        if (difference==null)
          throw new NullReferenceException();

        if (source.Count==0 && target.Count==0)
          return null;
        var someItems = source.Count!=0 ? source : target;
        var someItem = someItems.Cast<Node>().First();

        Func<Node, Pair<Node, object>> keyExtractor = n => new Pair<Node, object>(n, n.Name);
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
          var d = (NodeDifference) Visit(item, null); //, propertyName);
          if (d!=null)
            difference.ItemChanges.Add(item.Name, d);
        }

        // Comparing common items
        foreach (var key in commonKeys) {
          var item = sourceKeyMap[key];
          var d = (NodeDifference) Visit(item, targetKeyMap[key]); // , propertyName);
          if (d!=null)
            difference.ItemChanges.Add(item.Name, d);
        }

        // Comparing target only items
        foreach (var key in targetKeys.Except(commonKeys)) {
          var item = targetKeyMap[key];
          var d = (NodeDifference) Visit(null, item);
          if (d!=null)
            difference.ItemChanges.Add(item.Name, d);
        }

        return (difference.ItemChanges.Count!=0) ? difference : null;
      }
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
      using (TryActivate(source, target, (s,t) => new ValueDifference(s,t)))
        return new ValueDifference(source, target);
    }

    #region Helper methods

    protected Difference CreateFallbackValueDifference(Node source, Node target)
    {
      using (Context.ParentDifferenceContext.Activate(false))
        return new ValueDifference(source, target);
    }

    /// <summary>
    /// Tries to create the new <see cref="ComparisonContext"/> 
    /// for the specified <paramref name="source"/>
    /// and <paramref name="target"/> objects and activate it.
    /// </summary>
    /// <typeparam name="T">The type of <paramref name="source"/> and <paramref name="target"/> objects.</typeparam>
    /// <param name="source">The source object to compare.</param>
    /// <param name="target">The target object to compare.</param>
    /// <param name="differenceGenerator">The difference generator.</param>
    /// <returns>A disposable object deactivating the context, if it was activated;
    /// otherwise, <see langword="null" />.</returns>
    protected IDisposable TryActivate<T>(T source, T target, Func<T, T, Difference> differenceGenerator)
      where T: class
    {
      var context = Context;
      if (context!=null) {
        var difference = context.Difference;
        if (difference!=null && difference.Source==source && difference.Target==target)
          return null;
      }
      context = CreateContext();
      var result = context.Activate();
      try {
        context.Difference = differenceGenerator.Invoke(source, target);
        return result;
      }
      catch {
        result.DisposeSafely();
        throw;
      }
    }

    /// <summary>
    /// Creates new comparison context.
    /// </summary>
    /// <returns>Newly created <see cref="ComparisonContext"/> instance.</returns>
    protected virtual ComparisonContext CreateContext()
    {
      return new ComparisonContext();
    }

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
      var sourceType = sourceAncestors[sourceAncestors.Count - 1];
      var targetType = targetAncestors[targetAncestors.Count - 1];
      if (sourceType.IsAssignableFrom(targetType))
        return targetType;
      if (targetType.IsAssignableFrom(sourceType))
        return sourceType;
      var commonBase = typeof (object);
      for (int i = 0; i < Math.Min(sourceAncestors.Count, targetAncestors.Count); i++) {
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