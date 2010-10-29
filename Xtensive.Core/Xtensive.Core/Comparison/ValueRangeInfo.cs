// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.23

using System;
using Xtensive.Arithmetic;
using Xtensive.Internals.DocTemplates;
using Xtensive.Resources;

namespace Xtensive.Comparison
{
  // Note: This class implements immutable pattern
  /// <summary>
  /// Represents a pair of smallest and largest values defined for <typeparamref name="T"/>.
  /// </summary>
  /// <typeparam name="T">the type of <typeparamref name="T"/>.</typeparam>
  [Serializable]
  public sealed class ValueRangeInfo<T>
  {
    private bool hasMaxValue;
    private bool hasMinValue;
    private bool hasDeltaValue;
    private T maxValue;
    private T minValue;
    private T deltaValue;

    /// <summary>
    /// Gets a value indicating whether this instance has <see cref="MinValue"/>.
    /// </summary>
    /// <value>
    ///   <see langword="true"/> if this instance has <see cref="MinValue"/>; otherwise, <see langword="false"/>.
    /// </value>
    public bool HasMinValue
    {
      get { return hasMinValue; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance has <see cref="MaxValue"/>.
    /// </summary>
    /// <value>
    ///   <see langword="true"/> if this instance has <see cref="MaxValue"/>; otherwise, <see langword="false"/>.
    /// </value>
    public bool HasMaxValue
    {
      get { return hasMaxValue; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance has <see cref="DeltaValue"/>.
    /// </summary>
    /// <value>
    ///   <see langword="true"/> if this instance has <see cref="DeltaValue"/>; otherwise, <see langword="false"/>.
    /// </value>
    public bool HasDeltaValue
    {
      get { return hasDeltaValue; }
    }

    /// <summary>
    /// Represents the smallest possible value of a T.
    /// </summary>
    /// <value>The smallest possible value.</value>
    public T MinValue
    {
      get
      {
        if (!hasMinValue)
          throw new InvalidOperationException(Strings.ExValueIsNotAvailable);
        return minValue;
      }
    }

    /// <summary>
    /// Represents the largest possible value of a T.
    /// </summary>
    /// <value>The largest possible value.</value>
    public T MaxValue
    {
      get
      {
        if (!hasMaxValue)
          throw new InvalidOperationException(Strings.ExValueIsNotAvailable);
        return maxValue;
      }
    }

    /// <summary>
    /// Represents the smallest possible delta value of a <typeparamref name="T"/>.
    /// </summary>
    /// <value>The smallest possible delta value.</value>
    public T DeltaValue
    {
      get
      {
        if (!hasDeltaValue)
          throw new InvalidOperationException(Strings.ExValueIsNotAvailable);
        return deltaValue;
      }
    }

    /// <summary>
    /// Inverts the instance of <see cref="ValueRangeInfo{T}"/>.
    /// Exactly, exchanges <see cref="MinValue"/> and <see cref="MaxValue"/>,
    /// and replaced <see cref="DeltaValue"/> to a negative one.
    /// </summary>
    /// <returns>Inverted <see cref="ValueRangeInfo{T}"/> instance.</returns>
    public ValueRangeInfo<T> Invert()
    {
      return new ValueRangeInfo<T>(
        hasMaxValue, maxValue, 
        hasMinValue, minValue, 
        hasDeltaValue, hasDeltaValue ? Arithmetic<T>.Default.Negation(deltaValue) : deltaValue);
    }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="hasMinValue">if set to <see langword="true"/> then this instance has the smallest possible value of <typeparamref name="T"/>.</param>
    /// <param name="minValue">The smallest possible value of <typeparamref name="T"/>.</param>
    /// <param name="hasMaxValue">if set to <see langword="true"/> then this instance has the largest possible value of <typeparamref name="T"/>.</param>
    /// <param name="maxValue">The largest possible value of <typeparamref name="T"/>.</param>
    /// <param name="hasDeltaValue">if set to <see langword="true"/> then this instance has the smallest possible delta value of <typeparamref name="T"/>.</param>
    /// <param name="deltaValue">The smallest possible delta value of <typeparamref name="T"/>.</param>
    public ValueRangeInfo(bool hasMinValue, T minValue, bool hasMaxValue, T maxValue, bool hasDeltaValue, T deltaValue)
    {
      this.hasMaxValue = hasMaxValue;
      this.hasMinValue = hasMinValue;
      this.hasDeltaValue = hasDeltaValue;
      this.maxValue = maxValue;
      this.minValue = minValue;
      this.deltaValue = deltaValue;
    }

    private ValueRangeInfo()
    {
      minValue = default(T);
      maxValue = default(T);
    }
  }
}