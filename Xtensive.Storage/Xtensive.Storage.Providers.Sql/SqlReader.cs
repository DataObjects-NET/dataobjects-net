// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.06.30

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Comparison;
using Xtensive.Core.Tuples;
using Xtensive.Indexing;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Providers.Sql.Resources;

namespace Xtensive.Storage.Providers.Sql
{
  public class SqlReader : IIndexReader<Tuple, Tuple>
  {
    private readonly Range<IEntire<Tuple>> range;
    private long indexVersion;
    private SqlReaderState state;
    private static Direction direction;
    private readonly SqlRecordsetProvider provider;
    private readonly Deque<Tuple> prefetch = new Deque<Tuple>();
    private Tuple current;

    /// <inheritdoc/>
    public IEnumerator<Tuple> GetEnumerator()
    {
      return new SqlReader(range, provider);
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <inheritdoc/>
    public bool MoveNext()
    {
      CheckState();
      throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public void Reset()
    {
      UpdateVersion();
      current = null;
      prefetch.Clear();
      state = SqlReaderState.NotStarted;
    }

    /// <inheritdoc/>
    public Tuple Current
    {
      get
      {
        CheckState();
        if (state==SqlReaderState.NotStarted)
          throw new InvalidOperationException(Strings.ExEnumerationIsNotStarted);
        if (state==SqlReaderState.Finished)
          throw new InvalidOperationException(Strings.ExEnumerationIsAlreadyFinished);
        return current;
      }
    }

    /// <inheritdoc/>
    object IEnumerator.Current
    {
      get { return Current; }
    }

    /// <inheritdoc/>
    public void MoveTo(IEntire<Tuple> key)
    {
      CheckState();
      throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Range<IEntire<Tuple>> Range
    {
      get { return range; }
    }

    public Direction Direction
    {
      get { return direction; }
    }

    private void CheckState()
    {
      if (state==SqlReaderState.Inconsistent)
        throw new InvalidOperationException(Strings.ExReaderIsNotInConsistentState);
      if (provider.Handler.GetIndexVersion(provider.Index) != indexVersion) {
        state = SqlReaderState.Inconsistent;
        throw new InvalidOperationException(String.Format(Strings.ExIndexXXXChanged, provider.Index.Name));
      }
    }

    private void UpdateVersion()
    {
      indexVersion = provider.Handler.GetIndexVersion(provider.Index);
    }

    private SqlExpression BuildRangeExpression(Range<IEntire<Tuple>> range, int count)
    {
      SqlExpression expression;
      throw new NotImplementedException();
    }

    public SqlReader(Range<IEntire<Tuple>> range, SqlRecordsetProvider provider)
    {
      this.range = range;
      this.provider = provider;
//      provider.Header.
      direction = range.GetDirection(AdvancedComparer<IEntire<Tuple>>.Default);
      UpdateVersion();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
      // throw new System.NotImplementedException();
    }
  }
}