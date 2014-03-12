// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.07.13

using System;
using JetBrains.Annotations;
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
      var result = CreateTuple();
      result.SetValue(0, value1);
      return result;
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

    [UsedImplicitly]
    public static Key Create(string nodeId, TypeInfo type, Tuple tuple, TypeReferenceAccuracy accuracy, int[] keyIndexes)
    {
      return new Key<T>(nodeId, type, accuracy, tuple.GetValueOrDefault<T>(keyIndexes[0]));
    }

    [UsedImplicitly]
    public static Key Create(string nodeId, TypeInfo type, Tuple tuple, TypeReferenceAccuracy accuracy)
    {
      return new Key<T>(nodeId, type, accuracy, tuple.GetValueOrDefault<T>(0));
    }

    
    // Constructors

    internal Key(string nodeId, TypeInfo type, TypeReferenceAccuracy accuracy, T value)
      : base(nodeId, type, accuracy, null)
    {
      value1 = value;
    }
  }
}