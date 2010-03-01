// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.25

namespace Xtensive.Modelling.Comparison
{
  /// <summary>
  /// <see cref="Parent"/>-typed <see cref="IDifference{T}"/> contract.
  /// </summary>
  /// <typeparam name="T">The type of source and target objects.</typeparam>
  /// <typeparam name="TParent">The type of the parent.</typeparam>
  public interface IDifference<T, TParent> : IDifference<T>
    where TParent : Difference
  {
    /// <summary>
    /// Gets the parent difference.
    /// <see langword="null" />, if none.
    /// </summary>
    new TParent Parent { get; }
  }
}