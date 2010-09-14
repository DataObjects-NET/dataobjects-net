// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.02.25

using System;
using System.IO;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Linq.SerializableExpressions;
using Xtensive.Core.Serialization.Binary;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Serialization;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;
using System.Linq;

namespace Xtensive.Storage.Tests.Storage
{
  [TestFixture]
  public class SerializedQueryTest : NorthwindDOModelTest
  {
    [Test]
    public void CombinedTest()
    {
      var query = Query.All<Category>()
        .Where(c => c.CategoryName == "Beverages")
        .Where(c => c.Id > 0)
        .Take(5)
        .Skip(0);

      var serializableExpression = query.Expression.ToSerializableExpression();
      var serializedExpression = LegacyBinarySerializer.Instance.Clone(serializableExpression) as SerializableExpression;

      var deserializedExpression = serializedExpression.ToExpression();
      var deserializedQuery = new Queryable<Category>((QueryProvider) query.Provider, deserializedExpression);
      var result = deserializedQuery.ToList();
    }
  }
}