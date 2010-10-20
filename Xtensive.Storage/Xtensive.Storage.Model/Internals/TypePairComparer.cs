// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.03.17

using System.Collections.Generic;
using Xtensive.Comparison;
using Xtensive.Core;

namespace Xtensive.Storage.Model.Internals
{
  internal sealed class TypePairComparer: AdvancedComparerBase<Pair<TypeInfo, TypeInfo>>
  {
    private readonly Comparer<int> comparer = Comparer<int>.Default;

    public TypePairComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
    }

    #region IComparer<Pair<TypeInfo,TypeInfo>> Members

    public override int Compare(Pair<TypeInfo, TypeInfo> x, Pair<TypeInfo, TypeInfo> y)
    {
      if (x.First == y.First && x.Second == y.Second)
        return 0;
      if (x.First != y.First)
        return comparer.Compare(x.First.UnderlyingType.GetHashCode(), y.First.UnderlyingType.GetHashCode());
      if (x.Second == null)
        return -1;
      if (x.Second == x.First)
        return 1;
      if (y.Second == null)
        return 1;
      if (y.Second == y.First)
        return -1;
      return comparer.Compare(x.Second.UnderlyingType.GetHashCode(), y.Second.UnderlyingType.GetHashCode());
    }

    #endregion

    public override bool Equals(Pair<TypeInfo, TypeInfo> x, Pair<TypeInfo, TypeInfo> y)
    {
      return Compare(x, y) == 0;
    }

    public override int GetHashCode(Pair<TypeInfo, TypeInfo> obj)
    {
      return obj.GetHashCode();
    }

    protected override IAdvancedComparer<Pair<TypeInfo, TypeInfo>> CreateNew(ComparisonRules rules)
    {
      return new TypePairComparer(Provider, rules);
    }
  }
}