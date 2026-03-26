// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.01.29

using System;
using System.Runtime.Serialization;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Comparison
{
  [Serializable]
  internal sealed class TupleComparer : AdvancedComparerBase<Tuple>,
    ISystemComparer<Tuple>
  {
    [NonSerialized]
    private int nullHashCode;

    protected override IAdvancedComparer<Tuple> CreateNew(ComparisonRules rules)
      => new TupleComparer(Provider, ComparisonRules.Combine(rules));

    public override int Compare(Tuple x, Tuple y)
      => throw new NotSupportedException();

    public override bool Equals(Tuple x, Tuple y) => object.Equals(x, y);

    public override int GetHashCode(Tuple obj)
    {
      return obj is null
        ? nullHashCode
        : obj.GetHashCode();
    }

    private void Initialize()
      => nullHashCode = SystemComparerStruct<Tuple>.Instance.GetHashCode(null);


    // Constructors

    public TupleComparer(IComparerProvider provider, ComparisonRules comparisonRules) 
      : base(provider, comparisonRules)
    {
      Initialize();
    }

    public TupleComparer(SerializationInfo info, StreamingContext context)
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
