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
  /// A struct providing faster access for key <see cref="NodeComparer{T}"/> delegates.
  /// </summary>
  /// <typeparam name="T">The type of <see cref="INodeComparer{T}"/> generic argument.</typeparam>
  [Serializable]
  public struct NodeComparerStruct<T> : ISerializable
  {
    /// <summary>
    /// Gets <see cref="NodeComparerStruct{T}"/> for <see cref="NodeComparer{T}.Default"/> SQL comparer.
    /// </summary>
    public static readonly NodeComparerStruct<T> Default = new NodeComparerStruct<T>(NodeComparer<T>.Default);


    /// <summary>
    /// Gets the underlying SQL comparer for this cache.
    /// </summary>
    public readonly NodeComparer<T> NodeComparer;

    /// <summary>
    /// Gets <see cref="INodeComparer{T}.Compare"/> method delegate.
    /// </summary>
    public readonly Func<T, T, IComparisonResult<T>> Compare;

    /// <summary>
    /// Implicit conversion of <see cref="NodeComparer{T}"/> to <see cref="NodeComparerStruct{T}"/>.
    /// </summary>
    /// <param name="nodeComparer">SQL comparer to provide the struct for.</param>
    public static implicit operator NodeComparerStruct<T>(NodeComparer<T> nodeComparer)
    {
      return new NodeComparerStruct<T>(nodeComparer);
    }


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="nodeComparer">SQL comparer to provide the delegates for.</param>
    private NodeComparerStruct(NodeComparer<T> nodeComparer)
    {
      NodeComparer = nodeComparer;
      Compare = nodeComparer==null ? null : nodeComparer.Compare;
    }

    /// <summary>
    /// Deserializes the instance of this class.
    /// </summary>
    /// <param name="info">Serialization info.</param>
    /// <param name="context">Streaming context.</param>
    private NodeComparerStruct(SerializationInfo info, StreamingContext context)
    {
      NodeComparer = (NodeComparer<T>)info.GetValue("NodeComparer", typeof(NodeComparer<T>));
      Compare = NodeComparer == null ? null : NodeComparer.Compare;
    }

    /// <inheritdoc/>
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("NodeComparer", NodeComparer);
    }
  }
}