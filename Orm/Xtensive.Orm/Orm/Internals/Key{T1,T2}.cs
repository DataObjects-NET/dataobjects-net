// Copyright (C) 2003-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexander Nikolaev
// Created:    2009.07.13

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Xtensive.Core;
using Xtensive.Orm.Model;
using ComparerProvider = Xtensive.Comparison.ComparerProvider;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Internals
{
  [Serializable]
  internal sealed class Key<T1, T2> : Key
  {
    private static readonly Predicate<T1, T1> equalityComparer1 =
      ComparerProvider.Default.GetComparer<T1>().Equals;
    private static readonly Predicate<T2, T2> equalityComparer2 =
      ComparerProvider.Default.GetComparer<T2>().Equals;

    private readonly T1 value1;
    private readonly T2 value2;

    protected override Tuple GetValue()
    {
      var result = CreateTuple();
      result.SetValue(0, value1);
      result.SetValue(1, value2);
      return result;
    }

    protected override bool ValueEquals(Key other)
    {
      var otherKey = other as Key<T1, T2>;
      if (otherKey == null)
        return false;
      return
        equalityComparer2.Invoke(value2, otherKey.value2) &&
        equalityComparer1.Invoke(value1, otherKey.value1);
    }

    protected override int CalculateHashCode()
    {
      return Tuple.HashCodeMultiplier * value1.GetHashCode() ^ value2.GetHashCode();
    }

    [UsedImplicitly]
    public static Key Create(string nodeId, TypeInfo type, Tuple tuple, TypeReferenceAccuracy accuracy, IReadOnlyList<int> keyIndexes)
    {
      return new Key<T1, T2>(nodeId, type, accuracy,
        tuple.GetValueOrDefault<T1>(keyIndexes[0]),
        tuple.GetValueOrDefault<T2>(keyIndexes[1]));
    }

    [UsedImplicitly]
    public static Key Create(string nodeId, TypeInfo type, Tuple tuple, TypeReferenceAccuracy accuracy)
    {
      return new Key<T1, T2>(nodeId, type, accuracy,
        tuple.GetValueOrDefault<T1>(0),
        tuple.GetValueOrDefault<T2>(1));
    }


    // Constructors

    private Key(string nodeId, TypeInfo type, TypeReferenceAccuracy accuracy, T1 value1, T2 value2)
      : base(nodeId, type, accuracy, null)
    {
      this.value1 = value1;
      this.value2 = value2;
    }
  }
}