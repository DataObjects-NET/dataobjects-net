// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
        return ReferenceEquals(y, null) ? false : x.Equals(y);
      }
    }

    public override int GetHashCode(T obj)
      => ReferenceEquals(obj, null) ? nullHashCode : obj.GetHashCode();

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

    public ObjectComparer(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    public override void OnDeserialization(object sender)
    {
      base.OnDeserialization(sender);
      Initialize();
    }
  }
}
