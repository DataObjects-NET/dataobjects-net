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
  internal sealed class ByteComparer : ValueTypeComparer<byte>
  {
    protected override IAdvancedComparer<byte> CreateNew(ComparisonRules rules)
      => new ByteComparer(Provider, ComparisonRules.Combine(rules));


    // Constructors

    public ByteComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ValueRangeInfo = new ValueRangeInfo<byte>(true, byte.MinValue, true, byte.MaxValue, true, 1);
    }

    public ByteComparer(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}