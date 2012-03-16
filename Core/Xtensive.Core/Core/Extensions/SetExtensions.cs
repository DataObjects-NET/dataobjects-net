// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.10.24

using System;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Resources;

namespace Xtensive.Core
{
  /// <summary>
  /// <see cref="Collections.ISet{TItem}"/> related extension methods.
  /// </summary>
  public static class SetExtensions
  {
    #region IsXxx methods

    /// <summary>
    /// Determines whether the specified <paramref name="set"/> and the 
    /// specified set of <paramref name="items"/> are disjoint.
    /// </summary>
    /// <typeparam name="TItem">The type of set items.</typeparam>
    /// <param name="set">The set.</param>
    /// <param name="items">The set of items.</param>
    /// <returns>
    /// <see langword="True"/> if the specified <paramref name="set"/> and the specified 
    /// set of <paramref name="items"/> are disjoint; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsDisjointWith<TItem>(this Collections.ISet<TItem> set, IEnumerable<TItem> items)
    {
      if (ReferenceEquals(set, items) && set.Count==0)
        return true;
      AssertAreComparable(set, items);
      if (set.Count==0 || items.IsNullOrEmpty())
        return true;

      var itemsAsSet = items as Collections.ISet<TItem>;
      if (itemsAsSet != null)
        return (set.Count > itemsAsSet.Count ? set.ContainsNone(itemsAsSet) : itemsAsSet.ContainsNone(set));

      return set.ContainsNone(items);
    }

    /// <summary>
    /// Determines whether the specified <paramref name="set"/> and the 
    /// specified set of <paramref name="items"/> are equal.
    /// </summary>
    /// <typeparam name="TItem">The type of set items.</typeparam>
    /// <param name="set">The set.</param>
    /// <param name="items">The set of items.</param>
    /// <returns>
    /// <see langword="True"/> if the specified <paramref name="set"/> and the specified 
    /// set of <paramref name="items"/> are equal; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsEqualTo<TItem>(this Collections.ISet<TItem> set, IEnumerable<TItem> items)
    {
      if (ReferenceEquals(set, items))
        return true;
      if (!AreComparable(set, items))
        return false;

      if (set.IsNullOrEmpty() && items.IsNullOrEmpty())
        return true;

      var itemsAsSet = items as Collections.ISet<TItem> ?? new SetSlim<TItem>(items, set.Comparer);
      return set.Count != itemsAsSet.Count ? false : set.ContainsAll(itemsAsSet);
    }

    /// <summary>
    /// Determines whether the specified <paramref name="set"/> is a proper subset of the 
    /// specified set of <paramref name="items"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of set items.</typeparam>
    /// <param name="set">The set to check if it's proper subset of <paramref name="items"/>.</param>
    /// <param name="items">The set of items.</param>
    /// <returns>
    /// <see langword="True"/> if the specified <paramref name="set"/> is a proper subset of the specified 
    /// set of <paramref name="items"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsProperSubsetOf<TItem>(this Collections.ISet<TItem> set, IEnumerable<TItem> items)
    {
      if (ReferenceEquals(set, items))
        return false;
      AssertAreComparable(set, items);

      long? itemCount = items.TryGetLongCount();
      bool setIsEmpty = set.Count==0;
      bool itemsAreEmpty = itemCount.HasValue && itemCount.GetValueOrDefault()==0;

      if (setIsEmpty && itemsAreEmpty)
        return false;
      if (setIsEmpty)
        return true;
      if (itemsAreEmpty)
        return false;

      if (itemCount.HasValue && set.Count >= itemCount.GetValueOrDefault())
        return false;

      var itemsAsSet = items as Collections.ISet<TItem> ?? new SetSlim<TItem>(items, set.Comparer);
      return itemsAsSet.ContainsAll(set);
    }

