// Copyright (C) 2010-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2010.02.25

using System;
using System.Linq;
using TestCommon.Model;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.SerializableExpressions.Tests
{
  public class SerializedQueryTest : TestCommon.CommonModelTest
  {
    public enum SerializerType
    {
      Json,
      DataContractJson,
      DataContractXml
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        _ = new Bar(session) { Name = "Bar #1", Count = -2 };
        _ = new Bar(session) { Name = "Bar #1", Count = -1 };
        _ = new Bar(session) { Name = "Bar #1", Count = 1 };
        _ = new Bar(session) { Name = "Bar #1", Count = 1 };
        _ = new Bar(session) { Name = "Bar #1", Count = 1 };
        _ = new Bar(session) { Name = "Bar #2", Count = 1 };
        _ = new Bar(session) { Name = "Bar #3", Count = 1 };
        tx.Complete();
      }
    }

    [Test]
    [TestCase(SerializerType.Json)]
    [TestCase(SerializerType.DataContractJson)]
    [TestCase(SerializerType.DataContractXml)]
    public void MainTest(SerializerType serializerType)
    {
      Require.AllFeaturesSupported(ProviderFeatures.RowNumber);

      int[] ids;
      SerializableExpression serializedExpression;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var query = session.Query.All<Bar>()
        .Where(c => c.Name == "Bar #1")
        .Where(c => c.Count > 0)
        .Take(5)
        .Skip(0);

        var serializableExpression = query.Expression.ToSerializableExpression();
        serializedExpression = CloneExpression(serializableExpression, serializerType);

        ids = query.AsEnumerable().Select(b => b.Id).ToArray();
        Assert.That(ids.Length, Is.GreaterThan(2));
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var deserializedExpression = serializedExpression.ToExpression();
        var deserializedQuery = new Queryable<Bar>(session.Query.Provider, deserializedExpression);
        var result = deserializedQuery.AsEnumerable().Select(b => b.Id).ToArray();

        Assert.That(result.Length, Is.EqualTo(ids.Length));
        Assert.That(result.SequenceEqual(ids), Is.True);
      }
    }


    private SerializableExpression CloneExpression(SerializableExpression serializableExpression, SerializerType serializerType)
    {
      return serializerType switch {
        SerializerType.Json => Cloner.CloneViaJsonSerialization(serializableExpression),
        SerializerType.DataContractJson => Cloner.CloneViaJsonSerialization(serializableExpression, useDataContract: true),
        SerializerType.DataContractXml => Cloner.CloneViaXmlSerialization(serializableExpression, Cloner.SerializationDomains.System.Value),
        _ => throw new NotImplementedException()
      };
    }
  }
}
