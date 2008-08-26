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
  public abstract class WrappingSqlComparer<T, TBase1, TBase2, TBase3> : WrappingSqlComparer<T, TBase1, TBase2> 
  {
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
      BaseSqlComparer3 = provider.GetSqlComparer<TBase3>();
    }
  }
}