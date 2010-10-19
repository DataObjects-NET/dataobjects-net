// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.16

using System;
using System.Collections.Generic;

namespace Xtensive.Comparison
{
  [Serializable]
  internal sealed class EnumerableInterfaceComparer<TEnumerable, T>: WrappingComparer<TEnumerable, T>,
    ISystemComparer<TEnumerable>
    where TEnumerable: class, IEnumerable<T>
  {
    protected override IAdvancedComparer<TEnumerable> CreateNew(ComparisonRules rules)
    {
      return new EnumerableInterfaceComparer<TEnumerable,T>(Provider, ComparisonRules.Combine(rules));
    }

    public override int Compare(TEnumerable x, TEnumerable y)
    {
      if (x == null) {
        if (y == null)
          return 0;
        else
          return -DefaultDirectionMultiplier;
      }
      else {
        if (y == null)
          return DefaultDirectionMultiplier;
        else {
          IEnumerator<T> ex = x.GetEnumerator();
          IEnumerator<T> ey = y.GetEnumerator();
          int i = 1;
          while (true) {
            bool hasX = ex.MoveNext();
            bool hasY = ey.MoveNext();
            if (!hasX) {
              if (!hasY)
                return 0;
              else
                return -i * DefaultDirectionMultiplier;
            }
            else {
              if (!hasY)
                return i * DefaultDirectionMultiplier;
              else {
                int r = BaseComparer.Compare(ex.Current, ey.Current);
                if (r != 0) {
                  if (r<0)
                    return -i;
                  else
                    return i;
                }
              }
            }
            i++;
          }
        }
      }
    }

    public override bool Equals(TEnumerable x, TEnumerable y)
    {
      if (x == null) {
        if (y == null)
          return true;
        else
          return false;
      }
      else {
        if (y == null)
          return false;
        else {
          IEnumerator<T> ex = x.GetEnumerator();
          IEnumerator<T> ey = y.GetEnumerator();
          while (true) {
            bool hasX = ex.MoveNext();
            bool hasY = ey.MoveNext();
            if (!hasX) {
              if (!hasY)
                return true;
              else
                return false;
            }
            else {
              if (!hasY)
                return false;
              else {
                if (!BaseComparer.Equals(ex.Current, ey.Current))
                  return false;
              }
            }
          }
        }
      }
    }

    public override int GetHashCode(TEnumerable obj)
    {
      if (obj == null)
        return 0;
      int hashCode = 0;
      foreach (T current in obj)
        hashCode ^= BaseComparer.GetHashCode(current);
      return hashCode;
    }


    // Constructors

    public EnumerableInterfaceComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ValueRangeInfo = new ValueRangeInfo<TEnumerable>(true, null, false, null, false, null);
    }
  }
}
