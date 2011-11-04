// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.02.09

using System;
using System.Collections.Generic;
using Xtensive.Orm;
using Xtensive.Orm.Internals;

namespace Xtensive.Storage.Providers
{
  /// <summary>
  /// Handler for <see cref="CachingKeyGenerator{TKeyType}"/>.
  /// </summary>
  public interface ICachingKeyGeneratorService
  {
    /// <summary>
    /// Gets the bulk of keys.
    /// </summary>
    /// <typeparam name="TFieldType">The type of the key field.</typeparam>
    /// <param name="keyGenerator">The key generator requested the bulk of keys.</param>
    /// <returns>
    /// The sequence enumerating the next bulk of keys.
    /// </returns>
    IEnumerable<TFieldType> NextBulk<TFieldType>(CachingKeyGenerator<TFieldType> keyGenerator);
  }
}