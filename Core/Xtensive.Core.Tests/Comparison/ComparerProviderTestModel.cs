// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.12.17

using System;
using Xtensive.Comparison;

namespace Xtensive.Tests.Comparison
{
  public class Wrapper<T>
  {
    public T Value;

    public Wrapper(T value)
    {
      Value = value;
    }
  }

  public class Wrapper1<T> : Wrapper<T>
  {
    public Wrapper1(T value)
      : base(value)
    {
    }
  }

  public class Wrapper2<T1, T2>: Wrapper<T1>
  {
    public T2 Value2;

    public Wrapper2(T1 value, T2 value2) : base(value)
    {
      Value2 = value2;
    }
  }

  public class Wrapper2a<T1, T2>: Wrapper<T1>
  {
    public T2 Value2;

    public Wrapper2a(T1 value, T2 value2) : base(value)
    {
      Value2 = value2;
    }
  }

  public class WrapperComparer<T>: WrappingComparer<Wrapper<T>, T>
  {
    protected override IAdvancedComparer<Wrapper<T>> CreateNew(ComparisonRules rules)
    {
      return new WrapperComparer<T>(Provider, ComparisonRules.Combine(rules));
    }

    public override int Compare(Wrapper<T> x, Wrapper<T> y)
    {
      return BaseComparer.Compare(x.Value, y.Value);
    }

    public override bool Equals(Wrapper<T> x, Wrapper<T> y)
    {
      return BaseComparer.Equals(x.Value, y.Value);
    }

    public override int GetHashCode(Wrapper<T> obj)
    {
      return BaseComparer.GetHashCode(obj.Value);
    }

    public WrapperComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
    }
  }

  public class Wrapper1Comparer<T>: WrappingComparer<Wrapper1<T>, T>
  {
    protected override IAdvancedComparer<Wrapper1<T>> CreateNew(ComparisonRules rules)
    {
      return new Wrapper1Comparer<T>(Provider, ComparisonRules.Combine(rules));
    }

    public override int Compare(Wrapper1<T> x, Wrapper1<T> y)
    {
      return BaseComparer.Compare(x.Value, y.Value);
    }

    public override bool Equals(Wrapper1<T> x, Wrapper1<T> y)
    {
      return BaseComparer.Equals(x.Value, y.Value);
    }

    public override int GetHashCode(Wrapper1<T> obj)
    {
      return BaseComparer.GetHashCode(obj.Value);
    }

    public Wrapper1Comparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
    }
  }

  public class Wrapper2Comparer<T1,T2>: WrappingComparer<Wrapper2<T1,T2>, T1, T2>
  {
    protected override IAdvancedComparer<Wrapper2<T1, T2>> CreateNew(ComparisonRules rules)
    {
      return new Wrapper2Comparer<T1,T2>(Provider, ComparisonRules.Combine(rules));
    }

    public override int Compare(Wrapper2<T1, T2> x, Wrapper2<T1, T2> y)
    {
      int r = BaseComparer1.Compare(x.Value, y.Value);
      if (r==0)
        r = BaseComparer2.Compare(x.Value2, y.Value2);
      return r;
    }

    public override bool Equals(Wrapper2<T1, T2> x, Wrapper2<T1, T2> y)
    {
      bool r = BaseComparer1.Equals(x.Value, y.Value);
      if (r)
        r = BaseComparer2.Equals(x.Value2, y.Value2);
      return r;
    }

    public override int GetHashCode(Wrapper2<T1, T2> obj)
    {
      int r = BaseComparer1.GetHashCode(obj.Value);
      return r ^ BaseComparer2.GetHashCode(obj.Value2);
    }

    public Wrapper2Comparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
    }
  }
}
