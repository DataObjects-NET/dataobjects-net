// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.29

using System;
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
    {
      return new TupleComparer(Provider, ComparisonRules.Combine(rules));
    }

    public override int Compare(Tuple x, Tuple y)
    {
      throw new NotSupportedException();
    }

    public override bool Equals(Tuple x, Tuple y)
    {
      return object.Equals(x, y);
    }

    public override int GetHashCode(Tuple obj)
    {
      return ReferenceEquals(obj, null) 
        ? nullHashCode
        : obj.GetHashCode();
    }

    private void Initialize()
    {
      nullHashCode = SystemComparerStruct<Tuple>.Instance.GetHashCode(null);
    }


    // Constructors

    public TupleComparer(IComparerProvider provider, ComparisonRules comparisonRules) 
      : base(provider, comparisonRules)
    {
      Initialize();
    }

    public override void OnDeserialization(object sender)
    {
      base.OnDeserialization(sender);
      Initialize();
    }
  }
}
