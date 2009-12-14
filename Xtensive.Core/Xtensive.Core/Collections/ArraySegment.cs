// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.02.27

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Resources;

namespace Xtensive.Core.Collections
{
  /// <summary>
  /// Delimits a section of a one-dimensional array. In addition to system <see cref="System.ArraySegment{T}"/> implements <see cref="IEnumerable{T}"/> interface and indexer.
  /// </summary>
  /// <typeparam name="T">The type of the elements in the array segment.</typeparam>
  [Serializable]
  [DebuggerDisplay("Offset = {Offset}, Length = {Length}")]
  public struct ArraySegment<T> : IEnumerable<T>
  {
    private readonly T[] array;
    private readonly int offset;
    private readonly int length;

    /// <summary>
    /// Gets the original array containing the range of elements that the array segment delimits.
    /// </summary>
    /// <remarks>The Array property returns the original array, not a copy of the array; therefore, changes made through the property are made directly to the original array.</remarks>
    public T[] Array
    {
      [DebuggerStepThrough]
      get { return array; }
    }

    /// <summary>
    /// Gets the position of the first element in the range delimited by the array segment, relative to the start of the original array.
    /// </summary>
    public int Offset
    {
      [DebuggerStepThrough]
      get { return offset; }
    }

    /// <summary>
    /// Gets the number of elements in the range delimited by the array segment.
    /// </summary>
    public int Length
    {
      [DebuggerStepThrough]
      get { return length; }
    }

    /// <summary>
    /// Gets or sets element of <see cref="ArraySegment{T}"/> by index.
    /// </summary>
    public T this[int index]
    {
      get
      {
        ArgumentValidator.EnsureArgumentIsInRange(index, 0, length - 1, "index");
        return array[offset + index];
      }
      set
      {
        ArgumentValidator.EnsureArgumentIsInRange(index, 0, length - 1, "index");
        array[offset + index] = value;
      }
    }

    #region GetHashCode, Equals

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      return ((array.GetHashCode() ^ offset) ^ length);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      return ((obj is ArraySegment<T>) && Equals((ArraySegment<T>)obj));
    }

    /// <inheritdoc/>
    public bool Equals(ArraySegment<T> obj)
    {
      return (((obj.array==array) && (obj.offset==offset)) && (obj.length==length));
    }

    #endregion

    #region Operators ==, !=

    /// <summary>
    /// Indicates whether two <see cref="ArraySegment{T}"/> structures are equal.
    /// </summary>
    /// <param name="a">The <see cref="ArraySegment{T}"/> structure on the left side of the equality operator.</param>
    /// <param name="b">The <see cref="ArraySegment{T}"/> structure on the right side of the equality operator.</param>
    /// <returns><see langword="true"/> if a is equal to b; otherwise, <see langword="false"/>.</returns>
    public static bool operator ==(ArraySegment<T> a, ArraySegment<T> b)
    {
      return a.Equals(b);
    }

    /// <summary>
    /// Indicates whether two <see cref="ArraySegment{T}"/> structures are unequal.
    /// </summary>
    /// <param name="a">The <see cref="ArraySegment{T}"/> structure on the left side of the inequality operator.</param>
    /// <param name="b">The <see cref="ArraySegment{T}"/> structure on the right side of the inequality operator.</param>
    /// <returns><see langword="true"/> if a is not equal to b; otherwise, <see langword="false"/>.</returns>
    public static bool operator !=(ArraySegment<T> a, ArraySegment<T> b)
    {
      return !(a==b);
    }

    #endregion

    #region IEnumerable<...> Members

    /// <inheritdoc/>
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
      int lastPosition = offset + length;
      for (int i = offset; i < lastPosition; i++) {
        yield return array[i];
      }
    }

    /// <inheritdoc/>
    public IEnumerator GetEnumerator()
    {
      return ((IEnumerable<T>)this).GetEnumerator();
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="array">The array to wrap.</param>
    public ArraySegment(T[] array)
    {
      ArgumentValidator.EnsureArgumentNotNull(array, "array");
      this.array = array;
      offset = 0;
      length = array.Length;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="array">The array containing the range of elements to delimit.</param>
    /// <param name="offset">The zero-based index of the first element in the range.</param>
    /// <param name="length">The number of elements in the range.</param>
    public ArraySegment(T[] array, int offset, int length)
    {
      ArgumentValidator.EnsureArgumentNotNull(array, "array");
      if (length < 0)
        throw new ArgumentOutOfRangeException("length", Strings.ExArgumentValueMustBeGreaterThanOrEqualToZero);

      ArgumentValidator.EnsureArgumentIsInRange(offset, 0, array.Length - length, "offset");
      this.array = array;
      this.offset = offset;
      this.length = length;
    }
  }
}