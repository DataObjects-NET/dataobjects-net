// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.01.29

using System;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Comparison
{
  internal sealed class TupleComparer : AdvancedComparerBase<Tuple>,
    ISystemComparer<Tuple>
  {
    private readonly int nullHashCode;

    protected override TupleComparer CreateNew(ComparisonRules rules) => new(Provider, ComparisonRules.Combine(rules));

    public override int Compare(Tuple x, Tuple y)
      => throw new NotSupportedException();

    public override bool Equals(Tuple x, Tuple y) => object.Equals(x, y);

    public override int GetHashCode(Tuple obj)
    {
      return obj is null
        ? nullHashCode
        : obj.GetHashCode();
    }


    // Constructors

    public TupleComparer(IComparerProvider provider, ComparisonRules comparisonRules) 
      : base(provider, comparisonRules)
    {
      nullHashCode = SystemComparerStruct<Tuple>.Instance.GetHashCode(null);
    }
  }
}
