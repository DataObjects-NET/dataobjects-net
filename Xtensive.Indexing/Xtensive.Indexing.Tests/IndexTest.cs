// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.02.27

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.IoC;
using Xtensive.Testing;
using Xtensive.Indexing.Tests.Index;

namespace Xtensive.Indexing.Tests
{
  public static class IndexTest
  {
    #region Helper classes

    public class Configuration
    {
      public int Count;
      public Random Random;

      public Configuration(Random random, int count)
      {
        Random = random;
        Count = count;
      }
    }

    public class Context<TKey, TItem> : Context<Scope<TKey, TItem>>
    {
      public List<TItem> Items;
      public List<TItem> SortedItems;
      public AdvancedComparer<TItem> ItemComparer;
      public IInstanceGenerator<TItem> InstanceGenerator = InstanceGeneratorProvider.Default.GetInstanceGenerator<TItem>();
      public int Count;
      public Random Random;
      public List<TItem> MissingItems;

      public override bool IsActive
      {
        get { return Scope<TKey, TItem>.CurrentContext==this; }
      }

      protected override Scope<TKey, TItem> CreateActiveScope()
      {
        return new Scope<TKey, TItem>(this);
      }

      public Context(IOrderedIndex<TKey, TItem> index, Configuration configuration)
      {
        Random = configuration.Random;
        ItemComparer = index.KeyComparer.Provider.GetComparer<TItem>();
        Items = new List<TItem>(GetUniqueInstances<TItem>(configuration.Count, Random));
        SortedItems = new List<TItem>(Items);
        // Populate context with test data
        SetSlim<TItem> missingItems = GetUniqueInstances<TItem>(configuration.Count, Random);
        missingItems.ExceptWith(SortedItems);
        MissingItems = new List<TItem>(missingItems);
        Comparison<TItem> comparison = (x, y) => index.KeyComparer.Compare(index.KeyExtractor(x), index.KeyExtractor(y));
        SortedItems.Sort(comparison);
        MissingItems.Sort(comparison);
        Count = Items.Count;
      }
    }

    public sealed class Scope<TKey, TItem> : Scope<Context<TKey, TItem>>
    {
      public new static Context<TKey, TItem> CurrentContext
      {
        get { return Scope<Context<TKey, TItem>>.CurrentContext; }
      }

      public Scope(Context<TKey, TItem> context)
        : base(context)
      {
      }
    }

    #endregion

    public static void TestIndex<TKey, TItem>(IOrderedIndex<TKey, TItem> index, Configuration configuration)
    {
      Context<TKey, TItem> context = new Context<TKey, TItem>(index, configuration);
      using (context.Activate()) {
        TestIndex(index);
      }
    }

    private static void TestIndex<TKey, TItem>(IOrderedIndex<TKey, TItem> index)
    {
      PopulateIndex(index);
//      TryDumpIndex(index);
      Assert.Greater(index.Size, 0);
      if (index is IUniqueOrderedIndex<TKey, TItem>)
        TestGetItems((IUniqueOrderedIndex<TKey, TItem>) index);
      TestIndexReader(index);
      TestEnumerable(index);
      TestContains(index);
      TestModification(index);
      TestSeek(index);
      TestRemovableIterator(index);
    }


    private static void PopulateIndex<TKey, TItem>(IIndex<TKey, TItem> index)
    {
      Context<TKey, TItem> context = Scope<TKey, TItem>.CurrentContext;
      index.Clear();
      Assert.AreEqual(0, index.Count);
      foreach (TItem item in context.SortedItems)
        index.Add(item);
      Assert.AreEqual(context.SortedItems.Count, index.Count);
    }

    private static void TestEnumerable<TKey, TItem>(IIndex<TKey, TItem> index)
    {
      Context<TKey, TItem> context = Scope<TKey, TItem>.CurrentContext;
      IEnumerator<TItem> enumerator = index.GetEnumerator();
      int i = 0;
      while (enumerator.MoveNext()) {
        bool condition = context.ItemComparer.Equals(context.SortedItems[i++], enumerator.Current);
        Assert.IsTrue(condition);
      }
      Assert.AreEqual(i, context.SortedItems.Count);
    }

    private static void TestContains<TKey, TItem>(IIndex<TKey, TItem> index)
    {
      Context<TKey, TItem> context = Scope<TKey, TItem>.CurrentContext;
      for (int i = 0, count = context.SortedItems.Count; i < count; i++) {
        TItem item = context.SortedItems[i];
        Assert.IsTrue(index.Contains(item));
        Assert.IsTrue(index.ContainsKey(index.KeyExtractor(item)));
      }
      foreach (TItem missingItem in context.MissingItems) {
        Assert.IsFalse(index.Contains(missingItem));
        Assert.IsFalse(index.ContainsKey(index.KeyExtractor(missingItem)));
      }
    }

