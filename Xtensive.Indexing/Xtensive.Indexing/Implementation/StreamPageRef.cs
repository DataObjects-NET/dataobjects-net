// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.11.16
using System;
using System.Diagnostics;
using Xtensive.Indexing.Resources;

namespace Xtensive.Indexing.Implementation
{
  /// <summary>
  /// Reference to the index page stored in the stream.
  /// </summary>
  /// <typeparam name="TKey">The type of the key.</typeparam>
  /// <typeparam name="TValue">The type of the value.</typeparam>
  [Serializable]
  public sealed class StreamPageRef<TKey, TValue> : IPageRef,
    IEquatable<StreamPageRef<TKey, TValue>>
  {
    private static readonly StreamPageRef<TKey, TValue> nullPageRef = null;
    private static readonly StreamPageRef<TKey, TValue> undefinedPageRef = new StreamPageRef<TKey, TValue>((long)StreamPageRefType.Undefined);
    private static readonly StreamPageRef<TKey, TValue> descriptorPageRef = new StreamPageRef<TKey, TValue>((long)StreamPageRefType.Descriptor);
    private readonly long offset;

    /// <summary>
    /// Gets the offset of serialized page data in stream.
    /// </summary>
    public long Offset
    {
      [DebuggerStepThrough]
      get { return offset; }
    }

    /// <summary>
    /// Gets a value indicating whether this reference is defined (has defined offset).
    /// </summary>
    public bool IsDefined
    {
      [DebuggerStepThrough]
      get { return offset!=(long)StreamPageRefType.Undefined; }
    }

    /// <summary>
    /// Gets the type of reference.
    /// </summary>
    public StreamPageRefType Type
    {
      [DebuggerStepThrough]
      get { return offset >= 0 ? StreamPageRefType.Normal : (StreamPageRefType)offset; }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return String.Format("StreamPageRef<{0},{1}>({2})",
        typeof (TKey).Name, typeof (TValue).Name,
        offset >= 0 ? offset.ToString() : Type.ToString());
    }

    #region Equals and HetHashCode

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public bool Equals(StreamPageRef<TKey, TValue> streamPageRef)
    {
      if (streamPageRef==null) return false;
      return offset==streamPageRef.offset;
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(this, obj)) return true;
      return Equals(obj as StreamPageRef<TKey, TValue>);
    }

    /// <inheritdoc/>
    [DebuggerStepThrough]
    public override int GetHashCode()
    {
      return (int)offset;
    }

    #endregion


    // Static Create methods

    /// <summary>
    /// Creates the <see cref="StreamPageRef{TKey,TValue}"/> with specified <paramref name="offset"/>.
    /// </summary>
    /// <param name="offset">The <see cref="Offset"/> of <see cref="StreamPageRef{TKey,TValue}"/> to create.</param>
    /// <returns>Newly created <see cref="StreamPageRef{TKey,TValue}"/> with specified <paramref name="offset"/>.</returns>
    [DebuggerStepThrough]
    public static StreamPageRef<TKey, TValue> Create(long offset)
    {
      if (offset < 0)
        return Create((StreamPageRefType)offset);
      return new StreamPageRef<TKey, TValue>(offset);
    }

    /// <summary>
    /// Creates the <see cref="StreamPageRef{TKey,TValue}"/> of specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type of <see cref="StreamPageRef{TKey,TValue}"/> to create.</param>
    /// <returns>Newly created <see cref="StreamPageRef{TKey,TValue}"/> of specified <paramref name="type"/>.</returns>
    public static StreamPageRef<TKey, TValue> Create(StreamPageRefType type)
    {
      switch (type) {
      case StreamPageRefType.Undefined:
        return undefinedPageRef;
      case StreamPageRefType.Null:
        return nullPageRef;
      case StreamPageRefType.Descriptor:
        return descriptorPageRef;
      default:
        throw new ArgumentException(Strings.ExUseAnotherCreateMethod, "type");
      }
    }


    // Constructors

    private StreamPageRef(long offset)
    {
      this.offset = offset;
    }
  }
}