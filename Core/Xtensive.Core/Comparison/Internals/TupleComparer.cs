// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.29

using System;
using Xtensive.Collections;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Comparison
{
  [Serializable]
  internal sealed class TupleComparer : AdvancedComparerBase<Tuples.Tuple>,
    ISystemComparer<Tuples.Tuple>
  {
    [NonSerialized]
    private int nullHashCode;

    protected override IAdvancedComparer<Tuples.Tuple> CreateNew(ComparisonRules rules)
    {
      return new TupleComparer(Provider, ComparisonRules.Combine(rules));
    }

    public override int Compare(Tuples.Tuple x, Tuples.Tuple y)
    {
      throw new NotSupportedException();
    }

    public override bool Equals(Tuples.Tuple x, Tuples.Tuple y)
    {
      if (x == null)
        return y == null;
      if (y == null)
        return false;
      if (x.Descriptor != y.Descriptor)
        return false;
      for (int fieldIndex = 0; fieldIndex < x.Count; fieldIndex++) {
        TupleFieldState xState;
        TupleFieldState yState;
        var xValue = x.GetValue(fieldIndex, out xState);
        var yValue = y.GetValue(fieldIndex, out yState);
        if (xState != yState)
          return false;
        if (xState != TupleFieldState.Available)
          continue;
        if (!Equals(xValue, yValue))
          return false;
      }
      return true;
    }

    public override int GetHashCode(Tuples.Tuple obj)
    {
      return ReferenceEquals(obj, null) 
        ? nullHashCode 
        : obj.GetHashCode();
    }


    private void Initialize()
    {
      nullHashCode = SystemComparerStruct<Tuples.Tuple>.Instance.GetHashCode(null);
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
