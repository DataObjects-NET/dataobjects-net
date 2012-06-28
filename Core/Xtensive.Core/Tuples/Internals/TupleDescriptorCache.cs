// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.24

using System;
using System.Collections.Generic;
using System.Threading;
using Xtensive.Collections;
using Xtensive.Caching;
using Xtensive.Core;

using System.Linq;

namespace Xtensive.Tuples.Internals
{
  internal static class TupleDescriptorCache
  {
    private readonly static object _lock = new object();
    private readonly static SetSlim<TupleDescriptor> initializedDescriptors;
    private readonly static SetSlim<TupleDescriptor> newDescriptors;
    private readonly static Queue<TupleDescriptor> initializationQueue;

    public static TupleDescriptor Register(TupleDescriptor descriptor)
    {
      var existing = initializedDescriptors[descriptor];
      if (existing != null)
        return existing;
      existing = newDescriptors[descriptor];
      if (existing != null)
        return existing;
      lock (_lock) {
        existing = initializedDescriptors[descriptor];
        if (existing != null)
          return existing;
        existing = newDescriptors[descriptor];
        if (existing != null)
          return existing;

        int fieldCount = descriptor.Count;
        if (fieldCount > MaxGeneratedTupleLength.Value) {
          descriptor.Head(MaxGeneratedTupleLength.Value);
          descriptor.Tail(fieldCount - MaxGeneratedTupleLength.Value);
        }

        initializationQueue.Enqueue(descriptor);
        newDescriptors.Add(descriptor);
        return descriptor;
      }
    }

    public static void Initialize()
    {
      lock (_lock) {
        var newlyCompiled = new SetSlim<TupleDescriptor>();
        try {
          while (initializationQueue.Count > 0) {
            var descriptor = initializationQueue.Dequeue();
            if (descriptor == null)
              continue;
            descriptor.Initialize(TupleFactory.Create(descriptor));
            newlyCompiled.Add(descriptor);

          }
        }
        finally {
          newDescriptors.Clear();
          try {
            initializedDescriptors.UnionWith(newlyCompiled);
          }
          catch {}
        }
      }
    }


    // Constructors

    static TupleDescriptorCache()
    {
      initializedDescriptors = new SetSlim<TupleDescriptor>();
      newDescriptors = new SetSlim<TupleDescriptor>();
      initializationQueue = new Queue<TupleDescriptor>();
      initializedDescriptors.Add(EmptyTupleDescriptor.Instance);
    }
  }
}