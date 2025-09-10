// Copyright (C) 2016-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
    [Test]
    public void DistinctByDateTimeOffsetTest()
    {
      Require.AllFeaturesNotSupported(Providers.ProviderFeatures.DateTimeOffsetEmulation);

      ExecuteInsideSession((s) => DistinctPrivate<DateTimeOffsetEntity, DateTimeOffset>(s, c => c.DateTimeOffset));
    }

    [Test]
    public void DistinctByDateTimeOffsetWithMillisecondsTest()
    {
      Require.AllFeaturesNotSupported(Providers.ProviderFeatures.DateTimeOffsetEmulation);

      ExecuteInsideSession((s) => DistinctPrivate<MillisecondDateTimeOffsetEntity, DateTimeOffset>(s, c => c.DateTimeOffset));
    }

    [Test]
    public void DistinctByNullableDateTimeOffsetTest()
    {
      Require.AllFeaturesNotSupported(Providers.ProviderFeatures.DateTimeOffsetEmulation);

      ExecuteInsideSession((s) => DistinctPrivate<NullableDateTimeOffsetEntity, DateTimeOffset?>(s, c => c.DateTimeOffset));
    }

    private void DistinctPrivate<T, TK>(Session session, Expression<Func<T, TK>> selectExpression)
      where T : Entity
    {
      var compiledSelectExpression = selectExpression.Compile();
      var distinctLocal = session.Query.All<T>().AsEnumerable().Select(compiledSelectExpression).Distinct().OrderBy(c => c);
      var distinctByServer = session.Query.All<T>().Select(selectExpression).Distinct().OrderBy(c => c);
      Assert.IsTrue(distinctLocal.SequenceEqual(distinctByServer));

      distinctByServer = session.Query.All<T>().Select(selectExpression).Distinct().OrderByDescending(c => c);
      Assert.IsFalse(distinctLocal.SequenceEqual(distinctByServer));
    }
  }
}