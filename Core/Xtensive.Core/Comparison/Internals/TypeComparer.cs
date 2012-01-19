// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.22

using System;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Threading;

namespace Xtensive.Comparison
{
  [Serializable]
  internal sealed class TypeComparer: WrappingComparer<Type, string, Assembly>,
    ISystemComparer<Type>
  {
    [NonSerialized]
    private ThreadSafeDictionary<Pair<Type>, int> results;

    protected override IAdvancedComparer<Type> CreateNew(ComparisonRules rules)
    {
      return new TypeComparer(Provider, ComparisonRules.Combine(rules));
    }

    public override int Compare(Type x, Type y)
    {
      if (x==y)
        return 0;
      return results.GetValue(new Pair<Type>(x, y), 
        (pair, _this) => {
          int result = _this.BaseComparer1.Compare(pair.First.FullName, pair.Second.FullName);
          if (result==0)
            result = _this.BaseComparer2.Compare(pair.First.Assembly, pair.Second.Assembly);
          return result;
        }, 
        this);
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
      results = ThreadSafeDictionary<Pair<Type>, int>.Create(new object());
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
