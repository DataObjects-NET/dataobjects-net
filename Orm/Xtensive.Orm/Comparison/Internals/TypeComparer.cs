// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.01.22

using System;
using System.Collections.Concurrent;
using System.Reflection;


namespace Xtensive.Comparison
{
  internal sealed class TypeComparer: WrappingComparer<Type, string, Assembly>,
    ISystemComparer<Type>
  {
    private readonly ConcurrentDictionary<(Type, Type, TypeComparer), int> results = new();

    protected override TypeComparer CreateNew(ComparisonRules rules) => new TypeComparer(Provider, ComparisonRules.Combine(rules));

    public override int Compare(Type x, Type y)
    {
      static int TypeComparison((Type, Type, TypeComparer) tuple)
      {
        var (type1, type2, typeComparer) = tuple;
        var result = typeComparer.BaseComparer1.Compare(type1.FullName, type2.FullName);
        if (result == 0) {
          result = typeComparer.BaseComparer2.Compare(type1.Assembly, type2.Assembly);
        }
        return result;
      }

      return x == y ? 0 : results.GetOrAdd((x, y, this), TypeComparison);
    }

    public override bool Equals(Type x, Type y) => x == y;

    public override int GetHashCode(Type obj) => AdvancedComparerStruct<Type>.System.GetHashCode(obj);


    // Constructors

    public TypeComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
    }
  }
}
