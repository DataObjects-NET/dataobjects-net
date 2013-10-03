// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.05.17

using System;
using Xtensive.Core;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Internals.KeyGenerators
{
  internal sealed class CachingSequence<TValue>
  {
    private readonly IStorageSequenceAccessor accessor;
    private readonly object syncRoot;

    private long nextValue;
    private long nextValueBound;

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
        ResetUnsafe();
        return;
      }

      lock (syncRoot)
        ResetUnsafe();
    }

    private void ResetUnsafe()
    {
      nextValue = 0;
      nextValueBound = 0;
    }

    private TValue GetNextValueUnsafe(SequenceInfo sequenceInfo, Session session)
    {
      if (nextValue==nextValueBound) {
        var values = accessor.NextBulk(sequenceInfo, session);
        nextValue = values.Offset;
        nextValueBound = values.EndOffset;
      }
      var result = nextValue;
      nextValue++;
      return (TValue) Convert.ChangeType(result, typeof (TValue));
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