    /// <summary>
    /// Determines whether the specified set of <paramref name="items"/> is a proper subset of the 
    /// specified <paramref name="set"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of set items.</typeparam>
    /// <param name="set">The set.</param>
    /// <param name="items">The set of items to check if it's proper subset of <paramref name="set"/>.</param>
    /// <returns>
    /// <see langword="True"/> if the specified <paramref name="set"/> is a proper subset of the specified 
    /// set of <paramref name="items"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsProperSupersetOf<TItem>(this Collections.ISet<TItem> set, IEnumerable<TItem> items)
    {
      if (ReferenceEquals(set, items))
        return false;
      AssertAreComparable(set, items);

      long? itemCount = items.TryGetLongCount();
      bool setIsEmpty = set.Count==0;
      bool itemsAreEmpty = itemCount.HasValue && itemCount.GetValueOrDefault()==0;

      if (setIsEmpty && itemsAreEmpty)
        return false;
      if (itemsAreEmpty)
        return true;
      if (setIsEmpty)
        return false;

      if (itemCount.HasValue && set.Count <= itemCount.GetValueOrDefault())
        return false;

      return set.ContainsAll(items);
    }

    /// <summary>
    /// Determines whether the specified <paramref name="set"/> is a subset of the 
    /// specified set of <paramref name="items"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of set items.</typeparam>
    /// <param name="set">The set to check if it's subset of <paramref name="items"/>.</param>
    /// <param name="items">The set of items.</param>
    /// <returns>
    /// <see langword="True"/> if the specified <paramref name="set"/> is subset of the specified 
    /// set of <paramref name="items"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsSubsetOf<TItem>(this Collections.ISet<TItem> set, IEnumerable<TItem> items)
    {
      AssertAreComparable(set, items);
      Collections.ISet<TItem> superSet = items as Collections.ISet<TItem> ?? new SetSlim<TItem>(items, set.Comparer);
      return IsSupersetOf(superSet, set);
    }

    /// <summary>
    /// Determines whether the specified set of <paramref name="items"/> is a subset of the 
    /// specified <paramref name="set"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of set items.</typeparam>
    /// <param name="set">The set.</param>
    /// <param name="items">The set of items to check if it's a subset of <paramref name="set"/>.</param>
    /// <returns>
    /// <see langword="True"/> if the specified <paramref name="set"/> is subset of the specified 
    /// set of <paramref name="items"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsSupersetOf<TItem>(this Collections.ISet<TItem> set, IEnumerable<TItem> items)
    {
      if (ReferenceEquals(set, items))
        return true;
      AssertAreComparable(set, items);

      long? itemCount = items.TryGetLongCount();
      bool itemsAreEmpty = itemCount.HasValue && itemCount.GetValueOrDefault()==0;

      if (set.Count==0)
        return false;
      if (itemsAreEmpty)
        return true;

      if (itemCount.HasValue && set.Count < itemCount.GetValueOrDefault())
        return false;

      return set.ContainsAll(items);
    }

    /// <summary>
    /// Determines whether the specified <paramref name="set"/> overlaps the 
    /// specified set of <paramref name="items"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of set items.</typeparam>
    /// <param name="set">The set to check if it overlaps the <paramref name="items"/>.</param>
    /// <param name="items">The set of items.</param>
    /// <returns>
    /// <see langword="True"/> if the specified <paramref name="set"/> overlaps the specified 
    /// set of <paramref name="items"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsOverlappingWith<TItem>(this Collections.ISet<TItem> set, IEnumerable<TItem> items)
    {
      if (ReferenceEquals(set, items))
        return true;
      AssertAreComparable(set, items);

      long? itemCount = items.TryGetLongCount();
      bool itemsAreEmpty = itemCount.HasValue && itemCount.GetValueOrDefault()==0;

      if (set.Count==0)
        return false;
      if (itemsAreEmpty)
        return true;

      var itemsAsSet = items as Collections.ISet<TItem>;
      return
        itemsAsSet!=null && itemCount.HasValue && set.Count < itemCount.GetValueOrDefault()
          ? itemsAsSet.ContainsAny(set)
          : set.ContainsAny(items);
    }

    #endregion

    #region Union, UnionWith

    /// <summary>
    /// Unions the specified <paramref name="set"/> and the 
    /// specified set of <paramref name="items"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of set items.</typeparam>
    /// <typeparam name="TSet">The type of set to return.</typeparam>
    /// <param name="set">The set.</param>
    /// <param name="items">The set of items.</param>
    /// <returns>A set derived from union of the specified <paramref name="set"/> 
    /// and the specified set of <paramref name="items"/>.
    /// </returns>
    public static TSet Union<TItem, TSet>(this Collections.ISet<TItem> set, IEnumerable<TItem> items)
      where TSet: Collections.ISet<TItem>, new()
    {
      return (TSet)UnionWith(UnionWith(new TSet(), set), items);
    }

