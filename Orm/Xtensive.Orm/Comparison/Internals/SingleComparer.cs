// Copyright (C) 2007-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2007.11.14

using System;
using System.Runtime.Serialization;

namespace Xtensive.Comparison
{
  [Serializable]
  internal sealed class SingleComparer : ValueTypeComparer<float>
  {
    protected override IAdvancedComparer<float> CreateNew(ComparisonRules rules)
      => new SingleComparer(Provider, ComparisonRules.Combine(rules));


    // Constructors

    public SingleComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ValueRangeInfo = new ValueRangeInfo<float>(true, float.MinValue, true, float.MaxValue, false, default(float));
    }

    public SingleComparer(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}