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
  internal sealed class Key<T> : Key
  {
    private Tuple tupleValue;
    private readonly T value;

    public override Tuple Value
    {
      get
      {
        if (tupleValue == null) {
          tupleValue = Tuple.Create(value);
          Thread.MemoryBarrier();
        }
        return tupleValue;
      }
    }

    protected override bool ValueEquals(Key other)
    {
      var otherKey = other as Key<T>;
      if (otherKey != null)
        return value.Equals(otherKey.value);
      if (other.Value.Count != 1)
        return false;
      if (other.Value.Descriptor[0] != typeof(T))
        return false;
      return value.Equals(other.Value.GetValue<T>(0));
    }

    protected override int CalculateHashCode()
    {
      return (0 ^ value.GetHashCode()) ^ Hierarchy.GetHashCode();
    }

    internal static Key Create(Domain domain, TypeInfo type, int[] keyFields, Tuple tuple,
      bool exactType, bool canCache)
    {
      var key = new Key<T>(type.Hierarchy, exactType ? type : null, tuple.GetValue<T>(keyFields[0]));
      if (!canCache || domain==null)
        return key;
      var keyCache = domain.KeyCache;
      lock (keyCache) {
        Key foundKey;
        if (keyCache.TryGetItem(key, true, out foundKey))
          key = (Key<T>)foundKey;
        else {
          if (exactType)
            keyCache.Add(key);
        }
      }
      return key;
    }
    
    
    // Constructors

    internal Key(HierarchyInfo hierarchy, TypeInfo type, T value)
      : base(hierarchy, type, null)
    {
      this.value = value;
    }
  }
}