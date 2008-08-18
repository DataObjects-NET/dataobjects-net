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
  /// <typeparam name="TBase">Base (wrapped) type.</typeparam>
  [Serializable]
  public abstract class WrappingSqlComparer<T, TBase> : SqlComparerBase<T>
  {
    /// <summary>
    /// Hasher for base (wrapped) type.
    /// </summary>
    protected SqlComparerStruct<TBase> BaseHasher;


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="provider">Hashing provider this hasher is bound to.</param>
    protected WrappingSqlComparer(ISqlComparerProvider provider)
      : base(provider)
    {
      BaseHasher = provider.GetSqlComparer<TBase>();
      if (!TypeHelper.IsFinal<TBase>()
        && !(BaseHasher.SqlComparer.Implementation is IFinalAssociate)
          && typeof (TBase)!=typeof (object))
        BaseHasher = null;
    }
  }
}