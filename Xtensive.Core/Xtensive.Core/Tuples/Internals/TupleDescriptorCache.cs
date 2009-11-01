// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.24

using System.Collections.Generic;
using System.Threading;
using Xtensive.Core.Collections;
using Xtensive.Core.Threading;

namespace Xtensive.Core.Tuples.Internals
{
  internal static class TupleDescriptorCache
  {
    private readonly static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
    private readonly static SetSlim<TupleDescriptor>     compiledDescriptors    = new SetSlim<TupleDescriptor>();
    private readonly static WeakSetSlim<TupleDescriptor> newDescriptors         = new WeakSetSlim<TupleDescriptor>();

    public static TupleDescriptor Register(TupleDescriptor sample)
    {
      _lock.BeginRead();
      try {
        TupleDescriptor existing = compiledDescriptors[sample];
        if (existing!=null)
          return existing;
        existing = newDescriptors[sample];
        if (existing!=null)
          return existing;
        LockCookie? c = _lock.BeginWrite();
        try {
          newDescriptors.Add(TupleDescriptorGenerator.Generate(sample));
          return newDescriptors[sample];
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
      LockCookie? c = _lock.BeginWrite();
      try {
        List<TupleDescriptor> descriptors = new List<TupleDescriptor>(newDescriptors);
        SetSlim<TupleDescriptor> newlyCompiled = new SetSlim<TupleDescriptor>();
        try {
          TupleGenerator tupleGenerator = new TupleGenerator();
          foreach (TupleDescriptor tupleDescriptor in descriptors) {
            if (tupleDescriptor==null)
              continue;
            TupleInfo tupleInfo = new TupleInfo(tupleDescriptor);
            Tuple compiledTuple = tupleGenerator.Compile(tupleInfo);
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