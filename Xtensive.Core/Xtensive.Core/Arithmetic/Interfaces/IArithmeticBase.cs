// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.06

namespace Xtensive.Arithmetic
{
  /// <summary>
  /// Very base interface for any arithmetic implementation 
  /// supported by <see cref="IArithmeticProvider"/>.
  /// </summary>
  public interface IArithmeticBase
  {
    /// <summary>
    /// Gets the provider this arithmetic is associated with.
    /// </summary>
    IArithmeticProvider Provider { get; }
  }
}