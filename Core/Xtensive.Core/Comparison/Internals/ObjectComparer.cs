// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.23

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Xtensive.Comparison
{
  // Fall back to Comparer<T>.Default, EqualityComparer<T>.Default
  [Serializable]
  internal sealed class ObjectComparer<T>: AdvancedComparerBase<T>
    where T: class, IComparable<T>, IEquatable<T>
  {
    [NonSerialized]
    private AdvancedComparer<T> systemComparer;
    [NonSerialized]
    private int nullHashCode;

    protected override IAdvancedComparer<T> CreateNew(ComparisonRules rules)
    {
      return new ObjectComparer<T>(Provider, ComparisonRules.Combine(rules));
    }

    public override int Compare(T x, T y)
    {
      if (ReferenceEquals(x, null)) {
        if (ReferenceEquals(y, null))
          return 0;
        else
          return -DefaultDirectionMultiplier;
      }
      else {
        if (ReferenceEquals(y, null))
          return DefaultDirectionMultiplier;
        else
          return x.CompareTo(y) * DefaultDirectionMultiplier;
      }
    }

    public override bool Equals(T x, T y)
    {
      if (ReferenceEquals(x, null)) {
        if (ReferenceEquals(y, null))
          return true;
        else
          return false;
      }
      else {
        if (ReferenceEquals(y, null))
          return false;
        else
          return x.Equals(y);
      }
    }

    public override int GetHashCode(T obj)
    {
      if (ReferenceEquals(obj, null))
        return nullHashCode;
      else
        return obj.GetHashCode();
    }

    private void Initialize()
    {
      systemComparer = new AdvancedComparer<T>(null);
      nullHashCode   = systemComparer.GetHashCode(null);
      ValueRangeInfo = systemComparer.ValueRangeInfo;
    }


    // Constructors

    public ObjectComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      Initialize();
    }

    public override void OnDeserialization(object sender)
    {
      base.OnDeserialization(sender);
      Initialize();
    }
  }
}
