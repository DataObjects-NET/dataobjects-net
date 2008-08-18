// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.18

using System;
using Xtensive.Core;
using Xtensive.Core.Reflection;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// Base class for any wrapping <see cref="ISqlComparer{T}"/>s.
  /// </summary>
  /// <typeparam name="T">The type of node to compare.</typeparam>
  /// <typeparam name="TBase1">First base (wrapped) type.</typeparam>
  /// <typeparam name="TBase2">Second base (wrapped) type.</typeparam>
  /// <typeparam name="TBase3">Third base (wrapped) type.</typeparam>
  [Serializable]
  public abstract class WrappingSqlComparer<T, TBase1, TBase2, TBase3> : SqlComparerBase<T>
  {
    /// <summary>
    /// SQL comparer for the first base (wrapped) type.
    /// </summary>
    protected SqlComparerStruct<TBase1> BaseSqlComparer1;

    /// <summary>
    /// SQL comparer for the second base (wrapped) type.
    /// </summary>
    protected SqlComparerStruct<TBase2> BaseSqlComparer2;

    /// <summary>
    /// SQL comparer for the third base (wrapped) type.
    /// </summary>
    protected SqlComparerStruct<TBase3> BaseSqlComparer3;


    // Constructors

    /// <summary>
    /// Creates new instance of <see cref="WrappingSqlComparer{T,TBase1,TBase2}"/>.
    /// </summary>
    /// <param name="provider">SQL comparer provider.</param>
    protected WrappingSqlComparer(ISqlComparerProvider provider)
      : base(provider)
    {
      BaseSqlComparer1 = provider.GetSqlComparer<TBase1>();
      if (!TypeHelper.IsFinal<TBase1>() && !(BaseSqlComparer1.SqlComparer.Implementation is IFinalAssociate))
        BaseSqlComparer1 = null;
      BaseSqlComparer2 = provider.GetSqlComparer<TBase2>();
      if (!TypeHelper.IsFinal<TBase2>() && !(BaseSqlComparer2.SqlComparer.Implementation is IFinalAssociate))
        BaseSqlComparer2 = null;
      BaseSqlComparer3 = provider.GetSqlComparer<TBase3>();
      if (!TypeHelper.IsFinal<TBase3>() && !(BaseSqlComparer3.SqlComparer.Implementation is IFinalAssociate))
        BaseSqlComparer3 = null;
    }
  }
}