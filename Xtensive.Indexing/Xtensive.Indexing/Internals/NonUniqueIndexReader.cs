// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.02.15

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Indexing
{
  internal struct NonUniqueIndexReader<TKey,TUniqueKey,TItem> : IIndexReader<TKey,TItem>
  {
    private readonly NonUniqueIndex<TKey,TUniqueKey,TItem> index;
    private Range<Entire<TKey>> range; // Non-readonly - to avoid stack growth
    private readonly IIndexReader<TUniqueKey, TItem> reader;

    public IIndex<TKey, TItem> Index
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
      get { return reader.Direction; }
    }

    public TItem Current
    {
      [DebuggerStepThrough]
      get { return reader.Current; }
    }

    object IEnumerator.Current
    {
      [DebuggerStepThrough]
      get { return reader.Current; }
    }

    public bool MoveNext()
    {
      return reader.MoveNext();
    }

    public void MoveTo(Entire<TKey> key)
    {
      reader.MoveTo(index.EntireConverter(key));
    }

    public void Reset()
    {
      reader.Reset();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public IEnumerator<TItem> GetEnumerator()
    {
      return new NonUniqueIndexReader<TKey, TUniqueKey, TItem>(index, range);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="range">The range to read.</param>
    public NonUniqueIndexReader(NonUniqueIndex<TKey,TUniqueKey,TItem> index, Range<Entire<TKey>> range)
    {
      this.index = index;
      this.range = range;
      reader = index.UniqueIndex.CreateReader(index.GetUniqueIndexRange(range));
    }

    // Destructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    /// </summary>
    public void Dispose()
    {
      reader.Dispose();
    }
  }
}