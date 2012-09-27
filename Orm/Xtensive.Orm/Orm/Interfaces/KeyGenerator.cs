// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.08

using Xtensive.Orm.Model;
using Xtensive.Tuples;

namespace Xtensive.Orm
{
  /// <summary>
  /// Key generator contract.
  /// </summary>
  public abstract class KeyGenerator
  {
    /// <summary>
    /// Initializes key generator instance in the specified <paramref name="ownerDomain" />.
    /// Only keys that have specified <paramref name="keyTupleDescriptor" /> will be requested.
    /// </summary>
    /// <param name="ownerDomain">Domain to use.</param>
    /// <param name="keyTupleDescriptor">Tuple descriptor of requested keys.</param>
    public abstract void Initialize(Domain ownerDomain, TupleDescriptor keyTupleDescriptor);

    /// <summary>
    /// Generates new key for the specified <paramref name="keyInfo" />.
    /// </summary><param name="keyInfo"><see cref="KeyInfo" /> that defines key to generate.
    /// </param><param name="session">Current session.</param>
    /// <returns>Generated key value.</returns>
    public abstract Tuple GenerateKey(KeyInfo keyInfo, Session session);
  }
}