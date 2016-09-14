using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.DateTimeOffsets
{
  public class WhereTest : DateTimeOffsetBaseTest
  {
    [Test]
    public void DateTimeOffsetWhereTest()
    {
      ExecuteInsideSession(() => {
        WherePrivate<DateTimeOffsetEntity, long>(c => c.DateTimeOffset==FirstDateTimeOffset, c => c.Id);
        WherePrivate<DateTimeOffsetEntity, long>(c => c.DateTimeOffset.Hour==FirstDateTimeOffset.Hour, c => c.Id);
        WherePrivate<DateTimeOffsetEntity, long>(c => c.DateTimeOffset.Second==FirstDateTimeOffset.Second, c => c.Id);
        WherePrivate<DateTimeOffsetEntity, long>(c => c.DateTimeOffset.Offset==FirstDateTimeOffset.Offset, c => c.Id);
      });
    }

    [Test(Description = "Might be failed on SQLite because of certain restrictions of work with milliseconds")]
    public void MillisecondDateTimeOffsetWhereTest()
    {
      ExecuteInsideSession(() => {
        WherePrivate<MillisecondDateTimeOffsetEntity, long>(c => c.DateTimeOffset==FirstMillisecondDateTimeOffset, c => c.Id);
        WherePrivate<MillisecondDateTimeOffsetEntity, long>(c => c.DateTimeOffset.Hour==FirstMillisecondDateTimeOffset.Hour, c => c.Id);
        WherePrivate<MillisecondDateTimeOffsetEntity, long>(c => c.DateTimeOffset.Millisecond==FirstMillisecondDateTimeOffset.Millisecond, c => c.Id);
        WherePrivate<MillisecondDateTimeOffsetEntity, long>(c => c.DateTimeOffset.Offset==FirstMillisecondDateTimeOffset.Offset, c => c.Id);
      });
    }

    [Test]
    public void NullableDateTimeOffsetWhereTest()
    {
      ExecuteInsideSession(() => {
        WherePrivate<NullableDateTimeOffsetEntity, long>(c => c.DateTimeOffset==FirstDateTimeOffset, c => c.Id);
        WherePrivate<NullableDateTimeOffsetEntity, long>(c => c.DateTimeOffset==null, c => c.Id);
        WherePrivate<NullableDateTimeOffsetEntity, long>(c => c.DateTimeOffset.HasValue && c.DateTimeOffset.Value.Hour==FirstDateTimeOffset.Hour, c => c.Id);
        WherePrivate<NullableDateTimeOffsetEntity, long>(c => c.DateTimeOffset.HasValue && c.DateTimeOffset.Value.Second==FirstDateTimeOffset.Second, c => c.Id);
        WherePrivate<NullableDateTimeOffsetEntity, long>(c => c.DateTimeOffset.HasValue && c.DateTimeOffset.Value.Offset==FirstDateTimeOffset.Offset, c => c.Id);
      });
    }

    private void WherePrivate<T, TK>(Expression<Func<T, bool>> whereExpression, Expression<Func<T, TK>> orderByExpression)
      where T : Entity
    {
      var compiledWhereExpression = whereExpression.Compile();
      var compiledOrderByExpression = orderByExpression.Compile();

      var whereLocal = Query.All<T>().ToArray().Where(compiledWhereExpression).OrderBy(compiledOrderByExpression);
      var whereByServer = Query.All<T>().Where(whereExpression).OrderBy(orderByExpression);
      Assert.IsTrue(whereLocal.SequenceEqual(whereByServer));

      whereByServer = Query.All<T>().Where(whereExpression).OrderByDescending(orderByExpression);
      Assert.IsFalse(whereLocal.SequenceEqual(whereByServer));
    }
  }
}