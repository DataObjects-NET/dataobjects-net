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
using System.Linq.Expressions;

namespace Xtensive.Orm.Tests.Core.Linq
{
  [TestFixture]
  public class SerializableExpressionsTest : ExpressionTestBase
  {
    #region Nested Types
    [Serializable]
    internal class Foo
    {
      public int IntField;
      public int IntProperty { get; set; }
      public List<string> ListProperty { get; set; }

      public Foo() { }
    }

    [Serializable]
    internal class Address
    {
      public string City { get; set; }
      public string ZipCode { get; set; }

      public Address() { }

      public Address(string city, string zipCode)
      {
        City = city;
        ZipCode = zipCode;
      }
    }

    [Serializable]
    internal class Person
    {
      public Address HomeAddress { get; set; }
      public string Name { get; set; }
    }
    #endregion


    public IEnumerable<Type> SerializableExpressionTypes
    {
      get {
        yield return typeof(SerializableBinaryExpression);
        yield return typeof(SerializableConditionalExpression);
        yield return typeof(SerializableConstantExpression);
        yield return typeof(SerializableElementInit);
        yield return typeof(SerializableExpression);
        yield return typeof(SerializableInvocationExpression);
        yield return typeof(SerializableLambdaExpression);
        yield return typeof(SerializableListInitExpression);
        yield return typeof(SerializableMemberAssignment);
        yield return typeof(SerializableMemberBinding);
        yield return typeof(SerializableMemberExpression);
        yield return typeof(SerializableMemberInitExpression);
        yield return typeof(SerializableMemberListBinding);
        yield return typeof(SerializableMemberMemberBinding);
        yield return typeof(SerializableMethodCallExpression);
        yield return typeof(SerializableNewArrayExpression);
        yield return typeof(SerializableNewExpression);
        yield return typeof(SerializableParameterExpression);
        yield return typeof(SerializableTypeBinaryExpression);
        yield return typeof(SerializableUnaryExpression);
      }
    }

    #region Test Data

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

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
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
      foreach(var origin in ConstantExpressions) {
        Console.WriteLine(origin.ToString(true));
        var converted = origin.ToSerializableExpression().ToExpression();
        Assert.That(converted.ToExpressionTree(), Is.EqualTo(origin.ToExpressionTree()));
        Console.WriteLine("OK");
      }
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
    public void MethodCalllExpressionTest()
    {
      foreach (var origin in MethodCallExpressions) {
        Console.WriteLine(origin.ToString(true));
        var converted = origin.ToSerializableExpression().ToExpression();
        Assert.That(converted.ToExpressionTree(), Is.EqualTo(origin.ToExpressionTree()));
        Console.WriteLine("OK");
      }
    }

    [Test]
    public void NewArrayExpressionTest() {
      foreach (var origin in NewArrayExpressions) {
        Console.WriteLine(origin.ToString(true));
        var converted = origin.ToSerializableExpression().ToExpression();
        Assert.That(converted.ToExpressionTree(), Is.EqualTo(origin.ToExpressionTree()));
        Console.WriteLine("OK");
      }
    }