    /// <summary>
    /// Unions the specified <paramref name="target"/> and the 
    /// specified set of <paramref name="items"/> in the result set <paramref name="target"/>.
    /// </summary>
    /// <param name="target">The set.</param>
    /// <param name="items">The set of items.</param>
    /// <returns>A set derived from union of the specified <paramref name="target"/> 
    /// and the specified set of <paramref name="items"/>.
    /// </returns>
    public static Collections.ISet<TItem> UnionWith<TItem>(this Collections.ISet<TItem> target, IEnumerable<TItem> items)
    {
      if (ReferenceEquals(target, items))
        return target;
      AssertAreComparable(target, items);

      if (items.IsNullOrEmpty())
        return target;

      foreach (TItem item in items)
        target.Add(item);
      return target;
    }

    #endregion

    #region Intersect, IntersectWith, AddIntersectionOf
    
    /// <summary>
    /// Intersects the specified <paramref name="set"/> and the 
    /// specified set of <paramref name="items"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of set items.</typeparam>
    /// <typeparam name="TSet">The type of set to return.</typeparam>
    /// <param name="set">The set.</param>
    /// <param name="items">The set of items.</param>
    /// <returns>A set derived from intersection of the specified <paramref name="set"/> 
    /// and the specified set of <paramref name="items"/>.
    /// </returns>
    public static TSet Intersect<TItem, TSet>(this Collections.ISet<TItem> set, IEnumerable<TItem> items)
      where TSet: Collections.ISet<TItem>, new()
    {
      return (TSet)IntersectWith(UnionWith(new TSet(), set), items);
    }
    
    /// <summary>
    /// Intersects the specified <paramref name="set"/> and the 
    /// specified set of <paramref name="items"/> in the result set <paramref name="target"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of set items.</typeparam>
    /// <param name="target">The result set.</param>
    /// <param name="set">The set.</param>
    /// <param name="items">The set of items.</param>
    /// <returns>A set derived from intersection of the specified <paramref name="set"/> 
    /// and the specified set of <paramref name="items"/>.
    /// </returns>
    public static Collections.ISet<TItem> AddIntersectionOf<TItem>(this Collections.ISet<TItem> target, Collections.ISet<TItem> set, IEnumerable<TItem> items)
    {
      if (ReferenceEquals(set, items))
        return UnionWith(target, set);
      AssertAreComparable(set, items);

      if (set.Count==0 || items.IsNullOrEmpty())
        return target;

      var itemsAsSet = items as Collections.ISet<TItem>;
      if (itemsAsSet != null && itemsAsSet.Count > set.Count) {
        foreach (TItem item in set)
          if (itemsAsSet.Contains(item))
            target.Add(item);
      }
      else {
        foreach (TItem item in items)
          if (set.Contains(item))
            target.Add(item);
      }
      return target;
    }
    
    /// <summary>
    /// Intersects the specified <paramref name="target"/> and the 
    /// specified set of <paramref name="items"/> in the result set <paramref name="target"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of set items.</typeparam>
    /// <param name="target">The set.</param>
    /// <param name="items">The set of items.</param>
    /// <returns>A set derived from intersection of the specified <paramref name="target"/> 
    /// and the specified set of <paramref name="items"/>.
    /// </returns>
    public static Collections.ISet<TItem> IntersectWith<TItem>(this Collections.ISet<TItem> target, IEnumerable<TItem> items)
    {
      if (ReferenceEquals(target, items))
        return target;
      AssertAreComparable(target, items);

      if (target.Count==0)
        return target;
      if (items.IsNullOrEmpty()) {
        target.Clear();
        return target;
      }

      var set = items as Collections.ISet<TItem>;
      return set != null ? IntersectWithSet(target, set) : IntersectWithEnumerable(target, items);
    }

    #endregion

    #region Except, ExceptWith, AddExceptionOf
    
