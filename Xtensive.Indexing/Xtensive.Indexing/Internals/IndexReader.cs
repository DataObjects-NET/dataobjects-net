// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.02.29

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Indexing.Implementation;
using Xtensive.Indexing.Resources;

namespace Xtensive.Indexing
{
  internal sealed class IndexReader<TKey,TItem> : IndexReaderBase<Index<TKey,TItem>, TKey, TItem>
  {
    private HasVersion<SeekResultPointer<IndexPointer<TKey, TItem>>, int?> nextPtr;
    private HasVersion<SeekResultPointer<IndexPointer<TKey, TItem>>, int?> lastPtr;
    private TItem current;
    private TKey lastKey;
    private EnumerationState state;
    
    public override TItem Current {
      get {
        switch (state) {
        case EnumerationState.NotStarted:
          throw new InvalidOperationException(Strings.ExEnumerationIsNotStarted);
        case EnumerationState.Finished:
          throw new InvalidOperationException(Strings.ExEnumerationIsAlreadyFinished);
        }
        return current;
      }
    }

    public override bool MoveNext()
    {
      if (state==EnumerationState.Finished)
        return false;
      if (state==EnumerationState.Finishing) {
        state = EnumerationState.Finished;
        return false;
      }

      bool nextPtrIsNotActual = !GetIsActual(ref nextPtr);
      bool lastPtrIsNotActual = !GetIsActual(ref lastPtr);
      if (nextPtrIsNotActual || lastPtrIsNotActual) {
        // Actualizing both currentPtr and lastPtr
        if (nextPtrIsNotActual) {
          nextPtr.Value = Index.InternalSeek(Index.RootPage, lastKey);
          if (state == EnumerationState.Started
            && nextPtr.Value.ResultType == SeekResultType.Exact
            && !nextPtr.Value.Pointer.MoveNext(Direction)) {
            // We' moved nextPtr forward - and there are no more items in Index
            state = EnumerationState.Finishing;
            return true;
          }
          nextPtr.Version = nextPtr.Value.Pointer.Page.Version;
        }
        if (lastPtrIsNotActual) {
          lastPtr.Value = Index.InternalSeek(Index.RootPage, new Ray<Entire<TKey>>(Range.EndPoints.Second, Direction.Invert()));
          lastPtr.Version = lastPtr.Value.Pointer.Page.Version;
        }
        return MoveNext();
      }
      
      // nextPtr is actual, but probably points on value that is out of Range
      if (nextPtr.Value.ResultType == SeekResultType.None) {
        // nextPtr is out of the Index
        state = EnumerationState.Finished;
        return false;
      }

      var indexKey = new Entire<TKey>(Index.KeyExtractor(nextPtr.Value.Pointer.Current));
      if (!Range.Contains(indexKey) && MoveNextIsPossible(indexKey))
        while (Index.AsymmetricKeyCompare(indexKey, Range.EndPoints.First.Value) != 0) {
          nextPtr.Value.Pointer.MoveNext(Direction);
          indexKey = new Entire<TKey>(Index.KeyExtractor(nextPtr.Value.Pointer.Current));
        }

      // nextPtr is in Range; let's update current
      current = nextPtr.Value.Pointer.Current;
      lastKey = Index.KeyExtractor(current);

      if (nextPtr.Value.Pointer.Equals(lastPtr.Value.Pointer)) {
        // We've just set current to the last item in Range  
        state = EnumerationState.Finishing;
        return true;
      }
      if (!nextPtr.Value.Pointer.MoveNext(Direction)) {
        // !!! It seems this check isn't necessary, since above check should always ensure the same
        // We' moved nextPtr forward - and there are no more items in Index
        state = EnumerationState.Finishing;
        return true;
      }
      // Let's ensure pointers will be compared to check Range border conditions 
      // on the next MoveNext() call, rather then Entire<TKey>
      nextPtr.Value.ResultType = SeekResultType.Exact;
      // Version could change, since the Page could change on moving
      nextPtr.Version = nextPtr.Value.Pointer.Page.Version;
      state = EnumerationState.Started;
      return true;
    }

    public override void MoveTo(Entire<TKey> key)
    {
      current = default(TItem); // No Current yet
      nextPtr.Value = Index.InternalSeek(Index.RootPage, new Ray<Entire<TKey>>(key, Direction));
      nextPtr.Version = nextPtr.Value.Pointer.Page.Version;
      lastPtr.Value = Index.InternalSeek(Index.RootPage, new Ray<Entire<TKey>>(Range.EndPoints.Second, Direction.Invert()));
      lastPtr.Version = lastPtr.Value.Pointer.Page.Version;
      if (nextPtr.Value.ResultType != SeekResultType.None) {
        lastKey = Index.KeyExtractor(nextPtr.Value.Pointer.Current);
        state = EnumerationState.NotStarted;
        if (Direction==Direction.Positive && nextPtr.Value.Pointer.CompareTo(lastPtr.Value.Pointer) > 0)
          state = EnumerationState.Finished;
        if (Direction == Direction.Negative && nextPtr.Value.Pointer.CompareTo(lastPtr.Value.Pointer) < 0)
          state = EnumerationState.Finished;
      }
      else {
        lastKey = default(TKey);
        state = EnumerationState.Finished;
      }
    }

    public override void Reset()
    {
      MoveTo(Range.EndPoints.First);
    }

    public override IEnumerator<TItem> GetEnumerator()
    {
      return new IndexReader<TKey, TItem>(Index, Range);
    }

    #region Private \ internal methods

    private static bool GetIsActual(ref HasVersion<SeekResultPointer<IndexPointer<TKey,TItem>>,int?> pointer)
    {
      if (!pointer.Version.HasValue)
        return false;
      return pointer.Version==pointer.Value.Pointer.Page.Version;
    }

    private bool MoveNextIsPossible(Entire<TKey> key)
    {
      return (Direction == Direction.Positive && Index.AsymmetricKeyCompare(key, Range.EndPoints.First.Value) < 0) ||
             (Direction == Direction.Negative && Index.AsymmetricKeyCompare(key, Range.EndPoints.First.Value) > 0);
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="range">The range to read.</param>
    public IndexReader(Index<TKey, TItem> index, Range<Entire<TKey>> range)
      : base(index, range)
    {
      Reset();
    }

    // Destructors

    /// <inheritdoc/>
    public override void Dispose()
    {
    }
  }
}