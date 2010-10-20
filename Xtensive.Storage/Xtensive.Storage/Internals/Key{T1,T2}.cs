// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.07.13

using System;
using Xtensive.Core;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Storage.Model;
using ComparerProvider=Xtensive.Comparison.ComparerProvider;

namespace Xtensive.Storage.Internals
{
  [Serializable]
  internal sealed class Key<T1, T2> : Key
  {
    private static readonly Predicate<T1, T1> equalityComparer1 = 
      ComparerProvider.Default.GetComparer<T1>().Equals;
    private static readonly Predicate<T2, T2> equalityComparer2 = 
      ComparerProvider.Default.GetComparer<T2>().Equals;

    private readonly T1 value1;
    private readonly T2 value2;

    protected override Tuple GetValue()
    {
      return Tuple.Create(value1, value2);
    }

    protected override bool ValueEquals(Key other)
    {
      var otherKey = other as Key<T1, T2>;
      if (otherKey == null)
        return false;
      return
        equalityComparer2.Invoke(value2, otherKey.value2) && 
        equalityComparer1.Invoke(value1, otherKey.value1);
    }

    protected override int CalculateHashCode()
    {
      var result = Tuple.HashCodeMultiplier * 0 ^ value1.GetHashCode();
      result = (Tuple.HashCodeMultiplier * result ^ value2.GetHashCode());
      return result ^ TypeReference.Type.Key.EqualityIdentifier.GetHashCode();
    }

    public static Key Create(TypeInfo type, Tuple tuple, TypeReferenceAccuracy accuracy, int[] keyIndexes)
    {
      return new Key<T1, T2>(type, accuracy,
        tuple.GetValueOrDefault<T1>(keyIndexes[0]),
        tuple.GetValueOrDefault<T2>(keyIndexes[1]));
    }

    public static Key Create(TypeInfo type, Tuple tuple, TypeReferenceAccuracy accuracy)
    {
      return new Key<T1, T2>(type, accuracy,
        tuple.GetValueOrDefault<T1>(0),
        tuple.GetValueOrDefault<T2>(1));
    }

    
    // Constructors

    internal Key(TypeInfo type, TypeReferenceAccuracy accuracy, T1 value1, T2 value2)
      : base(type, accuracy, null)
    {
      this.value1 = value1;
      this.value2 = value2;
    }
  }
}