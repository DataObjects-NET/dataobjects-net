// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.22

using System;
using Xtensive.Core;

namespace Xtensive.Comparison
{
  [Serializable]
  internal class BaseComparerWrapper<T, TBase>: WrappingComparer<T, TBase>
    where T: TBase
  {
    protected override IAdvancedComparer<T> CreateNew(ComparisonRules rules)
    {
      return new BaseComparerWrapper<T, TBase>(Provider, ComparisonRules.Combine(rules));
    }

    public override int Compare(T x, T y)
    {
      return BaseComparer.Compare(x, y);
    }

    public override bool Equals(T x, T y)
    {
      return BaseComparer.Equals(x, y);
    }

    public override int GetHashCode(T obj)
    {
      return BaseComparer.GetHashCode(obj);
    }

    public override T GetNearestValue(T value, Direction direction)
    {
      return (T)BaseComparer.GetNearestValue(value, direction);
    }


    // Constructors

    public BaseComparerWrapper(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
    }
  }
}
