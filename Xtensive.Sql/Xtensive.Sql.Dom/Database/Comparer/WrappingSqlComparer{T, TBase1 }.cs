// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.18

using System;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// Base class for any wrapping <see cref="ISqlComparer{T}"/>s.
  /// </summary>
  /// <typeparam name="T">The type of node to compare.</typeparam>
  /// <typeparam name="TBase1">Base (wrapped) type.</typeparam>
  [Serializable]
  public abstract class WrappingSqlComparer<T, TBase1> : SqlComparerBase<T>
  {
    /// <summary>
    /// SQL comparer for base (wrapped) type.
    /// </summary>
    protected SqlComparerStruct<TBase1> BaseSqlComparer1;


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="provider">SQL comparer provider provider this comparer is bound to.</param>
    protected WrappingSqlComparer(ISqlComparerProvider provider)
      : base(provider)
    {
      BaseSqlComparer1 = provider.GetSqlComparer<TBase1>();
    }
  }
}