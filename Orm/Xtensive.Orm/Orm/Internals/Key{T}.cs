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
  internal sealed class Key<T> : Key
  {
    private static readonly Predicate<T, T> equalityComparer1 = 
      ComparerProvider.Default.GetComparer<T>().Equals;

    private readonly T value1;

    protected override Tuple GetValue()
    {
      return Tuple.Create(value1);
    }

    protected override bool ValueEquals(Key other)
    {
      var otherKey = other as Key<T>;
      if (otherKey == null)
        return false;
      return
        equalityComparer1.Invoke(value1, otherKey.value1);
    }

    protected override int CalculateHashCode()
    {
      return Tuple.HashCodeMultiplier ^ value1.GetHashCode() ^ TypeReference.Type.Key.EqualityIdentifier.GetHashCode();
    }

    public static Key Create(TypeInfo type, Tuple tuple, TypeReferenceAccuracy accuracy, int[] keyIndexes)
    {
      return new Key<T>(type, accuracy, tuple.GetValueOrDefault<T>(keyIndexes[0]));
    }

    public static Key Create(TypeInfo type, Tuple tuple, TypeReferenceAccuracy accuracy)
    {
      return new Key<T>(type, accuracy, tuple.GetValueOrDefault<T>(0));
    }

    
    // Constructors

    internal Key(TypeInfo type, TypeReferenceAccuracy accuracy, T value)
      : base(type, accuracy, null)
    {
      value1 = value;
    }
  }
}