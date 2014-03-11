// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.07.13

using System;
using Xtensive.Core;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Model;
using ComparerProvider=Xtensive.Comparison.ComparerProvider;

namespace Xtensive.Orm.Internals
{
  [Serializable]
  internal sealed class Key<T1, T2, T3, T4> : Key
  {
    private static readonly Predicate<T1, T1> equalityComparer1 = 
      ComparerProvider.Default.GetComparer<T1>().Equals;
    private static readonly Predicate<T2, T2> equalityComparer2 = 
      ComparerProvider.Default.GetComparer<T2>().Equals;
    private static readonly Predicate<T3, T3> equalityComparer3 = 
      ComparerProvider.Default.GetComparer<T3>().Equals;
    private static readonly Predicate<T4, T4> equalityComparer4 = 
      ComparerProvider.Default.GetComparer<T4>().Equals;

    private readonly T1 value1;
    private readonly T2 value2;
    private readonly T3 value3;
    private readonly T4 value4;

    protected override Tuple GetValue()
    {
      var result = CreateTuple();
      result.SetValue(0, value1);
      result.SetValue(1, value2);
      result.SetValue(2, value3);
      result.SetValue(3, value4);
      return result;
    }

    protected override bool ValueEquals(Key other)
    {
      var otherKey = other as Key<T1, T2, T3, T4>;
      if (otherKey == null)
        return false;
      return
        equalityComparer4.Invoke(value4, otherKey.value4) && 
        equalityComparer3.Invoke(value3, otherKey.value3) && 
        equalityComparer2.Invoke(value2, otherKey.value2) && 
        equalityComparer1.Invoke(value1, otherKey.value1);
    }

    protected override int CalculateHashCode()
    {
      var result = Tuple.HashCodeMultiplier * 0 ^ value1.GetHashCode();
      result = (Tuple.HashCodeMultiplier * result ^ value2.GetHashCode());
      result = (Tuple.HashCodeMultiplier * result ^ value3.GetHashCode());
      result = (Tuple.HashCodeMultiplier * result ^ value4.GetHashCode());
      return result ^ TypeReference.Type.Key.EqualityIdentifier.GetHashCode();
    }

    public static Key Create(string nodeId, TypeInfo type, Tuple tuple, TypeReferenceAccuracy accuracy, int[] keyIndexes)
    {
      return new Key<T1, T2, T3, T4>(nodeId, type, accuracy,
        tuple.GetValueOrDefault<T1>(keyIndexes[0]),
        tuple.GetValueOrDefault<T2>(keyIndexes[1]),
        tuple.GetValueOrDefault<T3>(keyIndexes[2]),
        tuple.GetValueOrDefault<T4>(keyIndexes[3]));
    }

    public static Key Create(string nodeId, TypeInfo type, Tuple tuple, TypeReferenceAccuracy accuracy)
    {
      return new Key<T1, T2, T3, T4>(nodeId, type, accuracy,
        tuple.GetValueOrDefault<T1>(0),
        tuple.GetValueOrDefault<T2>(1),
        tuple.GetValueOrDefault<T3>(2),
        tuple.GetValueOrDefault<T4>(3));
    }

    
    // Constructors

    internal Key(string nodeId, TypeInfo type, TypeReferenceAccuracy accuracy, T1 value1, T2 value2, T3 value3, T4 value4)
      : base(nodeId, type, accuracy, null)
    {
      this.value1 = value1;
      this.value2 = value2;
      this.value3 = value3;
      this.value4 = value4;
    }
  }
}