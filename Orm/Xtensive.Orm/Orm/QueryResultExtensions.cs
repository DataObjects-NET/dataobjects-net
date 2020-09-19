// Copyright (C) 2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

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