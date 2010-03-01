// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.10.24

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Comparison;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Testing;

namespace Xtensive.Indexing.Tests
{
  [TestFixture]
  public class JoinTest
  {
    [Test]
    public void MergeJoin()
    {
      SortedCollection<int> left = new SortedCollection<int>(AdvancedComparer<int>.Default);
      SortedCollection<int> right = new SortedCollection<int>(AdvancedComparer<int>.Default);
      left.Add(1);
      left.Add(1);
      left.Add(2);
      left.Add(2);
      left.Add(5);
      left.Add(5);
      left.Add(5);
      left.Add(6);
      left.Add(6);
      left.Add(100);

      right.Add(1);
      right.Add(2);
      right.Add(3);
      right.Add(5);
      right.Add(5);
      right.Add(6);
      right.Add(6);
      right.Add(100);

      int expected = 0;
      foreach (Pair<int, int> pair in Joiner.NestedLoopJoin(left, right, delegate(int v) { return v; }, delegate(int v) { return v; }, AdvancedComparer<int>.Default)) {
        expected++;
      }

      int count = 0;
      foreach (Pair<int, int> pair in Joiner.MergeJoin(left, right)) {
        count++;
      }
      Assert.AreEqual(expected, count);
    }

    [Test]
    public void MergeJoinLeft()
    {
      SortedCollection<int> left = new SortedCollection<int>(AdvancedComparer<int>.Default);
      SortedCollection<int> right = new SortedCollection<int>(AdvancedComparer<int>.Default);
      left.Add(1);
      left.Add(1);
      left.Add(2);
      left.Add(3);
      left.Add(5);
      left.Add(5);
      left.Add(100);

      right.Add(1);
      right.Add(1);
      right.Add(4);
      right.Add(4);
      right.Add(4);
      right.Add(4);
      right.Add(4);
      right.Add(4);
      right.Add(4);
      right.Add(4);
      right.Add(4);
      right.Add(4);
      right.Add(4);
      right.Add(4);
      right.Add(100);
      right.Add(400);

      Console.Out.WriteLine("####");
      int expected = 0;
      foreach (Pair<int, int> pair in Joiner.LoopJoinLeft(left, right, delegate(int v) { return v; })) {
        Console.Out.WriteLine("{0}-{1}", pair.First, pair.Second);
        expected++;
      }
      Console.Out.WriteLine("####");
      int count = 0;
      foreach (Pair<int, int> pair in Joiner.MergeJoinLeft(left, right)) {
        count++;
        Console.Out.WriteLine("{0}-{1}", pair.First, pair.Second);
      }
      
      Assert.AreEqual(expected, count);
    }

    [Test]
    public void MergeJoinLeftSimple()
    {
      SortedCollection<int> left = new SortedCollection<int>(AdvancedComparer<int>.Default);
      SortedCollection<int> right = new SortedCollection<int>(AdvancedComparer<int>.Default);
      left.Add(1);
      right.Add(1);

      Console.Out.WriteLine("####");
      int expected = 0;
      foreach (Pair<int, int> pair in Joiner.LoopJoinLeft(left, right, delegate(int v) { return v; })) {
        Console.Out.WriteLine("{0}-{1}", pair.First, pair.Second);
        expected++;
      }
      Console.Out.WriteLine("####");
      int count = 0;
      foreach (Pair<int, int> pair in Joiner.MergeJoinLeft(left, right)) {
        count++;
        Console.Out.WriteLine("{0}-{1}", pair.First, pair.Second);
      }

      Assert.AreEqual(expected, count);
    }


    [Test]
    public void LoopJoin()
    {
      IList<int> left = new List<int>();
      SortedCollection<int> right = new SortedCollection<int>(AdvancedComparer<int>.Default);
      left.Add(6);
      left.Add(5);
      left.Add(1);
      left.Add(2);
      left.Add(2);
      left.Add(6);
      left.Add(100);

      right.Add(2);
      right.Add(3);
      right.Add(5);
      right.Add(6);
      right.Add(6);
      right.Add(7);
      right.Add(8);
      right.Add(9);
      right.Add(10);
      right.Add(11);
      right.Add(12);
      right.Add(12);
      right.Add(13);
      right.Add(100);
      int count = 0;
      foreach (Pair<int, int> pair in Joiner.LoopJoin(left, right, delegate(int v) { return v; })) {
        count++;
      }
      Assert.AreEqual(8, count);
    }

    [Test]
    [Explicit, Category("Performance")]
    public void JoinPerformance()
    {
      int itemCount = 10000;
      Random random = RandomManager.CreateRandom();
      IList<int> leftList = new List<int>(itemCount);
      IList<int> rightList = new List<int>(itemCount);
      
      for (int i = 0; i < itemCount; i++) {
        leftList.Add(random.Next(itemCount/10));
        rightList.Add(random.Next(itemCount*10));
      }

      SortedCollection<int> left = new SortedCollection<int>(leftList, AdvancedComparer<int>.Default);
      SortedCollection<int> right = new SortedCollection<int>(rightList, AdvancedComparer<int>.Default);
 
      int lCount = 0;
      using (new Measurement()){
        foreach (Pair<int, int> pair in Joiner.LoopJoin(left, right, delegate(int v) { return v; })){
          lCount++;
        }
      }

      int mCount = 0;
      using (new Measurement()) {
        foreach (Pair<int, int> pair in Joiner.MergeJoin(left, right)) {
          mCount++;
        }
      }
      Assert.AreEqual(lCount, mCount);

      Console.Out.WriteLine("Item count: {0}", lCount);
    }

    [Test]
    [Explicit, Category("Performance")]
    public void JoinLeftPerformance()
    {
      int itemCount = 10000;
      Random random = RandomManager.CreateRandom();
      IList<int> leftList = new List<int>(itemCount);
      IList<int> rightList = new List<int>(itemCount);

      for (int i = 0; i < itemCount; i++) {
        leftList.Add(random.Next(itemCount/10));
        rightList.Add(random.Next(itemCount*10));
      }

      SortedCollection<int> left = new SortedCollection<int>(leftList, AdvancedComparer<int>.Default);
      SortedCollection<int> right = new SortedCollection<int>(rightList, AdvancedComparer<int>.Default);

      int lCount = 0;
      using (new Measurement()) {
        foreach (Pair<int, int> pair in Joiner.LoopJoinLeft(left, right, delegate(int v) { return v; })) {
          lCount++;
        }
      }

      int mCount = 0;
      using (new Measurement()) {
        foreach (
          Pair<int, int> pair in
            Joiner.MergeJoinLeft(left, right)) {
          mCount++;
        }
      }
      Assert.AreEqual(lCount, mCount);

      Console.Out.WriteLine("Item count: {0}", lCount);
    }

  }
}