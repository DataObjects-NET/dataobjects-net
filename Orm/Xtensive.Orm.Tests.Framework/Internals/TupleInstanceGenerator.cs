// Copyright (C) 2008-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2008.01.30

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Tests
{
  internal class TupleInstanceGenerator(IInstanceGeneratorProvider provider)
    : InstanceGeneratorBase<Tuples.Tuple>(provider)
  {
    private static readonly Dictionary<Type[], int> Descriptors = new Dictionary<Type[], int>(); // Descriptor - probability
    private static readonly int commonProbability;
    private readonly IInstanceGeneratorProvider provider = provider;

    internal struct TupleGeneratorData
    {
      public IInstanceGeneratorProvider Provider;
      public Tuples.Tuple Tuple;
      public Random Random;

      public TupleGeneratorData(IInstanceGeneratorProvider provider, Tuples.Tuple tuple, Random random)
      {
        Provider = provider;
        Tuple = tuple;
        Random = random;
      }
    }

    public override Tuples.Tuple GetInstance(Random random)
    {
      int position = random.Next(0, commonProbability);
      foreach (KeyValuePair<Type[], int> descriptor in Descriptors) {
        if (position <= descriptor.Value) {
          Type[] types = descriptor.Key;
          Tuples.Tuple tuple = Tuples.Tuple.Create(types);
          for (int i = 0; i < types.Length; i++) {
            var type = types[i];
            object value = provider.GetInstanceGenerator(type).GetInstance(random);
            tuple.SetValue(i, value);
          }
          return tuple;
        }
        position -= descriptor.Value;
      }
      Debug.Assert(false);
      return null;
    }

    static TupleInstanceGenerator()
    {
      Descriptors.Add([typeof (long), typeof(int), typeof(string), typeof(bool), typeof(decimal)], 50);
      foreach (KeyValuePair<Type[], int> descriptor in Descriptors) {
        commonProbability += descriptor.Value;
      }
    }
  }
}