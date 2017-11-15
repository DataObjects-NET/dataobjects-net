// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.02.25

using System;
using System.IO;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Linq.SerializableExpressions;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Serialization;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;
using System.Linq;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class SerializedQueryTest : NorthwindDOModelTest
  {
    [Test]
    public void CombinedTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.RowNumber);
      var query = Session.Query.All<Category>()
        .Where(c => c.CategoryName == "Beverages")
        .Where(c => c.Id > 0)
        .Take(5)
        .Skip(0);

      var serializableExpression = query.Expression.ToSerializableExpression();
      var serializedExpression = Cloner.Clone(serializableExpression);

      var deserializedExpression = serializedExpression.ToExpression();
      var deserializedQuery = new Queryable<Category>((QueryProvider) query.Provider, deserializedExpression);
      var result = deserializedQuery.ToList();
    }
  }
}