// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.02.14

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Indexing.Resources;

namespace Xtensive.Indexing
{
  internal class SortedListIndexReader<TKey,TItem> : IndexReaderBase<SortedListIndex<TKey,TItem>, TKey, TItem>
  {
    private HasVersion<SeekResultPointer<SortedListIndexPointer<TKey, TItem>>, int?> nextPtr;
    private HasVersion<SeekResultPointer<SortedListIndexPointer<TKey, TItem>>, int?> lastPtr;
    private TItem current;
    private Entire<TKey>? currentKey;
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
      switch (state) {
      case EnumerationState.Finished:
        throw new InvalidOperationException(Strings.ExEnumerationIsAlreadyFinished);
      case EnumerationState.Finishing:
        state = EnumerationState.Finished;
        return false;
      }

      bool nextPtrIsNotActual = !GetIsActual(ref nextPtr);
      bool lastPtrIsNotActual = !GetIsActual(ref lastPtr);
      if (nextPtrIsNotActual || lastPtrIsNotActual) {
        // Actualizing both currentPtr and lastPtr
        var key = GetCurrentKey();
        if (nextPtrIsNotActual)
          Seek(ref nextPtr, new Ray<Entire<TKey>>(key, Direction));
        if (lastPtrIsNotActual)
          Seek(ref lastPtr, new Ray<Entire<TKey>>(Range.EndPoints.Second, Direction.Invert()));
        if (Range.Contains(key, Index.EntireKeyComparer) || MoveNextIsPossible(key))
          state = EnumerationState.NotStarted;
        else
          state = EnumerationState.Finishing;
        return MoveNext();
      }
      
      // nextPtr is actual, but probably points on value that is out of Range
      switch (nextPtr.Value.ResultType) {
      case SeekResultType.None:
        // nextPtr is out of the Index
        state = EnumerationState.Finished;
        return false;
      case SeekResultType.Nearest:
        // nextPtr is in the Index, but probably out of Range
        var foundKey = new Entire<TKey>(Index.KeyExtractor(nextPtr.Value.Pointer.Current));
        if (!Range.Contains(foundKey, Index.EntireKeyComparer) && !MoveNextIsPossible(foundKey)) {
          state = EnumerationState.Finished;
          return false;
        }
        break;
      }

      var indexKey = new Entire<TKey>(Index.KeyExtractor(nextPtr.Value.Pointer.Current));
      if (!Range.Contains(indexKey) && MoveNextIsPossible(indexKey))
        while (Index.AsymmetricKeyCompare(indexKey, Range.EndPoints.First.Value) != 0) {
          nextPtr.Value.Pointer.MoveNext(Direction);
          indexKey = new Entire<TKey>(Index.KeyExtractor(nextPtr.Value.Pointer.Current));
        }

      // nextPtr is in Range; let's update current
      current = nextPtr.Value.Pointer.Current;
      currentKey = null; // This ensures GetCurrentKey() will return Current

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
      // on the next MoveNext() call, rather then IEntire<TKey>
      nextPtr.Value.ResultType = SeekResultType.Exact; 
      // Version couldn't change, since there are no Page changes
      // nextPtr.Version = nextPtr.Value.Pointer.Owner.Version;
      state = EnumerationState.Started;
      return true;
    }

    public override void MoveTo(Entire<TKey> key)
    {
      current = default(TItem); // No Current yet
      currentKey = key; // But GetCurrentKey() should return this
      nextPtr.Version = null; // Let's de-actualize it
      state = EnumerationState.NotStarted;
    }

    public override void Reset()
    {
      MoveTo(Range.EndPoints.First);
    }

    public override IEnumerator<TItem> GetEnumerator()
    {
      return new SortedListIndexReader<TKey, TItem>(Index, Range);
    }

    #region Private \ internal methods

    private Entire<TKey> GetCurrentKey()
    {
      if (currentKey.HasValue)
        return currentKey.Value;
      return new Entire<TKey>(Index.KeyExtractor(current));
    }

    private void Seek(ref HasVersion<SeekResultPointer<SortedListIndexPointer<TKey,TItem>>,int?> pointer, Ray<Entire<TKey>> newPosition)
    {
      pointer.Value   = Index.InternalSeek(newPosition);
      pointer.Version = pointer.Value.Pointer.Owner.Version;
    }

    private static bool GetIsActual(ref HasVersion<SeekResultPointer<SortedListIndexPointer<TKey,TItem>>,int?> pointer)
    {
      if (!pointer.Version.HasValue)
        return false;
      return pointer.Version==pointer.Value.Pointer.Owner.Version;
    }

    private bool MoveNextIsPossible(Entire<TKey> key)
    {
      return (Direction==Direction.Positive && Index.AsymmetricKeyCompare(key, Range.EndPoints.First.Value) < 0) ||
             (Direction==Direction.Negative && Index.AsymmetricKeyCompare(key, Range.EndPoints.First.Value) > 0);
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="range">The range to read.</param>
    public SortedListIndexReader(SortedListIndex<TKey, TItem> index, Range<Entire<TKey>> range)
      : base(index, range)
    {
      Reset();
    }

    // Destructor

    /// <summary>
    /// <see cref="ClassDocTemplate.Dispose" copy="true"/>
    /// </summary>
    public override void Dispose()
    {
    }
  }
}