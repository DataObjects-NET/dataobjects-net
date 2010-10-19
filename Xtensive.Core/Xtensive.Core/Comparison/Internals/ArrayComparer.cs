// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.16

using System;

namespace Xtensive.Comparison
{
  [Serializable]
  internal sealed class ArrayComparer<T> : WrappingComparer<T[], T>, 
    ISystemComparer<T[]>
  {
    protected override IAdvancedComparer<T[]> CreateNew(ComparisonRules rules)
    {
      return new ArrayComparer<T>(Provider, ComparisonRules.Combine(rules));
    }

    public override int Compare(T[] x, T[] y)
    {
      if (x==null) {
        if (y==null)
          return 0;
        return -DefaultDirectionMultiplier;
      }
      if (y==null)
        return DefaultDirectionMultiplier;
      int minLength = x.Length;
      int result = -minLength - 1;
      if (minLength > y.Length) {
        minLength = y.Length;
        result = minLength + 1;
      }
      for (int i = 0; i < minLength;) {
        int r = BaseComparer.Compare(x[i], y[i]);
        i++;
        if (r==0)
          continue;
        if (r > 0)
          return i;
        return -i;
      }
      return result * (int)ComparisonRules.GetDefaultRuleDirection(minLength);
    }

    public override bool Equals(T[] x, T[] y)
    {
      if (x==null) {
        if (y==null)
          return true;
        return false;
      }
      if (y==null)
        return false;
      int r = x.Length - y.Length;
      if (r!=0)
        return false;
      for (int i = x.Length - 1; i >= 0; i--) {
        if (!BaseComparer.Equals(x[i], y[i]))
          return false;
      }
      return true;
    }

    public override int GetHashCode(T[] obj)
    {
      if (obj==null)
        return 0;
      int hashCode = 0;
      for (int i = 0; i<obj.Length; i++)
        hashCode ^= BaseComparer.GetHashCode(obj[i]);
      return hashCode;
    }


    // Constructors

    public ArrayComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ValueRangeInfo = new ValueRangeInfo<T[]>(true, null, false, null, false, null);
    }
  }
}
