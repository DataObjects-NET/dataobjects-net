// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.24

using System;
using System.Collections.Generic;
using System.Threading;
using Xtensive.Core.Caching;
using Xtensive.Core.Collections;
using Xtensive.Core.Threading;
using System.Linq;

namespace Xtensive.Core.Tuples.Internals
{
  internal static class TupleDescriptorCache
  {
    private readonly static object _lock = new object();
    private readonly static SetSlim<TupleDescriptor> initializedDescriptors;
    private readonly static WeakestCache<TupleDescriptor, TupleDescriptor> newDescriptors;

    public static TupleDescriptor Register(TupleDescriptor descriptor)
    {
      var existing = initializedDescriptors[descriptor];
      if (existing != null)
        return existing;
      existing = newDescriptors[descriptor, true];
      if (existing != null)
        return existing;
      lock (_lock) {
        existing = initializedDescriptors[descriptor];
        if (existing != null)
          return existing;
        existing = newDescriptors[descriptor, true];
        if (existing != null)
          return existing;

        if (descriptor.Count > MaxGeneratedTupleLength.Value) {
          descriptor.TrimFields(MaxGeneratedTupleLength.Value);
          descriptor.SkipFields(MaxGeneratedTupleLength.Value);
        }

        newDescriptors.Add(descriptor, true);
        return descriptor;
      }
    }

    public static void Initialize()
    {
      lock (_lock) {
        var descriptors = newDescriptors
          .OrderBy(d => d.Count)
          .ToList();
        var newlyCompiled = new SetSlim<TupleDescriptor>();
        try {
          foreach (TupleDescriptor tupleDescriptor in descriptors) {
            if (tupleDescriptor == null)
              continue;
            var tuple = TupleFactory.Create(tupleDescriptor);
            tupleDescriptor.Initialize(tuple);
            newlyCompiled.Add(tupleDescriptor);
          }
        }
        finally {
          newDescriptors.Clear();
          try {
            initializedDescriptors.UnionWith(newlyCompiled);
          }
          catch { }
        }
      }
    }


    // Constructors

    static TupleDescriptorCache()
    {
      initializedDescriptors = new SetSlim<TupleDescriptor>();
      newDescriptors = new WeakestCache<TupleDescriptor, TupleDescriptor>(false, false, td => td);
      initializedDescriptors.Add(EmptyTupleDescriptor.Instance);
    }
  }
}