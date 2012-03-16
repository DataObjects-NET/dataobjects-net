// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.16

using System.Linq;
using NUnit.Framework;
using Xtensive.Comparison;
using Xtensive.Core;

namespace Xtensive.Indexing.Tests
{
  [TestFixture]
  public class RangeSetTest
  {
    [Test]
    public void UniteTest()
    {
      var rangeSetX = new RangeSet<Entire<int>>(new Range<Entire<int>>(20, 30),
        AdvancedComparer<Entire<int>>.Default);
      rangeSetX.Unite(CreateRangeSet<Entire<int>>(50, 30));
      rangeSetX.Unite(CreateRangeSet<Entire<int>>(-10, 2));
      rangeSetX.Unite(CreateRangeSet(0, Entire<int>.MinValue));

      Assert.AreEqual(2, rangeSetX.Count());

      var rangeSetY = new RangeSet<Entire<int>>(new Range<Entire<int>>(-20, -30),
        AdvancedComparer<Entire<int>>.Default);
      rangeSetY.Unite(CreateRangeSet(new Entire<int>(-50, Direction.Positive), -15));
      rangeSetY.Unite(CreateRangeSet<Entire<int>>(-50, -60));
      rangeSetY.Unite(CreateRangeSet<Entire<int>>(-50, -100));

      Assert.AreEqual(2, rangeSetY.Count());

      rangeSetX.Unite(rangeSetY);

      Assert.AreEqual(2, rangeSetX.Count());
    }

    [Test]
    public void IntersectTest()
    {
      var rangeSetX = new RangeSet<Entire<int>>(new Range<Entire<int>>(20, 30),
        AdvancedComparer<Entire<int>>.Default);
      rangeSetX.Unite(CreateRangeSet<Entire<int>>(100, 10));
      rangeSetX.Unite(CreateRangeSet<Entire<int>>(0, 200));
      rangeSetX.Unite(CreateRangeSet<Entire<int>>(50, 90));
      rangeSetX.Intersect(CreateRangeSet<Entire<int>>(30, 130));
      Assert.AreEqual(1, rangeSetX.Count());

      var rangeSetY = new RangeSet<Entire<int>>(new Range<Entire<int>>(20, 30),
        AdvancedComparer<Entire<int>>.Default);
      rangeSetY.Unite(CreateRangeSet<Entire<int>>(100, 140));
      rangeSetY.Unite(CreateRangeSet<Entire<int>>(500, 600));
      rangeSetY.Unite(CreateRangeSet<Entire<int>>(50, 90));
      rangeSetY.Intersect(CreateRangeSet<Entire<int>>(510, 130));
      Assert.AreEqual(2, rangeSetY.Count());

      rangeSetX.Intersect(rangeSetY);
      Assert.AreEqual(1, rangeSetX.Count());
    }

    [Test]
    public void InvertTest()
    {
      var rangeSet = new RangeSet<Entire<int>>(new Range<Entire<int>>(20, 30),
        AdvancedComparer<Entire<int>>.Default);
      rangeSet.Unite(CreateRangeSet<Entire<int>>(100, 140));
      rangeSet.Unite(CreateRangeSet<Entire<int>>(500, 600));
      rangeSet.Unite(CreateRangeSet<Entire<int>>(50, 90));
      rangeSet.Invert();

      Assert.AreEqual(5, rangeSet.Count());
    }

    [Test]
    public void EmptyTest()
    {
      var rangeSetX = new RangeSet<Entire<int>>(new Range<Entire<int>>(20, 30),
        AdvancedComparer<Entire<int>>.Default);
      var emptyRange = new RangeSet<Entire<int>>(Range<Entire<int>>.Empty,
        AdvancedComparer<Entire<int>>.Default);
      rangeSetX.Intersect(emptyRange);
      Assert.AreEqual(0, emptyRange.Count());

      var sourceRange = new Range<Entire<int>>(20, 30);
      rangeSetX = new RangeSet<Entire<int>>(sourceRange,
        AdvancedComparer<Entire<int>>.Default);
      rangeSetX.Unite(emptyRange);
      Assert.AreEqual(1, rangeSetX.Count());
      Assert.AreEqual(0, rangeSetX.First().CompareTo(sourceRange));

      emptyRange.Invert();
      Assert.AreEqual(1, emptyRange.Count());
      Assert.AreEqual(0, emptyRange.First().CompareTo(Range<Entire<int>>.Full));
    }

    private RangeSet<T> CreateRangeSet<T>(T first, T second)
    {
      return new RangeSet<T>(new Range<T>(first, second), AdvancedComparer<T>.Default);
    }

    [Test]
    public void RangeSetReaderTest()
    {
      var sortedListConfiguration = new IndexConfigurationBase<int, int>(item => item, AdvancedComparer<int>.Default);
      var sortedListIndex = new SortedListIndex<int, int>(sortedListConfiguration);
      for (int i = 0; i < 40; i++)
        sortedListIndex.Add(i);

      var reader1 = sortedListIndex.CreateReader(new Range<Entire<int>>(5, 10));
      var reader2 = sortedListIndex.CreateReader(new Range<Entire<int>>(15, 20));
      var reader3 = sortedListIndex.CreateReader(new Range<Entire<int>>(25, 30));

      var rangeSetReader = new RangeSetReader<int, int>(new[] { sortedListIndex.CreateReader(new Range<Entire<int>>(5, 10)) ,
                                                                sortedListIndex.CreateReader(new Range<Entire<int>>(15, 20)),
                                                                sortedListIndex.CreateReader(new Range<Entire<int>>(25, 30))});

      rangeSetReader.MoveTo(5); 
      reader1.MoveTo(5);

      for (int i = 0; i <= 5; i++) {
        rangeSetReader.MoveNext();
        reader1.MoveNext();
        Assert.AreEqual(rangeSetReader.Current, reader1.Current);
      }

      reader2.MoveTo(15);
      for (int i = 0; i <= 5; i++) {
        rangeSetReader.MoveNext();
        reader2.MoveNext();
        Assert.AreEqual(rangeSetReader.Current, reader2.Current);
      }

      reader3.MoveTo(25);
      for (int i = 0; i <= 5; i++) {
        rangeSetReader.MoveNext();
        reader3.MoveNext();
        Assert.AreEqual(rangeSetReader.Current, reader3.Current);
      }

      reader2.MoveTo(17);
      rangeSetReader.MoveTo(17);
      reader2.MoveNext();
      rangeSetReader.MoveNext();
      Assert.AreEqual(reader2.Current, rangeSetReader.Current);

      reader3.MoveTo(23);
      rangeSetReader.MoveTo(23);
      reader3.MoveNext();
      rangeSetReader.MoveNext();
      Assert.AreEqual(reader3.Current, rangeSetReader.Current);

      reader3.MoveTo(31);
      rangeSetReader.MoveTo(31);
      Assert.IsFalse(reader3.MoveNext());
      Assert.IsFalse(rangeSetReader.MoveNext());
    }

    [Test]
    public void CreateFullOrEmptyTest()
    {
      var rangeSet = RangeSet<Entire<int>>.CreateFullOrEmpty(true, AdvancedComparer<Entire<int>>.Default);
      Assert.IsTrue(rangeSet.IsFull());
      Assert.IsFalse(rangeSet.IsEmpty());

      rangeSet = RangeSet<Entire<int>>.CreateFullOrEmpty(false, AdvancedComparer<Entire<int>>.Default);
      Assert.IsFalse(rangeSet.IsFull());
      Assert.IsTrue(rangeSet.IsEmpty());
    }

    
  }
}
