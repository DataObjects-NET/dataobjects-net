// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.07

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Modelling.Attributes;
using Xtensive.Modelling.Comparison.Hints;


namespace Xtensive.Modelling.Comparison
{
  /// <summary>
  /// Implementation of <see cref="IComparer"/> for <see cref="Node"/> comparison.
  /// </summary>
  public class Comparer : IComparer
  {
    private static readonly AsyncLocal<Comparer> currentAsync = new AsyncLocal<Comparer>();

    #region Properties: Current, Context, Source, Target, Hints

    /// <summary>
    /// Gets the current comparer.
    /// </summary>
    public static Comparer Current { get { return currentAsync.Value; } }

    /// <summary>
    /// Gets the current comparison context.
    /// </summary>
    protected internal ComparisonContext Context { get; internal set; }

    /// <summary>
    /// Gets the source model.
    /// </summary>
    protected IModel Source { get; private set; }

    /// <summary>
    /// Gets the target model.
    /// </summary>
    protected IModel Target { get; private set; }

    /// <summary>
    /// Gets the comparison hints.
    /// </summary>
    protected HintSet Hints { get; private set; }

    /// <summary>
    /// Gets the dictionary of all already found differences of objects
    /// from <see cref="Source"/> and <see cref="Target"/> models.
    /// Maps objects from both <see cref="Source"/> and <see cref="Target"/>
    /// to their <see cref="Difference"/>.
    /// </summary>
    protected Dictionary<object, Difference> Results { get; private set; }

    /// <summary>
    /// Gets the current comparison stage.
    /// </summary>
    protected ComparisonStage Stage { get; private set; }

    #endregion

