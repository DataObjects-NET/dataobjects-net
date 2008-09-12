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
  public abstract class CachingGenerator<TFieldType> : IncrementalGenerator<TFieldType>
  {
    protected Queue<TFieldType> Cache { get; private set; }

    /// <summary>
    /// Gets the size of the cache.
    /// </summary>
    public int CacheSize { get; private set; }

    /// <inheritdoc/>
    public override Tuple Next()
    {
      Tuple result = Tuple.Create(Hierarchy.KeyTupleDescriptor);
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


    // Constructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="hierarchy">The hierarchy this instance will serve.</param>
    /// <param name="cacheSize">Size of the cache.</param>
    protected CachingGenerator(HierarchyInfo hierarchy, int cacheSize)
      : base(hierarchy)
    {
      CacheSize = cacheSize;
      Cache = new Queue<TFieldType>(CacheSize);
    }
  }
}