// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.04

using System;
using System.Collections;
using System.Collections.Generic;

namespace Xtensive.Core.Collections
{
  /// <summary>
  /// <see cref="IEnumerable"/> related utilities.
  /// </summary>
  public static class EnumerableUtils
  {
    /// <summary>
    /// Flattens the item's hierarchy.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="root">The root of the hierarchy.</param>
    /// <param name="childrenExtractor">The children extractor. It's always executed 
    /// before a root item will be returned.</param>
    /// <param name="exitAction">This action is always executed after a root item 
    /// was returned.</param>
    /// <param name="rootFirst">If set to <see langword="true"/> then a root item 
    /// will be returned before its children.</param>
    /// <returns>The <see cref="IEnumerable{T}"/> containing all items in the 
    /// specified hierarchy.</returns>
    public static IEnumerable<TItem> Flatten<TItem>(IEnumerable<TItem> root,
      Func<TItem, IEnumerable<TItem>> childrenExtractor, Action<TItem> exitAction, bool rootFirst)
    {
      // The validation of arguments is omitted to increase performance.
      foreach (var item in root) {
        var children = childrenExtractor.Invoke(item);
        if(rootFirst)
          yield return item;
        if(children != null)
          foreach (var childItem in Flatten(children, childrenExtractor, exitAction, rootFirst))
            yield return childItem;
        if(!rootFirst)
          yield return item;
        if(exitAction != null)
          exitAction.Invoke(item);
      }
    }

    /// <summary>
    /// Gets the enumerable with one element.
    /// </summary>
    /// <typeparam name="TItem">The type of enumerated item.</typeparam>
    /// <returns>Sequence with value inside.</returns>
    public static IEnumerable<TItem> One<TItem>(TItem value)
    {
      yield return value;
    }

    /// <summary>
    /// Unfolds the whole sequence from its <paramref name="first"/> item.
    /// If <paramref name="first"/> is <see langword="null" />,
    /// an empty sequence is returned.
    /// </summary>
    /// <typeparam name="TItem">The type of enumerated item.</typeparam>
    /// <param name="first">The first item.</param>
    /// <param name="next">The delegate returning the next item by the current one.
    /// The enumeration continues until it returns <see langword="null" />.</param>
    /// <returns>Unfolded sequence of items 
    /// starting from the <paramref name="first"/> one.</returns>
    public static IEnumerable<TItem> Unfold<TItem>(TItem first, Func<TItem, TItem> next)
    {
      ArgumentValidator.EnsureArgumentNotNull(next, "next");
      var current = first;
      while (current!=null) {
        yield return current;
        current = next.Invoke(current);
      }
    }

    /// <summary>
    /// Unfolds the whole sequence from its <paramref name="first"/> item.
    /// </summary>
    /// <typeparam name="TItem">The type of enumerated item.</typeparam>
    /// <param name="first">The first item.</param>
    /// <param name="include">The delegate indicating whether to include the current item
    /// into the sequence or not. Enumeration continues until this method returns
    /// <see langword="false" />.</param>
    /// <param name="next">The delegate returning the next item by the current one.</param>
    /// <returns>
    /// Unfolded sequence of items
    /// starting from the <paramref name="first"/> one.
    /// </returns>
    public static IEnumerable<TItem> Unfold<TItem>(TItem first, Func<TItem, bool> include, Func<TItem, TItem> next)
    {
      ArgumentValidator.EnsureArgumentNotNull(next, "next");
      var current = first;
      while (include.Invoke(current)) {
        yield return current;
        current = next.Invoke(current);
      }
    }
  }
}
