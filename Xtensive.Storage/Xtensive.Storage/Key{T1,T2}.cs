// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.07.13

using System;
using System.Threading;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  [Serializable]
  internal sealed class Key<T1, T2> : Key
  {
    private readonly T1 value1;
    private readonly T2 value2;

    public override Tuple Value {
      get {
        if (value==null)
          value = Tuple.Create(value1, value2);
        return value;
      }
    }

    protected override bool ValueEquals(Key other)
    {
      var otherKey = other as Key<T1, T2>;
      if (otherKey != null)
        return 
          value2.Equals(otherKey.value2) && 
          value1.Equals(otherKey.value1);
      return 
        value2.Equals(other.Value.GetValue<T1>(1)) && 
        value1.Equals(other.Value.GetValue<T2>(2));
    }

    protected override int CalculateHashCode()
    {
      var result = Tuple.HashCodeMultiplier * 0 ^ value1.GetHashCode();
      return (Tuple.HashCodeMultiplier * result ^ value2.GetHashCode()) ^ Hierarchy.GetHashCode();
    }

    public static Key Create(HierarchyInfo hierarchy, TypeInfo type, Tuple tuple, int[] keyIndexes)
    {
      return new Key<T1, T2>(hierarchy, type, 
        tuple.GetValueOrDefault<T1>(keyIndexes[0]),
        tuple.GetValueOrDefault<T2>(keyIndexes[1]));
    }

    public static Key Create(HierarchyInfo hierarchy, TypeInfo type, Tuple tuple)
    {
      return new Key<T1, T2>(hierarchy, type, 
        tuple.GetValueOrDefault<T1>(0),
        tuple.GetValueOrDefault<T2>(1));
    }

    
    // Constructors

    internal Key(HierarchyInfo hierarchy, TypeInfo type, T1 value1, T2 value2)
      : base(hierarchy, type, null)
    {
      this.value1 = value1;
      this.value2 = value2;
    }
  }
}