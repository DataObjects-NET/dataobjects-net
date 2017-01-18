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

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.DateTimeOffsets
{
  public class DistinctTest : DateTimeOffsetBaseTest
  {
    [Test(Description = "Might be failed on SQLite because of incomplete emulating datetimeoffset")]
    public void DistinctByDateTimeOffsetTest()
    {
      ExecuteInsideSession(() => DistinctPrivate<DateTimeOffsetEntity, DateTimeOffset>(c => c.DateTimeOffset));
    }

    [Test(Description = "Might be failed on SQLite because of incomplete emulating datetimeoffset")]
    public void DistinctByDateTimeOffsetWithMillisecondsTest()
    {
      ExecuteInsideSession(() => DistinctPrivate<MillisecondDateTimeOffsetEntity, DateTimeOffset>(c => c.DateTimeOffset));
    }

    [Test(Description = "Might be failed on SQLite because of incomplete emulating datetimeoffset")]
    public void DistinctByNullableDateTimeOffsetTest()
    {
      ExecuteInsideSession(() => DistinctPrivate<NullableDateTimeOffsetEntity, DateTimeOffset?>(c => c.DateTimeOffset));
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