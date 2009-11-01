// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.01.30

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Xtensive.Core.Collections;
using Xtensive.Core.Tuples;

namespace Xtensive.Core.Testing
{
  [Serializable]
  internal class TupleInstanceGenerator : InstanceGeneratorBase<Tuple>,
    ITupleActionHandler<TupleInstanceGenerator.TupleGeneratorData>
  {
    private static readonly Dictionary<Type[], int> descriptors = new Dictionary<Type[], int>(); // Descriptor - probability
    private static readonly int commonProbability;
    private readonly IInstanceGeneratorProvider provider;

    private struct TupleGeneratorData
    {
      public IInstanceGeneratorProvider Provider;
      public Tuple Tuple;
      public Random Random;

      public TupleGeneratorData(IInstanceGeneratorProvider provider, Tuple tuple, Random random)
      {
        Provider = provider;
        Tuple = tuple;
        Random = random;
      }
    }

    public override Tuple GetInstance(Random random)
    {
      int position = random.Next(0, commonProbability);
      foreach (KeyValuePair<Type[], int> descriptor in descriptors) {
        if (position <= descriptor.Value) {
          Type[] types = descriptor.Key;
          Tuple tuple = Tuple.Create(types);
          TupleGeneratorData data = new TupleGeneratorData(provider, tuple, random);
          tuple.Descriptor.Execute(this, ref data, Direction.Positive);
          return tuple;
        }
        position -= descriptor.Value;
      }
      Debug.Assert(false);
      return null;
    }

    bool ITupleActionHandler<TupleGeneratorData>.Execute<TFieldType>(ref TupleGeneratorData actionData, int fieldIndex)
    {
      actionData.Tuple.SetValue(fieldIndex,
        actionData.Provider.GetInstanceGenerator<TFieldType>().GetInstance(actionData.Random));
      return true;
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