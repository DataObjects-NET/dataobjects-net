// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.01.22

using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Serialization;
using Xtensive.Collections;
using Xtensive.Core;


namespace Xtensive.Comparison
{
  [Serializable]
  internal sealed class TypeComparer: WrappingComparer<Type, string, Assembly>,
    ISystemComparer<Type>
  {
    [NonSerialized]
    private ConcurrentDictionary<(Pair<Type>, TypeComparer), int> results;

    protected override IAdvancedComparer<Type> CreateNew(ComparisonRules rules)
      => new TypeComparer(Provider, ComparisonRules.Combine(rules));

    public override int Compare(Type x, Type y)
    {
      return x == y
        ? 0
        : results.GetOrAdd((new Pair<Type>(x, y), this), generator);

      static int generator((Pair<Type>, TypeComparer) key)
      {
        var (pair, _this) = key;
        var result = _this.BaseComparer1.Compare(pair.First.FullName, pair.Second.FullName);
        if (result == 0) {
          result = _this.BaseComparer2.Compare(pair.First.Assembly, pair.Second.Assembly);
        }
        return result;
      }
    }

    public override bool Equals(Type x, Type y) => x == y;

    public override int GetHashCode(Type obj) => AdvancedComparerStruct<Type>.System.GetHashCode(obj);

    private void Initialize() => results = new ConcurrentDictionary<(Pair<Type>, TypeComparer), int>();


    // Constructors

    public TypeComparer(IComparerProvider provider, ComparisonRules comparisonRules)
      : base(provider, comparisonRules)
    {
      Initialize();
    }

    public TypeComparer(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    public override void OnDeserialization(object sender) => Initialize();
  }
}
