// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.22

using System;
using System.Reflection;

namespace Xtensive.Comparison
{
  [Serializable]
  internal sealed class AssemblyComparer: WrappingComparer<Assembly, string>,
    ISystemComparer<Assembly>
  {
    protected override IAdvancedComparer<Assembly> CreateNew(ComparisonRules rules)
    {
      return new AssemblyComparer(Provider, ComparisonRules.Combine(rules));
    }

    public override int Compare(Assembly x, Assembly y)
    {
      if (ReferenceEquals(x, y))
        return 0;
      if (x == null) {
        if (y == null)
          return 0;
        else
          return -DefaultDirectionMultiplier;
      }
      else {
        if (y==null)
          return DefaultDirectionMultiplier;
        else
          return BaseComparer.Compare(x.FullName, y.FullName);
      }
    }

    public override bool Equals(Assembly x, Assembly y)
    {
      return ReferenceEquals(x, y);
    }

    public override int GetHashCode(Assembly obj)
    {
      return BaseComparer.GetHashCode(obj.FullName);
    }


    // Constructors

    public AssemblyComparer(IComparerProvider provider, ComparisonRules comparisonRules) 
      : base(provider, comparisonRules)
    {
    }
  }
}
