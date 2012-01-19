// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.03.24

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Indexing
{

  /// <summary>
  /// Reader for <see cref="RangeSet{T}"/>
  /// </summary>
  [Serializable]
  public sealed class RangeSetReader<TKey, TItem> : IIndexReader<TKey, TItem>
  {
    private readonly List<IIndexReader<TKey, TItem>> readers = new List<IIndexReader<TKey, TItem>>();
    private readonly Direction direction;
    private TItem current;
    private int currentReaderIndex;

    /// <inheritdoc/>
    public IEnumerator<TItem> GetEnumerator()
    {
      return new RangeSetReader<TKey, TItem>(readers);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <inheritdoc/>
    public bool MoveNext()
    {
      while (currentReaderIndex != readers.Count()) {
        if (readers[currentReaderIndex].MoveNext()) {
          current = readers[currentReaderIndex].Current;
          return true;
        }
        currentReaderIndex++;
      }
      return false;
    }

    /// <inheritdoc/>
    public void Reset()
    {
      currentReaderIndex = 0;
      foreach (var reader in readers)
        reader.Reset();
      current = default(TItem);
    }

    /// <inheritdoc/>
    public TItem Current
    {
      get { return current; }
    }

    object IEnumerator.Current
    {
      get { return Current; }
    }

    #region IIndexReader methods

    /// <inheritdoc/>
    public Range<Entire<TKey>> Range
    {
      get { throw new NotSupportedException(); }
    }

    /// <inheritdoc/>
    public Direction Direction
    {
      get { return direction;}
    }

    /// <inheritdoc/>
    public void MoveTo(Entire<TKey> key)
    {
      foreach (var reader in readers) 
        reader.MoveTo(key);
      currentReaderIndex = 0;
      current = default(TItem);  
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="readersList">The list of readers.</param>
    public RangeSetReader(IEnumerable<IIndexReader<TKey, TItem>> readersList)
    {
      if (readersList.Count() == 0)
        throw new InvalidOperationException();

      foreach (var reader in readersList) 
        readers.Add(reader);
      for(int i = 0; i < readers.Count()-1; i++) {
        if (readers[i].Direction != readers[i + 1].Direction)
          throw new InvalidOperationException();
      }
      direction = readers[0].Direction;
    }


    // Destructors

    /// <inheritdoc/>
    public void Dispose()
    {
    }
  }
}