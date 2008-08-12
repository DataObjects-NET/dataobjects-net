// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.12

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core.Comparison;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Collections
{
  public abstract partial class DictionaryBaseSlim<TKey, TValue>
  {
    // Nested class file.

    /// <summary>
    /// Base class for keys \ values collections used by 
    /// <see cref="DictionaryBaseSlim{TKey,TValue}"/>.
    /// </summary>
    /// <typeparam name="T">The type of stored item.</typeparam>
    protected abstract class KeyOrValueCollectionBase<T>: 
      ICollection<T>,
      ICountable<T>
    {
      /// <summary>
      /// Inner dictionary.
      /// </summary>
      protected readonly IDictionary<TKey, TValue> dictionary;

      /// <inheritdoc/>
      [DebuggerStepThrough]
      public int Count
      {
        get { return dictionary.Count; }
      }

      /// <inheritdoc/>
      [DebuggerStepThrough]
      long ICountable.Count
      {
        get { return Count; }
      }

      /// <inheritdoc/>
      [DebuggerStepThrough]
      public bool IsReadOnly
      {
        get { return true; }
      }

      /// <inheritdoc/>
      public void CopyTo(T[] array, int arrayIndex)
      {
        this.Copy(array, arrayIndex);
      }

      /// <inheritdoc/>
      public virtual bool Contains(T item)
      {
        foreach (T element in this)
          if (AdvancedComparerStruct<T>.System.Equals(element, item))
            return true;
        return false;
      }

      /// <inheritdoc/>
      public IEnumerator<T> GetEnumerator()
      {
        foreach (KeyValuePair<TKey, TValue> pair in this.dictionary)
          yield return GetItem(pair);
      }

      /// <inheritdoc/>
      protected abstract T GetItem(KeyValuePair<TKey, TValue> pair);

      /// <inheritdoc/>
      public bool Remove(T item)
      {
        throw Exceptions.CollectionIsReadOnly(null);
      }

      /// <inheritdoc/>
      public void Add(T item)
      {
        throw Exceptions.CollectionIsReadOnly(null);
      }

      /// <inheritdoc/>
      public void Clear()
      {
        throw Exceptions.CollectionIsReadOnly(null);
      }

      /// <inheritdoc/>
      [DebuggerStepThrough]
      IEnumerator IEnumerable.GetEnumerator()
      {
        return GetEnumerator();
      }

      
      // Constructors
      
      /// <summary>
      /// <see cref="ClassDocTemplate.Ctor" copy="true" />
      /// </summary>
      /// <param name="dictionary">Inner <see cref="dictionary"/> object.</param>
      protected KeyOrValueCollectionBase(IDictionary<TKey, TValue> dictionary)
      {
        this.dictionary = dictionary;
      }
    }
  }
}