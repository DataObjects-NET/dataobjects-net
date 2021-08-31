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
  internal sealed class Key<T> : Key
  {
    private static readonly Predicate<T, T> equalityComparer1 =
      ComparerProvider.Default.GetComparer<T>().Equals;

    private readonly T value1;

    protected override Tuple GetValue()
    {
      var result = CreateTuple();
      result.SetValue(0, value1);
      return result;
    }

    protected override bool ValueEquals(Key other)
    {
      var otherKey = other as Key<T>;
      if (otherKey == null)
        return false;
      return
        equalityComparer1.Invoke(value1, otherKey.value1);
    }

    protected override int CalculateHashCode()
    {
      return value1.GetHashCode();
    }

    [UsedImplicitly]
    public static Key Create(string nodeId, TypeInfo type, Tuple tuple, TypeReferenceAccuracy accuracy, IReadOnlyList<int> keyIndexes)
    {
      return new Key<T>(nodeId, type, accuracy, tuple.GetValueOrDefault<T>(keyIndexes[0]));
    }

    [UsedImplicitly]
    public static Key Create(string nodeId, TypeInfo type, Tuple tuple, TypeReferenceAccuracy accuracy)
    {
      return new Key<T>(nodeId, type, accuracy, tuple.GetValueOrDefault<T>(0));
    }


    // Constructors

    private Key(string nodeId, TypeInfo type, TypeReferenceAccuracy accuracy, T value)
      : base(nodeId, type, accuracy, null)
    {
      value1 = value;
    }
  }
}