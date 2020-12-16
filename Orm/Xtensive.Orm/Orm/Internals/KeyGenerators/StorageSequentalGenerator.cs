// Copyright (C) 2012-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
    private ICachingSequenceProvider<TValue> sequenceProvider;

    public override void Initialize(Domain ownerDomain, TupleDescriptor keyTupleDescriptor)
    {
      var accessor = ownerDomain.Services.Demand<IStorageSequenceAccessor>();

      sequenceProvider = ownerDomain.StorageProviderInfo.Supports(ProviderFeatures.TransactionalKeyGenerators)
        ? new SessionCachingSequenceProvider<TValue>(accessor)
        : (ICachingSequenceProvider<TValue>) new DomainCachingSequenceProvider<TValue>(accessor);
    }

    public override Tuple GenerateKey(KeyInfo keyInfo, Session session)
    {
      var sequence = sequenceProvider.GetSequence(keyInfo.Sequence, session);
      var keyValue = sequence.GetNextValue(keyInfo.Sequence, session);
      var keyTuple = Tuple.Create(keyInfo.TupleDescriptor);
      keyTuple.SetValue(0, keyValue);
      return keyTuple;
    }
  }
}