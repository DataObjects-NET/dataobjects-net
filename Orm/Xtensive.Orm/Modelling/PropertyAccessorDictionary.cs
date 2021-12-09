// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.01

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Xtensive.Modelling
{
  /// <summary>
  /// Read-only <see cref="PropertyAccessor"/> dictionary.
  /// Items returned by its enumerator are ordered by item <see cref="PropertyAccessor.Priority"/>.
  /// </summary>
  [Serializable]
  public sealed class PropertyAccessorDictionary : IReadOnlyDictionary<string, PropertyAccessor>
  {
    private readonly IReadOnlyDictionary<string, PropertyAccessor> items;
    private readonly List<KeyValuePair<string, PropertyAccessor>> sequence;

    /// <inheritdoc/>
    public IEnumerable<string> Keys => items.Keys;

    /// <inheritdoc/>
    public IEnumerable<PropertyAccessor> Values => items.Values;

    /// <inheritdoc/>
    public int Count => items.Count;

    /// <inheritdoc/>
    public PropertyAccessor this[string key] => items[key];

    /// <inheritdoc/>
    public bool ContainsKey(string key) => items.ContainsKey(key);

    /// <inheritdoc/>
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out PropertyAccessor value) => items.TryGetValue(key, out value);

    #region IEnumerable methods

    /// <inheritdoc/>
    public List<KeyValuePair<string, PropertyAccessor>>.Enumerator GetEnumerator() => sequence.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator<KeyValuePair<string, PropertyAccessor>> IEnumerable<KeyValuePair<string, PropertyAccessor>>.GetEnumerator() => GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion


    // Constructors

    /// <inheritdoc/>
    public PropertyAccessorDictionary(IReadOnlyDictionary<string, PropertyAccessor> items)
    {
      this.items = items;
      sequence = items.OrderBy(p => p.Value.Priority).ToList();
    }
  }
}