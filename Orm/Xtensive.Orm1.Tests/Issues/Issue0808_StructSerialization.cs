﻿using System;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Linq.SerializableExpressions;

namespace Xtensive.Orm.Tests.Issues
{
  public struct UnifiedCustomerID
  {
    public int Id;
  }

[TestFixture]
public class Issue0808_StructSerialization
{
  [Test]
  public void MainTest()
  {
    var t = typeof (UnifiedCustomerID);
    Expression<Func<int, UnifiedCustomerID>> ex = a => new UnifiedCustomerID {Id = 2};
    var serializableExpression = ex.ToSerializableExpression();
    var serializer = new NetDataContractSerializer();
    var memoryStream = new MemoryStream();
    serializer.Serialize(memoryStream, serializableExpression);
    memoryStream.Position = 0;
    var deserializedExpression = (SerializableExpression) serializer.Deserialize(memoryStream);
    var ex2 = deserializedExpression.ToExpression();
  }
}

}
