// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.01.22

using System.Reflection;
using System.Runtime.Serialization;
using Xtensive.Collections;
using Xtensive.Core;

using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Comparison
{
  [Serializable]
  internal sealed class TupleDescriptorComparer: WrappingComparer<TupleDescriptor, Type[]>,
    ISystemComparer<TupleDescriptor>
  {
    protected override IAdvancedComparer<TupleDescriptor> CreateNew(ComparisonRules rules)
      => new TupleDescriptorComparer(Provider, ComparisonRules.Combine(rules));

    public override int Compare(TupleDescriptor x, TupleDescriptor y) => throw new NotSupportedException();

    public override bool Equals(TupleDescriptor x, TupleDescriptor y) => x == y;

    public override int GetHashCode(TupleDescriptor obj)
      => AdvancedComparerStruct<TupleDescriptor>.System.GetHashCode(obj);

    // Constructors

    public TupleDescriptorComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
    }

    public TupleDescriptorComparer(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    public override void OnDeserialization(object sender)
    {
    }
  }
}
