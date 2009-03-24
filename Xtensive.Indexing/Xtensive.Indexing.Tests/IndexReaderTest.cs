// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2009.03.24

using System;
using NUnit.Framework;
using Xtensive.Core.Comparison;

namespace Xtensive.Indexing.Tests
{
  [TestFixture]
  [Serializable]
  public class IndexReaderTest
  {
    private const int count = 100;
    private Index<int, int> index;
    private SortedListIndex<int, int> sortedListIndex;
    //private NonUniqueIndex<int, int, int> nonUniqueIndex;
    private IndexReader<int, int> indexReader;
    private SortedListIndexReader<int, int> sortedListIndexReader;
    //private NonUniqueIndexReader<int, int, int> nonUniqueIndexReader;


    [Test]
    public void IndexReaderBehaviorTest()
    {
      CreateIndexes();
      PopulateIndexes();
      CreateReaders(new Range<Entire<int>>(0, count - 1));
      TestCommonBehavior(indexReader, sortedListIndexReader);
      TestEndPointsOfRange(0, count - 1, indexReader, sortedListIndexReader);
      CreateReaders(new Range<Entire<int>>(10, count - 10));
      TestCommonBehavior(indexReader, sortedListIndexReader);
      TestEndPointsOfRange(0, count - 1, indexReader, sortedListIndexReader);
    }

    private void TestCommonBehavior(IIndexReader<int, int> first, IIndexReader<int, int> second)
    {
      for (int i = first.Range.EndPoints.First.Value; i <= first.Range.EndPoints.Second.Value; i++) {
        first.MoveTo(new Entire<int>(i));
        first.MoveNext();
        second.MoveTo(new Entire<int>(i));
        second.MoveNext();
        Assert.AreEqual(i, first.Current);
        Assert.AreEqual(i, second.Current);

        if (i == first.Range.EndPoints.Second.Value) {
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
      first.MoveTo(leftKey);
      second.MoveTo(leftKey);
      Assert.IsTrue(first.MoveNext());
      Assert.IsTrue(second.MoveNext());
      Assert.AreEqual(first.Current, second.Current);
      first.MoveTo(rightKey);
      second.MoveTo(rightKey);
      if (rightKey == first.Range.EndPoints.Second.Value) {
        Assert.IsTrue(first.MoveNext());
        Assert.IsTrue(second.MoveNext());
        Assert.IsFalse(first.MoveNext());
        Assert.IsFalse(second.MoveNext());
      }
      if (rightKey > first.Range.EndPoints.Second.Value) {
        Assert.IsFalse(first.MoveNext());
        Assert.IsFalse(second.MoveNext());
      }
    }

    private void CreateIndexes()
    {
      var config = new IndexConfiguration<int, int>(item => item, AdvancedComparer<int>.Default);
      index = new Index<int, int>(config);
      sortedListIndex = new SortedListIndex<int, int>(config);
      //nonUniqueIndex = new NonUniqueIndex<int, int, int>(config);
    }

    private void PopulateIndexes()
    {
      for (int i = 0; i < count; i++) {
        index.Add(i);
        sortedListIndex.Add(i);
        //nonUniqueIndex.Add(i);
      }
    }

    private void CreateReaders(Range<Entire<int>> range)
    {
      indexReader = new IndexReader<int, int>(index, range);
      sortedListIndexReader = new SortedListIndexReader<int, int>(sortedListIndex, range);
      //nonUniqueIndexReader = new NonUniqueIndexReader<int, int, int>(nonUniqueIndex, range);
    }
  }
}