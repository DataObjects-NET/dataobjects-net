// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.12.20

using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Threading;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  /// <summary>
  /// Generator with caching capabilities.
  /// </summary>
  /// <typeparam name="TFieldType">The type of the field.</typeparam>
  public abstract class CachingKeyGenerator<TFieldType> : IncrementalKeyGenerator<TFieldType>
  {
    private readonly object _lock = new object();

    /// <summary>
    /// Gets the cache.
    /// </summary>
    protected Queue<TFieldType> Cache { get; private set; }

    /// <summary>
    /// Gets the size of the cache.
    /// </summary>
    public int CacheSize { get; private set; }

    /// <inheritdoc/>
    public override Tuple Next()
    {
      Tuple result = Tuple.Create(GeneratorInfo.TupleDescriptor);
      LockType.Exclusive.Execute(_lock, () => {
        if (Cache.Count==0)
          CacheNext();
        result.SetValue(0, Cache.Dequeue());
      });
      return result;
    }

    /// <summary>
    /// Caches the next portion of unique values.
    /// </summary>
    protected abstract void CacheNext();


    // Constructors

    /// <inheritdoc/>
    protected CachingKeyGenerator(GeneratorInfo generatorInfo)
      : base(generatorInfo)
    {
      CacheSize = GeneratorInfo.CacheSize;
      Cache = new Queue<TFieldType>(CacheSize);
    }
  }
}