    /// <summary>
    /// Excepts the specified set of <paramref name="items"/> from the 
    /// specified <paramref name="set"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of set items.</typeparam>
    /// <typeparam name="TSet">The type of set to return.</typeparam>
    /// <param name="set">The set.</param>
    /// <param name="items">The set of items.</param>
    /// <returns>A set derived from excepting of the specified set of <paramref name="items"/>
    /// from the specified <paramref name="set"/>.
    /// </returns>
    public static TSet Except<TItem, TSet>(this Collections.ISet<TItem> set, IEnumerable<TItem> items)
      where TSet: Collections.ISet<TItem>, new()
    {
      return (TSet)ExceptWith(UnionWith(new TSet(), set), items);
    }

    /// <summary>
    /// Excepts the specified set of <paramref name="items"/> from the 
    /// specified <paramref name="set"/> in the result set <paramref name="target"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of set items.</typeparam>
    /// <param name="target">The result set.</param>
    /// <param name="set">The set.</param>
    /// <param name="items">The set of items.</param>
    /// <returns>A set derived from excepting of the specified set of <paramref name="items"/>
    /// from the specified <paramref name="set"/>.
    /// </returns>
    public static Collections.ISet<TItem> AddExceptionOf<TItem>(this Collections.ISet<TItem> target, Collections.ISet<TItem> set, IEnumerable<TItem> items)
    {
      if (ReferenceEquals(set, items))
        return target;
      AssertAreComparable(set, items);

      if (set.Count==0)
        return target;
      if (items.IsNullOrEmpty())
        return UnionWith(target, set);

      Collections.ISet<TItem> itemsAsSet;
      if (!(items is Collections.ISet<TItem>)) {
        itemsAsSet = new SetSlim<TItem>(set.Comparer);
        AddIntersectionOf(itemsAsSet, set, items);
      }
      else
        itemsAsSet = items as Collections.ISet<TItem>;

      foreach (TItem item in set)
        if (!itemsAsSet.Contains(item))
          target.Add(item);
      return target;
    }

    /// <summary>
    /// Excepts the specified set of <paramref name="items"/> from the 
    /// specified <paramref name="target"/> in the result set <paramref name="target"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of set items.</typeparam>
    /// <param name="target">The set.</param>
    /// <param name="items">The set of items.</param>
    /// <returns>A set derived from excepting of the specified set of <paramref name="items"/>
    /// from the specified <paramref name="target"/>.
    /// </returns>
    public static Collections.ISet<TItem> ExceptWith<TItem>(this Collections.ISet<TItem> target, IEnumerable<TItem> items)
    {
      if (ReferenceEquals(target, items)) {
        target.Clear();
        return target;
      }
      AssertAreComparable(target, items);

      if (items.IsNullOrEmpty())
        return target;

      var set = items as Collections.ISet<TItem>;
      return set == null ? ExceptWithEnumerable(target, items) : ExceptWithSet(target, set);
    }

    #endregion

    #region SymmetricExcept, SymmetricExceptWith, AddSymmetricExceptionOf

    /// <summary>
    /// Excepts the elements of the specified set of <paramref name="items"/> contained in
    /// the  specified <paramref name="set"/> from <paramref name="set"/> and adds 
    /// the elements of the specified set of <paramref name="items"/> not contained in
    /// the  specified <paramref name="set"/> to <paramref name="set"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of set items.</typeparam>
    /// <typeparam name="TSet">The type of set to return.</typeparam>
    /// <param name="set">The set.</param>
    /// <param name="items">The set of items.</param>
    /// <returns>A set derived by excepting the elements of the specified set of <paramref name="items"/> contained in
    /// the  specified <paramref name="set"/> from <paramref name="set"/> and adding 
    /// the elements of the specified set of <paramref name="items"/> not contained in
    /// the  specified <paramref name="set"/> to <paramref name="set"/>.
    /// </returns>
    public static TSet SymmetricExcept<TItem, TSet>(this Collections.ISet<TItem> set, IEnumerable<TItem> items)
      where TSet: Collections.ISet<TItem>, new()
    {
      return (TSet)SymmetricExceptWith(UnionWith(new TSet(), set), items);
    }

