// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.13

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Comparison;
using Xtensive.Core.Testing;
using Xtensive.Indexing.Differential;

namespace Xtensive.Indexing.Tests
{
  [TestFixture]
  public class DifferentialIndexTest
  {
    private readonly AdvancedComparer<int> comparer = AdvancedComparer<int>.Default;
    private DifferentialIndex<int, int, SortedListIndex<int, int>> index;
    private List<int> sortedList;

    [Test]
    public void ConstructionTest()
    {
      Test1();
      TestIndex();
      ClearIndex();
      Test2();
      TestIndex();
      ClearIndex();
      Test3();
      TestIndex();
      ClearIndex();
      Test4();
      TestIndex();
      ClearIndex();
      Test5();
      TestIndex();
      ClearIndex();
      Test6();
      TestIndex();
      ClearIndex();
      Test7();
      TestIndex();
      ClearIndex();

      Test8();
    }

    private void TestIndex()
    {
      Assert.IsNotNull(index);
      Assert.AreEqual(sortedList.Count, index.Count);

      if (sortedList.Count!=0)
        Assert.Greater(index.Count, 0);

      TestEnumerable(index, sortedList);
      TestIndexReader(index, sortedList);
      TestContains(index, sortedList);
      TestSeek(index, sortedList);
      TestModification(index, sortedList);
      TestGetItems(index, sortedList);
    }

    private void ClearIndex()
    {
      index.Insertions.Clear();
      index.Removals.Clear();
      sortedList.Clear();
    }

    private void Test1()
    {
      // A)-------(0-1-2-5-6-7-10-11-12 || 3-4-8-9 || 1-2-6-7-11-12)
      IndexConfigurationBase<int, int> originConfiguration = new IndexConfigurationBase<int, int>(item => item, comparer);
      IUniqueOrderedIndex<int, int> origin = IndexFactory.CreateUniqueOrdered<int, int, SortedListIndex<int, int>>(originConfiguration);
      for (int i = 0; i <= 10; i = i + 5) {
        origin.Add(i);
        origin.Add(i + 1);
        origin.Add(i + 2);
      }
      DifferentialIndexConfiguration<int, int> indexConfig = new DifferentialIndexConfiguration<int, int>(origin);
      index = IndexFactory.CreateDifferential<int, int, DifferentialIndex<int, int, SortedListIndex<int, int>>, SortedListIndex<int, int>>(indexConfig);
      sortedList = new List<int>();
      for (int i = 3; i <= 10; i = i + 4) {
        index.Add(i++);
        index.Add(i);
      }
      for (int i = 1; i <= 10 + 1; i = i + 4) {
        index.Remove(i++);
        index.Remove(i);
      }
      for (int i = 0; i <= 10; i = i + 5) {
        sortedList.Add(i);
        if (i!=10) {
          sortedList.Add(i + 3);
          sortedList.Add(i + 4);
        }
      }
    }

    private void Test2()
    {
      // B)-------(1-3-5-7-9 || 0-2-4-6-8-10)
      IndexConfigurationBase<int, int> originConfiguration = new IndexConfigurationBase<int, int>(item => item, comparer);
      IUniqueOrderedIndex<int, int> origin = IndexFactory.CreateUniqueOrdered<int, int, SortedListIndex<int, int>>(originConfiguration);
      for (int i = 1; i < 10; i += 2)
        origin.Add(i);
      DifferentialIndexConfiguration<int, int> indexConfig = new DifferentialIndexConfiguration<int, int>(origin);
      index = IndexFactory.CreateDifferential<int, int, DifferentialIndex<int, int, SortedListIndex<int, int>>, SortedListIndex<int, int>>(indexConfig);
      sortedList = new List<int>();
      for (int i = 0; i <= 10; i++)
        index.Add(i);
      for (int i = 0; i <= 10; i++)
        sortedList.Add(i);
    }

