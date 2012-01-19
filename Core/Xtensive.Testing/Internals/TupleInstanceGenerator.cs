// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.30

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Testing
{
  [Serializable]
  internal class TupleInstanceGenerator : InstanceGeneratorBase<Tuples.Tuple>
  {
    private static readonly Dictionary<Type[], int> descriptors = new Dictionary<Type[], int>(); // Descriptor - probability
    private static readonly int commonProbability;
    private readonly IInstanceGeneratorProvider provider;

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
      foreach (KeyValuePair<Type[], int> descriptor in descriptors) {
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


    // Constructors

    public TupleInstanceGenerator(IInstanceGeneratorProvider provider)
      : base(provider)
    {
      this.provider = provider;
    }

    static TupleInstanceGenerator()
    {
//      descriptors.Add(new Type[] {typeof (int)}, 20);
//      descriptors.Add(new Type[] {typeof (Guid)}, 20);
      descriptors.Add(new Type[] {typeof (long), typeof(int), typeof(string), typeof(bool), typeof(decimal)}, 50);
//      descriptors.Add(new Type[] {typeof (long), typeof (Guid)}, 20);
//      descriptors.Add(new Type[] {typeof (long), typeof (Guid), typeof (byte)}, 10);
      foreach (KeyValuePair<Type[], int> descriptor in descriptors)
        commonProbability += descriptor.Value;
    }
  }
}