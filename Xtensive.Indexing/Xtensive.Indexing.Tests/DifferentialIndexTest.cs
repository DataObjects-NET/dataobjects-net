// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.13

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Testing;
using Xtensive.Indexing.Differential;

namespace Xtensive.Indexing.Tests
{
  [TestFixture]
  public class DifferentialIndexTest
  {
    private static readonly IInstanceGenerator<int> instanceGenerator = InstanceGeneratorProvider.Default.GetInstanceGenerator<int>();
    private readonly AdvancedComparer<int> comparer = AdvancedComparer<int>.Default;
    private DifferentialIndex<int, int, SortedListIndex<int, int>> index, index1, index2;
    private SortedListIndex<int, int> sortedListIndex;

    [Test]
    public void ConstructionTest()
    {
      Test1();
      Test2();
      Test3();
      Test4();
      Test5();
      Test6();
      Test7();
      Test8();
      Test9();
      Test10();
    }

    [Test]
    public void RandomTest()
    {
      const int count = 5;
      var list = new Triplet<int>[count];

      // Step 0.
      for (int j1 = 0; j1 < 2; j1++)
        for (int j2 = 0; j2 < 2; j2++)
          for (int j3 = 0; j3 < 2; j3++)
            // Step 1.
            for (int i1 = 0; i1 < 2; i1++)
              for (int i2 = 0; i2 < 2; i2++)
                for (int i3 = 0; i3 < 2; i3++)
                  // Step 2.
                  for (int k1 = 0; k1 < 2; k1++)
                    for (int k2 = 0; k2 < 2; k2++)
                      for (int k3 = 0; k3 < 2; k3++)
                        // Step 3.
                        for (int l1 = 0; l1 < 2; l1++)
                          for (int l2 = 0; l2 < 2; l2++)
                            for (int l3 = 0; l3 < 2; l3++)
                              // Step 4.
                              for (int m1 = 0; m1 < 2; m1++)
                                for (int m2 = 0; m2 < 2; m2++)
                                  for (int m3 = 0; m3 < 2; m3++) {
                                    list[0] = new Triplet<int>(m1, m2, m3);
                                    list[1] = new Triplet<int>(l1, l2, l3);
                                    list[2] = new Triplet<int>(k1, k2, k3);
                                    list[3] = new Triplet<int>(i1, i2, i3);
                                    list[4] = new Triplet<int>(j1, j2, j3);

                                    ConstructIndex(count, list);
                                    TestIndex();
                                    ClearIndex();
                                  }
    }

    #region Private / Internal methods

    private void TestIndex()
    {
      Assert.IsNotNull(index);
      Assert.AreEqual(sortedListIndex.Count, index.Count);

      if (sortedListIndex.Count!=0)
        Assert.Greater(index.Count, 0);

      TestEnumerable(index, sortedListIndex);
      TestIndexReader(index, sortedListIndex);
      TestContains(index, sortedListIndex);
      TestSeek(index, sortedListIndex);
      TestModification(index, sortedListIndex);
      TestGetItems(index, sortedListIndex);
    }

    private void ClearIndex()
    {
      index.Insertions.Clear();
      index.Removals.Clear();
      sortedListIndex.Clear();
    }

    private static void TestEnumerable<TKey, TItem>(IIndex<TKey, TItem> difIndex, SortedListIndex<TKey, TItem> list)
    {
      IEnumerator<TItem> enumerator = difIndex.GetEnumerator();
      int i = 0;
      while (enumerator.MoveNext()) {
        Assert.AreEqual(list[i++], enumerator.Current);
      }
      Assert.AreEqual(i, list.Count);
    }

    private static void TestIndexReader<TKey, TItem>(IOrderedIndex<TKey, TItem> difIndex, SortedListIndex<TKey, TItem> list)
    {
      IIndexReader<TKey, TItem> forwardReader = difIndex.CreateReader(difIndex.GetFullRange());
      IIndexReader<TKey, TItem> backwardReader = difIndex.CreateReader(difIndex.GetFullRange().Invert());
      for (int i = 0; i < list.Count; i++) {
        TItem item = list[i];
        forwardReader.MoveTo(new Entire<TKey>(difIndex.KeyExtractor(item)));
        forwardReader.MoveNext();
        backwardReader.MoveTo(new Entire<TKey>(difIndex.KeyExtractor(item)));
        backwardReader.MoveNext();
        Assert.AreEqual(item, forwardReader.Current);
        if (i==list.Count - 1)
          Assert.IsFalse(forwardReader.MoveNext());
        else {
          Assert.IsTrue(forwardReader.MoveNext());
          Assert.AreEqual(list[i + 1], forwardReader.Current);
        }
        if (i==0)
          Assert.IsFalse(backwardReader.MoveNext());
        else {
          Assert.IsTrue(backwardReader.MoveNext());
        }
      }
    }

