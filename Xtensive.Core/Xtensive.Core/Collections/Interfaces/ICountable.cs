// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.11.13

using System.Collections;

namespace Xtensive.Collections
{
  /// <summary>
  /// Exposes the a number of elements and the enumerator over a collection of a specified type.
  /// </summary>
  public interface ICountable : IEnumerable
  {
    /// <summary>
    /// Gets the number of elements contained in a collection.
    /// </summary>
    long Count { get; }
  }
}