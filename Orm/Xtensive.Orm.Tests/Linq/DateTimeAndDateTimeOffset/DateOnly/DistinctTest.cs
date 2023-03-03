// Copyright (C) 2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

#if NET6_0_OR_GREATER

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.DateOnlys
{
  public class DistinctTest : DateTimeBaseTest
  {
    [Test]
    public void DistinctByDateOnlyTest() =>
      ExecuteInsideSession(static (s) => DistinctPrivate<DateOnlyEntity, DateOnly>(s, c => c.DateOnly));

    [Test]
    public void DistinctByNullableDateOnlyTest() =>
      ExecuteInsideSession(static (s) => DistinctPrivate<DateOnlyEntity, DateOnly?>(s, c => c.NullableDateOnly));

    private static void DistinctPrivate<T, TK>(Session session, Expression<Func<T, TK>> selectExpression)
      where T : Entity
    {
      var compiledSelectExpression = selectExpression.Compile();
      var distinctLocal = session.Query.All<T>().ToArray().Select(compiledSelectExpression).Distinct().OrderBy(c => c);
      var distinctByServer = session.Query.All<T>().Select(selectExpression).Distinct().OrderBy(c => c);
      Assert.IsTrue(distinctLocal.SequenceEqual(distinctByServer));

      distinctByServer = session.Query.All<T>().Select(selectExpression).Distinct().OrderByDescending(c => c);
      Assert.IsFalse(distinctLocal.SequenceEqual(distinctByServer));
    }
  }
}
#endif