    private static void TestContains<TKey, TItem>(IIndex<TKey, TItem> difIndex, SortedListIndex<TKey, TItem> list)
    {
      for (int i = 0; i < list.Count; i++) {
        TItem item = list[i];
        Assert.IsTrue(difIndex.Contains(item));
        Assert.IsTrue(difIndex.ContainsKey(difIndex.KeyExtractor(item)));
      }
    }

    private static void TestModification<TKey, TItem>(IIndex<TKey, TItem> difIndex, SortedListIndex<TKey, TItem> list)
    {
      foreach (TItem item in list) {
        Assert.AreEqual(list.Count, difIndex.Count);
        Assert.IsTrue(difIndex.Contains(item));
        Assert.IsTrue(difIndex.ContainsKey(difIndex.KeyExtractor(item)));
        difIndex.Replace(item);
        Assert.IsTrue(difIndex.Contains(item));
        Assert.IsTrue(difIndex.ContainsKey(difIndex.KeyExtractor(item)));
        Assert.AreEqual(list.Count, difIndex.Count);
      }
    }

    private static void TestSeek<TKey, TItem>(IOrderedIndex<TKey, TItem> difIndex, SortedListIndex<TKey, TItem> list)
    {
      foreach (TItem item in list) {
        SeekResult<TItem> seekResult = difIndex.Seek(new Ray<Entire<TKey>>(new Entire<TKey>(difIndex.KeyExtractor(item))));
        Assert.AreEqual(SeekResultType.Exact, seekResult.ResultType);
        Assert.AreEqual(item, seekResult.Result);
      }
    }

    private static void TestGetItems<TKey, TItem>(IUniqueOrderedIndex<TKey, TItem> difIndex, SortedListIndex<TKey, TItem> list)
    {
      foreach (TItem item in list) {
        TItem foundItem = difIndex.GetItem(difIndex.KeyExtractor(item));
        Assert.AreEqual(item, foundItem);
      }

      //Getting all items in reverse order
      List<TItem> foundItems = difIndex.GetItems(new Range<Entire<TKey>>(
        new Entire<TKey>(InfinityType.Positive),
        new Entire<TKey>(InfinityType.Negative))).ToList();
      Assert.AreEqual(list.Count, foundItems.Count);

      int i = 0;
      while (i < list.Count) {
        Assert.AreEqual(list[(int) list.Count - 1 - i], foundItems[i]);
        i++;
      }

      //Getting all items in pre order
      foundItems = difIndex.GetItems(new Range<Entire<TKey>>(
        new Entire<TKey>(InfinityType.Negative),
        new Entire<TKey>(InfinityType.Positive))).ToList();
      Assert.AreEqual(list.Count, foundItems.Count);

      i = 0;
      while (i < list.Count) {
        Assert.AreEqual(list[i], foundItems[i]);
        i++;
      }
    }

    #region Random test methods.

    private void ConstructIndex(int count, Triplet<int>[] list)
    {
      var originConfiguration = new IndexConfigurationBase<int, int>(item => item, comparer);
      IUniqueOrderedIndex<int, int> origin = IndexFactory.CreateUniqueOrdered<int, int, SortedListIndex<int, int>>(originConfiguration);

      var insertionsConfiguration = new IndexConfigurationBase<int, int>(item => item, comparer);
      IUniqueOrderedIndex<int, int> insertions = IndexFactory.CreateUniqueOrdered<int, int, SortedListIndex<int, int>>(insertionsConfiguration);

      var removalsConfiguration = new IndexConfigurationBase<int, int>(item => item, comparer);
      IUniqueOrderedIndex<int, int> removals = IndexFactory.CreateUniqueOrdered<int, int, SortedListIndex<int, int>>(removalsConfiguration);

      var sortedListConfiguration = new IndexConfigurationBase<int, int>(item => item, comparer);
      sortedListIndex = new SortedListIndex<int, int>(sortedListConfiguration);

      IEnumerator<int> enumerator = instanceGenerator.GetInstances(RandomManager.CreateRandom(SeedVariatorType.CallingMethod), 10000).GetEnumerator();
      int value = enumerator.Current;
      for (int i = 0; i < count; i++) {
        while (list[i].First==1 && list[i].Second==1 && list[i].Third==0) {
          i++;
          if (i==count)
            break;
        }
        if (i < count) {
          if (IsInList(list[i]))
            sortedListIndex.Add(value);
          if (list[i].First==1)
            origin.Add(value);
          if (list[i].Second==1)
            insertions.Add(value);
          if (list[i].Third==1)
            removals.Add(value);
          enumerator.MoveNext();
          value = enumerator.Current;
        }
      }

      var indexConfig = new DifferentialIndexConfiguration<int, int>(origin, insertions, removals);
      index = IndexFactory.CreateDifferential<int, int, DifferentialIndex<int, int, SortedListIndex<int, int>>, SortedListIndex<int, int>>(indexConfig);
    }

