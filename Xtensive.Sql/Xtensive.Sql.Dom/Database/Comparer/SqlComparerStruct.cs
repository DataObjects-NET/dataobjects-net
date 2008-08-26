// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.18

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// A struct providing faster access for key <see cref="SqlComparer{T}"/> delegates.
  /// </summary>
  /// <typeparam name="T">The type of <see cref="ISqlComparer{T}"/> generic argument.</typeparam>
  [Serializable]
  public struct SqlComparerStruct<T> : ISerializable
  {
    /// <summary>
    /// Gets <see cref="SqlComparerStruct{T}"/> for <see cref="SqlComparer{T}.Default"/> SQL comparer.
    /// </summary>
    public static readonly SqlComparerStruct<T> Default = new SqlComparerStruct<T>(SqlComparer<T>.Default);


    /// <summary>
    /// Gets the underlying SQL comparer for this cache.
    /// </summary>
    public readonly SqlComparer<T> SqlComparer;

    /// <summary>
    /// Gets <see cref="ISqlComparer{T}.Compare"/> method delegate.
    /// </summary>
    public readonly Func<T, T, IEnumerable<ComparisonHintBase>, IComparisonResult<T>> Compare;

    /// <summary>
    /// Implicit conversion of <see cref="SqlComparer{T}"/> to <see cref="SqlComparerStruct{T}"/>.
    /// </summary>
    /// <param name="sqlComparer">SQL comparer to provide the struct for.</param>
    public static implicit operator SqlComparerStruct<T>(SqlComparer<T> sqlComparer)
    {
      return new SqlComparerStruct<T>(sqlComparer);
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="sqlComparer">SQL comparer to provide the delegates for.</param>
    private SqlComparerStruct(SqlComparer<T> sqlComparer)
    {
      SqlComparer = sqlComparer;
      Compare = SqlComparer==null ? null : SqlComparer.Compare;
    }

    /// <summary>
    /// Deserializes the instance of this class.
    /// </summary>
    /// <param name="info">Serialization info.</param>
    /// <param name="context">Streaming context.</param>
    private SqlComparerStruct(SerializationInfo info, StreamingContext context)
    {
      SqlComparer = (SqlComparer<T>)info.GetValue("SqlComparer", typeof(SqlComparer<T>));
      Compare = SqlComparer == null ? null : SqlComparer.Compare;
    }

    /// <inheritdoc/>
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("SqlComparer", SqlComparer);
    }
  }
}