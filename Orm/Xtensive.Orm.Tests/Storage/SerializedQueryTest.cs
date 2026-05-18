// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.02.25

using System;
using System.IO;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.ChinookDO;
using System.Linq;
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class SerializedQueryTest : ChinookDOModelTest
  {
    [Test]
    [Obsolete("No BinaryFormating allowed from now on")]
    public void BinarySerializationTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.RowNumber);
      var query = Session.Query.All<MediaType>()
        .Where(c => c.Name=="MPEG audio file")
        .Where(c => c.MediaTypeId > 0)
        .Take(5)
        .Skip(0);

      var serializableExpression = query.Expression.ToSerializableExpression();
      var serializedExpression = Cloner.CloneViaBinarySerialization(serializableExpression);

      var deserializedExpression = serializedExpression.ToExpression();
      var deserializedQuery = new Queryable<MediaType>((QueryProvider) query.Provider, deserializedExpression);
      var result = deserializedQuery.ToList();
    }

    [Test]
    public void DataContractXmlSerializationTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.RowNumber);
      var query = Session.Query.All<MediaType>()
        .Where(c => c.Name == "MPEG audio file")
        .Where(c => c.MediaTypeId > 0)
        .Take(5)
        .Skip(0);

      var serializableExpression = query.Expression.ToSerializableExpression();
      var serializedExpression = Cloner.CloneViaXmlSerialization(serializableExpression, Cloner.SerializationDomains.System.Value);

      var deserializedExpression = serializedExpression.ToExpression();
      var deserializedQuery = new Queryable<MediaType>((QueryProvider) query.Provider, deserializedExpression);
      var result = deserializedQuery.ToList();
    }

    [Test]
    public void DataContractJsonSerializationTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.RowNumber);
      var query = Session.Query.All<MediaType>()
        .Where(c => c.Name == "MPEG audio file")
        .Where(c => c.MediaTypeId > 0)
        .Take(5)
        .Skip(0);

      var serializableExpression = query.Expression.ToSerializableExpression();
      var serializedExpression = Cloner.CloneViaJsonSerialization(serializableExpression, useDataContract: true);

      var deserializedExpression = serializedExpression.ToExpression();
      var deserializedQuery = new Queryable<MediaType>((QueryProvider) query.Provider, deserializedExpression);
      var result = deserializedQuery.ToList();
    }

    [Test]
    public void JsonSerializerSerializationTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.RowNumber);
      var query = Session.Query.All<MediaType>()
        .Where(c => c.Name == "MPEG audio file")
        .Where(c => c.MediaTypeId > 0)
        .Take(5)
        .Skip(0);

      var serializableExpression = query.Expression.ToSerializableExpression();
      var serializedExpression = Cloner.CloneViaJsonSerialization(serializableExpression);

      var deserializedExpression = serializedExpression.ToExpression();
      var deserializedQuery = new Queryable<MediaType>((QueryProvider) query.Provider, deserializedExpression);
      var result = deserializedQuery.ToList();
    }
  }
}