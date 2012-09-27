// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.08

using Xtensive.Core;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Internals.KeyGenerators
{
  public sealed class StorageSequentalGenerator<TValue> : KeyGenerator
  {
    private Tuple prototype;
    private ICachingSequenceProvider<TValue> sequenceProvider;

    public override void Initialize(Domain ownerDomain, TupleDescriptor keyTupleDescriptor)
    {
      prototype = Tuple.Create(keyTupleDescriptor);

      var accessor = ownerDomain.Services.Demand<IStorageSequenceAccessor>();

      if (ownerDomain.StorageProviderInfo.Supports(ProviderFeatures.TransactionalKeyGenerators))
        sequenceProvider = new SessionCachingSequenceProvider<TValue>(accessor);
      else
        sequenceProvider = new DomainCachingSequenceProvider<TValue>(accessor);
    }

    public override Tuple GenerateKey(KeyInfo keyInfo, Session session)
    {
      var sequence = sequenceProvider.GetSequence(session);
      var keyValue = sequence.GetNextValue(keyInfo.Sequence, session);
      var keyTuple = prototype.CreateNew();
      keyTuple.SetValue(0, keyValue);
      return keyTuple;
    }
  }
}