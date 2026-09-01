// Copyright (C) 2009-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.05.13

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.SerializableExpressions.Tests
{
  [TestFixture]
  public class SerializableExpressionsTest
  {
    #region Nested Types
    private struct Struct
    {
      public int Id;
    }

    private class Helper
    {
      public int Field;

      public Helper NestedClass;

      public List<int> NestedCollection;

      public int InstanceGenericMethod<T>(int value)
      {
        return value + typeof(T).Name.Length + +GetHashCode();
      }

      public static int StaticGenericMethod<T>(int value)
      {
        return value + typeof(T).GetHashCode();
      }
    }

    [DataContract]
    internal class Foo
    {
      [JsonInclude, DataMember]
      public int IntField;

      [JsonInclude, DataMember]
      public int IntProperty { get; set; }

      [JsonInclude, DataMember]
      public List<string> ListProperty { get; set; }

      public Foo() { }
    }

    [DataContract]
    internal class Address
    {
      [JsonInclude, DataMember]
      public string City { get; set; }
      [JsonInclude, DataMember]
      public string ZipCode { get; set; }

      public override bool Equals(object obj) => Equals((Address) obj);

      public bool Equals(Address other)
      {
        if (other is null)
          return false;
        return City == other.City && ZipCode == other.ZipCode;
      }

      public static bool operator ==(Address a, Address b)
      {
        if (!ReferenceEquals(a, null) && !ReferenceEquals(b, null)) {
          return a.Equals(b);
        }
        return ReferenceEquals(a, b);
      }

      public static bool operator !=(Address a, Address b)
      {
        return !a.Equals(b);
      }

      public Address() { }

      public Address(string city, string zipCode)
      {
        City = city;
        ZipCode = zipCode;
      }
    }

    [DataContract]
    internal class Person
    {
      [JsonInclude, DataMember]
      public Address HomeAddress { get; set; }
      [JsonInclude, DataMember]
      public string Name { get; set; }

      public override bool Equals(object obj) => Equals((Person) obj);

      public bool Equals(Person other)
      {
        return HomeAddress == other.HomeAddress && Name == other.Name;
      }

      public static bool operator ==(Person a, Person b)
      {
        if (!ReferenceEquals(a, null) && !ReferenceEquals(b, null)) {
          return a.Equals(b);
        }
        return ReferenceEquals(a, b);
      }

      public static bool operator !=(Person a, Person b)
      {
        return !a.Equals(b);
      }
    }

    public enum ByteEnum : byte
    {
      None = 0,
      One, Two,
    }

    public enum ShortEnum : short
    {
      None = 10,
      One, Two,
    }

    public enum IntEnum : int
    {
      None = 100,
      One, Two,
    }

    public enum LongEnum : long
    {
      None = 1000,
      One, Two,
    }
    #endregion

    public static readonly IReadOnlyList<Type> SystemTypes = [
      typeof(DateOnly),       typeof(DateOnly?),       typeof(DateOnly[]),       typeof(DateOnly?[]),
      typeof(TimeOnly),       typeof(TimeOnly?),       typeof(TimeOnly[]),       typeof(TimeOnly?[]),
      typeof(DateTime),       typeof(DateTime?),       typeof(DateTime[]),       typeof(DateTime?[]),
      typeof(TimeSpan),       typeof(TimeSpan?),       typeof(TimeSpan[]),       typeof(TimeSpan?[]),
      typeof(DateTimeOffset), typeof(DateTimeOffset?), typeof(DateTimeOffset[]), typeof(DateTimeOffset?[]),
      typeof(Type),            typeof(Type[]),
      typeof(MethodInfo),      typeof(MethodInfo[]),
      typeof(MemberInfo),      typeof(MemberInfo[]),
      typeof(ConstructorInfo), typeof(ConstructorInfo[]),
      typeof(bool?),   typeof(bool[]),   typeof(bool?[]),
      typeof(byte?),   typeof(byte[]),   typeof(byte?[]),
      typeof(sbyte?),  typeof(sbyte[]),  typeof(sbyte?[]),
      typeof(short?),  typeof(short[]),  typeof(short?[]),
      typeof(ushort?), typeof(ushort[]), typeof(ushort?[]),
      typeof(int?),     typeof(int[]),     typeof(int?[]),
      typeof(uint?),    typeof(uint[]),    typeof(uint?[]),
      typeof(long?),    typeof(long[]),    typeof(long?[]),
      typeof(ulong?),   typeof(ulong[]),   typeof(ulong?[]),
      typeof(Int128),   typeof(Int128?),   typeof(Int128[]),    typeof(Int128?[]),
      typeof(UInt128),  typeof(UInt128?),  typeof(UInt128[]),   typeof(UInt128?[]),
      typeof(float?),   typeof(float[]),   typeof(float?[]),
      typeof(double?),  typeof(double[]),  typeof(double?[]),
      typeof(decimal?), typeof(decimal[]), typeof(decimal?[]),
      typeof(string[]),
      ];

    public LambdaExpression[] LambdaExpressions { get; private set; }

    #region Additional Test Data

    public ConstantExpression[] ConstantExpressions { get; private set; }
    public DefaultExpression[] DefaultExpressions { get; private set; }
    public ParameterExpression[] ParameterExpressions { get; private set; }
    public ParameterExpression[] VariableExpressions { get; private set; }
    public BinaryExpression[] BinaryExpressions { get; private set; }
    public ConditionalExpression[] ConditionalExpressions { get; private set; }
    public InvocationExpression[] InvocationExpressions { get; private set; }
    public ListInitExpression[] ListInitExpressions { get; private set; }
    public MemberExpression[] MemberExpressions { get; private set; }
    public MemberInitExpression[] MemberInitExpressions { get; private set; }
    public MethodCallExpression[] MethodCallExpressions { get; private set; }
    public NewArrayExpression[] NewArrayExpressions { get; private set; }
    public TypeBinaryExpression[] TypeBinaryExpressions { get; private set; }
    public UnaryExpression[] UnaryExpressions { get; private set; }
    public NewExpression[] NewExpressions { get; private set; }

    #endregion

    [OneTimeSetUp]
    public void TestFixtureSetUp()
    {
      LambdaExpressions = GetTestLambdaExpressions();

      ConstantExpressions = GetTestConstantExpressions();
      DefaultExpressions = GetTestDefaultExpressions();
      ParameterExpressions = GetTestParameterExpressions();
      VariableExpressions = GetTestVariableExpressions();
      BinaryExpressions = GetTestBinaryExpressions();
      ConditionalExpressions = GetTestConditionalExpressions();
      InvocationExpressions = GetTestInvocationExpressions();
      ListInitExpressions = GetTestListInitExpressions();
      MemberExpressions = GetTestMemberExpressions();
      NewExpressions = GetTestNewExpressions();
      MemberInitExpressions = GetTestMemberInitExressions();
      MethodCallExpressions = GetTestMethodCallExpressions();
      NewArrayExpressions = GetTestNewArrayExpressions();
      TypeBinaryExpressions = GetTestTypeBinaryExpressions();
      UnaryExpressions = GetTestUnaryExpressions();
    }


    [Test]
    public void ConstantExpressionTest()
    {
      foreach (var origin in ConstantExpressions) {
        Console.WriteLine(origin.ToString(true));
        var converted = origin.ToSerializableExpression().ToExpression();
        Assert.That(converted.ToExpressionTree(), Is.EqualTo(origin.ToExpressionTree()));
        Console.WriteLine("OK");
      }
    }

    [Test]
    public void ConstantExpressionDataContractCloneTest()
    {
      var settings = new DataContractSerializerSettings {
        KnownTypes = SystemTypes
          .Union(new[] { typeof(ByteEnum), typeof(ByteEnum?), typeof(ByteEnum[]), typeof(ByteEnum?[]),
            typeof(ShortEnum), typeof(ShortEnum?), typeof(ShortEnum[]), typeof(ShortEnum?[]),
            typeof(IntEnum), typeof(IntEnum?), typeof(IntEnum[]), typeof(IntEnum?[]),
            typeof(LongEnum), typeof(LongEnum?), typeof(LongEnum[]), typeof(LongEnum?[]),
            typeof(Person), typeof(Person[]),
            typeof(Address), typeof(Address[])
          }),
        PreserveObjectReferences = true
      };
      RunDataContractTest(new DataContractSerializer(typeof(SerializableExpression), settings), ConstantExpressions);
    }

    [Test]
    public void ConstantExpressionDataContractJsonCloneTest()
    {
      var settings = new DataContractJsonSerializerSettings {
        KnownTypes = SystemTypes
          .Union(new[] { typeof(ByteEnum), typeof(ByteEnum?), typeof(ByteEnum[]), typeof(ByteEnum?[]),
            typeof(ShortEnum), typeof(ShortEnum?), typeof(ShortEnum[]), typeof(ShortEnum?[]),
            typeof(IntEnum), typeof(IntEnum?), typeof(IntEnum[]), typeof(IntEnum?[]),
            typeof(LongEnum), typeof(LongEnum?), typeof(LongEnum[]), typeof(LongEnum?[]),
            typeof(Person), typeof(Person[]),
            typeof(Address), typeof(Address[])
          })
      };
      RunDataContractTest(new DataContractJsonSerializer(typeof(SerializableExpression), settings), ConstantExpressions);
    }

    [Test]
    public void ConstantExpressionJsonSerializerCloneTest()
    {
      var options = new JsonSerializerOptions { WriteIndented = true };
      RunJsonSerializerTest(ConstantExpressions, options);
    }

    [Test]
    public void DefaultExpressionTest()
    {
      foreach (var origin in DefaultExpressions) {
        Console.WriteLine(origin.ToString(true));
        var converted = origin.ToSerializableExpression().ToExpression();
        Assert.That(converted.ToExpressionTree(), Is.EqualTo(origin.ToExpressionTree()));
        Console.WriteLine("OK");
      }
    }

    [Test]
    public void DefaultExpressionDataContractCloneTest()
    {
      var settings = new DataContractSerializerSettings {
        KnownTypes = SystemTypes
          .Union(new[] { typeof(ByteEnum), typeof(ByteEnum?), typeof(ByteEnum[]), typeof(ByteEnum?[]),
            typeof(ShortEnum), typeof(ShortEnum?), typeof(ShortEnum[]), typeof(ShortEnum?[]),
            typeof(IntEnum), typeof(IntEnum?), typeof(IntEnum[]), typeof(IntEnum?[]),
            typeof(LongEnum), typeof(LongEnum?), typeof(LongEnum[]), typeof(LongEnum?[]),
            typeof(Person), typeof(Person[]),
            typeof(Address), typeof(Address[])
          }),
        PreserveObjectReferences = true
      };
      RunDataContractTest(new DataContractSerializer(typeof(SerializableExpression), settings), DefaultExpressions);
    }

    [Test]
    public void DefaultExpressionDataContractJsonCloneTest()
    {
      var settings = new DataContractJsonSerializerSettings {
        KnownTypes = SystemTypes
          .Union(new[] { typeof(ByteEnum), typeof(ByteEnum?), typeof(ByteEnum[]), typeof(ByteEnum?[]),
            typeof(ShortEnum), typeof(ShortEnum?), typeof(ShortEnum[]), typeof(ShortEnum?[]),
            typeof(IntEnum), typeof(IntEnum?), typeof(IntEnum[]), typeof(IntEnum?[]),
            typeof(LongEnum), typeof(LongEnum?), typeof(LongEnum[]), typeof(LongEnum?[]),
            typeof(Person), typeof(Person[]),
            typeof(Address), typeof(Address[])
          })
      };
      RunDataContractTest(new DataContractJsonSerializer(typeof(SerializableExpression), settings), DefaultExpressions);
    }

    [Test]
    public void DefaultExpressionJsonSerializerCloneTest()
    {
      var options = new JsonSerializerOptions { WriteIndented = true };
      RunJsonSerializerTest(DefaultExpressions, options);
    }

    [Test]
    public void ParameterExpressionTest()
    {
      foreach (var origin in ParameterExpressions) {
        Console.WriteLine(origin.ToString(true));
        var converted = origin.ToSerializableExpression().ToExpression();
        Assert.That(converted.ToExpressionTree(), Is.EqualTo(origin.ToExpressionTree()));
        Console.WriteLine("OK");
      }

      foreach (var origin in VariableExpressions) {
        Console.WriteLine(origin.ToString(true));
        var converted = origin.ToSerializableExpression().ToExpression();
        Assert.That(converted.ToExpressionTree(), Is.EqualTo(origin.ToExpressionTree()));
        Console.WriteLine("OK");
      }
    }

    [Test]
    public void ParameterExpressionDataContractCloneTest()
    {
      var settings = new DataContractSerializerSettings {
        KnownTypes = SystemTypes,
        PreserveObjectReferences = true
      };
      RunDataContractTest(new DataContractSerializer(typeof(SerializableExpression), settings), ParameterExpressions);
      RunDataContractTest(new DataContractSerializer(typeof(SerializableExpression), settings), VariableExpressions);
    }

    [Test]
    public void ParameterExpressionDataContractJsonCloneTest()
    {
      var settings = new DataContractJsonSerializerSettings { KnownTypes = SystemTypes };
      RunDataContractTest(new DataContractJsonSerializer(typeof(SerializableExpression), settings), ParameterExpressions);
      RunDataContractTest(new DataContractJsonSerializer(typeof(SerializableExpression), settings), VariableExpressions);
    }

    [Test]
    public void ParameterExpressionJsonSerializerCloneTest()
    {
      var options = new JsonSerializerOptions { WriteIndented = true };
      RunJsonSerializerTest(ParameterExpressions, options);
      RunJsonSerializerTest(VariableExpressions, options);
    }

    [Test]
    public void BinaryExpressionTest()
    {
      foreach (var origin in BinaryExpressions) {
        Console.WriteLine(origin.ToString(true));
        var converted = origin.ToSerializableExpression().ToExpression();
        Assert.That(converted.ToExpressionTree(), Is.EqualTo(origin.ToExpressionTree()));
        Console.WriteLine("OK");
      }
    }

    [Test]
    public void BinaryExpressionDataContractCloneTest()
    {
      var settings = new DataContractSerializerSettings {
        KnownTypes = SystemTypes,
        PreserveObjectReferences = true
      };
      RunDataContractTest(new DataContractSerializer(typeof(SerializableExpression), settings), BinaryExpressions);
    }

    [Test]
    public void BinaryExpressionDataContractJsonCloneTest()
    {
      var settings = new DataContractJsonSerializerSettings { KnownTypes = SystemTypes };
      RunDataContractTest(new DataContractJsonSerializer(typeof(SerializableExpression), settings), BinaryExpressions);
    }

    [Test]
    public void BinaryExpressionJsonSerializerCloneTest()
    {
      var options = new JsonSerializerOptions { WriteIndented = true };
      RunJsonSerializerTest(BinaryExpressions, options);
    }

    [Test]
    public void ConditionalExpressionTest()
    {
      foreach (var origin in ConditionalExpressions) {
        Console.WriteLine(origin.ToString(true));
        var converted = origin.ToSerializableExpression().ToExpression();
        Assert.That(converted.ToExpressionTree(), Is.EqualTo(origin.ToExpressionTree()));
        Console.WriteLine("OK");
      }
    }

    [Test]
    public void ConditionalExpressionDataContractCloneTest()
    {
      var settings = new DataContractSerializerSettings {
        KnownTypes = SystemTypes,
        PreserveObjectReferences = true
      };
      RunDataContractTest(new DataContractSerializer(typeof(SerializableExpression), settings), ConditionalExpressions);
    }

    [Test]
    public void ConditionalExpressionDataContractJsonCloneTest()
    {
      var settings = new DataContractJsonSerializerSettings { KnownTypes = SystemTypes };
      RunDataContractTest(new DataContractJsonSerializer(typeof(SerializableExpression), settings), ConditionalExpressions);
    }

    [Test]
    public void ConditionalExpressionJsonSerializerCloneTest()
    {
      var options = new JsonSerializerOptions { WriteIndented = true };
      RunJsonSerializerTest(ConditionalExpressions, options);
    }

    [Test]
    public void ListInitExpressionTest()
    {
      foreach (var origin in ListInitExpressions) {
        Console.WriteLine(origin.ToString(true));
        var converted = origin.ToSerializableExpression().ToExpression();
        Assert.That(converted.ToExpressionTree(), Is.EqualTo(origin.ToExpressionTree()));
        Console.WriteLine("OK");
      }
    }

    [Test]
    public void ListInitExpressionDataContractCloneTest()
    {
      var settings = new DataContractSerializerSettings {
        KnownTypes = SystemTypes,
        PreserveObjectReferences = true
      };
      RunDataContractTest(new DataContractSerializer(typeof(SerializableExpression), settings), ListInitExpressions);
    }

    [Test]
    public void ListInitExpressionDataContractJsonCloneTest()
    {
      var settings = new DataContractJsonSerializerSettings { KnownTypes = SystemTypes };
      RunDataContractTest(new DataContractJsonSerializer(typeof(SerializableExpression), settings), ListInitExpressions);
    }

    [Test]
    public void ListInitExpressionJsonSerializerCloneTest()
    {
      var options = new JsonSerializerOptions { WriteIndented = true };
      RunJsonSerializerTest(ListInitExpressions, options);
    }

    [Test]
    public void MemberExpressionTest()
    {
      foreach (var origin in MemberExpressions) {
        Console.WriteLine(origin.ToString(true));
        var converted = origin.ToSerializableExpression().ToExpression();
        Assert.That(converted.ToExpressionTree(), Is.EqualTo(origin.ToExpressionTree()));
        Console.WriteLine("OK");
      }
    }

    [Test]
    public void MemberExpressionDataContractCloneTest()
    {
      var settings = new DataContractSerializerSettings {
        KnownTypes = SystemTypes.Union(new[] { typeof(Foo) }),
        PreserveObjectReferences = true
      };
      RunDataContractTest(new DataContractSerializer(typeof(SerializableExpression), settings), MemberExpressions);
    }

    [Test]
    public void MemberExpressionDataContractJsonCloneTest()
    {
      var settings = new DataContractJsonSerializerSettings { KnownTypes = SystemTypes.Union(new[] { typeof(Foo) }) };
      RunDataContractTest(new DataContractJsonSerializer(typeof(SerializableExpression), settings), MemberExpressions);
    }

    [Test]
    public void MemberExpressionJsonSerializerCloneTest()
    {
      var options = new JsonSerializerOptions { WriteIndented = true, ReferenceHandler = ReferenceHandler.Preserve };
      RunJsonSerializerTest(MemberExpressions, options);
    }

    [Test]
    public void NewExpressionTest()
    {
      foreach (var origin in NewExpressions) {
        Console.WriteLine(origin.ToString(true));
        var converted = origin.ToSerializableExpression().ToExpression();
        Assert.That(converted.ToExpressionTree(), Is.EqualTo(origin.ToExpressionTree()));
        Console.WriteLine("OK");
      }
    }

    [Test]
    public void NewExpressionDataContractCloneTest()
    {
      var settings = new DataContractSerializerSettings {
        KnownTypes = SystemTypes,
        PreserveObjectReferences = true
      };
      RunDataContractTest(new DataContractSerializer(typeof(SerializableExpression), settings), NewExpressions);
    }

    [Test]
    public void NewExpressionDataContractJsonCloneTest()
    {
      var settings = new DataContractJsonSerializerSettings { KnownTypes = SystemTypes };
      RunDataContractTest(new DataContractJsonSerializer(typeof(SerializableExpression), settings), NewExpressions);
    }

    [Test]
    public void NewExpressionJsonSerializerCloneTest()
    {
      var options = new JsonSerializerOptions { WriteIndented = true };
      RunJsonSerializerTest(NewExpressions, options);
    }

    [Test]
    public void MemberInitExpressionTest()
    {
      foreach (var origin in MemberInitExpressions) {
        Console.WriteLine(origin.ToString(true));
        var converted = origin.ToSerializableExpression().ToExpression();
        Assert.That(converted.ToExpressionTree(), Is.EqualTo(origin.ToExpressionTree()));
        Console.WriteLine("OK");
      }
    }

    [Test]
    public void MemberInitExpressionDataContractCloneTest()
    {
      var settings = new DataContractSerializerSettings {
        KnownTypes = SystemTypes,
        PreserveObjectReferences = true
      };
      RunDataContractTest(new DataContractSerializer(typeof(SerializableExpression), settings), MemberInitExpressions);
    }

    [Test]
    public void MemberInitExpressionDataContractJsonCloneTest()
    {
      var settings = new DataContractJsonSerializerSettings { KnownTypes = SystemTypes };
      RunDataContractTest(new DataContractJsonSerializer(typeof(SerializableExpression), settings), MemberInitExpressions);
    }

    [Test]
    public void MemberInitExpressionJsonSerializerCloneTest()
    {
      var options = new JsonSerializerOptions { WriteIndented = true };
      RunJsonSerializerTest(MemberInitExpressions, options);
    }

    [Test]
    public void MethodCallExpressionTest()
    {
      foreach (var origin in MethodCallExpressions) {
        Console.WriteLine(origin.ToString(true));
        var converted = origin.ToSerializableExpression().ToExpression();
        Assert.That(converted.ToExpressionTree(), Is.EqualTo(origin.ToExpressionTree()));
        Console.WriteLine("OK");
      }
    }

    [Test]
    public void MethodCallExpressionDataContractCloneTest()
    {
      var settings = new DataContractSerializerSettings {
        KnownTypes = SystemTypes,
        PreserveObjectReferences = true
      };
      RunDataContractTest(new DataContractSerializer(typeof(SerializableExpression), settings), MethodCallExpressions);
    }

    [Test]
    public void MethodCallExpressionDataContractJsonCloneTest()
    {
      var settings = new DataContractJsonSerializerSettings { KnownTypes = SystemTypes };
      RunDataContractTest(new DataContractJsonSerializer(typeof(SerializableExpression), settings), MethodCallExpressions);
    }

    [Test]
    public void MethodCallExpressionJsonSerializerCloneTest()
    {
      var options = new JsonSerializerOptions { WriteIndented = true };
      RunJsonSerializerTest(MethodCallExpressions, options);
    }

    [Test]
    public void NewArrayExpressionTest()
    {
      foreach (var origin in NewArrayExpressions) {
        Console.WriteLine(origin.ToString(true));
        var converted = origin.ToSerializableExpression().ToExpression();
        Assert.That(converted.ToExpressionTree(), Is.EqualTo(origin.ToExpressionTree()));
        Console.WriteLine("OK");
      }
    }

    [Test]
    public void NewArrayExpressionDataContractCloneTest()
    {
      var settings = new DataContractSerializerSettings {
        KnownTypes = SystemTypes,
        PreserveObjectReferences = true
      };
      RunDataContractTest(new DataContractSerializer(typeof(SerializableExpression), settings), NewArrayExpressions);
    }

    [Test]
    public void NewArrayExpressionDataContractJsonCloneTest()
    {
      var settings = new DataContractJsonSerializerSettings { KnownTypes = SystemTypes };
      RunDataContractTest(new DataContractJsonSerializer(typeof(SerializableExpression), settings), NewArrayExpressions);
    }

    [Test]
    public void NewArrayExpressionJsonSerializerCloneTest()
    {
      var options = new JsonSerializerOptions { WriteIndented = true };
      RunJsonSerializerTest(NewArrayExpressions, options);
    }

    [Test]
    public void TypeBinaryExpressionTest()
    {
      foreach (var origin in TypeBinaryExpressions) {
        Console.WriteLine(origin.ToString(true));
        var converted = origin.ToSerializableExpression().ToExpression();
        Assert.That(converted.ToExpressionTree(), Is.EqualTo(origin.ToExpressionTree()));
        Console.WriteLine("OK");
      }
    }

    [Test]
    public void TypeBinaryExpressionDataContractCloneTest()
    {
      var settings = new DataContractSerializerSettings {
        KnownTypes = SystemTypes,
        PreserveObjectReferences = true
      };
      RunDataContractTest(new DataContractSerializer(typeof(SerializableExpression), settings), TypeBinaryExpressions);
    }

    [Test]
    public void TypeBinaryExpressionDataContractJsonCloneTest()
    {
      var settings = new DataContractJsonSerializerSettings { KnownTypes = SystemTypes };
      RunDataContractTest(new DataContractJsonSerializer(typeof(SerializableExpression), settings), TypeBinaryExpressions);
    }

    [Test]
    public void TypeBinaryExpressionJsonSerializerCloneTest()
    {
      var options = new JsonSerializerOptions { WriteIndented = true };
      RunJsonSerializerTest(TypeBinaryExpressions, options);
    }

    [Test]
    public void UnaryExpressionsTest()
    {
      foreach (var origin in UnaryExpressions) {
        Console.WriteLine(origin.ToString(true));
        var converted = origin.ToSerializableExpression().ToExpression();
        Assert.That(converted.ToExpressionTree(), Is.EqualTo(origin.ToExpressionTree()));
        Console.WriteLine("OK");
      }
    }

    [Test]
    public void UnaryExpressionDataContractCloneTest()
    {
      var settings = new DataContractSerializerSettings {
        KnownTypes = SystemTypes,
        PreserveObjectReferences = true
      };
      RunDataContractTest(new DataContractSerializer(typeof(SerializableExpression), settings), UnaryExpressions);
    }

    [Test]
    public void UnaryExpressionDataContractJsonCloneTest()
    {
      var settings = new DataContractJsonSerializerSettings { KnownTypes = SystemTypes };
      RunDataContractTest(new DataContractJsonSerializer(typeof(SerializableExpression), settings), UnaryExpressions);
    }

    [Test]
    public void UnaryExpressionJsonSerializerCloneTest()
    {
      var options = new JsonSerializerOptions { WriteIndented = true };
      RunJsonSerializerTest(UnaryExpressions, options);
    }

    [Test]
    public void InvocationExpressionTest()
    {
      foreach (var origin in InvocationExpressions) {
        Console.WriteLine(origin.ToString(true));
        var converted = origin.ToSerializableExpression().ToExpression();
        Assert.That(converted.ToExpressionTree(), Is.EqualTo(origin.ToExpressionTree()));
        Console.WriteLine("OK");
      }
    }

    [Test]
    public void InvocationExpressionDataContractCloneTest()
    {
      var settings = new DataContractSerializerSettings {
        KnownTypes = SystemTypes,
        PreserveObjectReferences = true
      };
      RunDataContractTest(new DataContractSerializer(typeof(SerializableExpression), settings), InvocationExpressions);

      settings = new DataContractSerializerSettings {
        KnownTypes = SystemTypes,
        PreserveObjectReferences = false
      };
      RunDataContractTest(new DataContractSerializer(typeof(SerializableExpression), settings), InvocationExpressions);
    }

    [Test]
    public void InvocationExpressionDataContractJsonCloneTest()
    {
      var settings = new DataContractJsonSerializerSettings { KnownTypes = SystemTypes };
      RunDataContractTest(new DataContractJsonSerializer(typeof(SerializableExpression), settings), InvocationExpressions);
    }

    [Test]
    public void InvocationExpressionJsonSerializerCloneTest()
    {
      var options = new JsonSerializerOptions { WriteIndented = true };
      RunJsonSerializerTest(InvocationExpressions, options);

      options = new JsonSerializerOptions { WriteIndented = true, ReferenceHandler = ReferenceHandler.Preserve };
      RunJsonSerializerTest(InvocationExpressions, options);
    }

    [Test]
    public void LambdaExpressionTest()
    {
      foreach (var origin in LambdaExpressions) {
        Console.WriteLine(origin.ToString(true));
        var converted = origin.ToSerializableExpression().ToExpression();
        Assert.That(converted.ToExpressionTree(), Is.EqualTo(origin.ToExpressionTree()));
        Console.WriteLine("OK");
      }
    }

    [Test]
    public void LambdaExpressionDataContractCloneTest()
    {
      var settings = new DataContractSerializerSettings {
        KnownTypes = SystemTypes,
        PreserveObjectReferences = true
      };
      RunDataContractTest(new DataContractSerializer(typeof(SerializableExpression), settings), LambdaExpressions);

      settings = new DataContractSerializerSettings {
        KnownTypes = SystemTypes,
        PreserveObjectReferences = false
      };
      RunDataContractTest(new DataContractSerializer(typeof(SerializableExpression), settings), LambdaExpressions);
    }

    [Test]
    public void LambdaExpressionDataContractJsonCloneTest()
    {
      var settings = new DataContractJsonSerializerSettings { KnownTypes = SystemTypes, };
      RunDataContractTest(new DataContractJsonSerializer(typeof(SerializableExpression), settings), LambdaExpressions);
    }

    [Test]
    public void LambdaExpressionJsonSerializerCloneTest()
    {
      var options = new JsonSerializerOptions { WriteIndented = true };
      RunJsonSerializerTest(LambdaExpressions, options);

      options = new JsonSerializerOptions { WriteIndented = true, ReferenceHandler = ReferenceHandler.Preserve };
      RunJsonSerializerTest(LambdaExpressions, options);
    }

    private void RunDataContractTest(XmlObjectSerializer serializer, IEnumerable<Expression> expressions)
    {
      using (var stream = new MemoryStream()) {
        foreach (var expression in expressions) {
          Console.WriteLine(expression.ToString(true));
          serializer.WriteObject(stream, expression.ToSerializableExpression());
          _ = stream.Seek(0, SeekOrigin.Begin);
          var serialized = (SerializableExpression) serializer.ReadObject(stream);
          stream.SetLength(0);
          CompareExpressions(expression, serialized.ToExpression());
          Console.WriteLine("OK");
        }
      }
    }

    private void RunJsonSerializerTest(IEnumerable<Expression> expressions, JsonSerializerOptions options)
    {
      using (var stream = new MemoryStream()) {
        foreach (var expression in expressions) {
          Console.WriteLine(expression.ToString(true));

          var serializableExpression = expression.ToSerializableExpression();

          JsonSerializer.Serialize(stream, serializableExpression, options);
          _ = stream.Seek(0, SeekOrigin.Begin);
          var serialized = JsonSerializer.Deserialize<SerializableExpression>(stream, options);
          stream.SetLength(0);
          CompareExpressions(expression, serialized.ToExpression());
          Console.WriteLine("OK");
        }
      }
    }

    private void CompareExpressions(Expression expression, Expression serialized)
    {
      if (expression is ConstantExpression cExpressions)
        CompareConstants(cExpressions, (ConstantExpression) serialized);
      else if (expression is MemberExpression mExpression)
        CompareMemberExpressions(mExpression, (MemberExpression) serialized);
      else if (expression is BinaryExpression bExpression && bExpression.NodeType == ExpressionType.ArrayIndex)
        CompareArrayIndex(bExpression, (BinaryExpression) serialized);
      else if (expression is UnaryExpression uExpression && uExpression.NodeType == ExpressionType.ArrayLength)
        CompareArrayLength(uExpression, (UnaryExpression) serialized);
      else
        Assert.That(serialized.ToExpressionTree(), Is.EqualTo(expression.ToExpressionTree()));
    }

    private void CompareMemberExpressions(MemberExpression origin, MemberExpression clone)
    {
      Assert.That(clone.Type, Is.EqualTo(origin.Type));

      var instanceOrig = origin.Expression as ConstantExpression;
      var instanceClone = clone.Expression as ConstantExpression;

      Assert.That(instanceClone.Type, Is.EqualTo(instanceOrig.Type));

      var instValueOrig = instanceOrig.Value as Foo;
      var instValueClone = instanceClone.Value as Foo;

      Assert.That(instValueClone.IntField, Is.EqualTo(instValueOrig.IntField));
      Assert.That(instValueClone.IntProperty, Is.EqualTo(instValueOrig.IntProperty));

      Assert.That(clone.Member, Is.EqualTo(origin.Member));
    }

    private void CompareArrayIndex(BinaryExpression origin, BinaryExpression clone)
    {
      if (origin.NodeType != ExpressionType.ArrayIndex)
        throw new ArgumentException();

      Assert.That(clone.Type, Is.EqualTo(origin.Type));

      CompareConstants((ConstantExpression) origin.Left, (ConstantExpression) clone.Left);
      CompareConstants((ConstantExpression) origin.Right, (ConstantExpression) clone.Right);

    }

    private void CompareArrayLength(UnaryExpression origin, UnaryExpression clone)
    {
      if (origin.NodeType != ExpressionType.ArrayLength)
        throw new ArgumentException();

      Assert.That(clone.Type, Is.EqualTo(origin.Type));

      var instanceOrig = (origin.Operand as ConstantExpression);
      var instanceClone = (clone.Operand as ConstantExpression);

      CompareConstants(instanceOrig, instanceClone);
    }

    private void CompareConstants(ConstantExpression origin, ConstantExpression clone)
    {
      Assert.That(clone.Type, Is.EqualTo(origin.Type));

      if (origin.Type.IsArray) {
        Assert.That(origin.Value, Is.Not.Null);
        Assert.That(clone.Value, Is.Not.Null);
        Assert.That(clone.Value, Is.EquivalentTo(origin.Value as Array));
      }
      else {
        Assert.That(clone.Value, Is.EqualTo(origin.Value));
      }
    }

    #region Performance test

    private const int warmUpOperationCount = 10;
    private const int actualOperationCount = 10000;

    [Test]
    [Category("Performance")]
    [Explicit]
    public void DataContractXmlSerializationBenchmarkTest()
    {
      var settings = new DataContractSerializerSettings {
        KnownTypes = SystemTypes,
        PreserveObjectReferences = true
      };

      var serializer = new DataContractSerializer(typeof(SerializableExpression), settings);
      RunSerializeBenchmark(serializer, true);
      RunSerializeBenchmark(serializer, false);
    }

    [Test]
    [Category("Performance")]
    [Explicit]
    public void DataContractJsonSerializationBenchmarkTest()
    {
      var settings = new DataContractJsonSerializerSettings { KnownTypes = SystemTypes };

      var serializer = new DataContractJsonSerializer(typeof(SerializableExpression), settings);
      RunSerializeBenchmark(serializer, true);
      RunSerializeBenchmark(serializer, false);
    }

    [Test]
    [Category("Performance")]
    [Explicit]
    public void JsonSerializerWithReferenceSerializationBenchmarkTest()
    {
      var options = new JsonSerializerOptions() { WriteIndented = true, ReferenceHandler = ReferenceHandler.Preserve };

      RunSerializeViaJsonSerializerBenchmark(true, options);
      RunSerializeViaJsonSerializerBenchmark(false, options);
    }

    [Test]
    [Category("Performance")]
    [Explicit]
    public void JsonSerializerWithoutReferenceSerializationBenchmarkTest()
    {
      var options = new JsonSerializerOptions() { WriteIndented = true };

      RunSerializeViaJsonSerializerBenchmark(true, options);
      RunSerializeViaJsonSerializerBenchmark(false, options);
    }

    private void RunSerializeBenchmark(XmlObjectSerializer serializer, bool warmUp)
    {
      var operationCount = warmUp ? warmUpOperationCount : actualOperationCount;
      using (var stream = new MemoryStream()) {
        int operation = 0;
        long length = 0;
        using (CreateMeasurement(warmUp, serializer.GetType().Name, operationCount)) {
          while (operation < operationCount) {
            foreach (var expression in LambdaExpressions) {
              operation++;
              if (operation > operationCount)
                break;
              serializer.WriteObject(stream, expression.ToSerializableExpression());
              length += stream.Position;
              _ = stream.Seek(0, SeekOrigin.Begin);
              var serialized = (SerializableExpression) serializer.ReadObject(stream);
              stream.SetLength(0);
            }
          }
        }

        Console.Out.WriteLine($"Stream size: {length / 1024} Kb");
      }
    }

    private void RunSerializeViaJsonSerializerBenchmark(bool warmUp, JsonSerializerOptions options)
    {
      var operationCount = warmUp ? warmUpOperationCount : actualOperationCount;
      using (var stream = new MemoryStream()) {
        int operation = 0;
        long length = 0;
        using (CreateMeasurement(warmUp, "JsonSerializer", operationCount)) {
          while (operation < operationCount) {
            foreach (var expression in LambdaExpressions) {
              operation++;
              if (operation > operationCount)
                break;
              var serializableExpression = expression.ToSerializableExpression();

              JsonSerializer.Serialize(stream, serializableExpression, options);
              length += stream.Position;
              _ = stream.Seek(0, SeekOrigin.Begin);
              var serialized = JsonSerializer.Deserialize<SerializableExpression>(stream, options);
              stream.SetLength(0);
            }
          }
        }

        Console.Out.WriteLine($"Stream size: {length / 1024} Kb");
      }
    }

    private static IDisposable CreateMeasurement(bool warmUp, string name, int operationCount)
    {
      return warmUp
        ? new Measurement(name, MeasurementOptions.None, operationCount)
        : new Measurement(name, operationCount);
    }

    #endregion

    #region Test Data initializers
    private LambdaExpression[] GetTestLambdaExpressions()
    {
      return new LambdaExpression[]
        {
          // Simple expression
          (Expression<Func<int, int>>) (k => k + 1),

          // Instance method call
          (Expression<Func<object, object>>) (p => p.ToString()),

          // Static method call
          (Expression<Action<int, int>>) ((a, b) => Console.Write($"{a} + {b} = {a + b}")),

          // Instance generic method call
          (Expression<Func<Helper, int>>) (h => h.InstanceGenericMethod<long>(0)),

          // Static generic method call
          (Expression<Func<int>>) (() => Helper.StaticGenericMethod<long>(0)),

          // Instance generic method call (with generic argument being generic type)
          (Expression<Func<Helper, int>>) (h => h.InstanceGenericMethod<Func<int>>(0)),

          // Static generic method call (with generic argument being generic type)
          (Expression<Func<int>>) (() => Helper.StaticGenericMethod<Func<int>>(0)),

          // Static (extension) generic method call
          (Expression<Func<IEnumerable<Func<int>>, IEnumerable<int>>>) (funcs => funcs.Select(f => f.Invoke())),

          // Anonymous type constructor
          (Expression<Func<string, object>>) (s => new {Value = s}),

          // Static property access + binary operator
          (Expression<Func<DateTime, string>>) (d => (d - DateTime.Now).Duration().ToString()),

          // Constructor + a lots of generics
          (Expression<Func<int, List<int>>>) (i => new List<int>(i)),
          (Expression<Func<int, List<List<int>>>>) (i => new List<List<int>>(i)),
          (Expression<Func<int, List<List<List<int>>>>>) (i => new List<List<List<int>>>(i)),
          (Expression<Func<int, List<List<List<List<int>>>>>>) (i => new List<List<List<List<int>>>>(i)),

          // Static generic method call + static method call + constant expression
          (Expression<Func<long, Expression<Func<long>>>>) (x => Expression.Lambda<Func<long>>(Expression.Constant(x))),

          // Constructor
          (Expression<Func<int, int, int, DateTime>>) ((y, m, d) => new DateTime(y, m, d)),

          // List init expression
          (Expression<Func<string, List<int>>>) (s => new List<int> {s.Length}),

          // Array init expression
          (Expression<Func<Guid, Guid[]>>) (g => new[] {g}),

          // Delegate call
          (Expression<Func<int>>) (() => ((Func<int, int>) (x => x + 1)).Invoke(5)),

          // Conditional + new array + static property of generic type
          // Stupid casts are required because otherwise expression won't construct at runtime
          (Expression<Func<int, byte[]>>) (k => k <= 0 ? (byte[])  Array.Empty<byte>() : (byte[]) new byte[k]),

          // TypeIs
          (Expression<Func<object, bool>>) (o => o is string),

          // Coalesce + constructor + member assignment
          (Expression<Func<object, object>>) (o => o ?? new Helper {Field = 5}),

          // Member list init
          (Expression<Func<object>>) (() => new Helper {NestedCollection = {1,2,3}}),

          // Member member binding
          (Expression<Func<object>>) (() => new Helper {NestedClass = {Field = 5}}),

          // Struct constructor
          (Expression<Func<int, Struct>>) (a => new Struct {Id = 2}),

          // Don't know how to write InvocationExpression in C# syntax :-(
          Expression.Lambda<Func<int>>(
            Expression.Invoke(
              Expression.Lambda<Func<int, int>>(Expression.Constant(0), Expression.Parameter(typeof(int), "p")),
              Expression.Constant(1)))
        };
    }

    private ConstantExpression[] GetTestConstantExpressions()
    {
      return new ConstantExpression[] {
        // Regular values
        Expression.Constant(false, typeof(bool)),
        Expression.Constant(true, typeof(bool)),
        Expression.Constant((byte) 64, typeof(byte)),
        Expression.Constant((sbyte) -64, typeof(sbyte)),
        Expression.Constant((short) -128, typeof(short)),
        Expression.Constant((ushort) 128, typeof(ushort)),
        Expression.Constant((int) -256, typeof(int)),
        Expression.Constant((uint) 256, typeof(uint)),
        Expression.Constant((long) -512, typeof(long)),
        Expression.Constant((ulong) 512, typeof(ulong)),
        Expression.Constant(new Int128(65, 64), typeof(Int128)),
        Expression.Constant(new UInt128(65, 64), typeof(UInt128)),
        Expression.Constant((float) 6.283185307f, typeof(float)),
        Expression.Constant((double) 6.283185307179586476925, typeof(double)),
        Expression.Constant((decimal) 6.2895762354m, typeof(decimal)),
        Expression.Constant(TimeSpan.FromSeconds(1024), typeof(TimeSpan)),
        Expression.Constant(new DateTime(2018, 9, 27, 18, 55, 56), typeof(DateTime)),
        Expression.Constant(new DateTimeOffset(2018, 9, 27, 18, 55, 56, new TimeSpan(6, 0, 0)), typeof(DateTimeOffset)),
        Expression.Constant(new DateOnly(2018, 9, 27), typeof(DateOnly)),
        Expression.Constant(new TimeOnly(18, 55, 56), typeof(TimeOnly)),
        Expression.Constant(null, typeof(string)),
        Expression.Constant(string.Empty, typeof(string)),
        Expression.Constant("DBC", typeof(string)),
        Expression.Constant(ByteEnum.One, typeof(ByteEnum)),
        Expression.Constant(ShortEnum.One, typeof(ShortEnum)),
        Expression.Constant(IntEnum.One, typeof(IntEnum)),
        Expression.Constant(LongEnum.One, typeof(LongEnum)),
        Expression.Constant(Guid.NewGuid(), typeof(Guid)),
        Expression.Constant(new Person() { Name = "The Person", HomeAddress = new Address() { City = "Landon", ZipCode = "666777" } }, typeof(Person)),

        // Arrays of regular values
        Expression.Constant(new bool[] { true, false, true }, typeof(bool[])),
        Expression.Constant(new byte[] { 44, 45, 46 }, typeof(byte[])),
        Expression.Constant(new sbyte[] { -44, -45, -46 }, typeof(sbyte[])),
        Expression.Constant(new short[] { -111, -112, -113 }, typeof(short[])),
        Expression.Constant(new ushort[] { 111, 112, 113 }, typeof(ushort[])),
        Expression.Constant(new long[] { -500, -501, -502 }, typeof(long[])),
        Expression.Constant(new ulong[] { 500, 501, 502 }, typeof(ulong[])),
        Expression.Constant(new Int128[] { -1000, -1001, -1002 }, typeof(Int128[])),
        Expression.Constant(new UInt128[] { 1000, 1001, 1002 }, typeof(UInt128[])),
        Expression.Constant(new float[] { float.Tau, float.Tau, float.Tau }, typeof(float[])),
        Expression.Constant(new double[] { double.Tau, double.Tau, double.Tau }, typeof(double[])),
        Expression.Constant(new decimal[] { 6.2895762354m, 6.2895762354m, 6.2895762354m }, typeof(decimal[])),
        Expression.Constant(new TimeSpan[] { TimeSpan.FromSeconds(1024), TimeSpan.FromSeconds(1024 * 2), TimeSpan.FromSeconds(1024 * 3) }, typeof(TimeSpan[])),
        Expression.Constant(new DateTime[] {
          new DateTime(2018, 9, 27, 18, 55, 56),
          new DateTime(2019, 9, 27, 18, 55, 56),
          new DateTime(2020, 9, 27, 18, 55, 56)
        }, typeof(DateTime[])),
        Expression.Constant(new DateOnly[] {
          new DateOnly(2018, 9, 27),
          new DateOnly(2018, 9, 28),
          new DateOnly(2018, 9, 29)
        }, typeof(DateOnly[])),
        Expression.Constant(new TimeOnly[] {
          new TimeOnly(18, 55, 56),
          new TimeOnly(18, 56, 56),
          new TimeOnly(18, 57, 56)
        }, typeof(TimeOnly[])),
        Expression.Constant(new[] { "1", "2", "3" }, typeof(string[])),
        Expression.Constant(new ByteEnum[] { ByteEnum.One, ByteEnum.Two }, typeof(ByteEnum[])),
        Expression.Constant(new ShortEnum[] { ShortEnum.One, ShortEnum.Two }, typeof(ShortEnum[])),
        Expression.Constant(new IntEnum[] { IntEnum.One, IntEnum.Two }, typeof(IntEnum[])),
        Expression.Constant(new LongEnum[] { LongEnum.One, LongEnum.Two }, typeof(LongEnum[])),
        Expression.Constant(new Guid[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() }, typeof(Guid[])),
        Expression.Constant(new Person[] {
            new Person() { Name = "The Person", HomeAddress = new Address() { City = "Landon", ZipCode = "666777" } },
            new Person() { Name = "The Person", HomeAddress = null },
            null
          },
          typeof(Person[])),

        //Nullables of regular values
        Expression.Constant(false, typeof(bool?)),
        Expression.Constant(true, typeof(bool?)),
        Expression.Constant((byte?) 64, typeof(byte?)),
        Expression.Constant((sbyte?) -64, typeof(sbyte?)),
        Expression.Constant((short?) -128, typeof(short?)),
        Expression.Constant((ushort?) 128, typeof(ushort?)),
        Expression.Constant((int?) -256, typeof(int?)),
        Expression.Constant((uint?) 256, typeof(uint?)),
        Expression.Constant((long?) -512, typeof(long?)),
        Expression.Constant((ulong?) 512, typeof(ulong?)),
        Expression.Constant(new Int128(65, 64), typeof(Int128?)),
        Expression.Constant(new UInt128(65, 64), typeof(UInt128?)),
        Expression.Constant((float?) 6.283185307f, typeof(float?)),
        Expression.Constant((double?) 6.283185307179586476925, typeof(double?)),
        Expression.Constant((decimal?) 6.2895762354m, typeof(decimal?)),
        Expression.Constant(TimeSpan.FromSeconds(1024), typeof(TimeSpan?)),
        Expression.Constant(new DateTime(2018, 9, 27, 18, 55, 56), typeof(DateTime?)),
        Expression.Constant(new DateTimeOffset(2018, 9, 27, 18, 55, 56, new TimeSpan(6, 0, 0)), typeof(DateTimeOffset?)),
        Expression.Constant(new DateOnly(2018, 9, 27), typeof(DateOnly?)),
        Expression.Constant(new TimeOnly(18, 55, 56), typeof(TimeOnly?)),
        Expression.Constant(ByteEnum.One, typeof(ByteEnum?)),
        Expression.Constant(ShortEnum.One, typeof(ShortEnum?)),
        Expression.Constant(IntEnum.One, typeof(IntEnum?)),
        Expression.Constant(LongEnum.One, typeof(LongEnum?)),
        Expression.Constant(Guid.NewGuid(), typeof(Guid?)),

        // Arrays of Nullables
        Expression.Constant(new bool?[] { true, false, null }, typeof(bool?[])),
        Expression.Constant(new byte?[] { 44, 45, null }, typeof(byte?[])),
        Expression.Constant(new sbyte?[] { -44, -45, null }, typeof(sbyte?[])),
        Expression.Constant(new short?[] { -111, -112, null }, typeof(short?[])),
        Expression.Constant(new ushort?[] { 111, 112, null }, typeof(ushort?[])),
        Expression.Constant(new long?[] { -500, -501, null }, typeof(long?[])),
        Expression.Constant(new ulong?[] { 500, 501, null }, typeof(ulong?[])),
        Expression.Constant(new Int128?[] { -1000, -1001, null }, typeof(Int128?[])),
        Expression.Constant(new UInt128?[] { 1000, 1001, null }, typeof(UInt128?[])),
        Expression.Constant(new float?[] { float.Tau, float.Tau, null }, typeof(float?[])),
        Expression.Constant(new double?[] { double.Tau, double.Tau, null }, typeof(double?[])),
        Expression.Constant(new decimal?[] { 6.2895762354m, 6.2895762354m, null }, typeof(decimal?[])),
        Expression.Constant(new TimeSpan?[] { TimeSpan.FromSeconds(1024), TimeSpan.FromSeconds(1024 * 2), null }, typeof(TimeSpan?[])),
        Expression.Constant(new DateTime?[] {
          new DateTime(2018, 9, 27, 18, 55, 56),
          new DateTime(2019, 9, 27, 18, 55, 56),
         null
        }, typeof(DateTime?[])),
        Expression.Constant(new DateOnly?[] {
          new DateOnly(2018, 9, 27),
          new DateOnly(2018, 9, 28),
          null
        }, typeof(DateOnly?[])),
        Expression.Constant(new TimeOnly?[] {
          new TimeOnly(18, 55, 56),
          new TimeOnly(18, 56, 56),
          null
        }, typeof(TimeOnly?[])),

        Expression.Constant(new ByteEnum?[] { ByteEnum.One, null }, typeof(ByteEnum?[])),
        Expression.Constant(new ShortEnum?[] { ShortEnum.One, null }, typeof(ShortEnum?[])),
        Expression.Constant(new IntEnum?[] { IntEnum.One, null }, typeof(IntEnum?[])),
        Expression.Constant(new LongEnum?[] { LongEnum.One, null }, typeof(LongEnum?[])),
        Expression.Constant(new Guid?[] { Guid.NewGuid(), Guid.NewGuid(), null }, typeof(Guid?[])),

      };
      ;
    }

    private DefaultExpression[] GetTestDefaultExpressions()
    {
      return new[] {
        Expression.Default(typeof(bool)),
        Expression.Default(typeof(byte)),
        Expression.Default(typeof(sbyte)),
        Expression.Default(typeof(short)),
        Expression.Default(typeof(ushort)),
        Expression.Default(typeof(int)),
        Expression.Default(typeof(uint)),
        Expression.Default(typeof(long)),
        Expression.Default(typeof(ulong)),
        Expression.Default(typeof(float)),
        Expression.Default(typeof(double)),
        Expression.Default(typeof(decimal)),
        Expression.Default(typeof(TimeSpan)),
        Expression.Default(typeof(DateTime)),
        Expression.Default(typeof(string)),
      };
    }

    private ParameterExpression[] GetTestParameterExpressions()
    {
      return new[] {
        Expression.Parameter(typeof(bool)), Expression.Parameter(typeof(bool), "boolP"),
        Expression.Parameter(typeof(byte)), Expression.Parameter(typeof(byte), "byteP"),
        Expression.Parameter(typeof(sbyte)), Expression.Parameter(typeof(sbyte), "sbyteP"),
        Expression.Parameter(typeof(short)), Expression.Parameter(typeof(short), "shortP"),
        Expression.Parameter(typeof(ushort)), Expression.Parameter(typeof(ushort), "ushortP"),
        Expression.Parameter(typeof(int)), Expression.Parameter(typeof(int), "intP"),
        Expression.Parameter(typeof(uint)), Expression.Parameter(typeof(uint), "uintP"),
        Expression.Parameter(typeof(long)), Expression.Parameter(typeof(long), "longP"),
        Expression.Parameter(typeof(ulong)), Expression.Parameter(typeof(ulong), "ulongP"),
        Expression.Parameter(typeof(float)), Expression.Parameter(typeof(float), "floatP"),
        Expression.Parameter(typeof(double)), Expression.Parameter(typeof(double), "doubleP"),
        Expression.Parameter(typeof(decimal)), Expression.Parameter(typeof(decimal), "decimalP"),
        Expression.Parameter(typeof(TimeSpan)), Expression.Parameter(typeof(TimeSpan), "timespanP"),
        Expression.Parameter(typeof(DateTime)), Expression.Parameter(typeof(DateTime), "datetimeP"),
        Expression.Parameter(typeof(string)), Expression.Parameter(typeof(string), "stringP"),
      };
    }

    private ParameterExpression[] GetTestVariableExpressions()
    {
      return new[] {
        Expression.Variable(typeof(bool)), Expression.Variable(typeof(bool), "boolP"),
        Expression.Variable(typeof(byte)), Expression.Variable(typeof(byte), "byteP"),
        Expression.Variable(typeof(sbyte)), Expression.Variable(typeof(sbyte), "sbyteP"),
        Expression.Variable(typeof(short)), Expression.Variable(typeof(short), "shortP"),
        Expression.Variable(typeof(ushort)), Expression.Variable(typeof(ushort), "ushortP"),
        Expression.Variable(typeof(int)), Expression.Variable(typeof(int), "intP"),
        Expression.Variable(typeof(uint)), Expression.Variable(typeof(uint), "uintP"),
        Expression.Variable(typeof(long)), Expression.Variable(typeof(long), "longP"),
        Expression.Variable(typeof(ulong)), Expression.Variable(typeof(ulong), "ulongP"),
        Expression.Variable(typeof(float)), Expression.Variable(typeof(float), "floatP"),
        Expression.Variable(typeof(double)), Expression.Variable(typeof(double), "doubleP"),
        Expression.Variable(typeof(decimal)), Expression.Variable(typeof(decimal), "decimalP"),
        Expression.Variable(typeof(TimeSpan)), Expression.Variable(typeof(TimeSpan), "timespanP"),
        Expression.Variable(typeof(DateTime)), Expression.Variable(typeof(DateTime), "datetimeP"),
        Expression.Variable(typeof(string)), Expression.Variable(typeof(string), "stringP"),
      };
    }

    private BinaryExpression[] GetTestBinaryExpressions()
    {
      return new[] {
        //Expression.Add(Expression.Constant(1), Expression.Constant(2)),
        //Expression.AddChecked(Expression.Constant(2), Expression.Constant(3)),
        //Expression.Subtract(Expression.Constant(4), Expression.Constant(2)),
        //Expression.SubtractChecked(Expression.Constant(5), Expression.Constant(6)),
        //Expression.Divide(Expression.Constant(2), Expression.Constant(1)),
        //Expression.Multiply(Expression.Constant(3), Expression.Constant(5)),
        //Expression.MultiplyChecked(Expression.Constant(4), Expression.Constant(4)),
        //Expression.Modulo(Expression.Constant(5), Expression.Constant(2)),
        Expression.Power(Expression.Constant(2.0), Expression.Constant(4.0)),

        Expression.And(Expression.Constant(10), Expression.Constant(6)),
        Expression.Or(Expression.Constant(10), Expression.Constant(6)),
        Expression.ExclusiveOr(Expression.Constant(10), Expression.Constant(6)),
        Expression.LeftShift(Expression.Constant(256), Expression.Constant(2)),
        Expression.RightShift(Expression.Constant(256), Expression.Constant(2)),

        Expression.Equal(Expression.Constant(256), Expression.Constant(2)),
        Expression.NotEqual(Expression.Constant(256), Expression.Constant(2)),
        Expression.GreaterThan(Expression.Constant(256), Expression.Constant(2)),
        Expression.GreaterThanOrEqual(Expression.Constant(256), Expression.Constant(2)),
        Expression.LessThan(Expression.Constant(256), Expression.Constant(2)),
        Expression.LessThanOrEqual(Expression.Constant(256), Expression.Constant(2)),

        Expression.AndAlso(Expression.Equal(Expression.Constant(256), Expression.Constant(2)), Expression.NotEqual(Expression.Constant(256), Expression.Constant(3))),
        Expression.OrElse(Expression.Equal(Expression.Constant(256), Expression.Constant(2)), Expression.NotEqual(Expression.Constant(256), Expression.Constant(3))),

        Expression.Assign(Expression.Variable(typeof(int), "addAssignVar"), Expression.Constant(111)),

        Expression.Coalesce(Expression.Constant("abc"), Expression.Constant("default")),
        Expression.ArrayIndex(Expression.Constant( new[] { 2, 8, 7 }), Expression.Constant(2))
      };
    }

    private ConditionalExpression[] GetTestConditionalExpressions()
    {
      return new[] {
        Expression.Condition(
          Expression.Equal(Expression.Constant(12), Expression.Constant(12)),
          Expression.Constant(111),
          Expression.Constant(222)),
        Expression.IfThen(
          Expression.Equal(Expression.Constant(13), Expression.Constant(13)),
          Expression.Constant(333)),
        Expression.IfThenElse(
          Expression.Equal(Expression.Constant(14), Expression.Constant(14)),
          Expression.Constant(222),
          Expression.Constant(444))
      };
    }

    private InvocationExpression[] GetTestInvocationExpressions()
    {
      Expression<Func<int, int, bool>> largeSumTest = (num1, num2) => (num1 + num2) > 1000;
      return new[] {
        Expression.Invoke(
        largeSumTest,
        Expression.Constant(539),
        Expression.Constant(281))
      };
    }

    private ListInitExpression[] GetTestListInitExpressions()
    {
      var listType = typeof(List<string>);
      var constructor = listType.GetConstructor(Type.EmptyTypes);
      var newListExpression = Expression.New(constructor);
      var addMethod = listType.GetMethod("Add");
      return new[] {
        Expression.ListInit(
          newListExpression,
          Expression.ElementInit(addMethod, Expression.Constant("Apple", typeof(string))),
          Expression.ElementInit(addMethod, Expression.Constant("Banana", typeof(string))),
          Expression.ElementInit(addMethod, Expression.Constant("Cherry", typeof(string)))
        )
      };
    }

    private MemberExpression[] GetTestMemberExpressions()
    {
      var fooType = typeof(Foo);
      var prop = fooType.GetProperty(nameof(Foo.IntProperty));
      var field = fooType.GetField(nameof(Foo.IntField));

      var instance = new Foo() { IntField = 123, IntProperty = 234 };

      var instanceExp = Expression.Constant(instance);

      return new[] {
        Expression.Field(instanceExp, nameof(Foo.IntField)),
        Expression.Field(instanceExp, field),
        Expression.Property(instanceExp, nameof(Foo.IntProperty)),
        Expression.Property(instanceExp, prop),
      };
    }

    private NewExpression[] GetTestNewExpressions()
    {
      var addressType = typeof(Address);
      var addressCtor1 = addressType.GetConstructor(Type.EmptyTypes);
      var addressCtor2 = addressType.GetConstructor(new Type[] { typeof(string), typeof(string) });

      var cityProp = addressType.GetProperty("City");
      var zipCodeProp = addressType.GetProperty("ZipCode");

      var dateTimeType = typeof(DateTime);
      var dateTimeCtor1 = dateTimeType.GetConstructor(new[] { typeof(long) });
      var dateTimeCtor2 = dateTimeType.GetConstructor(new[] { typeof(int), typeof(int), typeof(int) });

      return new[] {
        Expression.New(addressCtor1),
        Expression.New(addressCtor2, Expression.Constant("City"), Expression.Constant("ZipCode")),
        Expression.New(dateTimeCtor1, Expression.Constant(1224585425L)),
        Expression.New(dateTimeCtor2, Expression.Constant(21), Expression.Constant(24), Expression.Constant(26))
      };
    }

    private MemberInitExpression[] GetTestMemberInitExressions()
    {
      var listType = typeof(List<string>);
      var constructor = listType.GetConstructor(Type.EmptyTypes);
      var newListExpression = Expression.New(constructor);
      var addMethod = listType.GetMethod("Add");

      var fooType = typeof(Foo);
      var prop = fooType.GetProperty(nameof(Foo.IntProperty));
      var field = fooType.GetField(nameof(Foo.IntField));

      var parameter = Expression.Parameter(typeof(int), "i");
      var newExpression1 = Expression.New(fooType);
      var binding = Expression.Bind(prop, parameter);

      var init1 = Expression.ElementInit(addMethod, Expression.Constant("Hello"));
      var init2 = Expression.ElementInit(addMethod, Expression.Constant("World"));

      var messagesProperty = fooType.GetProperty(nameof(Foo.ListProperty));
      MemberListBinding listBinding = Expression.ListBind(messagesProperty, init1, init2);
      NewExpression newExpression2 = Expression.New(fooType);

      var personType = typeof(Person);
      var addressType = typeof(Address);

      var homeAddressMember = personType.GetProperty(nameof(Person.HomeAddress));
      var nameMember = personType.GetProperty(nameof(Person.Name));
      var cityMember = addressType.GetProperty(nameof(Address.City));
      var zipCodeMember = addressType.GetProperty(nameof(Address.ZipCode));

      var cityAssignment = Expression.Bind(cityMember, Expression.Constant("New York"));
      var zipCodeAssignment = Expression.Bind(zipCodeMember, Expression.Constant("10001"));

      var addressBinding = Expression.MemberBind(homeAddressMember, cityAssignment, zipCodeAssignment);
      var nameAssignment = Expression.Bind(nameMember, Expression.Constant("John Doe"));

      var newPerson = Expression.New(personType);

      return new[] {
        Expression.MemberInit(newExpression1, new[] { binding }),
        Expression.MemberInit(newExpression2, new MemberBinding[] { listBinding }),
        Expression.MemberInit(newPerson, addressBinding, nameAssignment)
      };
    }

    private MethodCallExpression[] GetTestMethodCallExpressions()
    {
      var param1 = Expression.Constant(2, typeof(int));
      var param2 = Expression.Constant(3L, typeof(long));
      var thisType = this.GetType();

      var nonpublicMethods = thisType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
      var publicMethods = thisType.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

      var staticLocalMethod = nonpublicMethods
        .FirstOrDefault(m => m.Name.StartsWith($"<{nameof(GetTestMethodCallExpressions)}>g__{nameof(SDummyMethod)}"));

      var publicStaticMethod = publicMethods
        .FirstOrDefault(m => m.Name.StartsWith($"{nameof(SFromCloneMethodCallExpressionDummy)}"));

      var privateStaticMethod = nonpublicMethods
        .FirstOrDefault(m => m.Name.StartsWith($"{nameof(PSFromCloneMethodCallExpressionDummy)}"));

      return new[] {
        Expression.Call(publicStaticMethod, param1, param2),
        Expression.Call(privateStaticMethod, param1, param2),
        Expression.Call(staticLocalMethod, param1, param2),
      };

      static void SDummyMethod(int a, long b)
      { }
    }

    private NewArrayExpression[] GetTestNewArrayExpressions()
    {
      return new[] {
        Expression.NewArrayBounds(typeof(string), Expression.Constant(1)),
        Expression.NewArrayBounds(typeof(string), Expression.Constant(2), Expression.Constant(3)),
        Expression.NewArrayInit(typeof(string), Expression.Constant("ABC"), Expression.Constant("DEF")),
      };
    }

    private TypeBinaryExpression[] GetTestTypeBinaryExpressions()
    {
      return new[] {
        Expression.TypeIs(Expression.Constant(DateTime.UtcNow), typeof(DateTime)),
        Expression.TypeEqual(Expression.Constant(DateTime.UtcNow), typeof(DateTime))
      };
    }

    private UnaryExpression[] GetTestUnaryExpressions()
    {
      return new[] {
        Expression.ArrayLength(Expression.Constant(new int[] { 1,2,3 })),
        Expression.Convert(Expression.Constant(23), typeof(long)),
        Expression.ConvertChecked(Expression.Constant(23), typeof(long)),
        Expression.Decrement(Expression.Constant(24)),
        Expression.Increment(Expression.Constant(25)),
        Expression.IsFalse(Expression.Constant(true)),
        Expression.IsTrue(Expression.Constant(true)),
        Expression.Negate(Expression.Constant(-25)),
        Expression.NegateChecked(Expression.Constant(-25)),
        Expression.Not(Expression.Constant(true)),
        Expression.OnesComplement(Expression.Constant(123)),
      };
    }

#pragma warning disable IDE0060 // Remove unused parameter
    public static void SFromCloneMethodCallExpressionDummy(int a, long b) { }
    private static void PSFromCloneMethodCallExpressionDummy(int a, long b) { }
#pragma warning restore IDE0060 // Remove unused parameter
  }
    #endregion
}
