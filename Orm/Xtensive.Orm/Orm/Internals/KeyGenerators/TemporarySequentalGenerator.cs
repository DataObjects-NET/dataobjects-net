// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.08

using Xtensive.Arithmetic;
using Xtensive.Comparison;
using Xtensive.Orm.Model;
using Xtensive.Tuples;

namespace Xtensive.Orm.Internals.KeyGenerators
{
  public sealed class TemporarySequentalGenerator<TValue> : TemporaryKeyGenerator
  {
    private static readonly Arithmetic<TValue> ValueArithmetic = Arithmetic<TValue>.Default;
    private static readonly AdvancedComparer<TValue> ValueComparer = AdvancedComparer<TValue>.Default;

    private readonly object syncRoot = new object(); // guards access to currentValue

    private Tuple prototype;
    private TValue currentValue;

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

    private TValue GenerateValue()
    {
      TValue result;
      lock (syncRoot) {
        result = currentValue = ValueArithmetic.Subtract(currentValue, ValueArithmetic.One);
      }
      return result;
    }

    public override bool IsTemporaryKey(Tuple keyTuple)
    {
      var value = keyTuple.GetValue<TValue>(0);

      if (ValueArithmetic.IsSigned)
        return ValueComparer.Compare(value, ValueArithmetic.Zero) < 0;

      var maxSigned = ValueArithmetic.Divide(ValueArithmetic.MaxValue, 2);
      return ValueComparer.Compare(maxSigned, value) < 0;
    }
  }
}