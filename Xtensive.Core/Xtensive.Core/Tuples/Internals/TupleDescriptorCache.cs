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

namespace Xtensive.Core.Tuples.Internals
{
  internal static class TupleDescriptorCache
  {
    private readonly static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
    private readonly static SetSlim<TupleDescriptor> initializedDescriptors = new SetSlim<TupleDescriptor>();
    private readonly static WeakestCache<TupleDescriptor, TupleDescriptor> newDescriptors = new WeakestCache<TupleDescriptor, TupleDescriptor>(false, false, td => td);

    public static TupleDescriptor Register(TupleDescriptor descriptor)
    {
      _lock.BeginRead();
      try {
        TupleDescriptor existing = initializedDescriptors[descriptor];
        if (existing!=null)
          return existing;
        existing = newDescriptors[descriptor, true];
        if (existing!=null)
          return existing;
        LockCookie? c = _lock.BeginWrite();
        try {
          newDescriptors.Add(descriptor, true);
          return descriptor;
        }
        finally {
          _lock.EndWrite(c);
        }
      }
      finally {
        _lock.EndRead();
      }
    }

    public static void Initialize()
    {
      var c = _lock.BeginWrite();
      try {
        var descriptors = new List<TupleDescriptor>(newDescriptors);
        var newlyCompiled = new SetSlim<TupleDescriptor>();
        try {
          foreach (TupleDescriptor tupleDescriptor in descriptors) {
            if (tupleDescriptor==null)
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
          catch {}
        }
      }
      finally {
        _lock.EndWrite(c);
      }
    }
  }
}