    /// <inheritdoc/>
    public Difference Compare(IModel source, IModel target)
    {
      return Compare(source, target, null);
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentOutOfRangeException"><c>hints.SourceModel</c> or <c>hints.TargetModel</c>
    /// is out of range.</exception>
    public Difference Compare(IModel source, IModel target, HintSet hints)
    {
      Source = source;
      Target = target;
      Hints = hints ?? new HintSet(Source, Target);
      if (Hints.SourceModel!=Source)
        throw new ArgumentOutOfRangeException("hints.SourceModel");
      if (Hints.TargetModel!=Target)
        throw new ArgumentOutOfRangeException("hints.TargetModel");
      var previous = currentAsync.Value;
      currentAsync.Value = this;
      Results = new Dictionary<object, Difference>();
      try {
        Stage = ComparisonStage.BaseComparison;
        Visit(Source, Target);
        CoreLog.Info("Base comparison complete.");
        Stage = ComparisonStage.ReferenceComparison;
        return Visit(Source, Target);
      }
      finally {
        currentAsync.Value = previous;
        Results = null;
      }
    }

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
      return VisitObject(source, target);
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
    /// <exception cref="NullReferenceException">Current difference is not <see cref="NodeDifference"/>.</exception>
    protected virtual Difference VisitNode(Node source, Node target)
    {
      using (TryActivate(source, target, (s, t) => new NodeDifference(s, t))) {
        IgnoreHint ignoreHint = null;
        if (source!=null)
          ignoreHint = Hints.GetHint<IgnoreHint>(source);
        if (ignoreHint!=null)
          return null;

        var context = Context;
        var difference = (NodeDifference) context.Difference;
        if (difference==null)
          throw new NullReferenceException();
        var any = source ?? target;
        if (any==null)
          throw Exceptions.InternalError(Strings.ExBothSourceAndTargetAreNull, CoreLog.Instance);

        
        bool isNewDifference = TryRegisterDifference(source, target, difference);
        if (isNewDifference) {
          // Build movement info
          difference.MovementInfo = BuildMovementInfo(source, target);
          // Detect data changes
          difference.IsDataChanged = HasDataChangeHint(difference.Source);
        }

        difference.PropertyChanges.Clear();
       
        // Compare properties
        CompareProperties(source, target, difference);

        // Check if remove on cleanup
        if (difference.IsRemoved) {
          var nodeProperties = GetPropertyDifferences(difference);
          difference.IsRemoveOnCleanup = HasDependencies(source) ||
            nodeProperties.Any(nodeProperty => nodeProperty.IsRemoveOnCleanup);
        }
        var recreated = MovementInfo.Created | MovementInfo.Removed;
        if (Stage == ComparisonStage.ReferenceComparison && source != null && target != null)
          if ((difference.MovementInfo & recreated) != recreated) {
            var propertyAccessors = difference.PropertyChanges
              .Where(p => p.Value.Source != null && p.Value.Target != null)
              .Select(p => new {Pair = p, NodeDifference = p.Value as NodeDifference})
              .Where(a => a.NodeDifference != null && !a.NodeDifference.IsNameChanged)
              .Select(a => source.PropertyAccessors[a.Pair.Key])
              .ToList();
            if (propertyAccessors.Count != 0 && propertyAccessors.Any(propertyAccessor => propertyAccessor.RecreateParent))
              difference.MovementInfo = recreated;
          }

        return difference.HasChanges ? difference : null;
      }
    }

    /// <summary>
    /// Builds the <see cref="MovementInfo"/> by specific source and target nodes.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="target">The target.</param>
    /// <returns>Movement info.</returns>
    protected virtual MovementInfo BuildMovementInfo(Node source, Node target)
    {
      MovementInfo movementInfo = 0;
      var context = Context;
      var difference = Context.Difference;
      // Filling MovementInfo
      if (source==null) {
        // => target!=null
        movementInfo |= MovementInfo.Created;
        if (difference.Parent!=null) {
          var parentSource = difference.Parent.Source as Node;
          if (parentSource!=null && context.PropertyAccessor!=null &&
            parentSource.GetProperty(context.PropertyAccessor.PropertyInfo.Name)!=null)
            movementInfo |= MovementInfo.Removed; // = recreated
        }
        return movementInfo;
      }

      if (target==null) {
        movementInfo |= MovementInfo.Removed;
        return movementInfo;
      }

      var sc = StringComparer.OrdinalIgnoreCase;

      // both source!=null && target!=null
      if (!(source is IUnnamedNode) && sc.Compare(source.Name, target.Name) != 0 && sc.Compare(GetTargetPath(source), target.Path) == 0)
        movementInfo |= MovementInfo.NameChanged;
      var collection = target.Nesting.PropertyValue as NodeCollection;
      if (source.Index!=target.Index &&
        !(collection!=null && (collection is IUnorderedNodeCollection))) // TODO: Fix this!
        movementInfo |= MovementInfo.IndexChanged;
      var pdc = context.GetParentDifferenceContext<NodeDifference>();
      if (pdc!=null) {
        var parentDifference = (NodeDifference) pdc.Difference;
        if (source.Parent!=parentDifference.Source || target.Parent!=parentDifference.Target)
          movementInfo |= MovementInfo.ParentChanged;
        var parentMovementInfo = parentDifference.MovementInfo;
        if ((parentMovementInfo & MovementInfo.Relocated)!=0)
          movementInfo |= MovementInfo.ParentRelocated;
      }
      return movementInfo;
    }

    /// <summary>
    /// Compares source and target node properties.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="target">The target.</param>
    /// <param name="difference">The difference.</param>
    protected virtual void CompareProperties(Node source, Node target, NodeDifference difference)
    {
      var any = source ?? target;
      
      foreach (var pair in any.PropertyAccessors) {
        var accessor = pair.Value;
        if (accessor.IgnoreInComparison)
          continue;

        var property = accessor.PropertyInfo;
        using (CreateContext().Activate()) {
          Context.PropertyAccessor = accessor;
          object sourceValue = (source==null || !accessor.HasGetter)
            ? accessor.Default : accessor.Getter.Invoke(source);
          object targetValue = (target==null || !accessor.HasGetter)
            ? accessor.Default : accessor.Getter.Invoke(target);
          object anyValue = sourceValue ?? targetValue;
          if (anyValue==null)
            continue; // Both are null

          Difference propertyDifference = null;
          if (!IsReference(sourceValue, targetValue))
            propertyDifference = Visit(sourceValue, targetValue);
          else if (Stage == ComparisonStage.ReferenceComparison) {
            if (IsIgnored(sourceValue))
              difference.MovementInfo = 0;
            else
              propertyDifference = GetReferencedPropertyDifference(sourceValue, targetValue, target, property.Name);
          }

          if (propertyDifference==null)
            continue;

          if (any.Nesting.PropertyInfo != null && accessor.DependencyRootType==any.Nesting.PropertyInfo.PropertyType) {
            if (propertyDifference is NodeDifference)
              ((NodeDifference) propertyDifference).IsDependentOnParent = true;
            else if (propertyDifference is NodeCollectionDifference)
              ((NodeCollectionDifference) propertyDifference).ItemChanges
                .ForEach(item=>item.IsDependentOnParent = true);
          }
          difference.PropertyChanges.Add(property.Name, propertyDifference);
        }
      }
    }

    /// <summary>
    /// Gets the referenced property difference.
    /// </summary>
    /// <param name="sourceValue">The source value.</param>
    /// <param name="targetValue">The target value.</param>
    /// <param name="target">The target.</param>
    /// <param name="property">The property.</param>
    /// <returns>Difference.</returns>
    /// <exception cref="InvalidOperationException"><c>InvalidOperationException</c>.</exception>
    protected Difference GetReferencedPropertyDifference(object sourceValue, object targetValue, Node target, string property)
    {
      if (targetValue!=null && sourceValue!=null) {
        var isDataDependent = IsDependOnData(target);
        var isImmutable = IsImmutable(target);
        var referencedPropertyDifference = Results.GetValueOrDefault(targetValue);
        if (referencedPropertyDifference!=null)
          return HasChanges(referencedPropertyDifference, isDataDependent, isImmutable) 
            ? new ValueDifference(sourceValue, targetValue) 
            : null;

        throw new InvalidOperationException(string.Format(
        Strings.ExNodeXMustBeProcessedBeforeBeingComparedAsReferenceValueOfYZ,
        targetValue, target, property));
      }

      return new ValueDifference(sourceValue, targetValue);
    }

    /// <summary>
    /// Gets the property differences for each property of type <see cref="Node"/> or <see cref="NodeCollection"/>.
    /// </summary>
    /// <param name="difference">The difference.</param>
    /// <returns>Property differences set.</returns>
    protected IEnumerable<NodeDifference> GetPropertyDifferences(NodeDifference difference)
    {
      return
        difference.PropertyChanges.Values.OfType<NodeDifference>().Union(
          difference.PropertyChanges.Values.OfType<NodeCollectionDifference>()
          .SelectMany(collectionDifference => collectionDifference.ItemChanges));
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
    /// <exception cref="NullReferenceException">Current difference is not <see cref="NodeCollectionDifference"/>.</exception>
    protected virtual Difference VisitNodeCollection(NodeCollection source, NodeCollection target)
    {
      using (TryActivate(source, target, (s,t) => new NodeCollectionDifference(s,t))) {
        var context = Context;
        var difference = (NodeCollectionDifference) context.Difference;
        if (difference==null)
          throw new NullReferenceException();
        
        bool isNewDifference = TryRegisterDifference(source, target, difference);
        difference.ItemChanges.Clear();

        // Inlining 2 below lines leads to error in PEVerify.exe!
        // (well-known issue with null coalescing operator + cast)
        var sourceItems = (IEnumerable) source;
        var targetItems = (IEnumerable) target;

        var src = sourceItems ?? new ReadOnlyList<Node>(new Node[] {});
        var tgt = targetItems ?? new ReadOnlyList<Node>(new Node[] {});

        var srcCount = source!=null ? source.Count : 0;
        var tgtCount = target!=null ? target.Count : 0;

        if (srcCount==0 && tgtCount==0)
          return null;
        var someItems = srcCount!=0 ? src : tgt;
        var someItem = someItems.Cast<Node>().First();

        Func<Node, Pair<Node, string>> keyExtractor = 
          n => new Pair<Node, string>(n, GetNodeComparisonKey(n));

        var sourceKeyMap = src
          .Cast<Node>()
          .Select(keyExtractor)
          .ToDictionary(pair => pair.Second, pair => pair.First, StringComparer.OrdinalIgnoreCase);
        var targetKeyMap = tgt
          .Cast<Node>()
          .Select(keyExtractor)
          .ToDictionary(pair => pair.Second, pair => pair.First, StringComparer.OrdinalIgnoreCase);

        var sourceKeys = src.Cast<Node>().Select(n => keyExtractor(n).Second);
        var targetKeys = tgt.Cast<Node>().Select(n => keyExtractor(n).Second);
        var commonKeys = sourceKeys.Intersect(targetKeys, StringComparer.OrdinalIgnoreCase);

        var sequence =
          sourceKeys.Except(commonKeys, StringComparer.OrdinalIgnoreCase)
            .Select(k => new {Index = sourceKeyMap[k].Index, Type = 0, 
              Source = sourceKeyMap[k], Target = (Node) null})
          .Concat(commonKeys
            .Select(k => new {Index = targetKeyMap[k].Index, Type = 1, 
              Source = sourceKeyMap[k], Target = targetKeyMap[k]}))
          .Concat(targetKeys.Except(commonKeys, StringComparer.OrdinalIgnoreCase)
            .Select(k => new {Index = targetKeyMap[k].Index, Type = 2, 
              Source = (Node) null, Target = targetKeyMap[k]}))
          .OrderBy(i => i.Type!=0).ThenBy(i => i.Index).ThenBy(i => i.Type);

        foreach (var i in sequence) {
          var d = Visit(i.Source, i.Target);
          if (d!=null)
            difference.ItemChanges.Add((NodeDifference) d);
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
    protected virtual Difference VisitObject(object source, object target)
    {
      using (TryActivate(source, target, (s, t) => new ValueDifference(s, t))) {
        var areEqual = Context.PropertyAccessor.CaseInsensitiveComparison
          ? string.Equals((string)source, (string)target, StringComparison.OrdinalIgnoreCase)
          : Equals(source, target);
        return areEqual ? null : Context.Difference;
      }
    }

    /// <summary>
    /// Determines whether the specified source is reference.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="target">The target.</param>
    /// <returns>
    /// <see langword="true"/> if the specified source is reference; otherwise, <see langword="false"/>.
    /// </returns>
    protected virtual bool IsReference(object source, object target)
    {
      var difference = Context.Difference;
      var any = (target ?? source) as Node;
      if (any!=null)
        return any.Parent!=difference.Source && any.Parent!=difference.Target;
      return false;
    }

    /// <summary>
    /// Determines whether the specified difference is relocated.
    /// </summary>
    /// <param name="difference">The difference.</param>
    /// <returns>
    ///  <see langword="true"/> if the specified difference is relocated; otherwise, <see langword="false"/>.
    /// </returns>
    protected virtual bool IsRelocated(Difference difference)
    {
      var nodeDifference = difference as NodeDifference;
      if (nodeDifference==null)
        return false;
      if ((nodeDifference.MovementInfo & MovementInfo.Relocated)==0)
        return false;
      var diffs = 
        EnumerableUtils.Unfold(difference, d => d.Parent).Reverse();
      var currentDiffs = 
        EnumerableUtils.Unfold(Context.Difference, d => d.Parent).Reverse();
      var commonDiffs = diffs.Zip(currentDiffs, (first, second) => new Pair<Difference>(first, second)).Where(p => p.First==p.Second).Select(p => p.First);
      var newDiffs = diffs.Except(commonDiffs);
      var query =
        from diff in newDiffs
        let nodeDiff = diff as NodeDifference
        where nodeDiff!=null && (nodeDiff.MovementInfo & MovementInfo.Changed)!=0
        select nodeDiff;
      // query = query.ToList();
      return query.Any();
    }

    /// <summary>
    /// Determines whether difference contains node property 
    /// with <see cref="MovementInfo"/> equals to <see cref="MovementInfo.Changed"/>.
    /// </summary>
    /// <param name="difference">The difference.</param>
    /// <returns>
    /// <see langword="true"/> if difference contains changed node properties; otherwise, <see langword="false"/>.
    /// </returns>
    protected virtual bool HasChangedNodeProperties(Difference difference)
    {
      var nodeDifference = difference as NodeDifference;
      if (nodeDifference!=null) {
        if ((nodeDifference.MovementInfo & MovementInfo.Changed)!=0)
          return true;
        if (nodeDifference.PropertyChanges.Any(pair => HasChangedNodeProperties(pair.Value)))
          return true;
      }
      var nodeCollectionDifference = difference as NodeCollectionDifference;
      return nodeCollectionDifference != null && 
        nodeCollectionDifference.ItemChanges.Any(HasChangedNodeProperties);
    }

    /// <summary>
    /// Determines whether difference contains node property 
    /// with <see cref="MovementInfo"/> equals to <see cref="MovementInfo.Changed"/>.
    /// </summary>
    /// <param name="difference">The difference.</param>
    /// <param name="isDataDependent"></param>
    /// <param name="propertyOwnerIsImmutable"></param>
    /// <returns>
    /// <see langword="true"/> if difference contains changed node properties; otherwise, <see langword="false"/>.
    /// </returns>
    protected virtual bool HasChanges(Difference difference, bool isDataDependent, bool propertyOwnerIsImmutable)
    {
      if (difference is ValueDifference)
        return true;

      var nodeDifference = difference as NodeDifference;
      if (nodeDifference!=null) {
        if (nodeDifference.MovementInfo != MovementInfo.NameChanged) {
          if ((nodeDifference.MovementInfo & MovementInfo.Relocated)!=0 || 
            (nodeDifference.MovementInfo & MovementInfo.Changed)!=0 || 
            (nodeDifference.PropertyChanges.Count!=0) || 
            (isDataDependent && nodeDifference.IsDataChanged))
          return true;
        }
        else if (propertyOwnerIsImmutable)
          return true;

        if (nodeDifference.PropertyChanges.Select(pair => pair.Value).Any(HasChangedNodeProperties))
          return true;
      }

      var nodeCollectionDifference = difference as NodeCollectionDifference;
      return nodeCollectionDifference!=null && 
        nodeCollectionDifference.ItemChanges.Any(HasChangedNodeProperties);
    }

    /// <summary>
    /// Determines whether specified node is depend on data changing.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns>
    /// <see langword="true"/> if node is depend on data changing; otherwise, <see langword="false"/>.
    /// </returns>
    protected virtual bool IsDependOnData(Node node)
    {
      if (node.GetType().GetAttribute<DataDependentAttribute>(AttributeSearchOptions.Default)!=null)
        return true;
      if (node.Parent==null)
        return false;
      return IsDependOnData(node.Parent);
    }

    /// <summary>
    /// Determines whether specified node is immutable.
    /// </summary>
    /// <returns><see langword="true"/> if the specified node is immutable; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    protected virtual bool IsImmutable(Node node)
    {
      var parent = node.Parent;
      if (parent == null)
        return true;
      var propertyName = node.Nesting.EscapedPropertyName;
      var propertyAccessor = parent.PropertyAccessors[propertyName];
      return propertyAccessor.IsImmutable;
    }
    
    /// <summary>
    /// Extracts the comparison key, that used to find associations 
    /// between old and new <see cref="NodeCollection"/> items.
    /// </summary>
    /// <param name="node">The node to get the comparison key for.</param>
    /// <returns>Comparison key for the specified node.</returns>
    protected virtual string GetNodeComparisonKey(Node node)
    {
      if (!(node is INodeReference))
        return GetTargetName(node);
      
      var targetNode = ((INodeReference) node).Value;
      return targetNode==null ? null : GetTargetPath(targetNode);
    }

    /// <summary>
    /// Determines whether the specified node has dependencies 
    /// and must be removed on <see cref="UpgradeStage.Cleanup"/>.
    /// </summary>
    /// <param name="source">The source node.</param>
    /// <returns>
    /// <see langword="true"/> if the specified source has dependencies; otherwise, <see langword="false"/>.
    /// </returns>
    protected bool HasDependencies(Node source)
    {
      return Hints.HasHints(source);
    }

    /// <summary>
    /// Determines whether <see cref="Hints"/> contains data change hints.
    /// </summary>
    /// <param name="source">The source node.</param>
    /// <returns>
    /// <see langword="true"/> if data change hints exists; otherwise, <see langword="false"/>.
    /// </returns>
    protected bool HasDataChangeHint(Node source)
    {
      if (source==null)
        return false;

      return 
        Hints.GetHints<CopyDataHint>(source).Any()
        || Hints.GetHints<DeleteDataHint>(source).Any()
        || Hints.GetHints<UpdateDataHint>(source).Any();
    }

    /// <summary>
    /// Determines whether the specified value must be ignored in comparison.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>
    /// <see langword="true"/> if the specified value must be ignored; otherwise, <see langword="false"/>.
    /// </returns>
    protected bool IsIgnored(object value)
    {
      return value is Node && Hints.GetHint<IgnoreHint>(value as Node)!=null;
    }

    #region Helper methods

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
        Context.Difference = differenceGenerator.Invoke(source, target);
        if (!(Context.Difference is ValueDifference)) {
          var difference = Results.GetValueOrDefault(target ?? source);
          if (difference!=null)
            Context.Difference = difference;
        }
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
    /// Gets the path of the target node.
    /// </summary>
    /// <param name="source">The source node.</param>
    /// <returns>The path of the target node.</returns>
    protected virtual string GetTargetPath(Node source)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      if (source.Model==Source) {
        var renameHint = Hints.GetHint<RenameHint>(source);
        if (renameHint!=null)
          return renameHint.TargetPath;
        var parent = source.Parent;
        if (parent!=null) {
          string path = source.Path;
          string parentPath = parent.Path;
          string pathTail = path.Substring(parentPath.Length);
          return GetTargetPath(parent) + pathTail;
        }
      }
      return source.Path;
    }

    /// <summary>
    /// Gets the name of the target node.
    /// </summary>
    /// <param name="source">The source node.</param>
    /// <returns>The name of the target node.</returns>
    protected virtual string GetTargetName(Node source)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      if (source.Model==Source) {
        var renameHint = Hints.GetHint<RenameHint>(source);
        if (renameHint!=null)
          return Target.Resolve(renameHint.TargetPath, true).Name;
      }
      return source.Name;
    }

    private bool TryRegisterDifference(object source, object target, Difference difference)
    {
      if (Results.ContainsKey(target ?? source))
        return false;
      if (source!=null)
        Results.Add(source, difference);
      if (target!=null)
        Results.Add(target, difference);
      return true;
    }

    /// <summary>
    /// Gets the highest common base type.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <param name="target">The target object.</param>
    /// <returns>The highest common base type.</returns>
    protected static Type GetCommonBase(object source, object target)
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

    private static List<Type> GetAncestors(Type type)
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
  }
}