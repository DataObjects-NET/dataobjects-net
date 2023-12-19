// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.13

using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Linq.SerializableExpressions;
using System.Collections.Generic;

namespace Xtensive.Orm.Tests.Core.Linq
{
  [TestFixture]
  public class SerializableExpressionsTest : ExpressionTestBase
  {
    public IEnumerable<Type> SerializableExpressionTypes
    {
      get {
        yield return typeof(Xtensive.Linq.SerializableExpressions.SerializableBinaryExpression);
        yield return typeof(Xtensive.Linq.SerializableExpressions.SerializableConditionalExpression);
        yield return typeof(Xtensive.Linq.SerializableExpressions.SerializableConstantExpression);
        yield return typeof(Xtensive.Linq.SerializableExpressions.SerializableElementInit);
        yield return typeof(Xtensive.Linq.SerializableExpressions.SerializableExpression);
        yield return typeof(Xtensive.Linq.SerializableExpressions.SerializableInvocationExpression);
        yield return typeof(Xtensive.Linq.SerializableExpressions.SerializableLambdaExpression);
        yield return typeof(Xtensive.Linq.SerializableExpressions.SerializableListInitExpression);
        yield return typeof(Xtensive.Linq.SerializableExpressions.SerializableMemberAssignment);
        yield return typeof(Xtensive.Linq.SerializableExpressions.SerializableMemberBinding);
        yield return typeof(Xtensive.Linq.SerializableExpressions.SerializableMemberExpression);
        yield return typeof(Xtensive.Linq.SerializableExpressions.SerializableMemberInitExpression);
        yield return typeof(Xtensive.Linq.SerializableExpressions.SerializableMemberListBinding);
        yield return typeof(Xtensive.Linq.SerializableExpressions.SerializableMemberMemberBinding);
        yield return typeof(Xtensive.Linq.SerializableExpressions.SerializableMethodCallExpression);
        yield return typeof(Xtensive.Linq.SerializableExpressions.SerializableNewArrayExpression);
        yield return typeof(Xtensive.Linq.SerializableExpressions.SerializableNewExpression);
        yield return typeof(Xtensive.Linq.SerializableExpressions.SerializableParameterExpression);
        yield return typeof(Xtensive.Linq.SerializableExpressions.SerializableTypeBinaryExpression);
        yield return typeof(Xtensive.Linq.SerializableExpressions.SerializableUnaryExpression);
      }
    }

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
      var list = SerializableExpressionTypes.ToList();
      var settings = new DataContractSerializerSettings();
      settings.KnownTypes = list;
      settings.PreserveObjectReferences = true;
      RunSerializeTest(new DataContractSerializer(typeof(SerializableExpression), settings));
    }

    private void RunSerializeTest(XmlObjectSerializer serializer)
    {
      using (var stream = new MemoryStream()) {
        foreach (var expression in Expressions) {
          Console.WriteLine(expression.ToString(true));
          serializer.WriteObject(stream, expression.ToSerializableExpression());
          stream.Seek(0, SeekOrigin.Begin);
          var serialized = (SerializableExpression) serializer.ReadObject(stream);
          stream.SetLength(0);
          Assert.AreEqual(expression.ToExpressionTree(), serialized.ToExpression().ToExpressionTree());
          Console.WriteLine("OK");
        }
      }
    }

    private void RunSerializeTest(IFormatter serializer)
    {
      using (var stream = new MemoryStream()) {
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

    #region Performance test

    private const int warmUpOperationCount = 10;
    private const int actualOperationCount = 10000;

    [Test]
    [Category("Performance")]
    [Explicit]
    public void SerializeBenchmarkTest()
    {
      var list = SerializableExpressionTypes.ToList();
      var settings = new DataContractSerializerSettings();
      settings.KnownTypes = list;
      settings.PreserveObjectReferences = true;

      RunSerializeBenchmark(new DataContractSerializer(typeof (SerializableExpression), settings), true);
      RunSerializeBenchmark(new DataContractSerializer(typeof (SerializableExpression), settings), false);
    }

    private void RunSerializeBenchmark(XmlObjectSerializer serializer, bool warmUp)
    {
      int operationCount = warmUp ? warmUpOperationCount : actualOperationCount;
      using (var stream = new MemoryStream()) {
        int operation = 0;
        long length = 0;
        using (CreateMeasurement(warmUp, serializer.GetType().Name, operationCount))
          while (operation < operationCount) {
            foreach (var expression in Expressions) {
              operation++;
              if (operation > operationCount)
                break;
              serializer.WriteObject(stream, expression.ToSerializableExpression());
              length += stream.Position;
              stream.Seek(0, SeekOrigin.Begin);
              var serialized = (SerializableExpression) serializer.ReadObject(stream);
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