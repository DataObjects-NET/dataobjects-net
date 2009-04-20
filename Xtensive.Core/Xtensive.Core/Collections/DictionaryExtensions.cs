// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.07.28

using System.Collections.Generic;

namespace Xtensive.Core.Collections
{
  /// <summary>
  /// <see cref="Dictionary{TKey,TValue}"/> related extensions.
  /// </summary>
  public static class DictionaryExtensions
  {
    /// <summary>
    /// Determines whether this <see cref="Dictionary{TKey,TValue}"/> equals to another, i.e. contains the same keys and corresponding values.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in dictionary.</typeparam>
    /// <typeparam name="TValue">The type of values in dictionary.</typeparam>
    /// <param name="dictionary">This dictionary.</param>
    /// <param name="other">The dictionary to compare with.</param>
    /// <returns>
    ///   <see langword="true"/> if this dictionary equals to the specified dictionary; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool EqualsTo<TKey, TValue>(
      this IDictionary<TKey, TValue> dictionary, 
      IDictionary<TKey, TValue> other)
    {
      if (dictionary.Count != other.Count)
        return false;

      foreach (TKey key in dictionary.Keys)
        if (!other.ContainsKey(key) || !other[key].Equals(dictionary[key]))
          return false;

      return true;
    }
  }
}