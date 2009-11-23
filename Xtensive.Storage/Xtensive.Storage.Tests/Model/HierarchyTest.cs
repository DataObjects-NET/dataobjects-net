// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.12.26

using System;
using NUnit.Framework;
using Xtensive.Storage.Building;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Model.Hierarchies;

namespace Xtensive.Storage.Tests.Model.Hierarchies
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
    {}

    public void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
    {
      if (!IsEnabled)
        return;
      TypeDef type;

      type = model.Types[typeof(A)];
      Assert.IsFalse(context.ModelDef.FindRoot(type)==type);

      type = model.Types[typeof(AB)];
      Assert.IsTrue(context.ModelDef.FindRoot(type)==type);

      type = model.Types[typeof(ABC)];
      Assert.IsFalse(context.ModelDef.FindRoot(type)==type);

      type = model.Types[typeof(B)];
      Assert.IsFalse(context.ModelDef.FindRoot(type)==type);

      type = model.Types[typeof(BC)];
      Assert.IsTrue(context.ModelDef.FindRoot(type)==type);
    }
  }
}

namespace Xtensive.Storage.Tests.Model
{
  public class HierarchyTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(typeof (A).Assembly, typeof(A).Namespace);
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
      Assert.IsFalse(Domain.Model.Types.Contains(typeof (A)));
      Assert.IsNotNull(Domain.Model.Types[typeof (AB)]);
      Assert.IsNotNull(Domain.Model.Types[typeof (AB)].Fields["ID"]);
      Assert.IsNotNull(Domain.Model.Types[typeof (AB)].Fields["ABName"]);
      Assert.IsNotNull(Domain.Model.Types[typeof (AB)].Fields["AName"]);
      Assert.AreEqual(Domain.Model.Types[typeof (AB)], Domain.Model.Types[typeof (AB)].Fields["ABName"].DeclaringType);
      Assert.AreEqual(Domain.Model.Types[typeof (AB)], Domain.Model.Types[typeof (AB)].Fields["AName"].DeclaringType);
      Assert.AreEqual(Domain.Model.Types[typeof (AB)], Domain.Model.Types[typeof (AB)].Hierarchy.Root);
      Assert.AreEqual(Domain.Model.Types[typeof (AB)], Domain.Model.Types[typeof (ABC)].Hierarchy.Root);
      Assert.AreEqual(Domain.Model.Types[typeof (AB)].Hierarchy, Domain.Model.Types[typeof (ABC)].Hierarchy);
      Assert.AreEqual(typeof (long), Domain.Model.Types[typeof (AB)].Fields["ID"].ValueType);
      Assert.AreEqual(typeof (long), Domain.Model.Types[typeof (ABC)].Fields["ID"].ValueType);
      Assert.AreEqual(typeof (Guid), Domain.Model.Types[typeof (BC)].Fields["ID"].ValueType);
      Assert.AreEqual(typeof (long), Domain.Model.Types[typeof (BD)].Fields["ID"].ValueType);
      Assert.AreEqual(typeof (int), Domain.Model.Types[typeof (BE)].Fields["ID"].ValueType);
    }

    [Test]
    public void AnotherTest()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var m = new MyEntity();
          m.Name = "Name";

          t.Complete();
        }
      }
    }
  }
}