    private static void TestIndexReader<TKey, TItem>(IOrderedIndex<TKey, TItem> index)
    {
      Context<TKey, TItem> context = Scope<TKey, TItem>.CurrentContext;
      IIndexReader<TKey, TItem> forwardReader = index.CreateReader(index.GetFullRange());
      IIndexReader<TKey, TItem> backwardReader = index.CreateReader(index.GetFullRange().Invert());
      for (int i = 0; i < context.SortedItems.Count; i++) {
        TItem item = context.SortedItems[i];
        forwardReader.MoveTo(new Entire<TKey>(index.KeyExtractor(item)));
        forwardReader.MoveNext();
        backwardReader.MoveTo(new Entire<TKey>(index.KeyExtractor(item)));
        backwardReader.MoveNext();
        Assert.IsTrue(context.ItemComparer.Equals(item, forwardReader.Current));
        if (i==context.SortedItems.Count - 1)
          Assert.IsFalse(forwardReader.MoveNext());
        else {
          Assert.IsTrue(forwardReader.MoveNext());
          Assert.IsTrue(context.ItemComparer.Equals(context.SortedItems[i + 1], forwardReader.Current));
        }
        if (i==0)
          Assert.IsFalse(backwardReader.MoveNext());
        else {
          Assert.IsTrue(backwardReader.MoveNext());
          Assert.IsTrue(context.ItemComparer.Equals(context.SortedItems[i - 1], backwardReader.Current));
        }
      }
    }

    private static void TestRemovableIterator<TKey, TItem>(IIndex<TKey, TItem> index)
    {
      long originalCount = index.Count;
      Assert.Greater(originalCount, 0);
      int removedCount = 0;
      foreach (TItem item in index) {
        Assert.IsTrue(index.Contains(item));
        Assert.IsTrue(index.ContainsKey(index.KeyExtractor(item)));
        index.Remove(item);
        removedCount++;
        Log.Info("Removed count: {0}", removedCount);
//        TryDumpIndex(index);
        TryValidateIndex(index);
        Assert.AreEqual(originalCount - removedCount, index.Count);
        Assert.IsFalse(index.Contains(item));
        Assert.IsFalse(index.ContainsKey(index.KeyExtractor(item)));
      }
      Assert.AreEqual(0, index.Count);
      PopulateIndex(index);
    }

    private static void TryValidateIndex<TItem, TKey>(IIndex<TKey, TItem> index)
    {
      if (index is Index<TKey, TItem>)
        ((Index<TKey, TItem>) index).CheckIntegrity();
    }

    private static void TryDumpIndex<TItem, TKey>(IIndex<TKey, TItem> index)
    {
      if (index is Index<TKey, TItem>)
        ((Index<TKey, TItem>) index).DumpIndex();
    }

    private static void TestModification<TKey, TItem>(IIndex<TKey, TItem> index)
    {
      Context<TKey, TItem> context = Scope<TKey, TItem>.CurrentContext;

      // Remove tests in TestRemovableIterator test.
      long originalCount = index.Count;
      foreach(TItem item in context.Items) {
        Assert.AreEqual(originalCount, index.Count);
        Assert.IsTrue(index.Contains(item));
        Assert.IsTrue(index.ContainsKey(index.KeyExtractor(item)));
        index.Replace(item);
        Assert.IsTrue(index.Contains(item));
        Assert.IsTrue(index.ContainsKey(index.KeyExtractor(item)));
        Assert.AreEqual(originalCount, index.Count);
      }

      foreach(TItem item in context.MissingItems) {
        Assert.IsFalse(index.Contains(item));
        Assert.IsFalse(index.ContainsKey(index.KeyExtractor(item)));
        AssertEx.ThrowsArgumentOutOfRangeException(() => index.Replace(item));
        Assert.IsFalse(index.Remove(item));
        Assert.IsFalse(index.RemoveKey(index.KeyExtractor(item)));
      }
    }

