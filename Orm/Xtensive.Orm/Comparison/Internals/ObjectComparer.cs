// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.01.23

using System;

namespace Xtensive.Comparison
{
  // Fall back to Comparer<T>.Default, EqualityComparer<T>.Default
  internal sealed class ObjectComparer<T>: AdvancedComparerBase<T>
    where T: class, IComparable<T>, IEquatable<T>
  {
    private readonly AdvancedComparer<T> systemComparer;
    private readonly int nullHashCode;

    protected override IAdvancedComparer<T> CreateNew(ComparisonRules rules)
      => new ObjectComparer<T>(Provider, ComparisonRules.Combine(rules));

    public override int Compare(T x, T y)
    {
      if (ReferenceEquals(x, null)) {
        return ReferenceEquals(y, null) ? 0 : -DefaultDirectionMultiplier;
      }
      else {
        return ReferenceEquals(y, null)
          ? DefaultDirectionMultiplier
          : x.CompareTo(y) * DefaultDirectionMultiplier;
      }
    }

    public override bool Equals(T x, T y)
    {
      if (ReferenceEquals(x, null)) {
        return ReferenceEquals(y, null);
      }
      else {
        return !ReferenceEquals(y, null) && x.Equals(y);
      }
    }

    public override int GetHashCode(T obj)
      => ReferenceEquals(obj, null) ? nullHashCode : obj.GetHashCode();


    // Constructors

    public ObjectComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      systemComparer = new AdvancedComparer<T>(null);
      nullHashCode = systemComparer.GetHashCode(null);
      ValueRangeInfo = systemComparer.ValueRangeInfo;
    }
  }
}
