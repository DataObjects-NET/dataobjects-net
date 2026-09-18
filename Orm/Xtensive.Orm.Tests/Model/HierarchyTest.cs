// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.12.26

using System;
using NUnit.Framework;
using Xtensive.Orm.Building;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Model.Hierarchies;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Tests.Model.Hierarchies
{
  public interface I0 : IEntity
  {
    [Field]
    string AName { get; set; }
  }

  public interface IA : I0
  {
  }

  public class A : Entity, IA
  {
    string I0.AName { get; set; }

    [Field]
    public string AName { get; set; }
  }

  [HierarchyRoot]
  public class AB : A
  {
    [Field, Key]
    public long ID { get; private set; }

    [Field]
    public string ABName { get; set; }
  }

  public class ABC : AB
  {
  }

  public abstract class B : Entity
  {
  }

  [HierarchyRoot]
  public class BC : B
  {
    [Field, Key]
    public Guid ID { get; private set; }
  }

  [HierarchyRoot]
  public class BD : B
  {
    [Field, FieldMapping("ID"), Key]
    public long ID { get; private set; }

    [Field]
    public string AName { get; set; }
  }

  [HierarchyRoot]
  public class BE : B
  {
    [Field, FieldMapping("ID"), Key]
    public int ID { get; private set; }
  }

  public class IdentifiableEntity : Entity
  {
    [Field, Key]
    public Guid Id { get; private set; }
  }

  [HierarchyRoot]
  public class MyEntity : IdentifiableEntity
  {
    [Field]
    public string Name { get; set; }
  }

  public class CustomStorageDefinitionBuilder : IModule
  {
    public static bool IsEnabled;

    public virtual void OnBuilt(Domain domain)
    {
    }

    public void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
    {
      if (!IsEnabled)
        return;

      TypeDef type;
      type = model.Types[typeof (A)];
      Assert.That(context.ModelDef.FindRoot(type)==type, Is.False);

      type = model.Types[typeof (AB)];
      Assert.That(context.ModelDef.FindRoot(type)==type, Is.True);

      type = model.Types[typeof (ABC)];
      Assert.That(context.ModelDef.FindRoot(type)==type, Is.False);

      type = model.Types[typeof (B)];
      Assert.That(context.ModelDef.FindRoot(type)==type, Is.False);

      type = model.Types[typeof (BC)];
      Assert.That(context.ModelDef.FindRoot(type)==type, Is.True);
    }
  }
}

namespace Xtensive.Orm.Tests.Model
{
  [TestFixture, Category("Model")]
  public class HierarchyTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.RegisterCaching(typeof (A).Assembly, typeof(A).Namespace);
      return config;
    }

    public override void TestFixtureSetUp()
    {
      try {
        CustomStorageDefinitionBuilder.IsEnabled = true;
        base.TestFixtureSetUp();
      }
      finally {
        CustomStorageDefinitionBuilder.IsEnabled = false;
      }
    }

    public override void TestFixtureTearDown()
    {
      CustomStorageDefinitionBuilder.IsEnabled = false;
      base.TestFixtureTearDown();
    }
    
    [Test]
    public void MainTest()
    {
      var modelTypes = Domain.Model.Types;

      Assert.That(modelTypes.Contains(typeof (A)), Is.False);

      var abType = modelTypes[typeof (AB)];
      Assert.That(abType, Is.Not.Null);
      Assert.That(abType.Fields["ID"], Is.Not.Null);
      Assert.That(abType.Fields["ID"].ValueType, Is.EqualTo(typeof(long)));
      Assert.That(abType.Fields["ABName"], Is.Not.Null);
      Assert.That(abType.Fields["AName"], Is.Not.Null);
      Assert.That(abType.Fields["ABName"].DeclaringType, Is.EqualTo(abType));
      Assert.That(abType.Fields["AName"].DeclaringType, Is.EqualTo(abType));
      Assert.That(abType.Hierarchy.Root, Is.EqualTo(abType));

      var abcType = modelTypes[typeof(ABC)];
      Assert.That(abcType.Hierarchy.Root, Is.EqualTo(abType));
      Assert.That(abcType.Hierarchy, Is.EqualTo(abType.Hierarchy));
      Assert.That(abcType.Fields["ID"].ValueType, Is.EqualTo(typeof (long)));

      var bcType = modelTypes[typeof (BC)];
      Assert.That(bcType.Fields["ID"].ValueType, Is.EqualTo(typeof (Guid)));
      Assert.That(modelTypes[typeof(BD)].Fields["ID"].ValueType, Is.EqualTo(typeof (long)));
      Assert.That(modelTypes[typeof(BE)].Fields["ID"].ValueType, Is.EqualTo(typeof (int)));
    }

    [Test]
    public void AnotherTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var m = new MyEntity();
          m.Name = "Name";

          t.Complete();
        }
      }
    }
  }
}