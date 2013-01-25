// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.22

using System;
using System.Reflection;
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
    {
      return new TupleDescriptorComparer(Provider, ComparisonRules.Combine(rules));
    }

    public override int Compare(TupleDescriptor x, TupleDescriptor y)
    {
      throw new NotSupportedException();
    }

    public override bool Equals(TupleDescriptor x, TupleDescriptor y)
    {
      return x==y;
    }

    public override int GetHashCode(TupleDescriptor obj)
    {
      return AdvancedComparerStruct<TupleDescriptor>.System.GetHashCode(obj);
    }

    // Constructors

    public TupleDescriptorComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
    }

    public override void OnDeserialization(object sender)
    {
    }
  }
}
