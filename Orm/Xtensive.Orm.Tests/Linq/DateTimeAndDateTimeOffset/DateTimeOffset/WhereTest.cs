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
    public void Test01()
    {
      ExecuteInsideSession(() => {
        var firstDateTimeOffset = TryMoveToLocalTimeZone(FirstDateTimeOffset);
        WherePrivate<DateTimeOffsetEntity, long>(c => c.DateTimeOffset==firstDateTimeOffset, c => c.Id);
      });
    }

    [Test]
    public void Test02()
    {
      ExecuteInsideSession(() => {
        var firstDateTimeOffset = TryMoveToLocalTimeZone(FirstDateTimeOffset);
        WherePrivate<DateTimeOffsetEntity, long>(c => c.DateTimeOffset.Hour==firstDateTimeOffset.Hour, c => c.Id);
      });
    }

    [Test]
    public void Test03()
    {
      ExecuteInsideSession(() => {
        var firstDateTimeOffset = TryMoveToLocalTimeZone(FirstDateTimeOffset);
        WherePrivate<DateTimeOffsetEntity, long>(c => c.DateTimeOffset.Second==firstDateTimeOffset.Second, c => c.Id);
      });
    }

    [Test]
    public void Test04()
    {
      ExecuteInsideSession(() => {
        var firstDateTimeOffset = TryMoveToLocalTimeZone(FirstDateTimeOffset);
        WherePrivate<DateTimeOffsetEntity, long>(c => c.DateTimeOffset.Offset==firstDateTimeOffset.Offset, c => c.Id);
      });
    }

    [Test]
    public void Test05()
    {
      ExecuteInsideSession(() => {
        var firstDateTimeOffset = TryMoveToLocalTimeZone(FirstDateTimeOffset);
        WherePrivate<DateTimeOffsetEntity, long>(c => c.DateTimeOffset.Offset.Hours==firstDateTimeOffset.Offset.Hours, c => c.Id);
      });
    }

    [Test]
    public void Test06()
    {
      ExecuteInsideSession(() => {
        var firstDateTimeOffset = TryMoveToLocalTimeZone(FirstDateTimeOffset);
        WherePrivate<DateTimeOffsetEntity, long>(c => c.DateTimeOffset.Offset.Minutes==firstDateTimeOffset.Offset.Minutes, c => c.Id);
      });
    }

    [Test]
    public void Test07()
    {
      ExecuteInsideSession(() => {
        var firstMillisecondDateTimeOffset = TryMoveToLocalTimeZone(FirstMillisecondDateTimeOffset);
        WherePrivate<MillisecondDateTimeOffsetEntity, long>(c => c.DateTimeOffset==firstMillisecondDateTimeOffset, c => c.Id);
      });
    }

    [Test]
    public void Test08()
    {
      ExecuteInsideSession(() => {
        var firstMillisecondDateTimeOffset = TryMoveToLocalTimeZone(FirstMillisecondDateTimeOffset);
        WherePrivate<MillisecondDateTimeOffsetEntity, long>(c => c.DateTimeOffset.Hour==firstMillisecondDateTimeOffset.Hour, c => c.Id);
      });
    }

    [Test]
    public void Test09()
    {
      ExecuteInsideSession(() => {
        var firstMillisecondDateTimeOffset = TryMoveToLocalTimeZone(FirstMillisecondDateTimeOffset);
        WherePrivate<MillisecondDateTimeOffsetEntity, long>(c => c.DateTimeOffset.Millisecond==firstMillisecondDateTimeOffset.Millisecond, c => c.Id);
      });
    }

    [Test]
    public void Test10()
    {
      ExecuteInsideSession(() => {
        var firstMillisecondDateTimeOffset = TryMoveToLocalTimeZone(FirstMillisecondDateTimeOffset);
        WherePrivate<MillisecondDateTimeOffsetEntity, long>(c => c.DateTimeOffset.Offset==firstMillisecondDateTimeOffset.Offset, c => c.Id);
      });
    }

    [Test]
    public void Test11()
    {
      ExecuteInsideSession(() => {
        var firstMillisecondDateTimeOffset = TryMoveToLocalTimeZone(FirstMillisecondDateTimeOffset);
        WherePrivate<MillisecondDateTimeOffsetEntity, long>(c => c.DateTimeOffset==firstMillisecondDateTimeOffset, c => c.Id);
      });
    }

    [Test]
    public void Test12()
    {
      ExecuteInsideSession(() => {
        var firstMillisecondDateTimeOffset = TryMoveToLocalTimeZone(FirstMillisecondDateTimeOffset);
        WherePrivate<MillisecondDateTimeOffsetEntity, long>(c => c.DateTimeOffset.Hour==firstMillisecondDateTimeOffset.Hour, c => c.Id);
      });
    }

    [Test]
    public void Test13()
    {
      ExecuteInsideSession(() => {
        var firstMillisecondDateTimeOffset = TryMoveToLocalTimeZone(FirstMillisecondDateTimeOffset);
        WherePrivate<MillisecondDateTimeOffsetEntity, long>(c => c.DateTimeOffset.Millisecond==firstMillisecondDateTimeOffset.Millisecond, c => c.Id);
      });
    }

    [Test]
    public void Test14()
    {
      ExecuteInsideSession(() => {
        var firstMillisecondDateTimeOffset = TryMoveToLocalTimeZone(FirstMillisecondDateTimeOffset);
        WherePrivate<MillisecondDateTimeOffsetEntity, long>(c => c.DateTimeOffset.Offset==firstMillisecondDateTimeOffset.Offset, c => c.Id);
      });
    }

    [Test]
    public void Test15()
    {
      ExecuteInsideSession(() => {
        var firstDateTimeOffset = TryMoveToLocalTimeZone(FirstDateTimeOffset);
        WherePrivate<NullableDateTimeOffsetEntity, long>(c => c.DateTimeOffset==firstDateTimeOffset, c => c.Id);
      });
    }

    [Test]
    public void Test16()
    {
      ExecuteInsideSession(() => {
        WherePrivate<NullableDateTimeOffsetEntity, long>(c => c.DateTimeOffset==null, c => c.Id);
      });
    }

    [Test]
    public void Test17()
    {
      ExecuteInsideSession(() => {
        var firstDateTimeOffset = TryMoveToLocalTimeZone(FirstDateTimeOffset);
        WherePrivate<NullableDateTimeOffsetEntity, long>(c => c.DateTimeOffset.HasValue && c.DateTimeOffset.Value.Hour==firstDateTimeOffset.Hour, c => c.Id);
      });
    }

    [Test]
    public void Test18()
    {
      ExecuteInsideSession(() => {
        var firstDateTimeOffset = TryMoveToLocalTimeZone(TryMoveToLocalTimeZone(FirstDateTime));
        WherePrivate<NullableDateTimeOffsetEntity, long>(c => c.DateTimeOffset.HasValue && c.DateTimeOffset.Value.Second==firstDateTimeOffset.Second, c => c.Id);
      });
    }

    [Test]
    public void Test19()
    {
      ExecuteInsideSession(() => {
        var firstDateTimeOffset = TryMoveToLocalTimeZone(TryMoveToLocalTimeZone(FirstDateTime));
        WherePrivate<NullableDateTimeOffsetEntity, long>(c => c.DateTimeOffset.HasValue && c.DateTimeOffset.Value.Offset==firstDateTimeOffset.Offset, c => c.Id);
      });
    }

    private void WherePrivate<T, TK>(Expression<Func<T, bool>> whereExpression, Expression<Func<T, TK>> orderByExpression)
      where T : Entity
    {
      var compiledWhereExpression = whereExpression.Compile();
      var compiledOrderByExpression = orderByExpression.Compile();

      var whereLocal = Query.All<T>().ToArray().Where(compiledWhereExpression).OrderBy(compiledOrderByExpression).ToArray();
      var whereByServer = Query.All<T>().Where(whereExpression).OrderBy(orderByExpression).ToArray();

      Assert.That(whereLocal.Length, Is.Not.EqualTo(0));
      Assert.That(whereByServer.Length, Is.Not.EqualTo(0));
      Assert.IsTrue(whereLocal.SequenceEqual(whereByServer));

      whereByServer = Query.All<T>().Where(whereExpression).OrderByDescending(orderByExpression).ToArray();
      whereLocal = Query.All<T>().ToArray().Where(compiledWhereExpression).OrderBy(compiledOrderByExpression).ToArray();

      Assert.That(whereLocal.Length, Is.Not.EqualTo(0));
      Assert.That(whereByServer.Length, Is.Not.EqualTo(0));
      Assert.IsFalse(whereLocal.SequenceEqual(whereByServer));
    }
  }
}