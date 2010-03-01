// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.02.04

using System;

namespace Xtensive.Indexing
{
  /// <summary>
  /// Base interface for any "Entire" type.
  /// </summary>
  /// <typeparam name="T">The type wrapped by entire.</typeparam>
  public interface IEntire<T>
    : IComparable<T>,
      IEquatable<T>,
      IComparable<IEntire<T>>,
      IEquatable<IEntire<T>>,
      ICloneable
  {
    /// <summary>
    /// Gets the underlying value of the entire.
    /// </summary>
    T Value { get; }

    /// <summary>
    /// Gets the types of each nested value of the entire.
    /// </summary>
    EntireValueType ValueType { get; }
  }
}