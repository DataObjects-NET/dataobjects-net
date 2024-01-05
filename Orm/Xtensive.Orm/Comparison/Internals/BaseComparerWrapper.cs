// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.01.22

using System.Runtime.Serialization;
using Xtensive.Core;

namespace Xtensive.Comparison
{
  [Serializable]
  internal class BaseComparerWrapper<T, TBase>: WrappingComparer<T, TBase>
    where T: TBase
  {
    protected override IAdvancedComparer<T> CreateNew(ComparisonRules rules)
      => new BaseComparerWrapper<T, TBase>(Provider, ComparisonRules.Combine(rules));

    public override int Compare(T x, T y) => BaseComparer.Compare(x, y);

    public override bool Equals(T x, T y) => BaseComparer.Equals(x, y);

    public override int GetHashCode(T obj) => BaseComparer.GetHashCode(obj);

    public override T GetNearestValue(T value, Direction direction)
      => (T) BaseComparer.GetNearestValue(value, direction);


    // Constructors

    public BaseComparerWrapper(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
    }

    public BaseComparerWrapper(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}