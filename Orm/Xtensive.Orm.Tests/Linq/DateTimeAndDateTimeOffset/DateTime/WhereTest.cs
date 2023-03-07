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
  public class WhereTest : DateTimeBaseTest
  {
    [Test]
    public void DateTimeWhereTest()
    {
      ExecuteInsideSession((s) => {
        WherePrivate<DateTimeEntity, long>(s, c => c.DateTime == FirstDateTime, c => c.Id);
        WherePrivate<DateTimeEntity, long>(s, c => c.DateTime.Hour == FirstDateTime.Hour, c => c.Id);
        WherePrivate<DateTimeEntity, long>(s, c => c.DateTime.Second == FirstDateTime.Second, c => c.Id);
      });
    }

    [Test]
    public void MillisecondDateTimeWhereTest()
    {
      Require.ProviderIsNot(StorageProvider.MySql);
      ExecuteInsideSession((s) => {
        WherePrivate<MillisecondDateTimeEntity, long>(s, c => c.DateTime == FirstMillisecondDateTime, c => c.Id);
        WherePrivate<MillisecondDateTimeEntity, long>(s, c => c.DateTime.Hour == FirstMillisecondDateTime.Hour, c => c.Id);
        WherePrivate<MillisecondDateTimeEntity, long>(s, c => c.DateTime.Millisecond == FirstMillisecondDateTime.Millisecond, c => c.Id);
      });
    }

    [Test]
    public void NullableDateTimeWhereTest()
    {
      ExecuteInsideSession((s) => {
        WherePrivate<NullableDateTimeEntity, long>(s, c => c.DateTime == FirstDateTime, c => c.Id);
        WherePrivate<NullableDateTimeEntity, long>(s, c => c.DateTime == null, c => c.Id);
        WherePrivate<NullableDateTimeEntity, long>(s, c => c.DateTime.HasValue && c.DateTime.Value.Hour == FirstDateTime.Hour, c => c.Id);
        WherePrivate<NullableDateTimeEntity, long>(s, c => c.DateTime.HasValue && c.DateTime.Value.Second == FirstDateTime.Second, c => c.Id);
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
