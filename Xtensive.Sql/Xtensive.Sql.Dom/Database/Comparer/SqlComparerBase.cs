// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.18

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
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

    protected IEnumerable<ComparisonHintBase> FindHints<T>(IEnumerable<ComparisonHintBase> hints, string objectName)
    {
      if (hints==null) {
        return Enumerable.Empty<ComparisonHintBase>();
      }
      return hints.Where(hint => hint!=null && hint.Name==objectName && hint.Type==typeof (T));
    }

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