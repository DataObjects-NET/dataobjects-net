// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2008.01.16

using System;
using System.Runtime.Serialization;

namespace Xtensive.Comparison
{
  [Serializable]
  internal sealed class ArrayComparer<T> : WrappingComparer<T[], T>, 
    ISystemComparer<T[]>
  {
    protected override ArrayComparer<T> CreateNew(ComparisonRules rules)
      => new ArrayComparer<T>(Provider, ComparisonRules.Combine(rules));

    public override int Compare(T[] x, T[] y)
    {
      if (x == null) {
        return y == null ? 0 : -DefaultDirectionMultiplier;
      }
      if (y == null) {
        return DefaultDirectionMultiplier;
      }

      var minLength = x.Length;
      var result = -minLength - 1;
      if (minLength > y.Length) {
        minLength = y.Length;
        result = minLength + 1;
      }
      for (var i = 0; i < minLength;) {
        var r = BaseComparer.Compare(x[i], y[i]);
        i++;
        if (r == 0) {
          continue;
        }
        return r > 0 ? i : -i;
      }
      return result * (int) ComparisonRules.GetDefaultRuleDirection(minLength);
    }

    public override bool Equals(T[] x, T[] y)
    {
      if (x == null) {
        return y == null;
      }
      if (y == null) {
        return false;
      }
      var r = x.Length - y.Length;
      if (r != 0) {
        return false;
      }
      for (var i = x.Length - 1; i >= 0; i--) {
        if (!BaseComparer.Equals(x[i], y[i])) {
          return false;
        }
      }
      return true;
    }

    public override int GetHashCode(T[] obj)
    {
      if (obj == null) {
        return 0;
      }
      var hashCode = 0;
      for (var i = 0; i<obj.Length; i++) {
        hashCode ^= BaseComparer.GetHashCode(obj[i]);
      }

      return hashCode;
    }


    // Constructors

    public ArrayComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ValueRangeInfo = new ValueRangeInfo<T[]>(true, null, false, null, false, null);
    }

    public ArrayComparer(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}