    private void Test3()
    {
      // C)-------(0-1-2-3-4-5-6 || 0-2-4-6-8 R)
      IndexConfigurationBase<int, int> originConfiguration = new IndexConfigurationBase<int, int>(item => item, comparer);
      IUniqueOrderedIndex<int, int> origin = IndexFactory.CreateUniqueOrdered<int, int, SortedListIndex<int, int>>(originConfiguration);
      for (int i = 0; i <= 6; i++)
        origin.Add(i);
      DifferentialIndexConfiguration<int, int> indexConfig = new DifferentialIndexConfiguration<int, int>(origin);
      index = IndexFactory.CreateDifferential<int, int, DifferentialIndex<int, int, SortedListIndex<int, int>>, SortedListIndex<int, int>>(indexConfig);
      sortedList = new List<int>();
      for (int i = 0; i <= 6; i += 2)
        index.Remove(i);
      for (int i = 1; i <= 5; i += 2)
        sortedList.Add(i);
    }

    private void Test4()
    {
      // D)--------(0-1-2-3-4-5 || 1-3-5-7 || 1-3-5-7)
      IndexConfigurationBase<int, int> originConfiguration = new IndexConfigurationBase<int, int>(item => item, comparer);
      IUniqueOrderedIndex<int, int> origin = IndexFactory.CreateUniqueOrdered<int, int, SortedListIndex<int, int>>(originConfiguration);
      for (int i = 0; i <= 5; i++)
        origin.Add(i);
      DifferentialIndexConfiguration<int, int> indexConfig = new DifferentialIndexConfiguration<int, int>(origin);
      index = IndexFactory.CreateDifferential<int, int, DifferentialIndex<int, int, SortedListIndex<int, int>>, SortedListIndex<int, int>>(indexConfig);
      sortedList = new List<int>();
      index.Remove(1);
      index.Remove(5);
      index.Add(1);
      index.Add(5);
      for (int i = 0; i <= 5; i++)
        sortedList.Add(i);
    }

    private void Test5()
    {
      // E)-------(I 1-2-3 || O 7-8-9-10 || R 9-10 || I 13-14-15)
      IndexConfigurationBase<int, int> originConfiguration = new IndexConfigurationBase<int, int>(item => item, comparer);
      IUniqueOrderedIndex<int, int> origin = IndexFactory.CreateUniqueOrdered<int, int, SortedListIndex<int, int>>(originConfiguration);
      for (int i = 7; i <= 10; i++)
        origin.Add(i);
      DifferentialIndexConfiguration<int, int> indexConfig = new DifferentialIndexConfiguration<int, int>(origin);
      index = IndexFactory.CreateDifferential<int, int, DifferentialIndex<int, int, SortedListIndex<int, int>>, SortedListIndex<int, int>>(indexConfig);
      sortedList = new List<int>();
      for (int i = 0; i <= 3; i++)
        index.Add(i);
      for (int i = 13; i <= 15; i++)
        index.Add(i);
      for (int i = 9; i <= 10; i++)
        index.Remove(i);
      for (int i = 0; i <= 3; i++)
        sortedList.Add(i);
      sortedList.Add(7);
      sortedList.Add(8);
      for (int i = 13; i <= 15; i++)
        sortedList.Add(i);
    }

    private void Test6()
    {
      // F)-----------------(O == R)
      IndexConfigurationBase<int, int> originConfiguration = new IndexConfigurationBase<int, int>(item => item, comparer);
      IUniqueOrderedIndex<int, int> origin = IndexFactory.CreateUniqueOrdered<int, int, SortedListIndex<int, int>>(originConfiguration);
      for (int i = 0; i <= 5; i++)
        origin.Add(i);
      DifferentialIndexConfiguration<int, int> indexConfig = new DifferentialIndexConfiguration<int, int>(origin);
      index = IndexFactory.CreateDifferential<int, int, DifferentialIndex<int, int, SortedListIndex<int, int>>, SortedListIndex<int, int>>(indexConfig);
      sortedList = new List<int>();
      for (int i = 0; i <= 5; i++)
        index.Remove(i);
    }

    private void Test7()
    {
      // G)------------------(O == R == I)
      IndexConfigurationBase<int, int> originConfiguration = new IndexConfigurationBase<int, int>(item => item, comparer);
      IUniqueOrderedIndex<int, int> origin = IndexFactory.CreateUniqueOrdered<int, int, SortedListIndex<int, int>>(originConfiguration);
      for (int i = 0; i <= 5; i++)
        origin.Add(i);
      DifferentialIndexConfiguration<int, int> indexConfig = new DifferentialIndexConfiguration<int, int>(origin);
      index = IndexFactory.CreateDifferential<int, int, DifferentialIndex<int, int, SortedListIndex<int, int>>, SortedListIndex<int, int>>(indexConfig);
      sortedList = new List<int>();
      for (int i = 0; i <= 5; i++)
        index.Remove(i);
      for (int i = 0; i <= 5; i++) {
        index.Add(i);
        sortedList.Add(i);
      }
    }

