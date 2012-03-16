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
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Tuples.Transform;

namespace Xtensive.Indexing.Composite
{
  internal struct IndexSegmentReader<TKey, TItem> : IIndexReader<TKey, TItem>
    where TKey : Tuple
    where TItem : Tuple
  {
    private readonly IndexSegment<TKey, TItem> index;
    private Range<Entire<TKey>> range; // Non-readonly - to avoid stack growth
    private readonly IIndexReader<TKey, TItem> reader;

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
      get
      {
        CutOutTransform currentTransform = index.GetCutOutTransform(reader.Current.Descriptor, new Segment<int>(index.KeyExtractor(reader.Current).Count, 1));
        return (TItem) currentTransform.Apply(TupleTransformType.TransformedTuple, reader.Current);
      }
    }

    object IEnumerator.Current
    {
      [DebuggerStepThrough]
      get { return reader.Current; }
    }

    public bool MoveNext()
    {
      // TODO: Optimize this
      while (reader.MoveNext()) {
        if (reader.Current.GetValueOrDefault<int>(index.KeyExtractor(reader.Current).Count)!=index.SegmentNumber)
          continue;
        return true;
      }
      return false;
    }

    public void MoveTo(Entire<TKey> key)
    {
      reader.MoveTo(index.EntireConverter(key));
    }

    public void Reset()
    {
      MoveTo(range.EndPoints.First);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public IEnumerator<TItem> GetEnumerator()
    {
      return new IndexSegmentReader<TKey, TItem>(index, range);
    }


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="range">The range to read.</param>
    public IndexSegmentReader(IndexSegment<TKey, TItem> index, Range<Entire<TKey>> range)
    {
      this.index = index;
      this.range = range;
      reader = index.CompositeIndex.Implementation.CreateReader(index.GetCompositeIndexRange(range));
    }

    // Destructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    /// </summary>
    public void Dispose()
    {
      reader.Dispose();
    }
  }
}