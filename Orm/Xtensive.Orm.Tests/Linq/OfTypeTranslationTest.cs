// Copyright (C) 2019 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kudelin
// Created:    2019.01.31

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Building;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Tests.Linq
{
  using OfTypeTranslationTestModels;

  public class OfTypeTranslationTest
  {
    [Test]
    [TestCase(InheritanceSchema.ClassTable)]
    [TestCase(InheritanceSchema.ConcreteTable)]
    [TestCase(InheritanceSchema.SingleTable)]
    public void Test01(InheritanceSchema schema)
    {
      using (var sto = OpenSessionTransaction(schema))
      {
        var result = sto.Query.All<TestEntity1>().OfType<ITestEntity2>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();
        Assert.That(result[0], Is.EqualTo("100000String1"));
        Assert.That(result[1], Is.EqualTo("200000String2"));
        Assert.That(result[2], Is.EqualTo("300000String3"));
      }
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable)]
    [TestCase(InheritanceSchema.ConcreteTable)]
    [TestCase(InheritanceSchema.SingleTable)]
    public void Test02(InheritanceSchema schema)
    {
      using (var sto = OpenSessionTransaction(schema)) {
        var result1 = sto.Query.All<TestEntity2b>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();

        var result2 = sto.Query.All<TestEntity1>().OfType<TestEntity2b>().OfType<ITestEntity2>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();

        Assert.That(result1, Is.Not.Empty);
        Assert.That(result1.SequenceEqual(result2));
      }
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable)]
    [TestCase(InheritanceSchema.ConcreteTable)]
    [TestCase(InheritanceSchema.SingleTable)]
    public void Test03(InheritanceSchema schema)
    {
      using (var sto = OpenSessionTransaction(schema))
      {
        var result1 = sto.Query.All<TestEntity2a>().OfType<ITestEntity2>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();
        Assert.That(result1.First(), Is.EqualTo("100000String1"));
        Assert.That(result1.Last(), Is.EqualTo("200000String2"));

        var result2 = sto.Query.All<TestEntity2b>().OfType<ITestEntity2>()
          .Select(x => x.Field3.Value2.Value1 + x.Field4).ToArray();
        Assert.That(result2.Single(), Is.EqualTo("300000String3"));
      }
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable)]
    [TestCase(InheritanceSchema.ConcreteTable)]
    [TestCase(InheritanceSchema.SingleTable)]
    public void Test04(InheritanceSchema schema)
    {
      using (var sto = OpenSessionTransaction(schema))
      {
        var result1 = sto.Query.All<TestEntity2a>().OfType<TestEntity1>()
          .Select(x => x.Field2).ToArray();
        Assert.That(result1.First(), Is.EqualTo(1000));
        Assert.That(result1.Last(), Is.EqualTo(2000));

        var result2 = sto.Query.All<TestEntity2b>().OfType<TestEntity1>()
          .Select(x => x.Field2).ToArray();
        Assert.That(result2.Single(), Is.EqualTo(3000));
      }
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable)]
    [TestCase(InheritanceSchema.ConcreteTable)]
    [TestCase(InheritanceSchema.SingleTable)]
    public void Test05(InheritanceSchema schema)
    {
      using (var sto = OpenSessionTransaction(schema)) {
        var result = sto.Query.All<TestEntity2a>().OfType<TestEntity2b>()
          .Select(x => x.Field2).ToArray();
        Assert.That(result, Is.Empty);
      }
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable)]
    [TestCase(InheritanceSchema.ConcreteTable)]
    [TestCase(InheritanceSchema.SingleTable)]
    public void Test06(InheritanceSchema schema)
    {
      using (var sto = OpenSessionTransaction(schema)) {
        var result = sto.Query.All<TestEntity2a>().OfType<ITestEntity2c>()
          .Select(x => x.Field3).ToArray();
        Assert.That(result, Is.Empty);
      }
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable)]
    [TestCase(InheritanceSchema.ConcreteTable)]
    [TestCase(InheritanceSchema.SingleTable)]
    public void Test07(InheritanceSchema schema)
    {
      using (var sto = OpenSessionTransaction(schema)) {
        var source = sto.Query.All<TestEntity1>().OfType<ITestEntity2>();

        var result1 = source.Select(x => x.Field3.Value1).ToArray();
        Assert.That(result1[0], Is.EqualTo(10000));
        Assert.That(result1[1], Is.EqualTo(20000));
        Assert.That(result1[2], Is.EqualTo(30000));
        Assert.That(result1.Length, Is.EqualTo(3));

        var result2 = source.OfType<ITestEntity2c>().Select(x => x.Field3.Value1).ToArray();
        Assert.That(result2, Is.Empty);
      }
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable)]
    [TestCase(InheritanceSchema.ConcreteTable)]
    [TestCase(InheritanceSchema.SingleTable)]
    public void Test08(InheritanceSchema schema)
    {
      using (var sto = OpenSessionTransaction(schema)) {
        var source = sto.Query.All<TestEntity1>().OfType<ITestEntity2>();
        var result1 = source.ToArray().OfType<ITestEntity3a>().Select(x => x.Field5).ToArray();
        var result2 = source.OfType<ITestEntity3a>().Select(x => x.Field5).ToArray();
        Assert.That(result1, Is.Not.Empty);
        Assert.That(result1.SequenceEqual(result2));
      }
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable)]
    [TestCase(InheritanceSchema.ConcreteTable)]
    [TestCase(InheritanceSchema.SingleTable)]
    public void Test09(InheritanceSchema schema)
    {
      using (var sto = OpenSessionTransaction(schema)) {
        var source = sto.Query.All<TestEntity1>().OfType<ITestEntity2>();
        var result1 = source.ToArray().OfType<ITestEntityBase>().Select(x => x.BaseField).ToArray();
        var result2 = source.OfType<ITestEntityBase>().Select(x => x.BaseField).ToArray();
        Assert.That(result1, Is.Not.Empty);
        Assert.That(result1.SequenceEqual(result2));
      }
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable)]
    [TestCase(InheritanceSchema.ConcreteTable)]
    [TestCase(InheritanceSchema.SingleTable)]
    public void Test10(InheritanceSchema schema)
    {
      using (var sto = OpenSessionTransaction(schema)) {
        var exception = Assert.Throws<QueryTranslationException>(
          () => sto.Query.All<TestEntity1>().OfType<ITestEntity2>().OfType<BaseEntity>().Run());
        Assert.That(exception.InnerException, Is.AssignableFrom(typeof (NotSupportedException)));
      }
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable)]
    [TestCase(InheritanceSchema.ConcreteTable)]
    [TestCase(InheritanceSchema.SingleTable)]
    public void Test11(InheritanceSchema schema)
    {
      using (var sto = OpenSessionTransaction(schema)) {
        var query = sto.Query.All<TestEntity1>().OfType<ITestEntity2>().OfType<IBaseEntity>();

        var result1 = query.Select(x => x.Field1).ToArray();
        Assert.That(result1[0], Is.EqualTo(10));
        Assert.That(result1[1], Is.EqualTo(20));
        Assert.That(result1[2], Is.EqualTo(30));
        Assert.That(result1.Length, Is.EqualTo(3));

        var result2 = query.OfType<TestEntity1>().Select(x => x.Field1).ToArray();
        Assert.That(result2[0], Is.EqualTo(10));
        Assert.That(result2[1], Is.EqualTo(20));
        Assert.That(result2[2], Is.EqualTo(30));
        Assert.That(result2.Length, Is.EqualTo(3));

        var result3 = query.OfType<TestEntity1>().OfType<IBaseEntity>().OfType<TestEntity3a>().Select(x => x.Field1).ToArray();
        Assert.That(result3[0], Is.EqualTo(10));
        Assert.That(result3[1], Is.EqualTo(20));
        Assert.That(result3.Length, Is.EqualTo(2));
      }
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable)]
    [TestCase(InheritanceSchema.ConcreteTable)]
    [TestCase(InheritanceSchema.SingleTable)]
    public void Test12(InheritanceSchema schema)
    {
      using (var sto = OpenSessionTransaction(schema)) {

        var source = sto.Query.All<TestEntity1>().OfType<ITestEntity3a>().SelectMany(x => x.Field6);

        var result1 = source.OfType<ITestEntity3a>().Select(x => x.Field5).ToArray();
        Assert.That(result1.Single(), Is.EqualTo(3.5));

        var result2 = source.OfType<ITestEntity2>().Select(x => x.Field3.Value1).ToArray();
        Assert.That(result2.Single(), Is.EqualTo(20000));

        var result3 = source.OfType<ITestEntityBase>().Select(x => x.BaseField).ToArray();
        Assert.That(result3.Single(), Is.EqualTo(2));
      }
    }

    [Test]
    [TestCase(InheritanceSchema.ClassTable)]
    [TestCase(InheritanceSchema.ConcreteTable)]
    [TestCase(InheritanceSchema.SingleTable)]
    public void Test13(InheritanceSchema schema)
    {
      using (var sto = OpenSessionTransaction(schema)) {
        var source = sto.Query.All<TestEntity1>();

        var result1A = source.ToArray().OfType<ITestEntity3a>().OfType<IBaseEntity>();
        var result1B = source.OfType<ITestEntity3a>().OfType<IBaseEntity>();

        var result2A = source.ToArray().OfType<ITestEntity2c>().OfType<IBaseEntity>();
        var result2B = source.OfType<ITestEntity2c>().OfType<IBaseEntity>();

        var resultA = result1A.Union(result2A).Select(x => x.Field1).ToArray();
        var resultB = result1B.Union(result2B).Select(x => x.Field1).ToArray();

        Assert.That(resultA, Is.Not.Empty);
        Assert.That(resultA.SequenceEqual(resultB));
      }
    }

    private SessionTransactionOpener OpenSessionTransaction(InheritanceSchema schema = InheritanceSchema.ClassTable)
    {
      return OpenSessionTransaction((c, d) => d.Hierarchies.Where(x => !x.Root.IsSystem).ForEach(x => x.Schema = schema));
    }

    private SessionTransactionOpener OpenSessionTransaction(Action<BuildingContext, DomainModelDef> onDefinitionsBuiltAction)
    {
      TestModule.OnDefinitionsBuiltAction = onDefinitionsBuiltAction;
      var domain = Domain.Build(BuildConfiguration());
      PopulateData(domain);
      return new SessionTransactionOpener(domain);
    }

    protected void PopulateData(Domain domain)
    {
      using (var sto = new SessionTransactionOpener(domain)) {
        new TestEntity3a() {
          BaseField = 1,
          Field1 = 10,
          Field2 = 1000,
          Field3 = new TestStructure1 {
            Value1 = 10000,
            Value2 = new TestStructure2 {
              Value1 = 100000,
              Value2 = DateTime.FromBinary(10)
            },
            Value3 = "StructureString1"
          },
          Field4 = "String1",
          Field5 = 2.5,
        }.Field6.Add(
          new TestEntity3a {
            BaseField = 2,
            Field1 = 20,
            Field2 = 2000,
            Field3 = new TestStructure1 {
              Value1 = 20000,
              Value2 = new TestStructure2 {
                Value1 = 200000,
                Value2 = DateTime.FromBinary(1000)
              },
              Value3 = "StructureString2"
            },
            Field4 = "String2",
            Field5 = 3.5,
          });
        new TestEntity2b() {
          BaseField = 3,
          Field1 = 30,
          Field2 = 3000,
          Field3 = new TestStructure1 {
            Value1 = 30000,
            Value2 = new TestStructure2 {
              Value1 = 300000,
              Value2 = DateTime.FromBinary(100000)
            },
            Value3 = "StructureString3"
          },
          Field4 = "String3"
        };
        new TestEntity2c() {
          BaseField = 4,
          Field1 = 40,
          Field2 = 4000,
          Field3 = new TestStructure1 {
            Value1 = 40000,
            Value2 = new TestStructure2 {
              Value1 = 400000,
              Value2 = DateTime.FromBinary(400000)
            },
            Value3 = "StructureString4"
          },
          Field4 = "String4"
        };

        sto.TransactionScope.Complete();
      }
    }

    private DomainConfiguration BuildConfiguration()
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(typeof(TestEntity1).Assembly, typeof(TestEntity1).Namespace);
      config.Types.Register(typeof(TestModule));
      return config;
    }

    public sealed class TestModule : IModule
    {
      public static Action<BuildingContext, DomainModelDef> OnDefinitionsBuiltAction { get; set; }

      public void OnBuilt(Domain domain)
      {
      }

      public void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
      {
        if (OnDefinitionsBuiltAction != null)
          OnDefinitionsBuiltAction(context, model);
      }
    }
  }
}