    private static bool IsInList(Triplet<int> item)
    {
      if (item.Second==1)
        return true;
      if (item.First==1 && item.Third==0)
        return true;
      return false;
    }

    #endregion

    #region Construction test methods.

    private void Test1()
    {
      var originConfiguration = new IndexConfigurationBase<int, int>(item => item, comparer);
      var origin = IndexFactory.CreateUniqueOrdered<int, int, SortedListIndex<int, int>>(originConfiguration);
      for (int i = 0; i <= 10; i = i + 5) {
        origin.Add(i);
        origin.Add(i + 1);
        origin.Add(i + 2);
      }
      var indexConfig = new DifferentialIndexConfiguration<int, int>(origin);
      index = IndexFactory.CreateDifferential<int, int, DifferentialIndex<int, int, SortedListIndex<int, int>>, SortedListIndex<int, int>>(indexConfig);
      var sortedListConfiguration = new IndexConfigurationBase<int, int>(item => item, comparer);
      sortedListIndex = new SortedListIndex<int, int>(sortedListConfiguration);
      for (int i = 3; i <= 10; i = i + 4) {
        index.Add(i++);
        index.Add(i);
      }
      for (int i = 1; i <= 10 + 1; i = i + 4) {
        index.Remove(i++);
        index.Remove(i);
      }
      for (int i = 0; i <= 10; i = i + 5) {
        sortedListIndex.Add(i);
        if (i!=10) {
          sortedListIndex.Add(i + 3);
          sortedListIndex.Add(i + 4);
        }
      }

      TestIndex();
      
      index.Merge();
      Assert.AreEqual(index.Count, index.Origin.Count);
      Assert.AreEqual(index.Removals.Count, index.Insertions.Count, 0);
      
      TestIndex();
      ClearIndex();
    }

    private void Test2()
    {
      var originConfiguration = new IndexConfigurationBase<int, int>(item => item, comparer);
      var origin = IndexFactory.CreateUniqueOrdered<int, int, SortedListIndex<int, int>>(originConfiguration);
      for (int i = 1; i < 10; i += 2)
        origin.Add(i);
      var indexConfig = new DifferentialIndexConfiguration<int, int>(origin);
      index = IndexFactory.CreateDifferential<int, int, DifferentialIndex<int, int, SortedListIndex<int, int>>, SortedListIndex<int, int>>(indexConfig);
      var sortedListConfiguration = new IndexConfigurationBase<int, int>(item => item, comparer);
      sortedListIndex = new SortedListIndex<int, int>(sortedListConfiguration);
      for (int i = 0; i <= 10; i++)
        index.Add(i);
      for (int i = 0; i <= 10; i++)
        sortedListIndex.Add(i);

      TestIndex();

      index.Merge();
      Assert.AreEqual(index.Count, index.Origin.Count);
      Assert.AreEqual(index.Removals.Count, index.Insertions.Count, 0);

      TestIndex();
      ClearIndex();
    }

