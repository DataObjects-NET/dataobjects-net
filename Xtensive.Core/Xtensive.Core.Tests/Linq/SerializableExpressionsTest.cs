// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.13

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using Xtensive.Core.Linq;
using Xtensive.Core.Linq.SerializableExpressions;

namespace Xtensive.Core.Tests.Linq
{
  [TestFixture]
  public class SerializableExpressionsTest : ExpressionTestBase
  {
    [Test]
    public void ConvertTest()
    {
      foreach (var origin in Expressions) {
        Console.WriteLine(origin.ToString(true));
        var converted = origin.ToSerializableExpression().ToExpression();
        Assert.AreEqual(origin.ToExpressionTree(), converted.ToExpressionTree());
        Console.WriteLine("OK");
      }
    }

    [Test]
    public void BinaryFormatterSerializeTest()
    {
      RunSerializeTest(new BinaryFormatter());
    }

    [Test]
    public void NetDataContractSerializeTest()
    {
      RunSerializeTest(new NetDataContractSerializer());
    }

    private void RunSerializeTest(IFormatter serializer)
    {
      var stream = new MemoryStream();
      foreach (var expression in Expressions) {
        Console.WriteLine(expression.ToString(true));
        serializer.Serialize(stream, expression.ToSerializableExpression());
        stream.Seek(0, SeekOrigin.Begin);
        var serialized = (SerializableExpression) serializer.Deserialize(stream);
        stream.SetLength(0);
        Assert.AreEqual(expression.ToExpressionTree(), serialized.ToExpression().ToExpressionTree());
        Console.WriteLine("OK");
      }
    }
  }
}