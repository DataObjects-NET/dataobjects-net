// Copyright (C) 2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.TimeOnlys
{
  public class WhereTest : DateTimeBaseTest
  {
    [Test]
    public void DateTimeWhereTest()
    {
      ExecuteInsideSession((s) => {
        WherePrivate<TimeOnlyEntity, long>(s, c => c.TimeOnly == FirstTimeOnly, c => c.Id);
        WherePrivate<TimeOnlyEntity, long>(s, c => c.TimeOnly.Hour == FirstTimeOnly.Hour, c => c.Id);
        WherePrivate<TimeOnlyEntity, long>(s, c => c.TimeOnly.Second == FirstTimeOnly.Second, c => c.Id);
      });
    }

    [Test]
    public void MillisecondDateTimeWhereTest()
    {
      Require.ProviderIsNot(StorageProvider.MySql);
      ExecuteInsideSession((s) => {
        WherePrivate<TimeOnlyEntity, long>(s, c => c.MillisecondTimeOnly == FirstMillisecondTimeOnly, c => c.Id);
        WherePrivate<TimeOnlyEntity, long>(s, c => c.MillisecondTimeOnly.Hour == FirstMillisecondTimeOnly.Hour, c => c.Id);
        WherePrivate<TimeOnlyEntity, long>(s, c => c.MillisecondTimeOnly.Millisecond == FirstMillisecondTimeOnly.Millisecond, c => c.Id);
      });
    }

    [Test]
    public void NullableDateTimeWhereTest()
    {
      ExecuteInsideSession((s) => {
        WherePrivate<TimeOnlyEntity, long>(s, c => c.NullableTimeOnly == NullableTimeOnly, c => c.Id);
        WherePrivate<TimeOnlyEntity, long>(s, c => c.NullableTimeOnly == null, c => c.Id);
        WherePrivate<TimeOnlyEntity, long>(s, c => c.NullableTimeOnly.HasValue && c.NullableTimeOnly.Value.Hour == NullableTimeOnly.Hour, c => c.Id);
        WherePrivate<TimeOnlyEntity, long>(s, c => c.NullableTimeOnly.HasValue && c.NullableTimeOnly.Value.Second == NullableTimeOnly.Second, c => c.Id);
      });
    }

    private static void WherePrivate<T, TK>(Session session, Expression<Func<T, bool>> whereExpression, Expression<Func<T, TK>> orderByExpression)
      where T : Entity
    {
      var compiledWhereExpression = whereExpression.Compile();
      var compiledOrderByExpression = orderByExpression.Compile();

      var whereLocal = session.Query.All<T>().ToArray().Where(compiledWhereExpression).OrderBy(compiledOrderByExpression);
      var whereByServer = session.Query.All<T>().Where(whereExpression).OrderBy(orderByExpression);
      Assert.IsTrue(whereLocal.SequenceEqual(whereByServer));

      whereByServer = session.Query.All<T>().Where(whereExpression).OrderByDescending(orderByExpression);
      Assert.IsFalse(whereLocal.SequenceEqual(whereByServer));
    }
  }
}