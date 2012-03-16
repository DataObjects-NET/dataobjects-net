// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.06.23

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Indexing
{
  internal abstract class IndexReaderBase<TIndex,TKey,TItem> : IIndexReader<TKey,TItem>
    where TIndex: IIndex<TKey, TItem>, IHasKeyComparers<TKey>
  {
    private readonly TIndex index;
    private Range<Entire<TKey>> range; // Non-readonly - to avoid stack growth
    private readonly Direction direction;
    
    public TIndex Index
    {
      [DebuggerStepThrough]
      get { return index; }
    }

    public Range<Entire<TKey>> Range
    {
      [DebuggerStepThrough]
      get { return range; }
    }

    public Direction Direction
    {
      [DebuggerStepThrough]
      get { return direction; }
    }

    public abstract TItem Current { get; }

    object IEnumerator.Current
    {
      [DebuggerStepThrough]
      get { return Current; }
    }

    public abstract bool MoveNext();

    public abstract void MoveTo(Entire<TKey> key);

    public abstract void Reset();

    [DebuggerStepThrough]
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public abstract IEnumerator<TItem> GetEnumerator();


    // Constructors


    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="range">The range to read.</param>
    public IndexReaderBase(TIndex index, Range<Entire<TKey>> range)
    {
      this.index = index;
      this.range = range;
      direction  = range.GetDirection(index.EntireKeyComparer);
    }

    // Destructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    /// </summary>
    public abstract void Dispose();
  }
}