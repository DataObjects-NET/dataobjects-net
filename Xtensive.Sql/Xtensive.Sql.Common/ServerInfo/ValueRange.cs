// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Common
{
  /// <summary>
  /// Represents a contiguous range of values.
  /// </summary>
  /// <typeparam name="T">A ranged type.</typeparam>
  /// <remarks>
  /// <para>Both minimum and maximum range bounds are belongs to the range.</para>
  /// <para>It is posible to create <see cref="ValueRange{T}"/> only for
  /// value types which are implements both <see cref="IComparable"/>
  /// and <see cref="IComparable{T}"/> interfaces.</para>
  /// </remarks>
  public struct ValueRange<T>: IEquatable<ValueRange<T>>
    where T: struct, IComparable, IComparable<T>
  {
    private readonly T? defaultValue;
    private readonly T maxValue;
    private readonly T minValue;

    /// <summary>
    /// A minimum value in the <see cref="ValueRange{T}"/>.
    /// </summary>
    public T MinValue
    {
      get { return minValue; }
    }

    /// <summary>
    /// A maximum value in the <see cref="ValueRange{T}"/>.
    /// </summary>
    public T MaxValue
    {
      get { return maxValue; }
    }

    /// <summary>
    /// A default value from the <see cref="ValueRange{T}"/>.
    /// </summary>
    /// <value>
    /// A value belonging to the <see cref="ValueRange{T}"/>
    /// or <see langword="null"/> if not specified.
    /// </value>
    public T? DefaultValue
    {
      get { return defaultValue; }
    }

    // Constructors

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (!(obj is ValueRange<T>))
        return false;
      return Equals((ValueRange<T>)obj);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      int result = minValue.GetHashCode();
      result = 29*result + maxValue.GetHashCode();
      result = 29*result + defaultValue.GetHashCode();
      return result;
    }

    #region IEquatable<ValueRange<T>> Members

    /// <inheritdoc/>
    public bool Equals(ValueRange<T> other)
    {
      if (!Equals(minValue, other.minValue))
        return false;
      if (!Equals(maxValue, other.maxValue))
        return false;
      if (!Equals(defaultValue, other.defaultValue))
        return false;
      return true;
    }

    #endregion

    /// <summary>
    /// Initializes a new <see cref="ValueRange{T}"/> instance.
    /// </summary>
    /// <param name="minValue">A minimum value in the range.</param>
    /// <param name="maxValue">A maximum value in the range.</param>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="maxValue"/>
    /// parameter specified is less then the <paramref name="minValue"/> one.</exception>
    public ValueRange(T minValue, T maxValue)
      : this(minValue, maxValue, null)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="ValueRange{T}"/> instance.
    /// </summary>
    /// <param name="minValue">A minimum value in the range.</param>
    /// <param name="maxValue">A maximum value in the range.</param>
    /// <param name="defaultValue">A default value from the range.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <para>The <paramref name="maxValue"/> parameter specified is less 
    /// then the <paramref name="minValue"/> one.</para>
    /// - or -
    /// <para>The <paramref name="defaultValue"/> specified is not in 
    /// [<paramref name="minValue"/>, <paramref name="maxValue"/>] range.</para>
    /// </exception>
    public ValueRange(T minValue, T maxValue, T? defaultValue)
    {
      if (minValue.CompareTo(maxValue) > 0)
        throw new ArgumentOutOfRangeException();
      if (defaultValue != null && (minValue.CompareTo(defaultValue) > 0 || maxValue.CompareTo(defaultValue) < 0))
        throw new ArgumentOutOfRangeException();
      this.minValue = minValue;
      this.maxValue = maxValue;
      this.defaultValue = defaultValue;
    }
  }
}