// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.02.06

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Diagnostics;
using Xtensive.Testing;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Indexing.Tests
{
  [TestFixture]
  public class EntireComparerTest
  {
    const int iterationCount = 100000;

    [Test]
    public void BestCaseTest()
    {
      Random random = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
      IInstanceGenerator<Tuple> generator = InstanceGeneratorProvider.Default.GetInstanceGenerator<Tuple>();
      IEnumerable<Tuple> generatedTuples = generator.GetInstances(random, iterationCount);
      
      List<Entire<Tuple>> entires = new List<Entire<Tuple>>();
      foreach (Tuple generatedTuple in generatedTuples)
        entires.Add(new Entire<Tuple>(generatedTuple));
     
      AdvancedComparer<Entire<Tuple>> comparer = AdvancedComparer<Entire<Tuple>>.Default;
      Entire<Tuple> x = entires[random.Next(iterationCount)];
      using(new Measurement("Comparison speed test", iterationCount)) {
        foreach (Entire<Tuple> entire in entires) {
          comparer.Compare(x, entire);
        }
      }
    }

    [Test]
    public void WorstCaseTest()
    {
      Random random = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
      IInstanceGenerator<Tuple> generator = InstanceGeneratorProvider.Default.GetInstanceGenerator<Tuple>();
      Tuple tuple = generator.GetInstance(random);

      AdvancedComparer<Entire<Tuple>> comparer = AdvancedComparer<Entire<Tuple>>.Default;
      Entire<Tuple> entire = new Entire<Tuple>(tuple);
      Entire<Tuple> entire1 = new Entire<Tuple>(tuple);
      using (new Measurement("Comparison speed test", iterationCount)) {
        int iteration = 0;
        while (iteration++ < iterationCount)
          comparer.Compare(entire, entire1);
      }
    }

    [Test]
    public void BestCaseAssymetricTest()
    {
      Random random = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
      IInstanceGenerator<Tuple> generator = InstanceGeneratorProvider.Default.GetInstanceGenerator<Tuple>();
      List<Tuple> tuples = new List<Tuple>(generator.GetInstances(random, iterationCount));
      
      Entire<Tuple> entire = new Entire<Tuple>(tuples[random.Next(iterationCount)]);
      AdvancedComparer<Entire<Tuple>> comparer = AdvancedComparer<Entire<Tuple>>.Default;
      Func<Entire<Tuple>, Tuple, int> asymmetric = comparer.GetAsymmetric<Tuple>();
      using (new Measurement("Comparison speed test", iterationCount)) {
        foreach (Tuple tuple in tuples){
          asymmetric(entire, tuple);
        }
      }
    }

    [Test]
    public void WorstCaseAssymetricTest()
    {
      Random random = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
      IInstanceGenerator<Tuple> generator = InstanceGeneratorProvider.Default.GetInstanceGenerator<Tuple>();
      Tuple tuple = generator.GetInstance(random);
      Entire<Tuple> entire = new Entire<Tuple>(tuple);
      AdvancedComparer<Entire<Tuple>> comparer = AdvancedComparer<Entire<Tuple>>.Default;
      Func<Entire<Tuple>, Tuple, int> asymmetric = comparer.GetAsymmetric<Tuple>();
      using (new Measurement("Comparison speed test", iterationCount)) {
        int iteration = 0;
        while (iteration++ < iterationCount)
          asymmetric(entire, tuple);
      }
    }

    [Test]
    public void BehaviorTest()
    {
      List<Type> fields = new List<Type>(2);
      fields.AddRange(new Type[] { typeof(int), typeof(int) });
      TupleDescriptor tupleDescriptor = TupleDescriptor.Create(fields);
      Tuple tuple = Tuple.Create(tupleDescriptor);
      tuple.SetValue(0, 1);
      tuple.SetValue(1, 1);

      AdvancedComparer<Entire<Tuple>> comparer = AdvancedComparer<Entire<Tuple>>.Default;

      // Exact equals
      Entire<Tuple> entire = new Entire<Tuple>(tuple);
      Assert.IsTrue(comparer.Equals(entire, entire));
      Assert.IsTrue(comparer.Equals(entire, new Entire<Tuple>(tuple, EntireValueType.Exact)));
      Assert.IsFalse(comparer.Equals(entire, new Entire<Tuple>(tuple, Direction.Positive)));
      Assert.IsFalse(comparer.Equals(entire, new Entire<Tuple>(tuple, Direction.Negative)));
      Assert.IsFalse(comparer.Equals(entire, new Entire<Tuple>(InfinityType.Positive)));
      Assert.IsFalse(comparer.Equals(entire, new Entire<Tuple>(InfinityType.Negative)));
      Assert.IsFalse(comparer.Equals(entire, new Entire<Tuple>(tuple, EntireValueType.NegativeInfinity)));
      Assert.IsFalse(comparer.Equals(entire, new Entire<Tuple>(tuple, EntireValueType.NegativeInfinitesimal)));
      Assert.IsFalse(comparer.Equals(entire, new Entire<Tuple>(tuple, EntireValueType.PositiveInfinitesimal)));
      Assert.IsFalse(comparer.Equals(entire, new Entire<Tuple>(tuple, EntireValueType.PositiveInfinity)));

      // Nearest positive equals
      entire = new Entire<Tuple>(tuple, Direction.Positive);
      Assert.IsTrue(comparer.Equals(entire, entire));
      Assert.IsFalse(comparer.Equals(entire, new Entire<Tuple>(tuple, EntireValueType.Exact)));
      Assert.IsTrue(comparer.Equals(entire, new Entire<Tuple>(tuple, Direction.Positive)));
      Assert.IsFalse(comparer.Equals(entire, new Entire<Tuple>(tuple, Direction.Negative)));
      Assert.IsFalse(comparer.Equals(entire, new Entire<Tuple>(InfinityType.Positive)));
      Assert.IsFalse(comparer.Equals(entire, new Entire<Tuple>(InfinityType.Negative)));
      Assert.IsFalse(comparer.Equals(entire, new Entire<Tuple>(tuple, EntireValueType.NegativeInfinity)));
      Assert.IsFalse(comparer.Equals(entire, new Entire<Tuple>(tuple, EntireValueType.NegativeInfinitesimal)));
      Assert.IsTrue(comparer.Equals(entire, new Entire<Tuple>(tuple, EntireValueType.PositiveInfinitesimal)));
      Assert.IsFalse(comparer.Equals(entire, new Entire<Tuple>(tuple, EntireValueType.PositiveInfinity)));

      // Nearest negative equals
      entire = new Entire<Tuple>(tuple, Direction.Negative);
      Assert.IsTrue(comparer.Equals(entire, entire));
      Assert.IsFalse(comparer.Equals(entire, new Entire<Tuple>(tuple, EntireValueType.Exact)));
      Assert.IsFalse(comparer.Equals(entire, new Entire<Tuple>(tuple, Direction.Positive)));
      Assert.IsTrue(comparer.Equals(entire, new Entire<Tuple>(tuple, Direction.Negative)));
      Assert.IsFalse(comparer.Equals(entire, new Entire<Tuple>(InfinityType.Positive)));
      Assert.IsFalse(comparer.Equals(entire, new Entire<Tuple>(InfinityType.Negative)));
      Assert.IsFalse(comparer.Equals(entire, new Entire<Tuple>(tuple, EntireValueType.NegativeInfinity)));
      Assert.IsTrue(comparer.Equals(entire, new Entire<Tuple>(tuple, EntireValueType.NegativeInfinitesimal)));
      Assert.IsFalse(comparer.Equals(entire, new Entire<Tuple>(tuple, EntireValueType.PositiveInfinitesimal)));
      Assert.IsFalse(comparer.Equals(entire, new Entire<Tuple>(tuple, EntireValueType.PositiveInfinity)));

      // Negative infinity equals
      entire = new Entire<Tuple>(InfinityType.Negative);
      Assert.IsTrue(comparer.Equals(entire, entire));
      Assert.IsFalse(comparer.Equals(entire, new Entire<Tuple>(tuple, EntireValueType.Exact)));
      Assert.IsFalse(comparer.Equals(entire, new Entire<Tuple>(tuple, Direction.Positive)));
      Assert.IsFalse(comparer.Equals(entire, new Entire<Tuple>(tuple, Direction.Negative)));
      Assert.IsFalse(comparer.Equals(entire, new Entire<Tuple>(InfinityType.Positive)));
      Assert.IsTrue(comparer.Equals(entire, new Entire<Tuple>(InfinityType.Negative)));
      Assert.IsTrue(comparer.Equals(entire, new Entire<Tuple>(tuple, EntireValueType.NegativeInfinity)));
      Assert.IsFalse(comparer.Equals(entire, new Entire<Tuple>(tuple, EntireValueType.NegativeInfinitesimal)));
      Assert.IsFalse(comparer.Equals(entire, new Entire<Tuple>(tuple, EntireValueType.PositiveInfinitesimal)));
      Assert.IsFalse(comparer.Equals(entire, new Entire<Tuple>(tuple, EntireValueType.PositiveInfinity)));

      // Positive infinity equals
      entire = new Entire<Tuple>(InfinityType.Positive);
      Assert.IsTrue(comparer.Equals(entire, entire));
      Assert.IsFalse(comparer.Equals(entire, new Entire<Tuple>(tuple, EntireValueType.Exact)));
      Assert.IsFalse(comparer.Equals(entire, new Entire<Tuple>(tuple, Direction.Positive)));
      Assert.IsFalse(comparer.Equals(entire, new Entire<Tuple>(tuple, Direction.Negative)));
      Assert.IsTrue(comparer.Equals(entire, new Entire<Tuple>(InfinityType.Positive)));
      Assert.IsFalse(comparer.Equals(entire, new Entire<Tuple>(InfinityType.Negative)));
      Assert.IsFalse(comparer.Equals(entire, new Entire<Tuple>(tuple, EntireValueType.NegativeInfinity)));
      Assert.IsFalse(comparer.Equals(entire, new Entire<Tuple>(tuple, EntireValueType.NegativeInfinitesimal)));
      Assert.IsFalse(comparer.Equals(entire, new Entire<Tuple>(tuple, EntireValueType.PositiveInfinitesimal)));
      Assert.IsTrue(comparer.Equals(entire, new Entire<Tuple>(tuple, EntireValueType.PositiveInfinity)));

      // Exact compare
      entire = new Entire<Tuple>(tuple);
      Assert.IsTrue(comparer.Compare(entire, entire) == 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(tuple, EntireValueType.Exact)) == 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(tuple, Direction.Positive)) < 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(tuple, Direction.Negative)) > 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(InfinityType.Positive)) < 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(InfinityType.Negative)) > 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(tuple, EntireValueType.NegativeInfinity)) > 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(tuple, EntireValueType.NegativeInfinitesimal)) > 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(tuple, EntireValueType.PositiveInfinitesimal)) < 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(tuple, EntireValueType.PositiveInfinity)) < 0);

      // Nearest positive compare
      entire = new Entire<Tuple>(tuple, Direction.Positive);
      Assert.IsTrue(comparer.Compare(entire, entire) == 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(tuple, EntireValueType.Exact)) > 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(tuple, Direction.Positive)) == 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(tuple, Direction.Negative)) > 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(InfinityType.Positive)) < 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(InfinityType.Negative)) > 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(tuple, EntireValueType.NegativeInfinity)) > 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(tuple, EntireValueType.NegativeInfinitesimal)) > 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(tuple, EntireValueType.PositiveInfinitesimal)) == 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(tuple, EntireValueType.PositiveInfinity)) < 0);

      // Nearest negative compare
      entire = new Entire<Tuple>(tuple, Direction.Negative);
      Assert.IsTrue(comparer.Compare(entire, entire) == 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(tuple, EntireValueType.Exact)) < 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(tuple, Direction.Positive)) < 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(tuple, Direction.Negative)) == 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(InfinityType.Positive)) < 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(InfinityType.Negative)) > 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(tuple, EntireValueType.NegativeInfinity)) > 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(tuple, EntireValueType.NegativeInfinitesimal)) == 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(tuple, EntireValueType.PositiveInfinitesimal)) < 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(tuple, EntireValueType.PositiveInfinity)) < 0);

      // Negative infinity compare
      entire = new Entire<Tuple>(InfinityType.Negative);
      Assert.IsTrue(comparer.Compare(entire, entire) == 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(tuple, EntireValueType.Exact)) < 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(tuple, Direction.Positive)) < 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(tuple, Direction.Negative)) < 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(InfinityType.Positive)) < 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(InfinityType.Negative)) == 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(tuple, EntireValueType.NegativeInfinity)) == 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(tuple, EntireValueType.NegativeInfinitesimal)) < 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(tuple, EntireValueType.PositiveInfinitesimal)) < 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(tuple, EntireValueType.PositiveInfinity)) < 0);

      // Positive infinity compare
      entire = new Entire<Tuple>(InfinityType.Positive);
      Assert.IsTrue(comparer.Compare(entire, entire) == 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(tuple, EntireValueType.Exact)) > 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(tuple, Direction.Positive)) > 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(tuple, Direction.Negative)) > 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(InfinityType.Positive)) == 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(InfinityType.Negative)) > 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(tuple, EntireValueType.NegativeInfinity)) > 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(tuple, EntireValueType.NegativeInfinitesimal)) > 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(tuple, EntireValueType.PositiveInfinitesimal)) > 0);
      Assert.IsTrue(comparer.Compare(entire, new Entire<Tuple>(tuple, EntireValueType.PositiveInfinity)) == 0);
    }
  }
}