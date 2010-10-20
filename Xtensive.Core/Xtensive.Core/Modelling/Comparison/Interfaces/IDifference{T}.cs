// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.25

namespace Xtensive.Modelling.Comparison
{
  /// <summary>
  /// Typed <see cref="IDifference"/> contract.
  /// </summary>
  /// <typeparam name="T">The type of <see cref="Source"/> and <see cref="Target"/> objects.</typeparam>
  public interface IDifference<T> : IDifference
  {
    /// <summary>
    /// Gets the source object.
    /// </summary>
    new T Source { get; }

    /// <summary>
    /// Gets the target object.
    /// </summary>
    new T Target { get; }
  }
}