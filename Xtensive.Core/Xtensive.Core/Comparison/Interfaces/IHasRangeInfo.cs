// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.23

namespace Xtensive.Comparison
{
  /// <summary>
  /// "An object providing <see cref="ValueRangeInfo"/>" contract.
  /// </summary>
  /// <typeparam name="T">Type of the value range boundaries.</typeparam>
  public interface IHasRangeInfo<T>
  {
    /// <summary>
    /// Gets the <see cref="ValueRangeInfo"/> object describing the range of type <typeparamref name="T"/>.
    /// </summary>
    /// <value>The <see cref="ValueRangeInfo"/> object.</value>
    ValueRangeInfo<T> ValueRangeInfo { get; }
  }
}