    private void Test3()
    {
      var originConfiguration = new IndexConfigurationBase<int, int>(item => item, comparer);
      IUniqueOrderedIndex<int, int> origin = IndexFactory.CreateUniqueOrdered<int, int, SortedListIndex<int, int>>(originConfiguration);
      for (int i = 0; i <= 6; i++)
        origin.Add(i);
      var indexConfig = new DifferentialIndexConfiguration<int, int>(origin);
      index = IndexFactory.CreateDifferential<int, int, DifferentialIndex<int, int, SortedListIndex<int, int>>, SortedListIndex<int, int>>(indexConfig);
      var sortedListConfiguration = new IndexConfigurationBase<int, int>(item => item, comparer);
      sortedListIndex = new SortedListIndex<int, int>(sortedListConfiguration);
      for (int i = 0; i <= 6; i += 2)
        index.Remove(i);
      for (int i = 1; i <= 5; i += 2)
        sortedListIndex.Add(i);

      TestIndex();

      index.Merge();
      Assert.AreEqual(index.Count, index.Origin.Count);
      Assert.AreEqual(index.Removals.Count, index.Insertions.Count, 0);

      TestIndex();
      ClearIndex();
    }

    private void Test4()
    {
      var originConfiguration = new IndexConfigurationBase<int, int>(item => item, comparer);
      IUniqueOrderedIndex<int, int> origin = IndexFactory.CreateUniqueOrdered<int, int, SortedListIndex<int, int>>(originConfiguration);
      for (int i = 0; i <= 5; i++)
        origin.Add(i);
      var indexConfig = new DifferentialIndexConfiguration<int, int>(origin);
      index = IndexFactory.CreateDifferential<int, int, DifferentialIndex<int, int, SortedListIndex<int, int>>, SortedListIndex<int, int>>(indexConfig);
      var sortedListConfiguration = new IndexConfigurationBase<int, int>(item => item, comparer);
      sortedListIndex = new SortedListIndex<int, int>(sortedListConfiguration);
      index.Remove(1);
      index.Remove(5);
      index.Add(1);
      index.Add(5);
      for (int i = 0; i <= 5; i++)
        sortedListIndex.Add(i);

      TestIndex();

      index.Merge();
      Assert.AreEqual(index.Count, index.Origin.Count);
      Assert.AreEqual(index.Removals.Count, index.Insertions.Count, 0);

      TestIndex();
      ClearIndex();
    }

    private void Test5()
    {
      var originConfiguration = new IndexConfigurationBase<int, int>(item => item, comparer);
      var origin = IndexFactory.CreateUniqueOrdered<int, int, SortedListIndex<int, int>>(originConfiguration);
      for (int i = 7; i <= 10; i++)
        origin.Add(i);
      var indexConfig = new DifferentialIndexConfiguration<int, int>(origin);
      index = IndexFactory.CreateDifferential<int, int, DifferentialIndex<int, int, SortedListIndex<int, int>>, SortedListIndex<int, int>>(indexConfig);
      var sortedListConfiguration = new IndexConfigurationBase<int, int>(item => item, comparer);
      sortedListIndex = new SortedListIndex<int, int>(sortedListConfiguration);
      for (int i = 0; i <= 3; i++)
        index.Add(i);
      for (int i = 13; i <= 15; i++)
        index.Add(i);
      for (int i = 9; i <= 10; i++)
        index.Remove(i);
      for (int i = 0; i <= 3; i++)
        sortedListIndex.Add(i);
      sortedListIndex.Add(7);
      sortedListIndex.Add(8);
      for (int i = 13; i <= 15; i++)
        sortedListIndex.Add(i);

      TestIndex();

      index.Merge();
      Assert.AreEqual(index.Count, index.Origin.Count);
      Assert.AreEqual(index.Removals.Count, index.Insertions.Count, 0);

      TestIndex();
      ClearIndex();
    }

    private void Test6()
    {
      var originConfiguration = new IndexConfigurationBase<int, int>(item => item, comparer);
      var origin = IndexFactory.CreateUniqueOrdered<int, int, SortedListIndex<int, int>>(originConfiguration);
      for (int i = 0; i <= 5; i++)
        origin.Add(i);
      var indexConfig = new DifferentialIndexConfiguration<int, int>(origin);
      index = IndexFactory.CreateDifferential<int, int, DifferentialIndex<int, int, SortedListIndex<int, int>>, SortedListIndex<int, int>>(indexConfig);
      var sortedListConfiguration = new IndexConfigurationBase<int, int>(item => item, comparer);
      sortedListIndex = new SortedListIndex<int, int>(sortedListConfiguration);
      for (int i = 0; i <= 5; i++)
        index.Remove(i);

      TestIndex();

      index.Merge();
      Assert.AreEqual(index.Count, index.Origin.Count);
      Assert.AreEqual(index.Removals.Count, index.Insertions.Count, 0);

      TestIndex();
      ClearIndex();
    }

