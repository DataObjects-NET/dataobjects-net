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

    protected static IEnumerable<RenameHint> FindHints<THintTarget, THintType>(IEnumerable<ComparisonHintBase> hints, string objectName)
    {
      if (hints==null) {
        return Enumerable.Empty<RenameHint>();
      }
      return hints.Where(hint => hint!=null
        && hint.Name==objectName
          && hint.Type==typeof (THintTarget)
            && hint.GetType()==typeof (THintType)).Convert(hint => (RenameHint) hint);
    }

    protected static void ValidateArguments(T originalNode, T newNode)
    {
      if (ReferenceEquals(originalNode, null) && ReferenceEquals(newNode, null)) {
        throw new InvalidOperationException(Resources.Strings.ExBothComparisonNodesAreNull);
      }
    }

    protected void ProcessDbName(T originalNode, T newNode, ComparisonResult<T> result)
    {
      if (result is NodeComparisonResult<T> && typeof (T).IsSubclassOf(typeof (Node))) {
        var nameResult = new ComparisonResult<string>();
        string originalName = ReferenceEquals(originalNode, null) ? null : ((Node) (object) originalNode).DbName;
        string newName = ReferenceEquals(newNode, null) ? null : ((Node) (object) newNode).DbName;
        nameResult.OriginalValue = originalName;
        nameResult.NewValue = newName;
        if (originalName==newName)
          nameResult.ResultType = ComparisonResultType.Unchanged;
        else
          nameResult.ResultType = originalName==null ? ComparisonResultType.Added : ComparisonResultType.Removed;
        ((NodeComparisonResult<T>) result).DbName = nameResult;
      }
    }

    protected static bool GetComparisonPairs<TNested>(IEnumerable<TNested> originalNests, IEnumerable<TNested> newNests, IEnumerable<ComparisonHintBase> hints, ref SqlComparerStruct<TNested> comparer, ICollection<ComparisonResult<TNested>> results) where TNested : class
    {
      if (originalNests==null && newNests==null)
        return false;
        bool hasChanges = false;
      if (originalNests==null) {
        foreach (var newNest in newNests) {
          ComparisonResult<TNested> compare = comparer.Compare((TNested) null, newNest, hints);
          if (compare.HasChanges)
            hasChanges = true;
          results.Add(compare);
        }
        return hasChanges;
      }
      if (newNests==null) {
        foreach (var originalNest in originalNests) {
          ComparisonResult<TNested> compare = comparer.Compare(originalNest, (TNested) null, hints);
          if (compare.HasChanges)
            hasChanges = true;
          results.Add(compare);
        }
        return hasChanges;
      }
      if (typeof (TNested).IsSubclassOf(typeof (Node))) {
        // Process "rename" hint, compare by name
        var originalNodes = new SetSlim<TNested>(originalNests);
        foreach (TNested newNest in newNests) {
          string newName = ((Node) (object) newNest).DbName;
          string originalName = newName;
          var renameHints = new List<RenameHint>(FindHints<TNested, RenameHint>(hints, newName));
          if (renameHints.Count > 1)
            throw new InvalidOperationException(String.Format(Resources.Strings.ExMultipleRenameHintsFoundForTypeXxx, typeof (TNested).FullName, newName));
          if (renameHints.Count==1)
            originalName = renameHints[0].OldName;
          IEnumerator<TNested> originalEnumerator = originalNodes.Where(node => ((Node) (object) node).DbName==originalName).GetEnumerator();
          TNested originalNode = null;
          if (originalEnumerator.MoveNext()) {
            originalNode = originalEnumerator.Current;
            originalNodes.Remove(originalNode);
          }
          ComparisonResult<TNested> compare = comparer.Compare(originalNode, newNest, hints);
          if (compare.HasChanges) 
            hasChanges = true;
          results.Add(compare);
        }
        foreach (TNested originalNode in originalNodes) {
          ComparisonResult<TNested> compare = comparer.Compare(originalNode, null, hints);
          if (compare.HasChanges)
            hasChanges = true;
          results.Add(compare);
        }
        return hasChanges;
      }
      else {
        // compare one-to-one
        var originalEnumerator = originalNests.GetEnumerator();
        var newEnumerator = newNests.GetEnumerator();
        while (originalEnumerator.MoveNext()) {
          ComparisonResult<TNested> compare;
          if (newEnumerator.MoveNext())
            compare = comparer.Compare(originalEnumerator.Current, newEnumerator.Current, hints);
          else
            compare = comparer.Compare(originalEnumerator.Current, null, hints);
          if (compare.HasChanges)
            hasChanges = true;
          results.Add(compare);
        }
        while (newEnumerator.MoveNext()) {
          ComparisonResult<TNested> compare = comparer.Compare(originalEnumerator.Current, newEnumerator.Current, hints);
          if (compare.HasChanges)
            hasChanges = true;
          results.Add(compare);
        }
        return hasChanges;
      }
    }


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