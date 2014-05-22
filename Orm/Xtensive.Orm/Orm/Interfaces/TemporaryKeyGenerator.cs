// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.08

using Xtensive.Orm.Configuration;
using Xtensive.Tuples;

namespace Xtensive.Orm
{
  /// <summary>
  /// Temporary key generator generates local keys when <see cref="SessionOptions.LazyKeyGeneration"/> is enabled.
  /// </summary>
  public abstract class TemporaryKeyGenerator : KeyGenerator
  {
    /// <summary>
    /// Checks if the specified key is local.
    /// </summary>
    /// <param name="keyTuple">Key tuple to check</param>
    /// <returns>true, if the specified <paramref name="keyTuple" /> represents local key;
    /// otherwise, false.</returns>
    public abstract bool IsTemporaryKey(Tuple keyTuple);
  }
}