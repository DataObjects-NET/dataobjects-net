// Copyright (C) 2007-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Nick Svetlov
// Created:    2007.11.28

using System.Runtime.Serialization;

namespace Xtensive.Comparison
{
  [Serializable]
  internal sealed class Int16Comparer : ValueTypeComparer<short>
  {
    protected override IAdvancedComparer<short> CreateNew(ComparisonRules rules)
      => new Int16Comparer(Provider, ComparisonRules.Combine(rules));


    // Constructors

    public Int16Comparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ValueRangeInfo = new ValueRangeInfo<short>(true, short.MinValue, true, short.MaxValue, true, 1);
    }

    public Int16Comparer(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}