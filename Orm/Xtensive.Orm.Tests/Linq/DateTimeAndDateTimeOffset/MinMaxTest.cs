// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Groznov
// Created:    2016.07.29

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset
{
  public class MinMaxTest : BaseDateTimeAndDateTimeOffsetTest
  {
    [Test]
    public void DateTimeMinMaxTest()
    {
      OpenSessionAndAction(() => MinMaxPrivate<DateTimeEntity, DateTime>(c => c.DateTime));
    }

    [Test]
    public void MillisecondDateTimeMinMaxTest()
    {
      OpenSessionAndAction(() => MinMaxPrivate<MillisecondDateTimeEntity, DateTime>(c => c.DateTime));
    }

    [Test]
    public void NullableDateTimeMinMaxTest()
    {
      OpenSessionAndAction(() => MinMaxPrivate<NullableDateTimeEntity, DateTime?>(c => c.DateTime));
    }

    [Test(Description = "Might be failed on SQLite because of incomplete emulating datetimeoffset")]
    public void DateTimeOffsetMinMaxTest()
    {
      Require.AnyFeatureSupported(ProviderFeatures.DateTimeOffset | ProviderFeatures.DateTimeOffsetEmulation);
      OpenSessionAndAction(() => MinMaxPrivate<DateTimeOffsetEntity, DateTimeOffset>(c => c.DateTimeOffset));
    }

    [Test(Description = "Might be failed on SQLite because of incomplete emulating datetimeoffset")]
    public void MillisecondDateTimeOffsetMinMaxTest()
    {
      Require.AnyFeatureSupported(ProviderFeatures.DateTimeOffset | ProviderFeatures.DateTimeOffsetEmulation);
      OpenSessionAndAction(() => MinMaxPrivate<MillisecondDateTimeOffsetEntity, DateTimeOffset>(c => c.DateTimeOffset));
    }

    [Test(Description = "Might be failed on SQLite because of incomplete emulating datetimeoffset")]
    public void NullableDateTimeOffsetMinMaxTest()
    {
      Require.AnyFeatureSupported(ProviderFeatures.DateTimeOffset | ProviderFeatures.DateTimeOffsetEmulation);
      OpenSessionAndAction(() => MinMaxPrivate<NullableDateTimeOffsetEntity, DateTimeOffset?>(c => c.DateTimeOffset));
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
