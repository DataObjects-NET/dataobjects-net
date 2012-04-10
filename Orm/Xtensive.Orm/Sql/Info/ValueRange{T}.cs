// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;


namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Represents a strictly typed contiguous range of values.
  /// </summary>
  public sealed class ValueRange<T> : ValueRange
    where T: struct, IComparable<T>
  {
    /// <summary>
    /// A minimum value in the <see cref="ValueRange{T}"/>.
    /// </summary>
    public T MinValue { get; private set; }

    /// <summary>
    /// A maximum value in the <see cref="ValueRange{T}"/>.
    /// </summary>
    public T MaxValue { get; private set; }

    /// <summary>
    /// A default value from the <see cref="ValueRange{T}"/>.
    /// </summary>
    /// <value>
    /// A value belonging to the <see cref="ValueRange{T}"/>
    /// or <see langword="null"/> if not specified.
    /// </value>
    public T? DefaultValue { get; private set; }
    
    public override object GetMinValue()
    {
      return MinValue;
    }

    public override object GetMaxValue()
    {
      return MaxValue;
    }

    public override object GetDefaultValue()
    {
      return DefaultValue;
    }

    public override bool HasDefaultValue()
    {
      return DefaultValue.HasValue;
    }
    

    // Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueRange&lt;T&gt;"/> class.
    /// </summary>
    /// <param name="minValue">The min value.</param>
    /// <param name="maxValue">The max value.</param>
    public ValueRange(T minValue, T maxValue)
      : this(minValue, maxValue, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueRange&lt;T&gt;"/> class.
    /// </summary>
    /// <param name="minValue">The min value.</param>
    /// <param name="maxValue">The max value.</param>
    /// <param name="defaultValue">The default value.</param>
    public ValueRange(T minValue, T maxValue, T? defaultValue)
    {
      if (minValue.CompareTo(maxValue) > 0)
        throw new ArgumentOutOfRangeException();
      if (defaultValue.HasValue)
        if (minValue.CompareTo(defaultValue.Value) > 0 || maxValue.CompareTo(defaultValue.Value) < 0)
          throw new ArgumentOutOfRangeException();
      MinValue = minValue;
      MaxValue = maxValue;
      DefaultValue = defaultValue;
    }
  }
}