    /// <summary>
    /// Excepts the elements of the specified set of <paramref name="items"/> contained in
    /// the  specified <paramref name="set"/> from <paramref name="set"/> and adds 
    /// the elements of the specified set of <paramref name="items"/> not contained in
    /// the  specified <paramref name="set"/> to <paramref name="set"/> in the result set <paramref name="target"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of set items.</typeparam>
    /// <param name="target">The result set.</param>
    /// <param name="set">The set.</param>
    /// <param name="items">The set of items.</param>
    /// <returns>A set derived by excepting the elements of the specified set of <paramref name="items"/> contained in
    /// the  specified <paramref name="set"/> from <paramref name="set"/> and adding 
    /// the elements of the specified set of <paramref name="items"/> not contained in
    /// the  specified <paramref name="set"/> to <paramref name="set"/>.
    /// </returns>
    public static Collections.ISet<TItem> AddSymmetricExceptionOf<TItem>(this Collections.ISet<TItem> target, Collections.ISet<TItem> set, IEnumerable<TItem> items)
    {
      if (ReferenceEquals(set, items))
        return target;
      AssertAreComparable(set, items);

      if (set.Count==0)
        return UnionWith(target, items);
      if (items.IsNullOrEmpty())
        return UnionWith(target, set);

      UnionWith(target, set);
      foreach (TItem item in items)
        if (!set.Contains(item))
          target.Add(item);
        else
          target.Remove(item);
      return target;
    }

    /// <summary>
    /// Excepts the elements of the specified set of <paramref name="items"/> contained in
    /// the  specified <paramref name="target"/> from <paramref name="target"/> and adds 
    /// the elements of the specified set of <paramref name="items"/> not contained in
    /// the  specified <paramref name="target"/> to <paramref name="target"/> in the result set <paramref name="target"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of set items.</typeparam>
    /// <param name="target">The set.</param>
    /// <param name="items">The set of items.</param>
    /// <returns>A set derived by excepting the elements of the specified set of <paramref name="items"/> contained in
    /// the  specified <paramref name="target"/> from <paramref name="target"/> and adding 
    /// the elements of the specified set of <paramref name="items"/> not contained in
    /// the  specified <paramref name="target"/> to <paramref name="target"/>.
    /// </returns>
    public static Collections.ISet<TItem> SymmetricExceptWith<TItem>(this Collections.ISet<TItem> target, IEnumerable<TItem> items)
    {
      if (ReferenceEquals(target, items)) {
        target.Clear();
        return target;
      }
      AssertAreComparable(target, items);

      foreach (TItem item in items)
        if (target.Contains(item))
          target.Remove(item);
        else
          target.Add(item);
      return target;
    }

    #endregion

    #region Private \ internal methods

    private static void AssertAreComparable<TItem>(Collections.ISet<TItem> set, IEnumerable<TItem> items)
    {
      if (!AreComparable(set, items))
        throw new InvalidOperationException(Strings.ExInconsistentComparisons);
    }

    private static bool AreComparable<TItem>(Collections.ISet<TItem> set, IEnumerable<TItem> items)
    {
      Collections.ISet<TItem> set2 = items as Collections.ISet<TItem>;
      return set2 == null || Equals(set.Comparer, set2.Comparer);
    }

    private static Collections.ISet<TItem> IntersectWithSet<TItem>(Collections.ISet<TItem> target, Collections.ISet<TItem> set)
    {
      if (target.Count*2 > set.Count)
        return IntersectWithEnumerable(target, set);

      foreach (TItem item in target)
        if (!set.Contains(item))
          target.Remove(item);
      return target;
    }

    private static Collections.ISet<TItem> IntersectWithEnumerable<TItem>(Collections.ISet<TItem> target, IEnumerable<TItem> items)
    {
      SetSlim<TItem> tmp = new SetSlim<TItem>(target.Comparer);
      foreach (TItem item in items) {
        if (target.Contains(item))
          tmp.Add(item);
      }
      target.Clear();
      return UnionWith(target, tmp);
    }

    private static Collections.ISet<TItem> ExceptWithSet<TItem>(Collections.ISet<TItem> target, Collections.ISet<TItem> set)
    {
      if (target.Count*2 > set.Count) {
        return ExceptWithEnumerable(target, set);
      }
      foreach (TItem item in target)
        if (set.Contains(item))
          target.Remove(item);
      return target;
    }

    private static Collections.ISet<TItem> ExceptWithEnumerable<TItem>(Collections.ISet<TItem> target, IEnumerable<TItem> items)
    {
      foreach (TItem item in items)
        if (target.Contains(item))
          target.Remove(item);
      return target;
    }

    #endregion
  }
}