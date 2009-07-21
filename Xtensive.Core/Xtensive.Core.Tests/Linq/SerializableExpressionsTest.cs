// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.13

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using NUnit.Framework;
using Xtensive.Core.Diagnostics;
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

    [Test, Ignore("Bug in formatter")]
    public void BinaryFormatterSerializeTest()
    {
      RunSerializeTest(new BinaryFormatter());
    }

    [Test]
    public void NetDataContractSerializeTest()
    {
      RunSerializeTest(new NetDataContractSerializer());
    }

    [Test, Ignore("Bug in formatter")]
    public void SoapSerializeTest()
    {
      RunSerializeTest(new SoapFormatter());
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

    #region Performance test

    private const int warmUpOperationCount = 10;
    private const int actualOperationCount = 10000;

    [Test]
    [Category("Performance")]
    [Explicit]
    public void SerializeBenchmarkTest()
    {
      RunSerializeBenchmark(new NetDataContractSerializer(), true, 1);
      RunSerializeBenchmark(new NetDataContractSerializer(), false, 1);
    }

    private void RunSerializeBenchmark(IFormatter serializer, bool warmUp, int expressionIndex)
    {
      int operationCount = warmUp ? warmUpOperationCount : actualOperationCount;
      var stream = new MemoryStream();
      using (CreateMeasurement(warmUp, serializer.GetType().Name, operationCount))
        for (int i = 0; i < operationCount; i++) {
          serializer.Serialize(stream, Expressions[expressionIndex].ToSerializableExpression());
          stream.SetLength(0);
        }
    }

    private static IDisposable CreateMeasurement(bool warmUp, string name, int operationCount)
    {
      return warmUp
        ? new Measurement(name, MeasurementOptions.None, operationCount)
        : new Measurement(name, operationCount);
    }

    #endregion
  }
}