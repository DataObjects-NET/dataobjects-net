// Copyright (C) 2003-2010 Xtensive LLC.
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
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Diagnostics;
using Xtensive.Linq.SerializableExpressions;

namespace Xtensive.Orm.Tests.Core.Linq
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

    [Test]
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
      RunSerializeBenchmark(new NetDataContractSerializer(), true);
      RunSerializeBenchmark(new NetDataContractSerializer(), false);
    }

    private void RunSerializeBenchmark(IFormatter serializer, bool warmUp)
    {
      int operationCount = warmUp ? warmUpOperationCount : actualOperationCount;
      var stream = new MemoryStream();
      int operation = 0;
      long length = 0;
      using (CreateMeasurement(warmUp, serializer.GetType().Name, operationCount))
        while (operation < operationCount) {
          foreach (var expression in Expressions) {
            operation++;
            if (operation > operationCount)
              break;
            serializer.Serialize(stream, expression.ToSerializableExpression());
            length += stream.Position;
            stream.Seek(0, SeekOrigin.Begin);
            var serialized = (SerializableExpression)serializer.Deserialize(stream);
            stream.SetLength(0);
          }
        }
//        for (int i = 0; i < operationCount; i++) {
//          serializer.Serialize(stream, Expressions[expressionIndex].ToSerializableExpression());
//          length += stream.Length;
//          stream.SetLength(0);
//        }
      Console.Out.WriteLine("Stream size: {0} Kb", length / 1024);
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