// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.22

using Xtensive.Indexing.SizeCalculators;

namespace Xtensive.Indexing.SizeCalculators
{
  /// <summary>
  /// Describes an object that is aware of <see cref="ISizeCalculator{T}"/>.
  /// </summary>
  public interface ISizeCalculatorAware
  {
    /// <summary>
    /// Gets the size of the object (in stack plus heap).
    /// </summary>
    /// <param name="provider">The size calculator provider to use for calculating fields size.</param>
    /// <returns>The size of the object (in stack plus heap, in bytes).</returns>
    int GetSize(ISizeCalculatorProvider provider);
  }
}