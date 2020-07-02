using System;
using System.Linq;
using Xtensive.Orm.Linq.Expressions;

namespace Xtensive.Orm
{
  internal static class QueryResultExtensions
  {
    public static TResult ToScalar<TResult>(this in QueryResult<TResult> sequence, ResultAccessMethod resultAccessMethod) =>
      resultAccessMethod switch {
        ResultAccessMethod.First => sequence.First(),
        ResultAccessMethod.FirstOrDefault => sequence.FirstOrDefault(),
        ResultAccessMethod.Single => sequence.Single(),
        ResultAccessMethod.SingleOrDefault => sequence.SingleOrDefault(),
        _ => throw new InvalidOperationException("Query is not scalar.")
      };
  }
}