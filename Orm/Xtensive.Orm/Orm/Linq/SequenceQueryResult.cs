using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xtensive.Orm.Linq.Expressions;
using Xtensive.Orm.Linq.Materialization;

namespace Xtensive.Orm.Linq
{
  public readonly struct SequenceQueryResult<TItem> : IEnumerable<TItem>, IAsyncEnumerable<TItem>
  {
    private readonly MaterializingReader<TItem> reader;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<TItem> GetEnumerator() => reader;

    public IAsyncEnumerator<TItem> GetAsyncEnumerator(CancellationToken cancellationToken = default) => reader;

    internal SequenceQueryResult(MaterializingReader<TItem> reader)
    {
      this.reader = reader;
    }
  }

  internal static class SequenceQueryResultExtensions
  {
    public static TResult ToScalar<TResult>(this in SequenceQueryResult<TResult> sequence, ResultType resultType) =>
      resultType switch {
        ResultType.First => sequence.First(),
        ResultType.FirstOrDefault => sequence.FirstOrDefault(),
        ResultType.Single => sequence.Single(),
        ResultType.SingleOrDefault => sequence.SingleOrDefault(),
        _ => throw new InvalidOperationException("Query is not scalar.")
      };
  }
}