namespace Xtensive.Orm.Tests.Linq.OfTypeTranslationTestModels
{
  public interface IBaseEntity : IEntity
  {
    [Field, Key]
    int Id { get; }

    [Field]
    long Field1 { get; set; }
  }

  public interface ITestEntityBase : IEntity
  {
    [Field]
    ulong BaseField { get; set; }
  }

  public interface ITestEntity2 : ITestEntityBase
  {
    [Field]
    TestStructure1 Field3 { get; set; }

    [Field]
    string Field4 { get; set; }
  }

  public interface ITestEntity3a : ITestEntityBase
  {
    [Field]
    double Field5 { get; set; }

    [Field]
    EntitySet<TestEntity2a> Field6 { get; set; }
  }

  public interface ITestEntity2c : ITestEntityBase
  {
    [Field]
    TestStructure1 Field3 { get; set; }

    [Field]
    string Field4 { get; set; }
  }

  public class BaseEntity : Entity, IBaseEntity
  {
    public int Id { get; private set; }

    public long Field1 { get; set; }
  }

  [HierarchyRoot]
  public class TestEntity1 : BaseEntity
  {
    [Field]
    public long Field2 { get; set; }
  }

  public class TestEntity2a : TestEntity1, ITestEntity2
  {
    public TestStructure1 Field3 { get; set; }

    public ulong BaseField { get; set; }

    public string Field4 { get; set; }
  }

  public class TestEntity3a : TestEntity2a, ITestEntity3a
  {
    public double Field5 { get; set; }

    public EntitySet<TestEntity2a> Field6 { get; set; }
  }

  public class TestEntity2b : TestEntity1, ITestEntity2
  {
    [Field]
    public EntitySet<TestEntity2b> Field5 { get; set; }

    public ulong BaseField { get; set; }

    public string Field4 { get; set; }

    public TestStructure1 Field3 { get; set; }
  }

  public class TestEntity2c : TestEntity1, ITestEntity2c
  {
    public ulong BaseField { get; set; }

    public TestStructure1 Field3 { get; set; }

    public string Field4 { get; set; }
  }

  public class TestStructure1 : Structure
  {
    [Field]
    public int Value1 { get; set; }

    [Field]
    public TestStructure2 Value2 { get; set; }

    [Field]
    public string Value3 { get; set; }
  }

  public class TestStructure2 : Structure
  {
    [Field]
    public int Value1 { get; set; }

    [Field]
    public DateTime Value2 { get; set; }
  }
}
