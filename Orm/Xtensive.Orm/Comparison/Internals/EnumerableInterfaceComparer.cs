// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2008.01.16

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Xtensive.Comparison
{
  [Serializable]
  internal sealed class EnumerableInterfaceComparer<TEnumerable, T> : WrappingComparer<TEnumerable, T>,
    ISystemComparer<TEnumerable>
    where TEnumerable: class, IEnumerable<T>
  {
    protected override EnumerableInterfaceComparer<TEnumerable, T> CreateNew(ComparisonRules rules)
      => new EnumerableInterfaceComparer<TEnumerable, T>(Provider, ComparisonRules.Combine(rules));

    public override int Compare(TEnumerable x, TEnumerable y)
    {
      if (x == null) {
        return y == null ? 0 : -DefaultDirectionMultiplier;
      }
      else {
        if (y == null) {
          return DefaultDirectionMultiplier;
        }
        else {
          var ex = x.GetEnumerator();
          var ey = y.GetEnumerator();
          var i = 1;
          while (true) {
            var hasX = ex.MoveNext();
            var hasY = ey.MoveNext();
            if (!hasX) {
              return !hasY ? 0 : -i * DefaultDirectionMultiplier;
            }
            else {
              if (!hasY) {
                return i * DefaultDirectionMultiplier;
              }
              else {
                var r = BaseComparer.Compare(ex.Current, ey.Current);
                if (r != 0) {
                  return r < 0 ? -i : i;
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
        return y == null;
      }
      else {
        if (y == null) {
          return false;
        }
        else {
          var ex = x.GetEnumerator();
          var ey = y.GetEnumerator();
          while (true) {
            var hasX = ex.MoveNext();
            var hasY = ey.MoveNext();
            if (!hasX) {
              return !hasY;
            }
            else {
              if (!hasY) {
                return false;
              }
              else {
                if (!BaseComparer.Equals(ex.Current, ey.Current)) {
                  return false;
                }
              }
            }
          }
        }
      }
    }

    public override int GetHashCode(TEnumerable obj)
    {
      if (obj == null) {
        return 0;
      }

      var hashCode = 0;
      foreach (T current in obj) {
        hashCode ^= BaseComparer.GetHashCode(current);
      }
      return hashCode;
    }


    // Constructors

    public EnumerableInterfaceComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ValueRangeInfo = new ValueRangeInfo<TEnumerable>(true, null, false, null, false, null);
    }

    public EnumerableInterfaceComparer(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
