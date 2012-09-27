// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.08

using Xtensive.Orm.Model;
using Xtensive.Tuples;

namespace Xtensive.Orm.Internals.KeyGenerators
{
  public abstract class GloballyUniqueValueGenerator<TValue> : TemporaryKeyGenerator
  {
    private Tuple prototype;

    public override void Initialize(Domain ownerDomain, TupleDescriptor keyTupleDescriptor)
    {
      prototype = Tuple.Create(keyTupleDescriptor);
    }

    public override Tuple GenerateKey(KeyInfo keyInfo, Session session)
    {
      var keyTuple = prototype.CreateNew();
      keyTuple.SetValue(0, GenerateValue());
      return keyTuple;
    }

    public override bool IsTemporaryKey(Tuple keyTuple)
    {
      return false;
    }

    protected abstract TValue GenerateValue();
  }
}