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
    private int flag = 1;
    private bool originFlag = true;
    private bool insertionsFlag = true;
    private bool removalsFlag = true;
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
          originFlag = false;
          reader = insertionsReader;
          flag = 2;
        }
        if (index.Insertions.Count==0) {
          insertionsFlag = false;
          reader = originReader;
          flag = 1;
        }
        if (!originFlag && !insertionsFlag)
          return false;
        if (index.Removals.Count==0)
          removalsFlag = false;
      }

      //---Try to move next.-----
      bool moveNextResult = reader.MoveNext();
      if (moveNextResult && MoveIsPossible())
        return true;
      moveNextResult = false;

      if ((flag==1) && (insertionsFlag)) {
        reader = insertionsReader;
        originFlag = false;
        moveNextResult = MoveIsPossible();
      }
      if ((flag==2) && (originFlag)) {
        reader = originReader;
        insertionsFlag = false;
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
      originFlag = true;
      insertionsFlag = true;
      removalsFlag = true;
      isFirstMove = true;

      IEntire<TKey> point;
      if (index.Insertions.ContainsKey(key.Value)) {
        insertionsReader.MoveTo(key);
        reader = insertionsReader;
        flag = 2;
        if (index.Origin.Count!=0) {
          point = Entire<TKey>.Create(index.KeyExtractor(index.Origin.Seek(new Ray<IEntire<TKey>>(key, reader.Direction)).Result));
          if (IsGreater(point.Value, key.Value) > 0)
            originFlag = false;
          else
            originReader.MoveTo(point);
        }
      }
      if ((index.Origin.ContainsKey(key.Value)) && (!index.Removals.ContainsKey(key.Value))) {
        originReader.MoveTo(key);
        reader = originReader;
        flag = 1;
        if (index.Insertions.Count!=0) {
          point = Entire<TKey>.Create(index.KeyExtractor(index.Insertions.Seek(new Ray<IEntire<TKey>>(key, reader.Direction)).Result));
          if (IsGreater(point.Value, key.Value) > 0)
            insertionsFlag = false;
          else
            insertionsReader.MoveTo(point);
        }
      }
      if (index.Removals.Count!=0) {
        point = Entire<TKey>.Create(index.KeyExtractor(index.Removals.Seek(new Ray<IEntire<TKey>>(key, reader.Direction)).Result));
        if (IsGreater(point.Value, key.Value) > 0)
          removalsFlag = false;
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
        if ((flag==1) && insertionsFlag && !insertionsReader.MoveNext())
          insertionsFlag = false;
        if ((flag==2) && originFlag && !originReader.MoveNext())
          originFlag = false;
        if (removalsFlag && !removalsReader.MoveNext())
          removalsFlag = false;
        isFirstMove = false;
      }
      while (originFlag && removalsFlag && (IsGreater(index.KeyExtractor(originReader.Current), index.KeyExtractor(removalsReader.Current))<=0)) {
        if (IsGreater(index.KeyExtractor(originReader.Current), index.KeyExtractor(removalsReader.Current))==0)
        if (originReader.MoveNext()) {
          reader = originReader;
          flag = 1;
        }
        else {
          originFlag = false;
          if (insertionsFlag) {
            reader = insertionsReader;
            flag = 2;
            break;
          }
        }
        if (!removalsReader.MoveNext()) {
          removalsFlag = false;
          break;
        }
      }
      if ((originFlag && !insertionsFlag) || (originFlag && insertionsFlag && (IsGreater(index.KeyExtractor(originReader.Current), index.KeyExtractor(insertionsReader.Current)) > 0))) {
        flag = 1;
        reader = originReader;
        return true;
      }

      if ((insertionsFlag && !originFlag) || (originFlag && insertionsFlag && (IsGreater(index.KeyExtractor(originReader.Current), index.KeyExtractor(insertionsReader.Current)) < 0))) {
        flag = 2;
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