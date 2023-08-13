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
  public class WhereTest : DateTimeBaseTest
  {
    [Test]
    public void DateOnlyWhereTest()
    {
      ExecuteInsideSession((s) => {
        WherePrivate<DateOnlyEntity, long>(s, c => c.DateOnly == FirstDateOnly, c => c.Id);
        WherePrivate<DateOnlyEntity, long>(s, c => c.DateOnly.Day == FirstDateOnly.Day, c => c.Id);
        WherePrivate<DateOnlyEntity, long>(s, c => c.DateOnly.Month == FirstDateOnly.Month, c => c.Id);
      });
    }

    [Test]
    public void NullableDateOnlyWhereTest()
    {
      ExecuteInsideSession((s) => {
        WherePrivate<DateOnlyEntity, long>(s, c => c.NullableDateOnly == FirstDateOnly, c => c.Id);
        WherePrivate<DateOnlyEntity, long>(s, c => c.NullableDateOnly == null, c => c.Id);
        WherePrivate<DateOnlyEntity, long>(s, c => c.NullableDateOnly.HasValue && c.NullableDateOnly.Value.Day == FirstDateOnly.Day, c => c.Id);
        WherePrivate<DateOnlyEntity, long>(s, c => c.NullableDateOnly.HasValue && c.NullableDateOnly.Value.Month == FirstDateOnly.Month, c => c.Id);
      });
    }

    private static void WherePrivate<T, TK>(Session session, Expression<Func<T, bool>> whereExpression, Expression<Func<T, TK>> orderByExpression)
      where T : Entity
    {
      var compiledWhereExpression = whereExpression.Compile();
      var compiledOrderByExpression = orderByExpression.Compile();

      var whereLocal = session.Query.All<T>().ToArray().Where(compiledWhereExpression).OrderBy(compiledOrderByExpression);
      var whereByServer = session.Query.All<T>().Where(whereExpression).OrderBy(orderByExpression).ToList();
      Assert.IsTrue(whereLocal.SequenceEqual(whereByServer));

      whereByServer = session.Query.All<T>().Where(whereExpression).OrderByDescending(orderByExpression).ToList();
      Assert.IsFalse(whereLocal.SequenceEqual(whereByServer));
    }
  }
}
#endif