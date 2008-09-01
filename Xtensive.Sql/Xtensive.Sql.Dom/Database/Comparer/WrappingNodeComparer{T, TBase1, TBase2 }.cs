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
  /// Base class for any wrapping <see cref="INodeComparer{T}"/>s.
  /// </summary>
  /// <typeparam name="T">The type of node to compare.</typeparam>
  /// <typeparam name="TBase1">First base (wrapped) type.</typeparam>
  /// <typeparam name="TBase2">Second base (wrapped) type.</typeparam>
  [Serializable]
  public abstract class WrappingNodeComparer<T, TBase1, TBase2> : WrappingNodeComparer<T, TBase1>
  {
    /// <summary>
    /// SQL comparer for the second base (wrapped) type.
    /// </summary>
    protected NodeComparerStruct<TBase2> BaseNodeComparer2;


    // Constructors

    /// <summary>
    /// Creates new instance of <see cref="WrappingNodeComparer{T,TBase1,TBase2}"/>.
    /// </summary>
    /// <param name="provider">SQL Comparer provider.</param>
    protected WrappingNodeComparer(INodeComparerProvider provider)
      : base(provider)
    {
      BaseNodeComparer2 = provider.GetNodeComparer<TBase2>();
    }
  }
}