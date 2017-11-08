// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.02.12

using System;
using System.Collections.Generic;
using Xtensive.Core;

namespace Xtensive.Orm.Tests
{
  /// <summary>
  /// Helper type providing instance generation facilities.
  /// </summary>
  /// <typeparam name="T">The type of generated items.</typeparam>
  public static class InstanceGenerationUtils<T>
  {
    /// <summary>
    /// Generates the sequence of pairs of type <typeparamref name="T"/>
    /// using default instance generator (see <see cref="InstanceGeneratorProvider.Default"/>).
    /// </summary>
    /// <param name="random">The random generator to use.</param>
    /// <param name="equalityProbability">The item equality probability.</param>
    /// <returns>Described sequence.</returns>
    public static IEnumerable<Pair<T>> GetPairs(Random random, double equalityProbability)
    {
      return GetPairs(InstanceGeneratorProvider.Default.GetInstanceGenerator<T>(), random, equalityProbability);
    }

    /// <summary>
    /// Generates the sequence of pairs of type <typeparamref name="T"/>
    /// using specified instance generator.
    /// </summary>
    /// <param name="generator">The instance generator to use.</param>
    /// <param name="random">The random generator to use.</param>
    /// <param name="equalityProbability">The item equality probability.</param>
    /// <returns>Described sequence.</returns>
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

    /// <summary>
    /// Generates the sequence of instances of type <typeparamref name="T"/>
    /// using default instance generator (see <see cref="InstanceGeneratorProvider.Default"/>).
    /// </summary>
    /// <param name="random">The random generator to use.</param>
    /// <param name="equalityProbability">The subsequent item equality probability.</param>
    /// <returns>Described sequence.</returns>
    public static IEnumerable<T> GetInstances(Random random, double equalityProbability)
    {
      return GetInstances(InstanceGeneratorProvider.Default.GetInstanceGenerator<T>(), random, equalityProbability);
    }

    /// <summary>
    /// Generates the sequence of instances of type <typeparamref name="T"/>
    /// using specified instance generator.
    /// </summary>
    /// <param name="generator">The instance generator to use.</param>
    /// <param name="random">The random generator to use.</param>
    /// <param name="equalityProbability">The subsequent item equality probability.</param>
    /// <returns>Described sequence.</returns>
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