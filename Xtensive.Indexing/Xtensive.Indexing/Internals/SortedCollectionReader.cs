// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.02.26

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Indexing.Resources;

namespace Xtensive.Indexing
{
  internal class SortedCollectionReader<TItem> : IndexReaderBase<SortedCollection<TItem>, TItem, TItem>
  {
    private HasVersion<SeekResultPointer<SortedCollectionPointer<TItem>>, int?> nextPtr;
    private HasVersion<SeekResultPointer<SortedCollectionPointer<TItem>>, int?> lastPtr;
    private TItem current;
    private IEntire<TItem> currentKey;
    private EnumerationState state;
    
    public override TItem Current
    {
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
          Seek(ref nextPtr, new Ray<IEntire<TItem>>(key, Direction));
        if (lastPtrIsNotActual)
          Seek(ref lastPtr, new Ray<IEntire<TItem>>(Range.EndPoints.Second, Direction.Invert()));
        if (Range.Contains(key, Index.EntireKeyComparer))
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
        var foundKey = Entire<TItem>.Create(Index.KeyExtractor(nextPtr.Value.Pointer.Current));
        if (!Range.Contains(foundKey, Index.EntireKeyComparer)) {
          state = EnumerationState.Finished;
          return false;
        }
        else
          break;
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
      // on the next MoveNext() call, rather then IEntire<TItem>
      nextPtr.Value.ResultType = SeekResultType.Exact; 
      // Version couldn't change, since there are no Page changes
      // nextPtr.Version = nextPtr.Value.Pointer.Owner.Version;
      state = EnumerationState.Started;
      return true;
    }

    public override void MoveTo(IEntire<TItem> key)
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
      return new SortedCollectionReader<TItem>(Index, Range);
    }

    #region Private \ internal methods

    private IEntire<TItem> GetCurrentKey()
    {
      if (currentKey!=null)
        return currentKey;
      else
        return Entire<TItem>.Create(Index.KeyExtractor(current));
    }

    private void Seek(ref HasVersion<SeekResultPointer<SortedCollectionPointer<TItem>>,int?> pointer, Ray<IEntire<TItem>> newPosition)
    {
      pointer.Value   = Index.InternalSeek(newPosition);
      pointer.Version = pointer.Value.Pointer.Owner.Version;
    }

    private static bool GetIsActual(ref HasVersion<SeekResultPointer<SortedCollectionPointer<TItem>>,int?> pointer)
    {
      if (!pointer.Version.HasValue)
        return false;
      return pointer.Version==pointer.Value.Pointer.Owner.Version;
    }

    #endregion


    // Constructors

    public SortedCollectionReader(SortedCollection<TItem> index, Range<IEntire<TItem>> range)
      : base(index, range)
    {
      Reset();
    }

    public override void Dispose()
    {
    }
  }
}