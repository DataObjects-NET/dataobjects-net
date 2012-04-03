// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.08

using System;
using System.Collections.Generic;
using Xtensive.Arithmetic;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Internals.KeyGenerators
{
  public sealed class StorageSequentalGenerator<TValue> : IKeyGenerator
  {
    private static readonly Arithmetic<TValue> ValueArithmetic = Arithmetic<TValue>.Default;

    private readonly object syncRoot = new object(); // guard access to cachedValues

    private Domain domain;
    private Tuple prototype;
    private Queue<TValue> cachedValues;

    public void Initialize(Domain ownerDomain, TupleDescriptor keyTupleDescriptor)
    {
      domain = ownerDomain;
      prototype = Tuple.Create(keyTupleDescriptor);
      cachedValues = new Queue<TValue>();
    }

    public Tuple GenerateKey(KeyInfo keyInfo, Session session)
    {
      TValue keyValue;
      lock (syncRoot) {
        if (cachedValues.Count==0)
          CacheValues(keyInfo.Sequence, session);
        keyValue = cachedValues.Dequeue();
      }
      var keyTuple = prototype.CreateNew();
      keyTuple.SetValue(0, keyValue);
      return keyTuple;
    }

    private void CacheValues(SequenceInfo sequenceInfo, Session session)
    {
      var accessor = domain.Services.Get<IStorageSequenceAccessor>();
      var values = accessor.NextBulk(sequenceInfo, session);
      var current = (TValue) Convert.ChangeType(values.Offset, typeof (TValue));
      for (int i = 0; i < values.Length; i++) {
        cachedValues.Enqueue(current);
        current = ValueArithmetic.Add(current, ValueArithmetic.One);
      }
    }
  }
}