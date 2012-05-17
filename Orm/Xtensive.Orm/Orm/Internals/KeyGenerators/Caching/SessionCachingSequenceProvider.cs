// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.05.17

using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Internals.KeyGenerators
{
  internal sealed class SessionCachingSequenceProvider<TValue> : ICachingSequenceProvider<TValue>
  {
    private sealed class CachingSequenceCollection
    {
      private readonly Dictionary<SessionCachingSequenceProvider<TValue>, CachingSequence<TValue>> sequences
        = new Dictionary<SessionCachingSequenceProvider<TValue>, CachingSequence<TValue>>();

      public CachingSequence<TValue> GetSequence(SessionCachingSequenceProvider<TValue> provider)
      {
        CachingSequence<TValue> result;
        if (!sequences.TryGetValue(provider, out result)) {
          result = new CachingSequence<TValue>(provider.accessor, false);
          sequences.Add(provider, result);
        }
        return result;
      }

      private void OnTransactionCompleted(object sender, TransactionEventArgs e)
      {
        foreach (var sequence in sequences.Values)
          sequence.Reset();
      }

      // Constructors

      public CachingSequenceCollection(Session session)
      {
        session.SystemEvents.TransactionRollbacked += OnTransactionCompleted;
        session.SystemEvents.TransactionCommitted += OnTransactionCompleted;
      }
    }

    private readonly IStorageSequenceAccessor accessor;

    public CachingSequence<TValue> GetSequence(Session session)
    {
      var items = session.Extensions.Get<CachingSequenceCollection>();
      if (items==null) {
        items = new CachingSequenceCollection(session);
        session.Extensions.Set(items);
      }
      return items.GetSequence(this);
    }

    // Constructors

    public SessionCachingSequenceProvider(IStorageSequenceAccessor accessor)
    {
      ArgumentValidator.EnsureArgumentNotNull(accessor, "accessor");

      this.accessor = accessor;
    }
  }
}