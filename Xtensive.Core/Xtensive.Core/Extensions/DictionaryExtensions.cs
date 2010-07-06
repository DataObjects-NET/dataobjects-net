// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.07.28

using System.Collections.Generic;

namespace Xtensive.Core
{
  /// <summary>
  /// <see cref="Dictionary{TKey,TValue}"/> related extensions.
  /// </summary>
  public static class DictionaryExtensions
  {
    /// <summary>
    /// Gets the value from the dictionary by its key.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="dictionary">The dictionary.</param>
    /// <param name="key">The key.</param>
    /// <returns>Found value.
    /// <see langword="default(T)" />, if there is no value corresponding to specified key.</returns>
    public static TValue GetValueOrDefault<TKey,TValue>(this IDictionary<TKey,TValue> dictionary, TKey key)
    {
      TValue value;
      if (dictionary.TryGetValue(key, out value))
        return value;
      else
        return default(TValue);
    }

    /// <summary>
    /// Unions the specified <see cref="IDictionary{TKey,TValue}"/> with <paramref name="right"/> enumerable int new <see cref="IDictionary{TKey,TValue}"/>.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="left">The left dictionary.</param>
    /// <param name="right">The right enumerable.</param>
    public static IDictionary<TKey,TValue> Union<TKey,TValue>(this IDictionary<TKey,TValue> left, IEnumerable<KeyValuePair<TKey, TValue>> right)
    {
      var result = new Dictionary<TKey, TValue>(left);
      foreach (var pair in right)
        if (!result.ContainsKey(pair.Key))
          result.Add(pair.Key, pair.Value);
      return result;
    }

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