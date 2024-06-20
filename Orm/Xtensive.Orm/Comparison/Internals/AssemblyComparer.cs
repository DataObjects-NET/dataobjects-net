// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.01.22

using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Xtensive.Comparison
{
  [Serializable]
  internal sealed class AssemblyComparer: WrappingComparer<Assembly, string>,
    ISystemComparer<Assembly>
  {
    protected override AssemblyComparer CreateNew(ComparisonRules rules)
      => new AssemblyComparer(Provider, ComparisonRules.Combine(rules));

    public override int Compare(Assembly x, Assembly y)
    {
      if (ReferenceEquals(x, y)) {
        return 0;
      }
      if (x == null) {
        return y == null ? 0 : -DefaultDirectionMultiplier;
      }
      else {
        return y == null
          ? DefaultDirectionMultiplier
          : BaseComparer.Compare(x.FullName, y.FullName);
      }
    }

    public override bool Equals(Assembly x, Assembly y)
      => ReferenceEquals(x, y);

    public override int GetHashCode(Assembly obj)
      => BaseComparer.GetHashCode(obj.FullName);


    // Constructors

    public AssemblyComparer(IComparerProvider provider, ComparisonRules comparisonRules) 
      : base(provider, comparisonRules)
    {
    }

    public AssemblyComparer(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
  }
}