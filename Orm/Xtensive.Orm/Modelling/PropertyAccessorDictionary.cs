// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.01

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Collections;

using System.Linq;

namespace Xtensive.Modelling
{
  /// <summary>
  /// Read-only <see cref="PropertyAccessor"/> dictionary.
  /// Items returned by its enumerator are ordered by item <see cref="PropertyAccessor.Priority"/>.
  /// </summary>
  [Serializable]
  public sealed class PropertyAccessorDictionary : ReadOnlyDictionary<string, PropertyAccessor>,
    IEnumerable<KeyValuePair<string, PropertyAccessor>>,
    IEnumerable<PropertyAccessor>
  {
    private KeyValuePair<string, PropertyAccessor>[] sequence;

    private void Initialize(IEnumerable<KeyValuePair<string, PropertyAccessor>> dictionary)
    {
      sequence = dictionary.OrderBy(p => p.Value.Priority).ToArray();
    }

    #region IEnumerable methods

    /// <inheritdoc/>
    public new IEnumerator<KeyValuePair<string, PropertyAccessor>> GetEnumerator()
    {
      foreach (var pair in sequence)
        yield return pair;
    }

    /// <inheritdoc/>
    IEnumerator<PropertyAccessor> IEnumerable<PropertyAccessor>.GetEnumerator()
    {
      foreach (var pair in sequence)
        yield return pair.Value;
    }

    #endregion

    
    // Constructors

    /// <inheritdoc/>
    public PropertyAccessorDictionary(IDictionary<string, PropertyAccessor> dictionary)
      : base(dictionary)
    {
      Initialize(dictionary);
    }

    /// <inheritdoc/>
    public PropertyAccessorDictionary(IDictionary<string, PropertyAccessor> dictionary, bool copy)
      : base(dictionary, copy)
    {
      Initialize(dictionary);
    }
  }
}