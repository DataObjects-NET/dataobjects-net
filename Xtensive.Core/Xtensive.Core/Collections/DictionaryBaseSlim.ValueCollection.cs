// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.12

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Resources;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Collections
{
  public abstract partial class DictionaryBaseSlim<TKey, TValue>
  {
    // Nested class file.

    /// <summary>
    /// Value collection used by <see cref="DictionaryBaseSlim{TKey,TValue}"/>.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(PREFIX + "DictionaryValueCollectionDebugView`2" + SUFFIX)]
    protected class ValueCollection: KeyOrValueCollectionBase<TValue>
    {
      /// <inheritdoc/>
      protected override TValue GetItem(KeyValuePair<TKey, TValue> pair)
      {
        return pair.Value;
      }

      
      // Constructors
      
      /// <summary>
      /// <see cref="ClassDocTemplate.Ctor" copy="true" />
      /// </summary>
      /// <param name="dictionary">Inner <see cref="KeyOrValueCollectionBase{T}.dictionary"/> object.</param>
      public ValueCollection(IDictionary<TKey, TValue> dictionary)
        : base(dictionary)
      {
      }
    }
  }
}