    [Test]
    public void TypeBinaryExpressionTest() {
      foreach (var origin in TypeBinaryExpressions) {
        Console.WriteLine(origin.ToString(true));
        var converted = origin.ToSerializableExpression().ToExpression();
        Assert.That(converted.ToExpressionTree(), Is.EqualTo(origin.ToExpressionTree()));
        Console.WriteLine("OK");
      }
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
    public void LambdaExpressionTest()
    {
      foreach (var origin in Expressions) {
        Console.WriteLine(origin.ToString(true));
        var converted = origin.ToSerializableExpression().ToExpression();
        Assert.That(converted.ToExpressionTree(), Is.EqualTo(origin.ToExpressionTree()));
        Console.WriteLine("OK");
      }
    }

    [Test]
    public void BinaryFormatterSerializeTest()
    {
      RunSerializeTest(new BinaryFormatter(), Expressions);
    }

    [Test]
    public void NetDataContractSerializeTest()
    {
      var list = SerializableExpressionTypes.ToList();
      var settings = new DataContractSerializerSettings();
      settings.KnownTypes = list;
      settings.PreserveObjectReferences = true;
      RunSerializeTest(new DataContractSerializer(typeof(SerializableExpression), settings), Expressions);
    }

    private void RunSerializeTest(XmlObjectSerializer serializer, IEnumerable<Expression> expressions)
    {
      using (var stream = new MemoryStream()) {
        foreach (var expression in expressions) {
          Console.WriteLine(expression.ToString(true));
          serializer.WriteObject(stream, expression.ToSerializableExpression());
          _ = stream.Seek(0, SeekOrigin.Begin);
          var serialized = (SerializableExpression) serializer.ReadObject(stream);
          stream.SetLength(0);
          Assert.That(serialized.ToExpression().ToExpressionTree(), Is.EqualTo(expression.ToExpressionTree()));
          Console.WriteLine("OK");
        }
      }
    }

    private void RunSerializeTest(IFormatter serializer, IEnumerable<Expression> expressions)
    {
      using (var stream = new MemoryStream()) {
        foreach (var expression in expressions) {
          Console.WriteLine(expression.ToString(true));
          serializer.Serialize(stream, expression.ToSerializableExpression());
          _ = stream.Seek(0, SeekOrigin.Begin);
          var serialized = (SerializableExpression) serializer.Deserialize(stream);
          stream.SetLength(0);
          Assert.That(serialized.ToExpression().ToExpressionTree(), Is.EqualTo(expression.ToExpressionTree()));
          Console.WriteLine("OK");
        }
      }
    }

    //private static T GetClone<T>(T testExp, IEnumerable<Type> types)
    //  where T : Expression
    //{
    //  var serializable = testExp.ToSerializableExpression();
    //  var clone = Cloner.CloneViaDataContractSerializer<SerializableExpression>(serializable, types, out var serializedVersion);
    //  var native = (T) new SerializableExpressionToExpressionConverter(clone).Convert();
    //  return native;
    //}

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

    #region Test Data initializers
    private ConstantExpression[] GetTestConstantExpressions()
    {
      return new ConstantExpression[] {
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
        Expression.Constant((float) 6.283185307f, typeof(float)),
        Expression.Constant((double) 6.283185307179586476925, typeof(double)),
        Expression.Constant((decimal) 6.2895762354m, typeof(decimal)),
        Expression.Constant(TimeSpan.FromSeconds(1024), typeof(TimeSpan)),
        Expression.Constant(new DateTime(2018, 9, 27, 18, 55, 56), typeof(DateTime)),
        Expression.Constant(null, typeof(string)),
        Expression.Constant(string.Empty, typeof(string)),
        Expression.Constant("DBC", typeof(string)),
        Expression.Constant(new int[] { 1, 2, 3 }),
        Expression.Constant(new string[] { "1", "2", "3" })
      };
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
        Expression.Add(Expression.Constant(1), Expression.Constant(2)),
        Expression.AddChecked(Expression.Constant(2), Expression.Constant(3)),
        Expression.Subtract(Expression.Constant(4), Expression.Constant(2)),
        Expression.SubtractChecked(Expression.Constant(5), Expression.Constant(6)),
        Expression.Divide(Expression.Constant(2), Expression.Constant(1)),
        Expression.Multiply(Expression.Constant(3), Expression.Constant(5)),
        Expression.MultiplyChecked(Expression.Constant(4), Expression.Constant(4)),
        Expression.Modulo(Expression.Constant(5), Expression.Constant(2)),
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
          Expression.Constant(222),
          Expression.Constant(333)),
        Expression.IfThen(
          Expression.Equal(Expression.Constant(12), Expression.Constant(12)),
          Expression.Constant(222)),
        Expression.IfThenElse(
          Expression.Equal(Expression.Constant(14), Expression.Constant(14)),
          Expression.Constant(222),
          Expression.Constant(333))
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
        Expression.ArrayIndex(Expression.Constant(new[] { 8, 9, 10 }), new[] { Expression.Constant(1) }),
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

    public static void SFromCloneMethodCallExpressionDummy(int a, long b) { }
    private static void PSFromCloneMethodCallExpressionDummy(int a, long b) { }
  }
  #endregion
}