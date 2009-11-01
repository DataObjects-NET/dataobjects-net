// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.02.06

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Comparison;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Testing;
using Xtensive.Core.Tuples;

namespace Xtensive.Indexing.Tests
{
  [TestFixture]
  public class EntireInterfaceComparerTest
  {
    const int iterationCount = 100000;

    [Test]
    public void BestCaseTest()
    {
      Random random = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
      IInstanceGenerator<Tuple> generator = InstanceGeneratorProvider.Default.GetInstanceGenerator<Tuple>();
      IEnumerable<Tuple> generatedTuples = generator.GetInstances(random, iterationCount);
      
      List<IEntire<Tuple>> entires = new List<IEntire<Tuple>>();
      foreach (Tuple generatedTuple in generatedTuples)
        entires.Add(Entire<Tuple>.Create(generatedTuple));
     
      AdvancedComparer<IEntire<Tuple>> comparer = AdvancedComparer<IEntire<Tuple>>.Default;
      IEntire<Tuple> x = entires[random.Next(iterationCount)];
      using(new Measurement("Comparison speed test", iterationCount)) {
        foreach (IEntire<Tuple> entire in entires) {
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

      AdvancedComparer<IEntire<Tuple>> comparer = AdvancedComparer<IEntire<Tuple>>.Default;
      IEntire<Tuple> entire = Entire<Tuple>.Create(tuple);
      IEntire<Tuple> entire1 = Entire<Tuple>.Create(tuple);
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
      
      IEntire<Tuple> entire = Entire<Tuple>.Create(tuples[random.Next(iterationCount)]);
      AdvancedComparer<IEntire<Tuple>> comparer = AdvancedComparer<IEntire<Tuple>>.Default;
      Func<IEntire<Tuple>, Tuple, int> asymmetric = comparer.GetAsymmetric<Tuple>();
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
      IEntire<Tuple> entire = Entire<Tuple>.Create(tuple);
      AdvancedComparer<IEntire<Tuple>> comparer = AdvancedComparer<IEntire<Tuple>>.Default;
      Func<IEntire<Tuple>, Tuple, int> asymmetric = comparer.GetAsymmetric<Tuple>();
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

      AdvancedComparer<IEntire<Tuple>> comparer = AdvancedComparer<IEntire<Tuple>>.Default;

      // Exact equals
      IEntire<Tuple> entire = Entire<Tuple>.Create(tuple);
      Assert.IsTrue(comparer.Equals(entire, entire));
      Assert.IsTrue(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.Exact)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, Direction.Positive)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, Direction.Negative)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(InfinityType.Positive)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(InfinityType.Negative)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.NegativeInfinity)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.NegativeInfinitesimal)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.PositiveInfinitesimal)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.PositiveInfinity)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.NegativeInfinity, EntireValueType.Exact)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.NegativeInfinitesimal, EntireValueType.Exact)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.PositiveInfinitesimal, EntireValueType.Exact)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.PositiveInfinity, EntireValueType.Exact)));

      // Nearest positive equals
      entire = Entire<Tuple>.Create(tuple, Direction.Positive);
      Assert.IsTrue(comparer.Equals(entire, entire));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.Exact)));
      Assert.IsTrue(comparer.Equals(entire, Entire<Tuple>.Create(tuple, Direction.Positive)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, Direction.Negative)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(InfinityType.Positive)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(InfinityType.Negative)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.NegativeInfinity)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.NegativeInfinitesimal)));
      Assert.IsTrue(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.PositiveInfinitesimal)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.PositiveInfinity)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.NegativeInfinity, EntireValueType.Exact)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.NegativeInfinitesimal, EntireValueType.Exact)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.PositiveInfinitesimal, EntireValueType.Exact)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.PositiveInfinity, EntireValueType.Exact)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.PositiveInfinitesimal, EntireValueType.PositiveInfinitesimal)));

      // Nearest negative equals
      entire = Entire<Tuple>.Create(tuple, Direction.Negative);
      Assert.IsTrue(comparer.Equals(entire, entire));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.Exact)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, Direction.Positive)));
      Assert.IsTrue(comparer.Equals(entire, Entire<Tuple>.Create(tuple, Direction.Negative)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(InfinityType.Positive)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(InfinityType.Negative)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.NegativeInfinity)));
      Assert.IsTrue(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.NegativeInfinitesimal)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.PositiveInfinitesimal)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.PositiveInfinity)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.NegativeInfinity, EntireValueType.Exact)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.NegativeInfinitesimal, EntireValueType.Exact)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.PositiveInfinitesimal, EntireValueType.Exact)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.PositiveInfinity, EntireValueType.Exact)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.NegativeInfinitesimal, EntireValueType.NegativeInfinitesimal)));

      // Negative infinity equals
      entire = Entire<Tuple>.Create(InfinityType.Negative);
      Assert.IsTrue(comparer.Equals(entire, entire));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.Exact)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, Direction.Positive)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, Direction.Negative)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(InfinityType.Positive)));
      Assert.IsTrue(comparer.Equals(entire, Entire<Tuple>.Create(InfinityType.Negative)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.NegativeInfinity)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.NegativeInfinitesimal)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.PositiveInfinitesimal)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.PositiveInfinity)));
      Assert.IsTrue(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.NegativeInfinity, EntireValueType.Exact)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.NegativeInfinitesimal, EntireValueType.Exact)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.PositiveInfinitesimal, EntireValueType.Exact)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.PositiveInfinity, EntireValueType.Exact)));
      Assert.IsTrue(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.NegativeInfinity, EntireValueType.NegativeInfinity)));

      // Positive infinity equals
      entire = Entire<Tuple>.Create(InfinityType.Positive);
      Assert.IsTrue(comparer.Equals(entire, entire));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.Exact)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, Direction.Positive)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, Direction.Negative)));
      Assert.IsTrue(comparer.Equals(entire, Entire<Tuple>.Create(InfinityType.Positive)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(InfinityType.Negative)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.NegativeInfinity)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.NegativeInfinitesimal)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.PositiveInfinitesimal)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.PositiveInfinity)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.NegativeInfinity, EntireValueType.Exact)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.NegativeInfinitesimal, EntireValueType.Exact)));
      Assert.IsFalse(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.PositiveInfinitesimal, EntireValueType.Exact)));
      Assert.IsTrue(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.PositiveInfinity, EntireValueType.Exact)));
      Assert.IsTrue(comparer.Equals(entire, Entire<Tuple>.Create(tuple, EntireValueType.PositiveInfinity, EntireValueType.PositiveInfinity)));

      // Exact compare
      entire = Entire<Tuple>.Create(tuple);
      Assert.IsTrue(comparer.Compare(entire, entire) == 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.Exact)) == 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, Direction.Positive)) < 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, Direction.Negative)) > 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(InfinityType.Positive)) < 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(InfinityType.Negative)) > 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.NegativeInfinity)) > 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.NegativeInfinitesimal)) > 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.PositiveInfinitesimal)) < 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.PositiveInfinity)) < 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.NegativeInfinity, EntireValueType.Exact)) > 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.NegativeInfinitesimal, EntireValueType.Exact)) > 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.PositiveInfinitesimal, EntireValueType.Exact)) < 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.PositiveInfinity, EntireValueType.Exact)) < 0);

      // Nearest positive compare
      entire = Entire<Tuple>.Create(tuple, Direction.Positive);
      Assert.IsTrue(comparer.Compare(entire, entire) == 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.Exact)) > 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, Direction.Positive)) == 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, Direction.Negative)) > 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(InfinityType.Positive)) < 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(InfinityType.Negative)) > 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.NegativeInfinity)) > 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.NegativeInfinitesimal)) > 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.PositiveInfinitesimal)) == 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.PositiveInfinity)) < 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.NegativeInfinity, EntireValueType.Exact)) > 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.NegativeInfinitesimal, EntireValueType.Exact)) > 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.PositiveInfinitesimal, EntireValueType.Exact)) < 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.PositiveInfinity, EntireValueType.Exact)) < 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.PositiveInfinitesimal, EntireValueType.PositiveInfinitesimal)) < 0);

      // Nearest negative compare
      entire = Entire<Tuple>.Create(tuple, Direction.Negative);
      Assert.IsTrue(comparer.Compare(entire, entire) == 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.Exact)) < 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, Direction.Positive)) < 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, Direction.Negative)) == 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(InfinityType.Positive)) < 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(InfinityType.Negative)) > 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.NegativeInfinity)) > 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.NegativeInfinitesimal)) == 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.PositiveInfinitesimal)) < 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.PositiveInfinity)) < 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.NegativeInfinity, EntireValueType.Exact)) > 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.NegativeInfinitesimal, EntireValueType.Exact)) > 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.PositiveInfinitesimal, EntireValueType.Exact)) < 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.PositiveInfinity, EntireValueType.Exact)) < 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.NegativeInfinitesimal, EntireValueType.NegativeInfinitesimal)) > 0);

      // Negative infinity compare
      entire = Entire<Tuple>.Create(InfinityType.Negative);
      Assert.IsTrue(comparer.Compare(entire, entire) == 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.Exact)) < 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, Direction.Positive)) < 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, Direction.Negative)) < 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(InfinityType.Positive)) < 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(InfinityType.Negative)) == 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.NegativeInfinity)) < 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.NegativeInfinitesimal)) < 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.PositiveInfinitesimal)) < 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.PositiveInfinity)) < 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.NegativeInfinity, EntireValueType.Exact)) == 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.NegativeInfinitesimal, EntireValueType.Exact)) < 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.PositiveInfinitesimal, EntireValueType.Exact)) < 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.PositiveInfinity, EntireValueType.Exact)) < 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.NegativeInfinity, EntireValueType.NegativeInfinity)) == 0);

      // Positive infinity compare
      entire = Entire<Tuple>.Create(InfinityType.Positive);
      Assert.IsTrue(comparer.Compare(entire, entire) == 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.Exact)) > 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, Direction.Positive)) > 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, Direction.Negative)) > 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(InfinityType.Positive)) == 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(InfinityType.Negative)) > 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.NegativeInfinity)) > 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.NegativeInfinitesimal)) > 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.PositiveInfinitesimal)) > 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.Exact, EntireValueType.PositiveInfinity)) > 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.NegativeInfinity, EntireValueType.Exact)) > 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.NegativeInfinitesimal, EntireValueType.Exact)) > 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.PositiveInfinitesimal, EntireValueType.Exact)) > 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.PositiveInfinity, EntireValueType.Exact)) == 0);
      Assert.IsTrue(comparer.Compare(entire, Entire<Tuple>.Create(tuple, EntireValueType.PositiveInfinity, EntireValueType.PositiveInfinity)) == 0);

    }
  }
}