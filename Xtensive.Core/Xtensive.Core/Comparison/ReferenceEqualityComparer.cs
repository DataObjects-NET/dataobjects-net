// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.09.01

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Comparison
{
  /// <summary>
  /// Compares two objects by their reference values.
  /// </summary>
  /// <typeparam name="T">Type of the object to compare.</typeparam>
  /// <remarks>
  /// <para id="About"><see cref="SingletonDocTemplate" copy="true" /></para>
  /// </remarks>
  [Serializable]
  public sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T>
  {
    /// <see cref="SingletonDocTemplate.Instance" copy="true" />
    public static ReferenceEqualityComparer<T> Instance = new ReferenceEqualityComparer<T>();

    /// <inheritdoc/>
    public bool Equals(T x, T y)
    {
      return ReferenceEquals(x, y);
    }

    /// <inheritdoc/>
    public int GetHashCode(T obj)
    {
      return RuntimeHelpers.GetHashCode(obj);
    }
  }
}