    private static void TestSeek<TKey, TItem>(IOrderedIndex<TKey, TItem> index)
    {
      Context<TKey, TItem> context = Scope<TKey, TItem>.CurrentContext;
      foreach(TItem item in context.Items) {
        SeekResult<TItem> seekResult = index.Seek(new Ray<Entire<TKey>>(new Entire<TKey>(index.KeyExtractor(item))));
        Assert.AreEqual(SeekResultType.Exact, seekResult.ResultType);
        Assert.IsTrue(context.ItemComparer.Equals(item, seekResult.Result));
      }
      foreach (TItem item in context.MissingItems) {
        SeekResult<TItem> seekResult = index.Seek(new Ray<Entire<TKey>>(new Entire<TKey>(index.KeyExtractor(item))));
        Assert.AreNotEqual(SeekResultType.Exact, seekResult.ResultType);
        Assert.IsFalse(context.ItemComparer.Equals(item, seekResult.Result));
      }

      int itemIndex = context.Random.Next((int) (index.Count - 1));
      TItem removedItem = context.SortedItems[itemIndex];
      index.Remove(removedItem);
      SeekResult<TItem> removeSeekResult = index.Seek(new Ray<Entire<TKey>>(new Entire<TKey>(index.KeyExtractor(removedItem))));
      Assert.AreEqual(SeekResultType.Nearest, removeSeekResult.ResultType);
      Assert.IsTrue(context.ItemComparer.Equals(context.SortedItems[itemIndex + 1], removeSeekResult.Result));
      context.SortedItems.RemoveAt(itemIndex);

      removedItem = context.SortedItems[context.SortedItems.Count - 1];
      index.Remove(removedItem);

      removeSeekResult = index.Seek(new Ray<Entire<TKey>>(new Entire<TKey>(index.KeyExtractor(removedItem))));
      Assert.AreEqual(SeekResultType.None, removeSeekResult.ResultType);
      context.SortedItems.RemoveAt(context.SortedItems.Count - 1);
      PopulateIndex(index);
    }

    private static void TestGetItems<TKey, TItem>(IUniqueOrderedIndex<TKey, TItem> index)
    {
      Context<TKey, TItem> context = Scope<TKey, TItem>.CurrentContext;

      foreach (TItem item in context.Items) {
        TItem foundItem = index.GetItem(index.KeyExtractor(item));
        Assert.IsTrue(context.ItemComparer.Equals(item, foundItem));
      }
      foreach (TItem missingItem in context.MissingItems)
        AssertEx.Throws<KeyNotFoundException>(() => index.GetItem(index.KeyExtractor(missingItem)));

      //Getting all items in reverse order
      List<TItem> foundItems = index.GetItems(new Range<Entire<TKey>>(
        new Entire<TKey>(InfinityType.Positive),
        new Entire<TKey>(InfinityType.Negative))).ToList();
      Assert.AreEqual(context.Count, foundItems.Count);

      int i = 0;
      while (i < context.Items.Count)
      {
        Assert.IsTrue(context.ItemComparer.Equals(context.SortedItems[context.SortedItems.Count - 1 - i], foundItems[i]));
        i++;
      }

       //Getting all items in pre order
      foundItems = index.GetItems(new Range<Entire<TKey>>(
        new Entire<TKey>(InfinityType.Negative),
        new Entire<TKey>(InfinityType.Positive))).ToList();
      Assert.AreEqual(context.Items.Count, foundItems.Count);

      i = 0;
      while (i < context.Count) {
        Assert.IsTrue(context.ItemComparer.Equals(context.SortedItems[i], foundItems[i]));
        i++;
      }

      //Getting random range
      int position1 = context.Random.Next(context.Count - 1);
      int position2 = context.Random.Next(context.Count - 1);
      TItem first = context.SortedItems[position1];
      TItem second = context.SortedItems[position2];

      Range<Entire<TKey>> range = new Range<Entire<TKey>>(
        new Entire<TKey>(index.KeyExtractor(first)),
        new Entire<TKey>(index.KeyExtractor(second)));

      Direction direction = range.GetDirection(index.EntireKeyComparer);
      foundItems = index.GetItems(range).ToList();
      Assert.AreEqual((position2 - position1) * (int) direction + 1, foundItems.Count);
      Assert.IsTrue(context.ItemComparer.Equals(first, foundItems[0]));
      Assert.IsTrue(context.ItemComparer.Equals(second, foundItems[foundItems.Count - 1]));
      List<TKey> keys = index.GetKeys(range).ToList();
      Assert.AreEqual(foundItems.Count, keys.Count);
      for (i = 0; i < keys.Count; i++)
        Assert.IsTrue(index.KeyComparer.Compare(index.KeyExtractor(foundItems[i]), keys[i])==0);
    }

    private static SetSlim<T> GetUniqueInstances<T>(int count, Random random)
    {
      return new SetSlim<T>(InstanceGeneratorProvider.Default.GetInstanceGenerator<T>().GetInstances(random, count));
    }
  }
}
