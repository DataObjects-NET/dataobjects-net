// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.18

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Xtensive.Core;
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
    public ComparisonResult<T> Compare(T originalNode, T newNode, IEnumerable<ComparisonHintBase> hints)
    {
      ValidateArguments(originalNode, newNode);
      var result = new ComparisonResult<T>();
      bool hasChanges = false;
      IEnumerable<ComparisonResult> properties = CompareProperties(originalNode, newNode, hints);
      if (properties!=null) {
        foreach (ComparisonResult property in properties) {
          result.Properties.Add(property);
          if (property.HasChanges) {
            hasChanges = true;
          }
        }
      }
      IEnumerable<ComparisonResult> nests = CompareNests(originalNode, newNode, hints);
      if (nests!=null) {
        foreach (ComparisonResult nest in nests) {
          result.Nested.Add(nest);
          if (nest.HasChanges) {
            hasChanges = true;
          }
        }
      }
      result.OriginalValue = originalNode;
      result.NewValue = newNode;
      result.HasChanges = hasChanges;
      if (ReferenceEquals(newNode, null))
        result.ResultType = ComparisonResultType.Removed;
      else if (ReferenceEquals(originalNode, null))
        result.ResultType = ComparisonResultType.Added;
      else
        result.ResultType = hasChanges ? ComparisonResultType.Modified : ComparisonResultType.Unchanged;
      result.Lock(true);
      return result;
    }

    /// <inheritdoc/>
    public ISqlComparerProvider Provider
    {
      get { return provider; }
    }

    protected IEnumerable<ComparisonHintBase> FindHints<T>(IEnumerable<ComparisonHintBase> hints, string objectName)
    {
      if (hints==null) {
        return Enumerable.Empty<ComparisonHintBase>();
      }
      return hints.Where(hint => hint!=null && hint.Name==objectName && hint.Type==typeof (T));
    }

    protected virtual IEnumerable<ComparisonResult> CompareProperties(T originalNode, T newNode, IEnumerable<ComparisonHintBase> hints)
    {
      return Enumerable.Empty<ComparisonResult>();
    }

    protected virtual IEnumerable<ComparisonResult> CompareNests(T originalNode, T newNode, IEnumerable<ComparisonHintBase> hints)
    {
      return Enumerable.Empty<ComparisonResult>();
    }

    # region Private / internal methods

    private static void ValidateArguments(T originalNode, T newNode)
    {
      if (ReferenceEquals(originalNode, null) && ReferenceEquals(newNode, null)) {
        throw new InvalidOperationException(Resources.Strings.ExBothComparisonNodesAreNull);
      }
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