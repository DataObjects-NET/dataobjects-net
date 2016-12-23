using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.DateTimes
{
  public class MinMaxTest : DateTimeBaseTest
  {
    [Test]
    public void DateTimeMinMaxTest()
    {
      ExecuteInsideSession(() => MinMaxPrivate<DateTimeEntity, DateTime>(c => c.DateTime));
    }

    [Test]
    public void MillisecondDateTimeMinMaxTest()
    {
      ExecuteInsideSession(() => MinMaxPrivate<MillisecondDateTimeEntity, DateTime>(c => c.DateTime));
    }

    [Test]
    public void NullableDateTimeMinMaxTest()
    {
      ExecuteInsideSession(() => MinMaxPrivate<NullableDateTimeEntity, DateTime?>(c => c.DateTime));
    }

    private void MinMaxPrivate<T, TK>(Expression<Func<T, TK>> selectExpression)
      where T : Entity
    {
      var compiledSelectExpression = selectExpression.Compile();
      var minLocal = Query.All<T>().ToArray().Min(compiledSelectExpression);
      var maxLocal = Query.All<T>().ToArray().Max(compiledSelectExpression);
      var minServer = Query.All<T>().Min(selectExpression);
      var maxServer = Query.All<T>().Max(selectExpression);

      Assert.AreEqual(minLocal, minServer);
      Assert.AreEqual(maxLocal, maxServer);
      Assert.AreNotEqual(minLocal, maxServer);
      Assert.AreNotEqual(maxLocal, minServer);
    }
  }
}