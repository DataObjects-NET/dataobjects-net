// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.02.04

namespace Xtensive.Tuples
{
  /// <summary>
  /// <see cref="Tuple"/> factory contract.
  /// </summary>
  public interface ITupleFactory
  {
    /// <summary>
    /// Creates new instance of the tuple of the same type.
    /// </summary>
    /// <returns>A new instance of the tuple of the same type.</returns>
    Tuple CreateNew();
  }
}