    private void Test8()
    {
      IndexConfigurationBase<int, int> originConfiguration = new IndexConfigurationBase<int, int>(item => item, comparer);
      IUniqueOrderedIndex<int, int> origin = IndexFactory.CreateUniqueOrdered<int, int, SortedListIndex<int, int>>(originConfiguration);
      DifferentialIndexConfiguration<int, int> indexConfig = new DifferentialIndexConfiguration<int, int>(origin);
      index = IndexFactory.CreateDifferential<int, int, DifferentialIndex<int, int, SortedListIndex<int, int>>, SortedListIndex<int, int>>(indexConfig);
      IndexTest.TestIndex(index, new IndexTest.Configuration(RandomManager.CreateRandom(SeedVariatorType.CallingMethod), 100));
    }

    private void TestEnumerable<TKey, TItem>(IIndex<TKey, TItem> difIndex, List<TItem> list)
    {
      IEnumerator<TItem> enumerator = difIndex.GetEnumerator();
      int i = 0;
      while (enumerator.MoveNext()) {
        Assert.AreEqual(list[i++], enumerator.Current);
      }
      Assert.AreEqual(i, list.Count);
    }

    private void TestIndexReader<TKey, TItem>(IOrderedIndex<TKey, TItem> difIndex, List<TItem> list)
    {
      IIndexReader<TKey, TItem> forwardReader = difIndex.CreateReader(difIndex.GetFullRange());
      IIndexReader<TKey, TItem> backwardReader = difIndex.CreateReader(difIndex.GetFullRange().Invert());
      for (int i = 0; i < list.Count; i++) {
        TItem item = list[i];
        forwardReader.MoveTo(Entire<TKey>.Create(difIndex.KeyExtractor(item)));
        forwardReader.MoveNext();
        backwardReader.MoveTo(Entire<TKey>.Create(difIndex.KeyExtractor(item)));
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

    private void TestContains<TKey, TItem>(IIndex<TKey, TItem> difIndex, List<TItem> list)
    {
      for (int i = 0; i < list.Count; i++) {
        TItem item = list[i];
        Assert.IsTrue(difIndex.Contains(item));
        Assert.IsTrue(difIndex.ContainsKey(difIndex.KeyExtractor(item)));
      }
    }

    private void TestModification<TKey, TItem>(IIndex<TKey, TItem> difIndex, List<TItem> list)
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

    private void TestSeek<TKey, TItem>(IOrderedIndex<TKey, TItem> difIndex, List<TItem> list)
    {
      foreach (TItem item in list) {
        SeekResult<TItem> seekResult = difIndex.Seek(new Ray<IEntire<TKey>>(Entire<TKey>.Create(difIndex.KeyExtractor(item))));
        Assert.AreEqual(SeekResultType.Exact, seekResult.ResultType);
        Assert.AreEqual(item, seekResult.Result);
      }
    }

    private void TestGetItems<TKey, TItem>(IUniqueOrderedIndex<TKey, TItem> difIndex, List<TItem> list)
    {
      foreach (TItem item in list) {
        TItem foundItem = difIndex.GetItem(difIndex.KeyExtractor(item));
        Assert.AreEqual(item, foundItem);
      }

      //Getting all items in reverse order
      List<TItem> foundItems = difIndex.GetItems(new Range<IEntire<TKey>>(
        Entire<TKey>.Create(InfinityType.Positive),
        Entire<TKey>.Create(InfinityType.Negative))).ToList();
      Assert.AreEqual(list.Count, foundItems.Count);

      int i = 0;
      while (i < list.Count) {
        Assert.AreEqual(list[list.Count - 1 - i], foundItems[i]);
        i++;
      }

      //Getting all items in pre order
      foundItems = difIndex.GetItems(new Range<IEntire<TKey>>(
        Entire<TKey>.Create(InfinityType.Negative),
        Entire<TKey>.Create(InfinityType.Positive))).ToList();
      Assert.AreEqual(list.Count, foundItems.Count);

      i = 0;
      while (i < list.Count) {
        Assert.AreEqual(list[i], foundItems[i]);
        i++;
      }
    }
  }
}