// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.07.13

using System;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;
using ComparerProvider=Xtensive.Core.Comparison.ComparerProvider;

namespace Xtensive.Storage.Internals
{
  [Serializable]
  internal sealed class Key<T> : Key
  {
    private static readonly Predicate<T, T> equalityComparer1 = 
      ComparerProvider.Default.GetComparer<T>().Equals;

    private readonly T value1;

    public override Tuple Value {
      get {
        if (value==null)
          value = Tuple.Create(value1);
        return value;
      }
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
      return (0 ^ value1.GetHashCode()) ^ Hierarchy.GetHashCode();
    }

    public static Key Create(HierarchyInfo hierarchy, TypeInfo type, Tuple tuple, int[] keyIndexes)
    {
      return new Key<T>(hierarchy, type, tuple.GetValueOrDefault<T>(keyIndexes[0]));
    }

    public static Key Create(HierarchyInfo hierarchy, TypeInfo type, Tuple tuple)
    {
      return new Key<T>(hierarchy, type, tuple.GetValueOrDefault<T>(0));
    }

    
    // Constructors

    internal Key(HierarchyInfo hierarchy, TypeInfo type, T value)
      : base(hierarchy, type, null)
    {
      value1 = value;
    }
  }
}