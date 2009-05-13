// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.13

using System;
using System.IO;
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
    public void SerializeTest()
    {
      var stream = new MemoryStream();
      var formatter = new BinaryFormatter();
      foreach (var expression in Expressions) {
        Console.WriteLine(expression.ToString(true));
        var origin = expression.ToSerializableExpression();
        formatter.Serialize(stream, origin);
        stream.Seek(0, SeekOrigin.Begin);
        var serialized = (SerializableExpression) formatter.Deserialize(stream);
        stream.SetLength(0);
        Assert.AreEqual(origin.ToExpression().ToExpressionTree(), serialized.ToExpression().ToExpressionTree());
        Console.WriteLine("OK");
      }
    }
  }
}