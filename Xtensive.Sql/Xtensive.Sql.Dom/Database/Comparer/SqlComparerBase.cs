// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.18

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// Base class for any <see cref="ISqlComparer{T}"/>s.
  /// </summary>
  /// <typeparam name="T">The schema node type to compare.</typeparam>
  [Serializable]
  public abstract class SqlComparerBase<T> : ISqlComparer<T>,
    IDeserializationCallback
  {
    private ISqlComparerProvider provider;

    /// <inheritdoc/>
    public abstract ComparisonResult<T> Compare(T originalNode, T newNode, IEnumerable<ComparisonHintBase> hints);

    /// <inheritdoc/>
    public ISqlComparerProvider Provider
    {
      get { return provider; }
    }

    protected static IEnumerable<ComparisonHintBase> FindHints<THintTarget>(IEnumerable<ComparisonHintBase> hints, string objectName)
    {
      if (hints==null) {
        return Enumerable.Empty<ComparisonHintBase>();
      }
      return hints.Where(hint => hint!=null
        && hint.Name==objectName
          && hint.Type==typeof (THintTarget));
    }

    protected static IEnumerable<THintType> FindHints<THintTarget, THintType>(IEnumerable<ComparisonHintBase> hints, string objectName)
    {
      if (hints==null) {
        return Enumerable.Empty<THintType>();
      }
      return hints.Where(hint => hint!=null
        && hint.Name==objectName
          && hint.Type==typeof (THintTarget)
            && hint.GetType() == typeof(THintType)).Convert(hint => (THintType)(object)hint);
    }

    protected static bool CompareNestedNodes<TNode, TResult>(IEnumerable<TNode> originalNodes, IEnumerable<TNode> newNodes, IEnumerable<ComparisonHintBase> hints, SqlComparerStruct<TNode> comparer, ICollection<TResult> results)
      where TNode : class
      where TResult : ComparisonResult<TNode>
    {
      if (!ProcessNullNodes(originalNodes, newNodes, comparer, hints, results))
        return false;
      if (typeof (TNode).IsSubclassOf(typeof (Node)))
        return CompareUnnamedNodes(originalNodes, newNodes, comparer, hints, results);
      return CompareNamedNodes(originalNodes, newNodes, comparer, hints, results);
    }

    protected static void ProcessDbName<TNode>(TNode originalNode, TNode newNode, ComparisonResult<TNode> result)
    {
      if (result is NodeComparisonResult<TNode> && typeof (T).IsSubclassOf(typeof (Node))) {
        var nameResult = new ComparisonResult<string>();
        string originalName = ReferenceEquals(originalNode, null) ? null : ((Node) (object) originalNode).DbName;
        string newName = ReferenceEquals(newNode, null) ? null : ((Node) (object) newNode).DbName;
        nameResult.OriginalValue = originalName;
        nameResult.NewValue = newName;
        if (originalName==newName)
          nameResult.ResultType = ComparisonResultType.Unchanged;
        else if (originalName==null)
          nameResult.ResultType = ComparisonResultType.Added;
        else
          nameResult.ResultType = newName==null
            ? ComparisonResultType.Removed
            : ComparisonResultType.Modified;
        ((NodeComparisonResult<TNode>) result).DbName = nameResult;
        if (result.ResultType==ComparisonResultType.Unchanged
          && nameResult.ResultType!=ComparisonResultType.Unchanged)
          result.ResultType = ComparisonResultType.Modified;
      }
    }

    protected static ComparisonResult<TNode> CompareSimpleNodes<TNode>(TNode originalNode, TNode newNode)
    {
      var result = new ComparisonResult<TNode>{
          OriginalValue = originalNode, 
          NewValue = newNode,
        };
      if (Equals(originalNode, newNode)) {
        result.ResultType = ComparisonResultType.Unchanged;
      }else if (ReferenceEquals(originalNode, null)) {
        result.ResultType = ComparisonResultType.Added;
      } else if (ReferenceEquals(newNode, null)) {
        result.ResultType = ComparisonResultType.Removed;
      } else {
        result.ResultType = ComparisonResultType.Modified;
      }
      return result;
    }

    #region Private methods

    private static bool ProcessNullNodes<TNode, TResult>(IEnumerable<TNode> originalNodes, IEnumerable<TNode> newNodes, SqlComparerStruct<TNode> comparer, IEnumerable<ComparisonHintBase> hints, ICollection<TResult> results)
      where TNode : class
      where TResult : ComparisonResult<TNode>
    {
      if (originalNodes==null && newNodes==null)
        return false;
      bool hasChanges = false;
      if (originalNodes==null) {
        foreach (var newNest in newNodes) {
          ComparisonResult<TNode> compare = comparer.Compare((TNode) null, newNest, hints);
          if (compare.HasChanges)
            hasChanges = true;
          results.Add((TResult) compare);
        }
        return hasChanges;
      }
      if (newNodes==null) {
        foreach (var originalNest in originalNodes) {
          ComparisonResult<TNode> compare = comparer.Compare(originalNest, (TNode) null, hints);
          if (compare.HasChanges)
            hasChanges = true;
          results.Add((TResult) compare);
        }
        return hasChanges;
      }
      return true;
    }

    private static bool CompareUnnamedNodes<TNode, TResult>(IEnumerable<TNode> originalNodes, IEnumerable<TNode> newNodes, SqlComparerStruct<TNode> comparer, IEnumerable<ComparisonHintBase> hints, ICollection<TResult> results)
      where TNode : class
      where TResult : ComparisonResult<TNode>
    {
      // Process "rename" hint, compare by name
      bool hasChanges = false;
      var originalNodeSet = new SetSlim<TNode>(originalNodes);
      foreach (TNode newNest in newNodes) {
        string newName = ((Node) (object) newNest).DbName;
        string originalName = newName;
        var renameHints = new List<RenameHint>(FindHints<TNode, RenameHint>(hints, newName));
        if (renameHints.Count > 1)
          throw new InvalidOperationException(String.Format(Resources.Strings.ExMultipleRenameHintsFoundForTypeXxx, typeof (TNode).FullName, newName));
        if (renameHints.Count==1)
          originalName = renameHints[0].OldName;
        IEnumerator<TNode> originalEnumerator = originalNodeSet.Where(node => ((Node) (object) node).DbName==originalName).GetEnumerator();
        TNode originalNode = null;
        if (originalEnumerator.MoveNext()) {
          originalNode = originalEnumerator.Current;
          originalNodeSet.Remove(originalNode);
        }
        ComparisonResult<TNode> compare = comparer.Compare(originalNode, newNest, hints);
        if (compare.HasChanges)
          hasChanges = true;
        results.Add((TResult) compare);
      }
      foreach (TNode originalNode in originalNodeSet) {
        ComparisonResult<TNode> compare = comparer.Compare(originalNode, null, hints);
        if (compare.HasChanges)
          hasChanges = true;
        results.Add((TResult) compare);
      }
      return hasChanges;
    }

    private static bool CompareNamedNodes<TNode, TResult>(IEnumerable<TNode> originalNodes, IEnumerable<TNode> newNodes, SqlComparerStruct<TNode> comparer, IEnumerable<ComparisonHintBase> hints, ICollection<TResult> results)
      where TNode : class
      where TResult : ComparisonResult<TNode>
    {
      bool hasChanges = false;
      var originalEnumerator = originalNodes.GetEnumerator();
      var newEnumerator = newNodes.GetEnumerator();
      while (originalEnumerator.MoveNext()) {
        ComparisonResult<TNode> compare = newEnumerator.MoveNext()
          ? comparer.Compare(originalEnumerator.Current, newEnumerator.Current, hints)
          : comparer.Compare(originalEnumerator.Current, null, hints);
        if (compare.HasChanges)
          hasChanges = true;
        results.Add((TResult) compare);
      }
      while (newEnumerator.MoveNext()) {
        ComparisonResult<TNode> compare = comparer.Compare(originalEnumerator.Current, newEnumerator.Current, hints);
        if (compare.HasChanges)
          hasChanges = true;
        results.Add((TResult) compare);
      }
      return hasChanges;
    }

    protected TResult InitializeResult<TNode, TResult>(TNode originalNode, TNode newNode)
      where TNode : Node
      where TResult : ComparisonResult<TNode>, new()
    {
      if (ReferenceEquals(originalNode, null) && ReferenceEquals(newNode, null))
        throw new InvalidOperationException(Resources.Strings.ExBothComparisonNodesAreNull);
      var result = new TResult();
      ProcessDbName(originalNode, newNode, result);
      if (originalNode==null)
        result.ResultType = ComparisonResultType.Added;
      else if (newNode==null)
        result.ResultType = ComparisonResultType.Removed;
      result.OriginalValue = originalNode;
      result.NewValue = newNode;
      return result;
    }

    #endregion


    // Constructors.

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="provider">SQL comparer provider this comparer is bound to.</param>
    protected SqlComparerBase(ISqlComparerProvider provider)
    {
      this.provider = provider;
    }

    /// <see cref="SerializableDocTemplate.OnDeserialization" copy="true" />
    public virtual void OnDeserialization(object sender)
    {
      if (provider==null || provider.GetType()==typeof (SqlComparerProvider))
        provider = SqlComparerProvider.Default;
    }
  }
}