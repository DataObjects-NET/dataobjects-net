// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.12

using System;
using System.Collections.Generic;

namespace Xtensive.Core.Testing
{
  public static class InstanceGenerationUtils<T>
  {
    public static IEnumerable<Pair<T>> GetPairs(Random random, double equalityProbability)
    {
      return GetPairs(InstanceGeneratorProvider.Default.GetInstanceGenerator<T>(), random, equalityProbability);
    }

    public static IEnumerable<Pair<T>> GetPairs(IInstanceGenerator<T> generator, Random random, double equalityProbability)
    {
      bool isValueType = typeof (T).IsValueType;
      bool isCloneable = typeof(ICloneable).IsAssignableFrom(typeof(T));
      while (true) {
        T x = generator.GetInstance(random);
        if (random.NextDouble()<equalityProbability) {
          T y;
          if (isValueType)
            y = x;
          else if (isCloneable)
            y = (T) ((ICloneable)x).Clone();
          else
            y = x;
          yield return new Pair<T>(x, x);
        }
        else
          yield return new Pair<T>(x, generator.GetInstance(random));
      }
    }

    public static IEnumerable<T> GetInstances(Random random, double equalityProbability)
    {
      return GetInstances(InstanceGeneratorProvider.Default.GetInstanceGenerator<T>(), random, equalityProbability);
    }

    public static IEnumerable<T> GetInstances(IInstanceGenerator<T> generator, Random random, double equalityProbability)
    {
      bool isValueType = typeof (T).IsValueType;
      bool isCloneable = typeof(ICloneable).IsAssignableFrom(typeof(T));
      T x = default(T);
      bool first = true;
      while (true) {
        if (first || random.NextDouble()>=equalityProbability)
          x = generator.GetInstance(random);
        else {
          T y;
          if (isValueType)
            y = x;
          else if (isCloneable)
            y = (T) ((ICloneable)x).Clone();
          else
            y = x;
          x = y;
        }
        first = false;
        yield return x;
      }
    }
  }
}