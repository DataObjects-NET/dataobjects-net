// Copyright (C) 2007-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Nick Svetlov
// Created:    2007.11.28

using System.Runtime.Serialization;

namespace Xtensive.Comparison
{
  [Serializable]
  internal sealed class CharComparer : ValueTypeComparer<char>
  {
    protected override IAdvancedComparer<char> CreateNew(ComparisonRules rules)
      => new CharComparer(Provider, ComparisonRules.Combine(rules));


    // Constructors

    public CharComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      ValueRangeInfo = new ValueRangeInfo<char>(true, char.MinValue, true, char.MaxValue, true, '\x0001');
    }

    public CharComparer(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}