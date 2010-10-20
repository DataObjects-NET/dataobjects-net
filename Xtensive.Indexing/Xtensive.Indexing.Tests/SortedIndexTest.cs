// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.02.13

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Testing;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Indexing.Tests
{
  [TestFixture]
  public class SortedIndexTest
  {
    [Test]
    public void OrderTest()
    {
      const int iterationCount = 1000;
      const double probability = 0.1;
      AdvancedComparer<Tuple> comparer = AdvancedComparer<Tuple>.Default;
      comparer = comparer.ApplyRules(new ComparisonRules(Direction.Positive, 
                                                         Direction.Positive, Direction.Positive, Direction.None));

      IndexConfigurationBase<Tuple,Tuple> configuration = new IndexConfigurationBase<Tuple, Tuple>(delegate(Tuple item) { return item; }, comparer);
      SortedListIndex<Tuple, Tuple> index = new SortedListIndex<Tuple, Tuple>(configuration);

      Random random = RandomManager.CreateRandom(SeedVariatorType.CallingMethod);
      IInstanceGenerator<Tuple> generator = InstanceGeneratorProvider.Default.GetInstanceGenerator<Tuple>();
      
      IEnumerable<Tuple> generatedTuples = InstanceGenerationUtils<Tuple>.GetInstances(generator, random, probability);

      int iteration = 0;
      foreach (Tuple tuple in generatedTuples) {
        if (!index.Contains(tuple)){
          index.Add(tuple);
          if (iteration++ >=iterationCount)
            break;
        }
      }
      Tuple previousTuple = null;
      foreach (Tuple tuple in index) {
        if (previousTuple!=null)
          Assert.IsTrue(comparer.Compare(previousTuple, tuple) < 0);

        previousTuple = tuple;
      }

      previousTuple = null;
      foreach (Tuple tuple in index) {
        if (previousTuple != null)
          Assert.IsTrue(comparer.Compare(previousTuple, tuple) < 0);
        previousTuple = tuple;
        index.Remove(tuple);
      }

      Assert.AreEqual(0, index.Count);
    }

    [Test]
    public void BehaviorTest()
    {
      IndexConfigurationBase<int, Pair<int, string>> configuration = new IndexConfigurationBase<int, Pair<int, string>>(
        delegate (Pair<int,string> pair){ return pair.First;},
        AdvancedComparer<int>.Default
        );
      SortedListIndex<int,Pair<int,string>> index = new SortedListIndex<int, Pair<int, string>>(configuration);

      index.Add(new Pair<int, string>(123,"123"));
      index.Add(new Pair<int, string>(1,"1"));
      index.Add(new Pair<int, string>(12,"12"));
      index.Add(new Pair<int, string>(3,"3"));
      index.Add(new Pair<int, string>(5,"5"));
      index.Add(new Pair<int, string>(7,"7"));

      Assert.AreEqual(6, index.Count);
      List<Pair<int, string>> list;

      list = new List<Pair<int, string>>(index.GetItems(new Range<Entire<int>>(new Entire<int>(7), new Entire<int>(12))));
      Assert.AreEqual(2, list.Count);
      Assert.AreEqual(7, list[0].First);
      Assert.AreEqual(12, list[1].First);

      list = new List<Pair<int, string>>(index.GetItems(new Range<Entire<int>>(new Entire<int>(13), new Entire<int>(6))));
      Assert.AreEqual(2, list.Count);
      Assert.AreEqual(12, list[0].First);
      Assert.AreEqual(7, list[1].First);

      Assert.AreEqual("1", index.GetItem(1).Second);

      IndexTest.Configuration testConfig = new IndexTest.Configuration(RandomManager.CreateRandom(SeedVariatorType.CallingMethod), 10000);
      IndexTest.TestIndex(index, testConfig);
    }


  }
}