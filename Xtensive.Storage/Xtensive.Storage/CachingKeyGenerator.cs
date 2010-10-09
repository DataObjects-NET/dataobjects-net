// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.12.20

using System.Collections.Generic;
using Xtensive.Core.IoC;
using Xtensive.Core.Tuples;
using Tuple = Xtensive.Core.Tuples.Tuple;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Providers;

namespace Xtensive.Storage
{
  /// <summary>
  /// Generator with caching capabilities.
  /// </summary>
  /// <typeparam name="TKeyType">The type of the field.</typeparam>
  public class CachingKeyGenerator<TKeyType> : KeyGenerator<TKeyType>
  {
    /// <summary>
    /// Gets or sets the size of the cache.
    /// </summary>
    public int CacheSize { get; protected set; }

    /// <inheritdoc/>
    public override long? SequenceIncrement { 
      get {
        if (WellKnown.SupportedIntegerTypes.Contains(typeof (TKeyType)))
          return CacheSize;
        else
          return null;
      }
    }

    /// <summary>
    /// Gets the cache.
    /// </summary>
    protected Queue<TKeyType> CachedKeys { get; private set; }

    /// <inheritdoc/>
    public override Tuple TryGenerateKey(bool temporaryKey)
    {
      if (temporaryKey)
        return base.TryGenerateKey(true);
      if (!WellKnown.SupportedIntegerTypes.Contains(typeof(TKeyType)))
        return base.TryGenerateKey(false);
      var result = TuplePrototype.CreateNew();
      lock (SyncRoot) {
        if (CachedKeys.Count==0) {
          foreach (var key in NextBulk())
            CachedKeys.Enqueue(key);
        }
        result.SetValue(0, CachedKeys.Dequeue());
      };
      return result;
    }

    /// <summary>
    /// Retrieves the next portion of unique key values.
    /// </summary>
    protected virtual IEnumerable<TKeyType> NextBulk()
    {
      var service = Handlers.Domain.Services.Get<ICachingKeyGeneratorService>();
      if (service!=null) {
        // Using SessionHandler's service
        foreach (var value in service.NextBulk(this))
          yield return value;
      }
      else {
        // Fallback to default behavior
        for (int i = 0; i < CacheSize; i++) {
          var tuple = DefaultNext(false);
          yield return tuple.GetValue<TKeyType>(0);
        }
      }
    }


    // Constructors

    /// <inheritdoc/>
    [ServiceConstructor]
    public CachingKeyGenerator(DomainConfiguration configuration)
    {
      CacheSize = configuration.KeyGeneratorCacheSize;
      CachedKeys = new Queue<TKeyType>(CacheSize);
    }
  }
}