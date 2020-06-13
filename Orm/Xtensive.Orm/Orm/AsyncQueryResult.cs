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
  public readonly struct QueryResult<TItem> : IEnumerable<TItem>, IAsyncEnumerable<TItem>
  {
    private readonly IMaterializingReader<TItem> reader;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<TItem> GetEnumerator() => reader.AsEnumerator();

    public IAsyncEnumerator<TItem> GetAsyncEnumerator(CancellationToken token = default) =>
      reader.AsAsyncEnumerator(token);

    internal QueryResult(IMaterializingReader<TItem> reader)
    {
      this.reader = reader;
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
      await foreach (var element in result.WithCancellation(cancellationToken)) {
        yield return element;
      }
    }

    internal AsyncQueryResult(Task<QueryResult<TElement>> queryResultTask)
    {
      this.queryResultTask = queryResultTask;
    }
  }
}