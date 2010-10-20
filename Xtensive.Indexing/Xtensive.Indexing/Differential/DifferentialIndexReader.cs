// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.12

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Comparison;
using Xtensive.Core;
using Xtensive.Disposing;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Indexing.Differential
{
  /// <summary>
  /// Differential index reader.
  /// </summary>
  /// <typeparam name="TKey">The type of the key.</typeparam>
  /// <typeparam name="TItem">The type of the item.</typeparam>
  /// <typeparam name="TImpl">The type of the impl.</typeparam>
  public struct DifferentialIndexReader<TKey, TItem, TImpl> : IIndexReader<TKey, TItem>
    where TImpl : IUniqueOrderedIndex<TKey, TItem>, IConfigurable<IndexConfigurationBase<TKey, TItem>>, new()
  {
    private readonly DifferentialIndex<TKey, TItem, TImpl> index;
    private Range<Entire<TKey>> range;
    private IIndexReader<TKey, TItem> originReader;
    private IIndexReader<TKey, TItem> insertionsReader;
    private IIndexReader<TKey, TItem> removalsReader;
    private IIndexReader<TKey, TItem> currentReader;
    private DifferentialReaderEndMark readerEndMark;
    private DifferentialReaderState readerState;
    private bool atTheBeginning;


    /// <summary>
    /// Gets the index.
    /// </summary>
    public IIndex<TKey, TItem> Index
    {
      [DebuggerStepThrough]
      get { return index; }
    }

    /// <inheritdoc/>
    public Range<Entire<TKey>> Range
    {
      [DebuggerStepThrough]
      get { return range; }
    }

    /// <inheritdoc/>
    public Direction Direction
    {
      [DebuggerStepThrough]
      get { return originReader.Direction; }
    }

    /// <inheritdoc/>
    public TItem Current
    {
      [DebuggerStepThrough]
      get { return currentReader.Current; }
    }

    /// <inheritdoc/>
    object IEnumerator.Current
    {
      [DebuggerStepThrough]
      get { return Current; }
    }


    /// <inheritdoc/>
    public IEnumerator<TItem> GetEnumerator()
    {
      return new DifferentialIndexReader<TKey, TItem, TImpl>(index, range);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <inheritdoc/>
    public bool MoveNext()
    {
      if (IndexIsEmpty())
        return false;

      // Try to move next.
      bool moveNextResult = currentReader.MoveNext();
      if (moveNextResult && TryMoveNextCurrentReader())
        return true;
      moveNextResult = false;

      if (readerState==DifferentialReaderState.ReadingOrigin && !EndOfInsertionsReached) {
        currentReader = insertionsReader;
        EndOfOriginReached = true;
        moveNextResult = TryMoveNextCurrentReader();
      }
      if (readerState==DifferentialReaderState.ReadingInsertions && !EndOfOriginReached) {
        currentReader = originReader;
        EndOfInsertionsReached = true;
        moveNextResult = TryMoveNextCurrentReader();
      }
      return moveNextResult;
    }

    private bool IndexIsEmpty()
    {
      if (atTheBeginning) {
        if (index.Origin.Count==0) {
          EndOfOriginReached = true;
          currentReader = insertionsReader;
          readerState = DifferentialReaderState.ReadingInsertions;
        }
        if (index.Insertions.Count==0) {
          EndOfInsertionsReached = true;
          currentReader = originReader;
          readerState = DifferentialReaderState.ReadingOrigin;
        }
        if (EndOfOriginReached && EndOfInsertionsReached)
          return true;
        if (index.Removals.Count==0)
          EndOfRemovalsReached = true;
      }
      return false;
    }

    /// <inheritdoc/>
    public void Reset()
    {
      MoveTo(range.EndPoints.First);
    }

    /// <inheritdoc/>
    public void MoveTo(Entire<TKey> key)
    {
      EndOfOriginReached = false;
      EndOfInsertionsReached = false;
      EndOfRemovalsReached = false;
      atTheBeginning = true;

      Entire<TKey> point;
      if (index.Insertions.ContainsKey(key.Value)) {
        insertionsReader.MoveTo(key);
        currentReader = insertionsReader;
        readerState = DifferentialReaderState.ReadingInsertions;
        if (index.Origin.Count!=0) {
          point = new Entire<TKey>(index.KeyExtractor(index.Origin.Seek(new Ray<Entire<TKey>>(key, currentReader.Direction)).Result));
          if (Compare(point.Value, key.Value) > 0)
            EndOfOriginReached = true;
          else
            originReader.MoveTo(point);
        }
      }
      if (index.Origin.ContainsKey(key.Value) && !index.Removals.ContainsKey(key.Value)) {
        originReader.MoveTo(key);
        currentReader = originReader;
        readerState = DifferentialReaderState.ReadingOrigin;
        if (index.Insertions.Count!=0) {
          point = new Entire<TKey>(index.KeyExtractor(index.Insertions.Seek(new Ray<Entire<TKey>>(key, currentReader.Direction)).Result));
          if (Compare(point.Value, key.Value) > 0)
            EndOfInsertionsReached = true;
          else
            insertionsReader.MoveTo(point);
        }
      }
      if (index.Removals.Count!=0) {
        point = new Entire<TKey>(index.KeyExtractor(index.Removals.Seek(new Ray<Entire<TKey>>(key, currentReader.Direction)).Result));
        if (Compare(point.Value, key.Value) > 0)
          EndOfRemovalsReached = true;
        else
          removalsReader.MoveTo(point);
      }
      atTheBeginning = true;
    }

    #region Private / Internal methods.

    private bool EndOfOriginReached {
      get { return (readerEndMark & DifferentialReaderEndMark.EndOfOriginReached)!=0; }
      set { readerEndMark = 
        (readerEndMark & ~DifferentialReaderEndMark.EndOfOriginReached) | 
        (value ? DifferentialReaderEndMark.EndOfOriginReached : 0); }
    }

    private bool EndOfInsertionsReached {
      get { return (readerEndMark & DifferentialReaderEndMark.EndOfInsertionsReached)!=0; }
      set { readerEndMark = 
        (readerEndMark & ~DifferentialReaderEndMark.EndOfInsertionsReached) | 
        (value ? DifferentialReaderEndMark.EndOfInsertionsReached : 0); }
    }

    private bool EndOfRemovalsReached {
      get { return (readerEndMark & DifferentialReaderEndMark.EndOfRemovalsReached)!=0; }
      set { readerEndMark = 
        (readerEndMark & ~DifferentialReaderEndMark.EndOfRemovalsReached) | 
        (value ? DifferentialReaderEndMark.EndOfRemovalsReached : 0); }
    }

    private int Compare(TKey left, TKey right)
    {
      AdvancedComparer<TKey> comparer = AdvancedComparer<TKey>.Default;
      int result = comparer.Compare(right, left);
      if (Direction==Direction.Positive)
        return result;
      return -result;
    }

    private bool TryMoveNextCurrentReader()
    {
      if (atTheBeginning) {
        if (readerState==DifferentialReaderState.ReadingOrigin && !EndOfInsertionsReached && !insertionsReader.MoveNext())
          EndOfInsertionsReached = true;
        if (readerState==DifferentialReaderState.ReadingInsertions && !EndOfOriginReached && !originReader.MoveNext())
          EndOfOriginReached = true;
        if (!EndOfRemovalsReached && !removalsReader.MoveNext())
          EndOfRemovalsReached = true;
        atTheBeginning = false;
      }
      while (!EndOfOriginReached && !EndOfRemovalsReached && 
        Compare(index.KeyExtractor(originReader.Current), index.KeyExtractor(removalsReader.Current))<=0) {
        if (Compare(index.KeyExtractor(originReader.Current), index.KeyExtractor(removalsReader.Current))==0)
        if (originReader.MoveNext()) {
          currentReader = originReader;
          readerState = DifferentialReaderState.ReadingOrigin;
        }
        else {
          EndOfOriginReached = true;
          if (!EndOfInsertionsReached) {
            currentReader = insertionsReader;
            readerState = DifferentialReaderState.ReadingInsertions;
            break;
          }
        }
        if (!removalsReader.MoveNext()) {
          EndOfRemovalsReached = true;
          break;
        }
      }
      if ((!EndOfOriginReached && EndOfInsertionsReached) || 
        (!EndOfOriginReached && !EndOfInsertionsReached 
        && Compare(index.KeyExtractor(originReader.Current), index.KeyExtractor(insertionsReader.Current)) > 0)) {
        readerState = DifferentialReaderState.ReadingOrigin;
        currentReader = originReader;
        return true;
      }

      if ((!EndOfInsertionsReached && EndOfOriginReached) ||
        (!EndOfOriginReached && !EndOfInsertionsReached && 
        Compare(index.KeyExtractor(originReader.Current), index.KeyExtractor(insertionsReader.Current)) < 0)) {
          readerState = DifferentialReaderState.ReadingInsertions;
        currentReader = insertionsReader;
        return true;
      }
      return false;
    }

    #endregion


    // Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="DifferentialIndexReader&lt;TKey, TItem, TImplementation&gt;"/> class.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="range">The range.</param>
    public DifferentialIndexReader(DifferentialIndex<TKey, TItem, TImpl> index, Range<Entire<TKey>> range)
    {
      this.index = index;
      this.range = range;
      atTheBeginning = true;
      readerEndMark = 0;
      readerState = DifferentialReaderState.ReadingOrigin;
      originReader = index.Origin.CreateReader(range);
      currentReader = originReader;
      insertionsReader = index.Insertions.CreateReader(range);
      removalsReader = index.Removals.CreateReader(range);
    }

    // Destructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    /// </summary>
    public void Dispose()
    {
      try {
        originReader.DisposeSafely();
        originReader = null;
      }
      finally {
        try {
          insertionsReader.DisposeSafely();
          insertionsReader = null;
        }
        finally {
          removalsReader.DisposeSafely();
          removalsReader = null;
        }
      }
    }
  }
}