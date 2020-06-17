using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Orm.Linq.Expressions;
using Xtensive.Orm.Linq.Materialization;

namespace Xtensive.Orm
{
  public readonly struct QueryResult<TItem> : IEnumerable<TItem>
  {
    internal readonly IMaterializingReader<TItem> Reader;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<TItem> GetEnumerator() => Reader.AsEnumerator();

    internal QueryResult(IMaterializingReader<TItem> reader)
    {
      Reader = reader;
    }
  }

  internal static class QueryResultExtensions
  {
    public static TResult ToScalar<TResult>(this in QueryResult<TResult> sequence, ResultType resultType) =>
      resultType switch {
        ResultType.First => sequence.First(),
        ResultType.FirstOrDefault => sequence.FirstOrDefault(),
        ResultType.Single => sequence.Single(),
        ResultType.SingleOrDefault => sequence.SingleOrDefault(),
        _ => throw new InvalidOperationException("Query is not scalar.")
      };
  }

  public readonly struct AsyncQueryResult<TElement>: IAsyncEnumerable<TElement>
  {
    private readonly Task<QueryResult<TElement>> queryResultTask;

    public TaskAwaiter<QueryResult<TElement>> GetAwaiter() => queryResultTask.GetAwaiter();

    public async IAsyncEnumerator<TElement> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
      var result = await queryResultTask;
      var asyncEnumerator = result.Reader.AsAsyncEnumerator(cancellationToken);
      while (await asyncEnumerator.MoveNextAsync()) {
        yield return asyncEnumerator.Current;
      }
    }

    internal AsyncQueryResult(Task<QueryResult<TElement>> queryResultTask)
    {
      this.queryResultTask = queryResultTask;
    }
  }
}