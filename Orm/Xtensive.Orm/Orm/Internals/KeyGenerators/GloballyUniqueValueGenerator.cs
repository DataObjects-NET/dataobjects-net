// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.08

using Xtensive.Orm.Model;
using Xtensive.Tuples;

namespace Xtensive.Orm.Internals.KeyGenerators
{
  public abstract class GloballyUniqueValueGenerator<TValue> : ITemporaryKeyGenerator
  {
    private Tuple prototype;

    public void Initialize(Domain ownerDomain, TupleDescriptor keyTupleDescriptor)
    {
      prototype = Tuple.Create(keyTupleDescriptor);
    }

    public Tuple GenerateKey(KeyInfo keyInfo, Session session)
    {
      var keyTuple = prototype.CreateNew();
      keyTuple.SetValue(0, GenerateValue());
      return keyTuple;
    }

    public bool IsTemporaryKey(Tuple keyTuple)
    {
      return false;
    }

    protected abstract TValue GenerateValue();
  }
}