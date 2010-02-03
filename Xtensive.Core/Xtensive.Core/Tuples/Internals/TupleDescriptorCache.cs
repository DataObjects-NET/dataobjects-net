// Copyright (C) 2008 Xtensive LLC.
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
    private readonly static SetSlim<TupleDescriptor> compiledDescriptors = new SetSlim<TupleDescriptor>();
    private readonly static WeakestCache<TupleDescriptor, TupleDescriptor> newDescriptors = new WeakestCache<TupleDescriptor, TupleDescriptor>(false, false, td => td);

    public static TupleDescriptor Register(TupleDescriptor sample)
    {
      _lock.BeginRead();
      try {
        TupleDescriptor existing = compiledDescriptors[sample];
        if (existing!=null)
          return existing;
        existing = newDescriptors[sample, true];
        if (existing!=null)
          return existing;
        LockCookie? c = _lock.BeginWrite();
        try {
          newDescriptors.Add(TupleDescriptorGenerator.Generate(sample), true);
          return newDescriptors[sample, true];
        }
        finally {
          _lock.EndWrite(c);
        }
      }
      finally {
        _lock.EndRead();
      }
    }

    public static void Compile()
    {
      var c = _lock.BeginWrite();
      try {
        var descriptors = new List<TupleDescriptor>(newDescriptors);
        var newlyCompiled = new SetSlim<TupleDescriptor>();
        try {
          foreach (TupleDescriptor tupleDescriptor in descriptors) {
            if (tupleDescriptor==null)
              continue;
            var tupleInfo = new TupleInfo(tupleDescriptor);
            var compiledTuple = TupleGenerator.Compile(tupleInfo);
            tupleDescriptor.Compiled(compiledTuple);
            newlyCompiled.Add(tupleDescriptor);
          }
        }
        finally {
          newDescriptors.Clear();
          try {
            compiledDescriptors.UnionWith(newlyCompiled);
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