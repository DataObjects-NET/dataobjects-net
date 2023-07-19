// Copyright (C) 2016-2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Groznov
// Created:    2016.08.01

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.DateTimes
{
  public class DistinctTest : DateTimeBaseTest
  {
    [Test]
    public void DistinctByDateTimeTest() =>
      ExecuteInsideSession((s) => DistinctPrivate<DateTimeEntity, DateTime>(s,c => c.DateTime));

    [Test]
    public void DistinctByDateTimeWithMillisecondsTest() =>
      ExecuteInsideSession((s) => DistinctPrivate<MillisecondDateTimeEntity, DateTime>(s, c => c.DateTime));

    [Test]
    public void DistinctByNullableDateTimeTest() =>
      ExecuteInsideSession((s) => DistinctPrivate<NullableDateTimeEntity, DateTime?>(s, c => c.DateTime));

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
