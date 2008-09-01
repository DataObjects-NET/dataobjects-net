// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.12.20

using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Building;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Resources;
using Xtensive.Core.Helpers;
using Xtensive.Core.Disposable;

namespace Xtensive.Storage
{
  ///<summary>
  /// Default key generator.
  ///</summary>
  public abstract class Generator : HandlerBase,
    IDisposable
  {
    private readonly object _lock = new object();
    private volatile Queue<Tuple> preCachedValues = new Queue<Tuple>();
    private volatile Future<IEnumerable<Tuple>> preCachingTask;
    private int cacheSize;
    private bool isGuid;
    private TaskManager taskManager;

    /// <summary>
    /// Gets the hierarchy this instance serves.
    /// </summary>
    public HierarchyInfo Hierarchy { get; internal set; }

    /// <summary>
    /// Create the <see cref="Tuple"/> with the unique values in key sequence.
    ///  </summary>
    public Tuple Next()
    {
      if (isGuid)
        return Tuple.Create(Hierarchy.KeyTupleDescriptor, Guid.NewGuid());
      if (cacheSize > 0)
        return NextOneCached();
      return NextOne();
    }

    private Tuple NextOneCached()
    {
//      var minCached = cacheSize / 2;
      lock (_lock) {
        if (preCachedValues.Count == 0) {
          foreach (var tuple in NextMany(cacheSize))
            preCachedValues.Enqueue(tuple);
        }
//        if (preCachedValues.Count <= minCached) {
//          if (preCachingTask==null)
//            preCachingTask = Future<IEnumerable<Tuple>>.Create(() => NextMany(cacheSize));
//          if (preCachedValues.Count == 0) {
//            foreach (var tuple in preCachingTask.Value)
//              preCachedValues.Enqueue(tuple);
//            preCachingTask.Dispose();
//            preCachingTask = null;
//          }
//        }
        return preCachedValues.Dequeue();
      }
    }
    
    /// <summary>
    /// Create the <see cref="Tuple"/> with the unique values in key sequence where key is a number.
    ///  </summary>
    protected abstract Tuple NextOne();

    /// <summary>
    /// Create an <see cref="Array"/> of <see cref="Tuple"/>s with the unique values in key sequence.
    ///  </summary>
    /// <param name="count">The number of <see cref="Tuple"/> instances to retrieve.</param>
    /// <returns>An <see cref="Array"/> of <see cref="Tuple"/>s with unique values in key sequence.</returns>
    protected abstract IEnumerable<Tuple> NextMany(int count);

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    /// <remarks>
    /// Should call <see langword="base"/> when overridden.
    /// </remarks>
    public virtual void Initialize()
    {
      cacheSize = BuildingContext.Current.Configuration.GeneratorCacheSize;
      if (cacheSize < 0)
        cacheSize = 0;
      Type fieldType = Hierarchy.KeyTupleDescriptor[0];
      isGuid = fieldType==typeof (Guid);
      taskManager = new TaskManager();
    }

    /// <summary>
    ///   <see cref="ClassDocTemplate.Dispose" copy="true"/>
    /// </summary>
    public virtual void Dispose()
    {
      lock (_lock) {
        if (preCachingTask!=null)
          preCachingTask.Wait();
        preCachingTask.DisposeSafely();
      }
    }
  }
}