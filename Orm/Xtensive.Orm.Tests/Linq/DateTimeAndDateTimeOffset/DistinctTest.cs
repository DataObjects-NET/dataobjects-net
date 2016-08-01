// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Groznov
// Created:    2016.08.01

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset
{
  public class DistinctTest : BaseDateTimeAndDateTimeOffsetTest
  {
    [Test]
    public void DateTimeDistinctTest()
    {
      OpenSessionAndAction(() => DistinctPrivate<DateTimeEntity, DateTime>(c => c.DateTime));
    }

    [Test]
    public void MillisecondDateTimeDistinctTest()
    {
      OpenSessionAndAction(() => DistinctPrivate<MillisecondDateTimeEntity, DateTime>(c => c.DateTime));
    }

    [Test]
    public void NullableDateTimeDistinctTest()
    {
      OpenSessionAndAction(() => DistinctPrivate<NullableDateTimeEntity, DateTime?>(c => c.DateTime));
    }

    [Test(Description = "Might be failed on SQLite because of incomplete emulating datetimeoffset")]
    public void DateTimeOffsetDistinctTest()
    {
      Require.AnyFeatureSupported(ProviderFeatures.DateTimeOffset | ProviderFeatures.DateTimeOffsetEmulation);
      OpenSessionAndAction(() => DistinctPrivate<DateTimeOffsetEntity, DateTimeOffset>(c => c.DateTimeOffset));
    }

    [Test(Description = "Might be failed on SQLite because of incomplete emulating datetimeoffset")]
    public void MillisecondDateTimeOffsetDistinctTest()
    {
      Require.AnyFeatureSupported(ProviderFeatures.DateTimeOffset | ProviderFeatures.DateTimeOffsetEmulation);
      OpenSessionAndAction(() => DistinctPrivate<MillisecondDateTimeOffsetEntity, DateTimeOffset>(c => c.DateTimeOffset));
    }

    [Test(Description = "Might be failed on SQLite because of incomplete emulating datetimeoffset")]
    public void NullableDateTimeOffsetDistinctTest()
    {
      Require.AnyFeatureSupported(ProviderFeatures.DateTimeOffset | ProviderFeatures.DateTimeOffsetEmulation);
      OpenSessionAndAction(() => DistinctPrivate<NullableDateTimeOffsetEntity, DateTimeOffset?>(c => c.DateTimeOffset));
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
