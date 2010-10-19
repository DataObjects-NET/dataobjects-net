// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.12

using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Collections
{
  public abstract partial class DictionaryBaseSlim<TKey, TValue>
  {
    // Nested class file.

    /// <summary>
    /// Key collection used by <see cref="DictionaryBaseSlim{TKey,TValue}"/>.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(PREFIX + "DictionaryKeyCollectionDebugView`2" + SUFFIX)]
    protected class KeyCollection: KeyOrValueCollectionBase<TKey>
    {
      /// <inheritdoc/>
      protected override TKey GetItem(KeyValuePair<TKey, TValue> pair)
      {
        return pair.Key;
      }

      /// <inheritdoc/>
      public override bool Contains(TKey item)
      {
        return dictionary.ContainsKey(item);
      }

      
      // Constructors
      
      /// <summary>
      /// <see cref="ClassDocTemplate.Ctor" copy="true" />
      /// </summary>
      /// <param name="dictionary">Inner <see cref="KeyOrValueCollectionBase{T}.dictionary"/> object.</param>
      public KeyCollection(IDictionary<TKey, TValue> dictionary)
        : base(dictionary)
      {
      }
    }
  }
}