// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.02.04

namespace Xtensive.Core.Tuples
{
  /// <summary>
  /// <see cref="ITuple"/> factory contract.
  /// </summary>
  public interface ITupleFactory
  {
    /// <summary>
    /// Creates new instance of the tuple of the same type.
    /// </summary>
    /// <returns>A new instance of the tuple of the same type.</returns>
    ITuple CreateNew();
  }
}