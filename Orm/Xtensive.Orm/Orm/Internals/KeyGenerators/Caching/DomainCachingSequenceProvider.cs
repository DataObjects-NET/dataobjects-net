// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.05.17

using Xtensive.Core;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Internals.KeyGenerators
{
  internal sealed class DomainCachingSequenceProvider<TValue> : ICachingSequenceProvider<TValue>
  {
    private readonly CachingSequence<TValue> sequence;

    public CachingSequence<TValue> GetSequence(Session session)
    {
      return sequence;
    }

    // Constructor

    public DomainCachingSequenceProvider(IStorageSequenceAccessor accessor)
    {
      ArgumentValidator.EnsureArgumentNotNull(accessor, "accessor");

      sequence = new CachingSequence<TValue>(accessor, true);
    }
  }
}