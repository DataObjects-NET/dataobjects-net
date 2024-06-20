// Copyright (C) 2007-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Nick Svetlov
// Created:    2007.11.28

using System;
using System.Runtime.Serialization;

namespace Xtensive.Comparison
{
  [Serializable]
  internal sealed class SByteComparer : ValueTypeComparer<sbyte>
  {
    protected override SByteComparer CreateNew(ComparisonRules rules)
      => new SByteComparer(Provider, ComparisonRules.Combine(rules));


    // Constructors

    public SByteComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ValueRangeInfo = new ValueRangeInfo<sbyte>(true, sbyte.MinValue, true, sbyte.MaxValue, true, 1);
    }

    public SByteComparer(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}