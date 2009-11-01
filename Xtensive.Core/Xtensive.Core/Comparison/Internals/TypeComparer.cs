// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.22

using System;
using System.Reflection;
using Xtensive.Core.Collections;

namespace Xtensive.Core.Comparison
{
  [Serializable]
  internal sealed class TypeComparer: WrappingComparer<Type, string, Assembly>,
    ISystemComparer<Type>
  {
    [NonSerialized]
    private ThreadSafeDictionary<Pair<Type>, int?> cache;

    protected override IAdvancedComparer<Type> CreateNew(ComparisonRules rules)
    {
      return new TypeComparer(Provider, ComparisonRules.Combine(rules));
    }

    public override int Compare(Type x, Type y)
    {
      if (x==y)
        return 0;
      Pair<Type> pair = new Pair<Type>(x, y);
      int? result = cache.GetValue(pair);
      if (result==null) lock (this) {
        result = cache.GetValue(pair);
        if (result==null) {
          result = BaseComparer1.Compare(x.FullName, y.FullName);
          if (result==0)
            result = BaseComparer2.Compare(x.Assembly, y.Assembly);
          cache.SetValue(pair, result);
        }
      }
      return result.GetValueOrDefault();
    }

    public override bool Equals(Type x, Type y)
    {
      return x==y;
    }

    public override int GetHashCode(Type obj)
    {
      return AdvancedComparerStruct<Type>.System.GetHashCode(obj);
    }

    private void Initialize()
    {
      cache = ThreadSafeDictionary<Pair<Type>, int?>.Create();
    }


    // Constructors

    public TypeComparer(IComparerProvider provider, ComparisonRules comparisonRules)
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
