// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.12

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Comparison;

namespace Xtensive.Indexing.Differential
{
  public class DifferentialIndexReader<TKey, TItem, TImpl> : IIndexReader<TKey, TItem>
    where TImpl : IUniqueOrderedIndex<TKey, TItem>, IConfigurable<IndexConfigurationBase<TKey, TItem>>, new()
  {
    private readonly DifferentialIndex<TKey, TItem, TImpl> index;
    private Range<IEntire<TKey>> range;
    private readonly IIndexReader<TKey, TItem> originReader;
    private readonly IIndexReader<TKey, TItem> insertionsReader;
    private readonly IIndexReader<TKey, TItem> removalsReader;
    private IIndexReader<TKey, TItem> reader;
    private DifferentialReaderState readerState = DifferentialReaderState.OriginReader;
    private bool isEndOfOrigin = false;
    private bool isEndOfInsertions = false;
    private bool isEndOfRemovals = false;
    private bool isFirstMove = true;


    /// <summary>
    /// Gets the index.
    /// </summary>
    public IIndex<TKey, TItem> Index
    {
      [DebuggerStepThrough]
      get { return index; }
    }

    /// <summary>
    /// Gets the reader.
    /// </summary>
    public IIndexReader<TKey, TItem> Reader
    {
      [DebuggerStepThrough]
      get { return reader; }
    }

    /// <inheritdoc/>
    public Range<IEntire<TKey>> Range
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
      get { return reader.Current; }
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
      //---Check if Index is Empty.-----
      if (isFirstMove) {
        if (index.Origin.Count==0) {
          isEndOfOrigin = true;
          reader = insertionsReader;
          readerState = DifferentialReaderState.InsertionsReader;
        }
        if (index.Insertions.Count==0) {
          isEndOfInsertions = true;
          reader = originReader;
          readerState = DifferentialReaderState.OriginReader;
        }
        if (isEndOfOrigin && isEndOfInsertions)
          return false;
        if (index.Removals.Count==0)
          isEndOfRemovals = true;
      }

      //---Try to move next.-----
      bool moveNextResult = reader.MoveNext();
      if (moveNextResult && MoveIsPossible())
        return true;
      moveNextResult = false;

      if (readerState==DifferentialReaderState.OriginReader && !isEndOfInsertions) {
        reader = insertionsReader;
        isEndOfOrigin = true;
        moveNextResult = MoveIsPossible();
      }
      if (readerState==DifferentialReaderState.InsertionsReader && !isEndOfOrigin) {
        reader = originReader;
        isEndOfInsertions = true;
        moveNextResult = MoveIsPossible();
      }
      return moveNextResult;
    }

    /// <inheritdoc/>
    public void Reset()
    {
      MoveTo(range.EndPoints.First);
    }

    /// <inheritdoc/>
    public void MoveTo(IEntire<TKey> key)
    {
      isEndOfOrigin = false;
      isEndOfInsertions = false;
      isEndOfRemovals = false;
      isFirstMove = true;

      IEntire<TKey> point;
      if (index.Insertions.ContainsKey(key.Value)) {
        insertionsReader.MoveTo(key);
        reader = insertionsReader;
        readerState = DifferentialReaderState.InsertionsReader;
        if (index.Origin.Count!=0) {
          point = Entire<TKey>.Create(index.KeyExtractor(index.Origin.Seek(new Ray<IEntire<TKey>>(key, reader.Direction)).Result));
          if (IsGreater(point.Value, key.Value) > 0)
            isEndOfOrigin = true;
          else
            originReader.MoveTo(point);
        }
      }
      if (index.Origin.ContainsKey(key.Value) && !index.Removals.ContainsKey(key.Value)) {
        originReader.MoveTo(key);
        reader = originReader;
        readerState = DifferentialReaderState.OriginReader;
        if (index.Insertions.Count!=0) {
          point = Entire<TKey>.Create(index.KeyExtractor(index.Insertions.Seek(new Ray<IEntire<TKey>>(key, reader.Direction)).Result));
          if (IsGreater(point.Value, key.Value) > 0)
            isEndOfInsertions = true;
          else
            insertionsReader.MoveTo(point);
        }
      }
      if (index.Removals.Count!=0) {
        point = Entire<TKey>.Create(index.KeyExtractor(index.Removals.Seek(new Ray<IEntire<TKey>>(key, reader.Direction)).Result));
        if (IsGreater(point.Value, key.Value) > 0)
          isEndOfRemovals = true;
        else
          removalsReader.MoveTo(point);
      }
      isFirstMove = true;
    }

    #region Private / Internal methods.

    private int IsGreater(TKey key1, TKey key2)
    {
      AdvancedComparer<TKey> comparer = AdvancedComparer<TKey>.Default;
      int result = comparer.Compare(key2, key1);
      if (Direction==Direction.Positive)
        return result;
      return -result;
    }

    private bool MoveIsPossible()
    {
      if (isFirstMove) {
        if (readerState==DifferentialReaderState.OriginReader && !isEndOfInsertions && !insertionsReader.MoveNext())
          isEndOfInsertions = true;
        if (readerState==DifferentialReaderState.InsertionsReader && !isEndOfOrigin && !originReader.MoveNext())
          isEndOfOrigin = true;
        if (!isEndOfRemovals && !removalsReader.MoveNext())
          isEndOfRemovals = true;
        isFirstMove = false;
      }
      while (!isEndOfOrigin && !isEndOfRemovals && 
        IsGreater(index.KeyExtractor(originReader.Current), index.KeyExtractor(removalsReader.Current))<=0) {
        if (IsGreater(index.KeyExtractor(originReader.Current), index.KeyExtractor(removalsReader.Current))==0)
        if (originReader.MoveNext()) {
          reader = originReader;
          readerState = DifferentialReaderState.OriginReader;
        }
        else {
          isEndOfOrigin = true;
          if (!isEndOfInsertions) {
            reader = insertionsReader;
            readerState = DifferentialReaderState.InsertionsReader;
            break;
          }
        }
        if (!removalsReader.MoveNext()) {
          isEndOfRemovals = true;
          break;
        }
      }
      if ((!isEndOfOrigin && isEndOfInsertions) || 
        (!isEndOfOrigin && !isEndOfInsertions 
        && IsGreater(index.KeyExtractor(originReader.Current), index.KeyExtractor(insertionsReader.Current)) > 0)) {
        readerState = DifferentialReaderState.OriginReader;
        reader = originReader;
        return true;
      }

      if ((!isEndOfInsertions && isEndOfOrigin) ||
        (!isEndOfOrigin && !isEndOfInsertions && 
        IsGreater(index.KeyExtractor(originReader.Current), index.KeyExtractor(insertionsReader.Current)) < 0)) {
        readerState = DifferentialReaderState.InsertionsReader;
        reader = insertionsReader;
        return true;
      }
      return false;
    }

    #endregion


    //Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="DifferentialIndexReader&lt;TKey, TItem, TImplementation&gt;"/> class.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="range">The range.</param>
    public DifferentialIndexReader(DifferentialIndex<TKey, TItem, TImpl> index, Range<IEntire<TKey>> range)
    {
      this.index = index;
      this.range = range;
      originReader = index.Origin.CreateReader(range);
      reader = originReader;
      insertionsReader = index.Insertions.CreateReader(range);
      removalsReader = index.Removals.CreateReader(range);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
      originReader.Dispose();
      insertionsReader.Dispose();
      removalsReader.Dispose();
      reader.Dispose();
    }
  }
}