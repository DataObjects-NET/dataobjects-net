// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.03.24

using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Indexing.Tests
{
  [TestFixture]
  [Serializable]
  public class IndexReaderTest
  {
    private const int count = 100;
    private Index<int, int> index;
    private SortedListIndex<int, int> sortedListIndex;
    private IndexReader<int, int> indexReader;
    private SortedListIndexReader<int, int> sortedListIndexReader;


    [Test]
    public void IndexReaderBehaviorTest()
    {
      CreateIndexes();
      PopulateIndexes();

      TestIndexReader(0, count - 1, 0, count - 1);
      TestIndexReader(count - 1, 0, count - 1, 0);
      TestIndexReader(10, count - 10, 5, count - 5);
      TestIndexReader(count - 10, 10, count - 5, 5);
      TestIndexReader(10, 10, 10, 10);

      var items = index.GetItems(new Range<Entire<int>>(10, 10));
      Assert.AreEqual(1, items.Count());
      Assert.AreEqual(10, items.First());
    }

    [Test]
    public void TupleIndexReaderTest()
    {
      var config = new IndexConfiguration<Tuple, Tuple>(
        t => t, 
        AdvancedComparer<Tuple>.Default.ApplyRules(new ComparisonRules(ComparisonRule.Positive, ComparisonRules.Positive)));
      var tupleIndex = new Index<Tuple, Tuple>(config);
      var aItem = Tuple.Create("A", 0);
      var bItem = Tuple.Create("B", 1);
      var xItem = Tuple.Create("X", 1024);
      var x1Item = Tuple.Create("X1", 4096);
      var zItem = Tuple.Create("Z", 2048);
      tupleIndex.Add(aItem);
      tupleIndex.Add(bItem);
      tupleIndex.Add(xItem);
      tupleIndex.Add(zItem);
      tupleIndex.Add(x1Item);

      var items = tupleIndex.GetItems(new Range<Entire<Tuple>>(Tuple.Create("X"), Tuple.Create("X" + (char) 0xDBFF + (char)0xDFFF)));
//      var items = tupleIndex.GetItems(new Range<Entire<Tuple>>(
//                                          new Entire<Tuple>(Tuple.Create("X"), EntireValueType.NegativeInfinitesimal),
//                                          new Entire<Tuple>(Tuple.Create("X" + (char)0xDBFF + (char)0xDFFF), EntireValueType.PositiveInfinitesimal)));
//      var items = tupleIndex.GetItems(new Range<Entire<Tuple>>(
//                                        new Entire<Tuple>(Tuple.Create("X"), EntireValueType.NegativeInfinitesimal),
//                                        new Entire<Tuple>(Tuple.Create("X"), EntireValueType.PositiveInfinitesimal)));
      Assert.AreEqual(2, items.Count());
    }


    

    #region Private methods

    private void TestIndexReader(int rLeft, int rRight, int left, int right)
    {
      CreateReaders(new Range<Entire<int>>(rLeft, rRight));
      TestCommonBehavior(indexReader, sortedListIndexReader);
      TestEndPointsOfRange(left, right, indexReader, sortedListIndexReader);
    }

    private void TestCommonBehavior(IIndexReader<int, int> first, IIndexReader<int, int> second)
    {
      var left = first.Range.EndPoints.First.Value;
      var right = first.Range.EndPoints.Second.Value;
      var isNegativeDirection = false;

      if (left > right) {
        var p = right;
        right = left;
        left = p;
        isNegativeDirection = true;
      }

      for (int i = left; i <= right; i++) {
        var ind = i;
        if (isNegativeDirection)
          ind = right - i + left;
        first.MoveTo(new Entire<int>(ind));
        first.MoveNext();
        second.MoveTo(new Entire<int>(ind));
        second.MoveNext();
        Assert.AreEqual(ind, first.Current);
        Assert.AreEqual(ind, second.Current);

        if ((ind == right && !isNegativeDirection) || (ind == left && isNegativeDirection)) {
          Assert.IsFalse(first.MoveNext());
          Assert.IsFalse(second.MoveNext());
        }
        else {
          Assert.IsTrue(first.MoveNext());
          Assert.IsTrue(second.MoveNext());
        }
      }
    }

    private void TestEndPointsOfRange(int leftKey, int rightKey, IIndexReader<int, int> first, IIndexReader<int, int> second)
    {
      var isNegativeDirection = first.Range.GetDirection(AdvancedComparer<Entire<int>>.Default)==Direction.Negative;

      first.MoveTo(leftKey);
      second.MoveTo(leftKey);
      Assert.IsTrue(first.MoveNext());
      Assert.IsTrue(second.MoveNext());
      Assert.AreEqual(first.Current, second.Current);
      if ((leftKey < first.Range.EndPoints.First.Value && !isNegativeDirection) ||
          (rightKey > first.Range.EndPoints.Second.Value && isNegativeDirection)) {
        Assert.AreEqual(first.Current, first.Range.EndPoints.First.Value);
        Assert.AreEqual(second.Current, second.Range.EndPoints.First.Value);
      }
      first.MoveTo(rightKey);
      second.MoveTo(rightKey);
      if ((rightKey == first.Range.EndPoints.Second.Value && !isNegativeDirection) ||
          (leftKey == first.Range.EndPoints.First.Value && isNegativeDirection)) {
        Assert.IsTrue(first.MoveNext());
        Assert.IsTrue(second.MoveNext());
        Assert.IsFalse(first.MoveNext());
        Assert.IsFalse(second.MoveNext());
      }
      if ((rightKey > first.Range.EndPoints.Second.Value && !isNegativeDirection) ||
          (leftKey < first.Range.EndPoints.First.Value && isNegativeDirection)) {
        Assert.IsFalse(first.MoveNext());
        Assert.IsFalse(second.MoveNext());
      }
    }

    private void CreateIndexes()
    {
      var config = new IndexConfiguration<int, int>(item => item, AdvancedComparer<int>.Default);
      index = new Index<int, int>(config);
      sortedListIndex = new SortedListIndex<int, int>(config);
    }

    private void PopulateIndexes()
    {
      for (int i = 0; i < count; i++) {
        index.Add(i);
        sortedListIndex.Add(i);
      }
    }

    private void CreateReaders(Range<Entire<int>> range)
    {
      indexReader = new IndexReader<int, int>(index, range);
      sortedListIndexReader = new SortedListIndexReader<int, int>(sortedListIndex, range);
    }

    #endregion

  }
}