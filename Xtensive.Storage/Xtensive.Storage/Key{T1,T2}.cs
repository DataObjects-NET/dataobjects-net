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
    private Tuple tupleValue;
    private readonly T1 value1;
    private readonly T2 value2;

    public override Tuple Value
    {
      get
      {
        if (tupleValue == null) {
          tupleValue = Tuple.Create(value1, value2);
          Thread.MemoryBarrier();
        }
        return tupleValue;
      }
    }

    protected override bool ValueEquals(Key other)
    {
      var otherKey = other as Key<T1, T2>;
      if (otherKey != null)
        return value1.Equals(otherKey.value1) && value2.Equals(otherKey.value2);
      if (other.Value.Count != 2)
        return false;
      if (other.Value.Descriptor[0] != typeof(T1) || other.Value.Descriptor[1] != typeof(T2))
        return false;
      return value1.Equals(other.Value.GetValue<T1>(0)) && value2.Equals(other.Value.GetValue<T2>(1));
    }

    protected override int CalculateHashCode()
    {
      var result = Tuple.HashCodeMultiplier * 0 ^ value1.GetHashCode();
      return (Tuple.HashCodeMultiplier * result ^ value2.GetHashCode()) ^ Hierarchy.GetHashCode();
    }

    internal static Key Create(Domain domain, TypeInfo type, int[] keyFields, Tuple tuple,
      bool exactType, bool canCache)
    {
      var key = new Key<T1, T2>(type.Hierarchy, exactType ? type : null, tuple.GetValue<T1>(keyFields[0]),
        tuple.GetValue<T2>(keyFields[1]));
      if (!canCache || domain==null)
        return key;
      var keyCache = domain.KeyCache;
      lock (keyCache) {
        Key foundKey;
        if (keyCache.TryGetItem(key, true, out foundKey))
          key = (Key<T1,T2>)foundKey;
        else {
          if (exactType)
            keyCache.Add(key);
        }
      }
      return key;
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