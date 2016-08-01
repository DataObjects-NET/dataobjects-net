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
  public class SkipTakeTest : BaseDateTimeAndDateTimeOffsetTest
  {
    private const int SkipCount = 5;
    private const int TakeCount = 15;

    [Test]
    public void DateTimeSkipTakeTest()
    {
      OpenSessionAndAction(() => SkipTakePrivate<DateTimeEntity, DateTime, long>(c => c.DateTime, c => c.Id, SkipCount, TakeCount));
    }

    [Test]
    public void MillisecondDateTimeSkipTakeTest()
    {
      OpenSessionAndAction(() => SkipTakePrivate<MillisecondDateTimeEntity, DateTime, long>(c => c.DateTime, c => c.Id, SkipCount, TakeCount));
    }

    [Test]
    public void NullableDateTimeSkipTakeTest()
    {
      OpenSessionAndAction(() => SkipTakePrivate<NullableDateTimeEntity, DateTime?, long>(c => c.DateTime, c => c.Id, SkipCount, TakeCount));
    }

    [Test]
    public void DateTimeOffsetSkipTakeTest()
    {
      Require.AnyFeatureSupported(ProviderFeatures.DateTimeOffset | ProviderFeatures.DateTimeOffsetEmulation);
      OpenSessionAndAction(() => SkipTakePrivate<DateTimeOffsetEntity, DateTimeOffset, long>(c => c.DateTimeOffset, c => c.Id, SkipCount, TakeCount));
    }

    [Test]
    public void MillisecondDateTimeOffsetSkipTakeTest()
    {
      Require.AnyFeatureSupported(ProviderFeatures.DateTimeOffset | ProviderFeatures.DateTimeOffsetEmulation);
      OpenSessionAndAction(() => SkipTakePrivate<MillisecondDateTimeOffsetEntity, DateTimeOffset, long>(c => c.DateTimeOffset, c => c.Id, SkipCount, TakeCount));
    }

    [Test]
    public void NullableDateTimeOffsetSkipTakeTest()
    {
      Require.AnyFeatureSupported(ProviderFeatures.DateTimeOffset | ProviderFeatures.DateTimeOffsetEmulation);
      OpenSessionAndAction(() => SkipTakePrivate<NullableDateTimeOffsetEntity, DateTimeOffset?, long>(c => c.DateTimeOffset, c => c.Id, SkipCount, TakeCount));
    }

    private void SkipTakePrivate<T, TK1, TK2>(Expression<Func<T, TK1>> orderByExpression, Expression<Func<T, TK2>> thenByExpression, int skipCount, int takeCount)
      where T : Entity
    {
      var compiledOrderByExpression = orderByExpression.Compile();
      var compiledThenByExpression = thenByExpression.Compile();
      var skipTakeLocal = Query.All<T>().ToArray().OrderBy(compiledOrderByExpression).ThenBy(compiledThenByExpression).Skip(skipCount).Take(takeCount).ToArray();
      var skipTakeServer = Query.All<T>().OrderBy(orderByExpression).ThenBy(thenByExpression).Skip(skipCount).Take(takeCount);
      Assert.IsTrue(skipTakeLocal.SequenceEqual(skipTakeServer));

      skipTakeServer = Query.All<T>().OrderByDescending(orderByExpression).Skip(skipCount).Take(takeCount);
      Assert.IsFalse(skipTakeLocal.SequenceEqual(skipTakeServer));
    }
  }
}
