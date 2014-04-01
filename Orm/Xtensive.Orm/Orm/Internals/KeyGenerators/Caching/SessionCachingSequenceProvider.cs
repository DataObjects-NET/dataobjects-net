// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.05.17

using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Internals.KeyGenerators
{
  internal sealed class SessionCachingSequenceProvider<TValue> : ICachingSequenceProvider<TValue>
  {
    private sealed class CachingSequenceCollection
    {
      private readonly Dictionary<SequenceInfo, CachingSequence<TValue>> sequences
        = new Dictionary<SequenceInfo, CachingSequence<TValue>>();

      public CachingSequence<TValue> GetSequence(SequenceInfo sequenceInfo, IStorageSequenceAccessor accessor)
      {
        CachingSequence<TValue> result;
        if (!sequences.TryGetValue(sequenceInfo, out result)) {
          result = new CachingSequence<TValue>(accessor, false);
          sequences.Add(sequenceInfo, result);
        }
        return result;
      }

      private void OnTransactionRollbacked(object sender, TransactionEventArgs e)
      {
        foreach (var sequence in sequences.Values)
          sequence.Reset();
      }

      // Constructors

      public CachingSequenceCollection(Session session)
      {
        session.SystemEvents.TransactionRollbacked += OnTransactionRollbacked;
      }
    }

    private readonly IStorageSequenceAccessor accessor;

    public CachingSequence<TValue> GetSequence(SequenceInfo sequenceInfo, Session session)
    {
      var items = session.Extensions.Get<CachingSequenceCollection>();
      if (items==null) {
        items = new CachingSequenceCollection(session);
        session.Extensions.Set(items);
      }
      return items.GetSequence(sequenceInfo, accessor);
    }

    // Constructors

    public SessionCachingSequenceProvider(IStorageSequenceAccessor accessor)
    {
      ArgumentValidator.EnsureArgumentNotNull(accessor, "accessor");

      this.accessor = accessor;
    }
  }
}