    private void Test7()
    {
      var originConfiguration = new IndexConfigurationBase<int, int>(item => item, comparer);
      var origin = IndexFactory.CreateUniqueOrdered<int, int, SortedListIndex<int, int>>(originConfiguration);
      for (int i = 0; i <= 5; i++)
        origin.Add(i);
      var indexConfig = new DifferentialIndexConfiguration<int, int>(origin);
      index = IndexFactory.CreateDifferential<int, int, DifferentialIndex<int, int, SortedListIndex<int, int>>, SortedListIndex<int, int>>(indexConfig);
      var sortedListConfiguration = new IndexConfigurationBase<int, int>(item => item, comparer);
      sortedListIndex = new SortedListIndex<int, int>(sortedListConfiguration);
      for (int i = 0; i <= 5; i++)
        index.Remove(i);
      for (int i = 0; i <= 5; i++) {
        index.Add(i);
        sortedListIndex.Add(i);
      }

      TestIndex();

      index.Merge();
      Assert.AreEqual(index.Count, index.Origin.Count);
      Assert.AreEqual(index.Removals.Count, index.Insertions.Count, 0);

      TestIndex();
      ClearIndex();
    }

    private void Test8()
    {
      var originConfiguration = new IndexConfigurationBase<int, int>(item => item, comparer);
      var origin = IndexFactory.CreateUniqueOrdered<int, int, SortedListIndex<int, int>>(originConfiguration);
      origin.Add(0);
      origin.Add(1);
      origin.Add(5);
      var indexConfig = new DifferentialIndexConfiguration<int, int>(origin);
      index = IndexFactory.CreateDifferential<int, int, DifferentialIndex<int, int, SortedListIndex<int, int>>, SortedListIndex<int, int>>(indexConfig);
      index.Remove(0);
      index.Remove(1);
      index.Add(-3);
      index.Add(-2);
      index.Add(-1);
      index.Add(2);
      index.Add(3);
      index.Remove(-3);
      index.Add(1);

      var sortedListConfiguration = new IndexConfigurationBase<int, int>(item => item, comparer);
      sortedListIndex = new SortedListIndex<int, int>(sortedListConfiguration) {-2, -1, 1, 2, 3, 5};

      TestIndex();

      index.Merge();
      Assert.AreEqual(index.Count, index.Origin.Count);
      Assert.AreEqual(index.Removals.Count, index.Insertions.Count, 0);

      TestIndex();
      ClearIndex();
    }

    private void Test9()
    {
      var originConfiguration = new IndexConfigurationBase<int, int>(item => item, comparer);
      var origin = IndexFactory.CreateUniqueOrdered<int, int, SortedListIndex<int, int>>(originConfiguration);
      origin.Add(-3);
      origin.Add(-2);
      origin.Add(-1);
      origin.Add(4);
      origin.Add(5);
      var indexConfig = new DifferentialIndexConfiguration<int, int>(origin);
      index = IndexFactory.CreateDifferential<int, int, DifferentialIndex<int, int, SortedListIndex<int, int>>, SortedListIndex<int, int>>(indexConfig);
      index.Add(1);
      index.Add(2);
      index.Remove(1);
      index.Remove(2);

      var sortedListConfiguration = new IndexConfigurationBase<int, int>(item => item, comparer);
      sortedListIndex = new SortedListIndex<int, int>(sortedListConfiguration) {-3, -2, -1, 4, 5};

      TestIndex();

      index.Merge();
      Assert.AreEqual(index.Count, index.Origin.Count);
      Assert.AreEqual(index.Removals.Count, index.Insertions.Count, 0);

      TestIndex();
      ClearIndex();
    }

    private void Test10()
    {
      var originConfiguration = new IndexConfigurationBase<int, int>(item => item, comparer);
      var origin = IndexFactory.CreateUniqueOrdered<int, int, SortedListIndex<int, int>>(originConfiguration);
      origin.Add(0);
      origin.Add(1);
      var indexConfig = new DifferentialIndexConfiguration<int, int>(origin);
      index = IndexFactory.CreateDifferential<int, int, DifferentialIndex<int, int, SortedListIndex<int, int>>, SortedListIndex<int, int>>(indexConfig);
      index.Remove(0);
      index.Remove(1);
      IndexTest.TestIndex(index, new IndexTest.Configuration(RandomManager.CreateRandom(SeedVariatorType.CallingMethod), 100));

      ClearIndex();
    }

    #endregion

    #endregion
  }
}