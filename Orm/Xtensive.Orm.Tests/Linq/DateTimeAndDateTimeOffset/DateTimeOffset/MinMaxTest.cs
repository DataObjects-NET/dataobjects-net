// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Groznov
// Created:    2016.07.29

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.DateTimeOffsets
{
  public class MinMaxTest : DateTimeOffsetBaseTest
  {
    [Test(Description = "Might be failed on SQLite because of incomplete emulating datetimeoffset")]
    public void DateTimeOffsetMinMaxTest()
    {
      ExecuteInsideSession(() => MinMaxPrivate<DateTimeOffsetEntity, DateTimeOffset>(c => c.DateTimeOffset));
    }

    [Test(Description = "Might be failed on SQLite because of incomplete emulating datetimeoffset")]
    public void MillisecondDateTimeOffsetMinMaxTest()
    {
      ExecuteInsideSession(() => MinMaxPrivate<MillisecondDateTimeOffsetEntity, DateTimeOffset>(c => c.DateTimeOffset));
    }

    [Test(Description = "Might be failed on SQLite because of incomplete emulating datetimeoffset")]
    public void NullableDateTimeOffsetMinMaxTest()
    {
      ExecuteInsideSession(() => MinMaxPrivate<NullableDateTimeOffsetEntity, DateTimeOffset?>(c => c.DateTimeOffset));
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
