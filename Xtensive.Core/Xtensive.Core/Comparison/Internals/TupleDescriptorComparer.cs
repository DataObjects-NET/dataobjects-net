// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.22

using System;
using System.Reflection;
using Xtensive.Core.Threading;
using Xtensive.Core.Tuples;

namespace Xtensive.Core.Comparison
{
  [Serializable]
  internal sealed class TupleDescriptorComparer: WrappingComparer<TupleDescriptor, Type[]>,
    ISystemComparer<TupleDescriptor>
  {
    [NonSerialized]
    private ThreadSafeDictionary<Pair<TupleDescriptor>, int> results;

    protected override IAdvancedComparer<TupleDescriptor> CreateNew(ComparisonRules rules)
    {
      return new TupleDescriptorComparer(Provider, ComparisonRules.Combine(rules));
    }

    public override int Compare(TupleDescriptor x, TupleDescriptor y)
    {
      if (x==y)
        return 0;
      return results.GetValue(new Pair<TupleDescriptor>(x, y), 
        (pair, _this) => _this.BaseComparer.Compare(pair.First.fieldTypes, pair.Second.fieldTypes), 
        this);
    }

    public override bool Equals(TupleDescriptor x, TupleDescriptor y)
    {
      return x==y;
    }

    public override int GetHashCode(TupleDescriptor obj)
    {
      return AdvancedComparerStruct<TupleDescriptor>.System.GetHashCode(obj);
    }

    private void Initialize()
    {
      results = ThreadSafeDictionary<Pair<TupleDescriptor>, int>.Create(new object());
    }


    // Constructors

    public TupleDescriptorComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      Initialize();
    }

    public override void OnDeserialization(object sender)
    {
      Initialize();
    }
  }
}
