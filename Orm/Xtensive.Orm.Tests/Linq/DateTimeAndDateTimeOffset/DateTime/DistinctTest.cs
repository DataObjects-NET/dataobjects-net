// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
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
    public void DistinctByDateTimeTest()
    {
      ExecuteInsideSession(() => DistinctPrivate<DateTimeEntity, DateTime>(c => c.DateTime));
#if DO_DATEONLY
      ExecuteInsideSession(() => DistinctPrivate<DateTimeEntity, DateOnly>(c => c.DateOnly));
      ExecuteInsideSession(() => DistinctPrivate<DateTimeEntity, TimeOnly>(c => c.TimeOnly));
#endif
    }

    [Test]
    public void DistinctByDateTimeWithMillisecondsTest()
    {
      ExecuteInsideSession(() => DistinctPrivate<MillisecondDateTimeEntity, DateTime>(c => c.DateTime));
    }

    [Test]
    public void DistinctByNullableDateTimeTest()
    {
      ExecuteInsideSession(() => DistinctPrivate<NullableDateTimeEntity, DateTime?>(c => c.DateTime));
    }

    private void DistinctPrivate<T, TK>(Expression<Func<T, TK>> selectExpression)
      where T : Entity
    {
      var compiledSelectExpression = selectExpression.Compile();
      var distinctLocal = Query.All<T>().ToArray().Select(compiledSelectExpression).Distinct().OrderBy(c => c);
      var distinctByServer = Query.All<T>().Select(selectExpression).Distinct().OrderBy(c => c);
      Assert.IsTrue(distinctLocal.SequenceEqual(distinctByServer));

      distinctByServer = Query.All<T>().Select(selectExpression).Distinct().OrderByDescending(c => c);
      Assert.IsFalse(distinctLocal.SequenceEqual(distinctByServer));
    }
  }
}
