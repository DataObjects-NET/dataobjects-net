// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.06.04

using Xtensive.Core.Tuples;

namespace Xtensive.Storage
{
  /// <summary>
  /// Represents contract for <see cref="Key"/> generator (key provider).
  /// </summary>
  /// <remarks><see cref="IKeyProvider"/> implementors must have thread-safe implementation in order 
  /// to provide unique <see cref="Key"/>s.</remarks>
  public interface IKeyProvider
  {
    /// <summary>
    ///  Fills the key tuple with the unique values.
    ///  </summary><param name="keyTuple">The target key tuple to fill with values.</param>
    void GetNext(Tuple keyTuple);

    /// <summary>
    ///  Fills the key tuple with specified set of key data.
    ///  </summary><param name="keyTuple">The target key tuple to fill with key data.</param><param name="keyData">Key field values.</param>
    void Build(Tuple keyTuple, params object[] keyData);
  }
}