// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.05.17

using System;
using System.Collections.Generic;
using Xtensive.Arithmetic;
using Xtensive.Core;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Internals.KeyGenerators
{
  internal sealed class CachingSequence<TValue>
  {
    private static readonly Arithmetic<TValue> ValueArithmetic = Arithmetic<TValue>.Default;

    private readonly IStorageSequenceAccessor accessor;
    private readonly Queue<TValue> cachedValues = new Queue<TValue>();
    private readonly object syncRoot;

    public TValue GetNextValue(SequenceInfo sequenceInfo, Session session)
    {
      if (syncRoot==null)
        return GetNextValueUnsafe(sequenceInfo, session);

      lock (syncRoot)
        return GetNextValueUnsafe(sequenceInfo, session);
    }

    public void Reset()
    {
      if (syncRoot==null) {
        cachedValues.Clear();
        return;
      }

      lock (syncRoot)
        cachedValues.Clear();
    }

    private TValue GetNextValueUnsafe(SequenceInfo sequenceInfo, Session session)
    {
      if (cachedValues.Count==0)
        CacheValues(sequenceInfo, session);
      return cachedValues.Dequeue();
    }

    private void CacheValues(SequenceInfo sequenceInfo, Session session)
    {
      var values = accessor.NextBulk(sequenceInfo, session);
      var current = (TValue) Convert.ChangeType(values.Offset, typeof (TValue));
      for (int i = 0; i < values.Length; i++) {
        cachedValues.Enqueue(current);
        current = ValueArithmetic.Add(current, ValueArithmetic.One);
      }
    }

    // Constructors

    public CachingSequence(IStorageSequenceAccessor accessor, bool threadSafe)
    {
      ArgumentValidator.EnsureArgumentNotNull(accessor, "accessor");

      this.accessor = accessor;

      if (threadSafe)
        syncRoot = new object();
    }
  }
}