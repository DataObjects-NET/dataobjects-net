// Copyright (C) 2016-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
    [Test]
    public void DateTimeOffsetMinMaxTest()
    {
      Require.AllFeaturesNotSupported(Providers.ProviderFeatures.DateTimeOffsetEmulation);

      ExecuteInsideSession((s) => MinMaxPrivate<DateTimeOffsetEntity, DateTimeOffset>(s, c => c.DateTimeOffset));
    }

    [Test]
    public void MillisecondDateTimeOffsetMinMaxTest()
    {
      Require.AllFeaturesNotSupported(Providers.ProviderFeatures.DateTimeOffsetEmulation);

      ExecuteInsideSession((s) => MinMaxPrivate<MillisecondDateTimeOffsetEntity, DateTimeOffset>(s, c => c.DateTimeOffset));
    }

    [Test]
    public void NullableDateTimeOffsetMinMaxTest()
    {
      Require.AllFeaturesNotSupported(Providers.ProviderFeatures.DateTimeOffsetEmulation);

      ExecuteInsideSession((s) => MinMaxPrivate<NullableDateTimeOffsetEntity, DateTimeOffset?>(s, c => c.DateTimeOffset));
    }

    private void MinMaxPrivate<T, TK>(Session session, Expression<Func<T, TK>> selectExpression)
      where T : Entity
    {
      var compiledSelectExpression = selectExpression.Compile();
      var minLocal = session.Query.All<T>().AsEnumerable().Min(compiledSelectExpression);
      var maxLocal = session.Query.All<T>().AsEnumerable().Max(compiledSelectExpression);
      var minServer = session.Query.All<T>().Min(selectExpression);
      var maxServer = session.Query.All<T>().Max(selectExpression);

      Assert.AreEqual(minLocal, minServer);
      Assert.AreEqual(maxLocal, maxServer);
      Assert.AreNotEqual(minLocal, maxServer);
      Assert.AreNotEqual(maxLocal, minServer);
    }
  }
}
