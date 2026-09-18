// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.09.01

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;


namespace Xtensive.Comparison
{
  /// <summary>
  /// Compares two objects by their reference values.
  /// </summary>
  /// <typeparam name="T">Type of the object to compare.</typeparam>
  public sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T>
  {
    /// <summary>
    /// Gets the only instance of this type.
    /// </summary>
    public readonly static ReferenceEqualityComparer<T> Instance = new();

    /// <inheritdoc/>
    public bool Equals(T x, T y) => ReferenceEquals(x, y);

    /// <inheritdoc/>
    public int GetHashCode(T obj) => RuntimeHelpers.GetHashCode(obj);
  }
}