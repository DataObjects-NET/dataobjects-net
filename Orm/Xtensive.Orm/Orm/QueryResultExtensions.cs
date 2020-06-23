using System;
using System.Linq;
using Xtensive.Orm.Linq.Expressions;

namespace Xtensive.Orm
{
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
}