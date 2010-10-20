// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.02.14

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Indexing.Tests
{
  [TestFixture]
  public class NonUniqueIndexTest
  {
    [Test]
    public void BehaviorTest()
    {
      const int iterationCount = 1000;
      TupleDescriptor descriptor = TupleDescriptor.Create<int, int>();

      // Creating unique index
      AdvancedComparer<Tuple> comparer = AdvancedComparer<Tuple>.Default;
      comparer = comparer.ApplyRules(new ComparisonRules(
        Direction.Positive,
        Direction.Positive, 
        Direction.Positive, 
        Direction.None));
      var configuration = new IndexConfigurationBase<Tuple, Tuple>(item => item, comparer);
      IUniqueOrderedIndex<Tuple, Tuple> index =  IndexFactory.CreateUniqueOrdered<Tuple, Tuple, SortedListIndex<Tuple, Tuple>>(configuration);

      for (int i = 0; i < iterationCount; i++)
        index.Add(Tuple.Create(descriptor, i / 10, i));

      // Creating non-unique index
      var nuic = new NonUniqueIndexConfiguration<int, Tuple, Tuple>(configuration);
      nuic.KeyExtractor = item => item.GetValue<int>(0);
      nuic.KeyComparer = AdvancedComparer<int>.Default;
      nuic.EntireConverter = item => new Entire<Tuple>(Tuple.Create(item.Value),item.ValueType);
      INonUniqueIndex<int, Tuple> nonUniqueIndex = IndexFactory.CreateNonUnique<int, Tuple, Tuple, SortedListIndex<Tuple, Tuple>>(nuic);

      for (int i = 0; i < iterationCount; i++)
        nonUniqueIndex.Add(Tuple.Create(descriptor, i / 10, i));

      // Tests
      var result = new List<Tuple>(nonUniqueIndex.GetItems(8));
      Assert.AreEqual(10, result.Count);

      result = new List<Tuple>(
        nonUniqueIndex.GetItems(new Range<Entire<int>>(
          new Entire<int>(20, EntireValueType.NegativeInfinitesimal), 
          new Entire<int>(10))));
      Assert.AreEqual(100, result.Count);
      Assert.AreEqual(19, result[0].GetValue(0));
      Assert.AreEqual(10, result[99].GetValue(0));

      result = new List<Tuple>(nonUniqueIndex.GetItems(new Range<Entire<int>>(Entire<int>.MinValue, Entire<int>.MaxValue)));
      Assert.AreEqual(iterationCount, result.Count);
      result = new List<Tuple>(nonUniqueIndex.GetItems(new Range<Entire<int>>(Entire<int>.MaxValue, Entire<int>.MinValue)));
      Assert.AreEqual(iterationCount, result.Count);
      result = new List<Tuple>(nonUniqueIndex.GetItems(new Range<Entire<int>>(Entire<int>.MaxValue, Entire<int>.MaxValue)));
      Assert.AreEqual(0, result.Count);
    }
  }
}