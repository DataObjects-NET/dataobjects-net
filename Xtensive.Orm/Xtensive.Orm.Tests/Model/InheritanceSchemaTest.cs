// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.11.30

using System;
using NUnit.Framework;
using Xtensive.Orm.Building;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Model.InheritanceSchemaModel;
using System.Linq;

namespace Xtensive.Orm.Tests.Model.InheritanceSchemaModel
{
  [Index("Name")]
  public interface IHasName : IEntity
  {
    [Field(Length = 1000)]
    string Name { get; set; }
  }

  [Index("Name")]
  public interface IHasName2 : IEntity
  {
    [Field(Length = 1000)]
    string Name { get; set; }
  }

  [MaterializedView, TableMapping("Creatures")]
  public interface ICreature : IHasName
  {
  }

  [HierarchyRoot]
  public abstract class A : Entity
  {
    [Field, Key]
    public long ID { get; private set; }
  }

  [Index("Tag")]
  public class B : A, IHasName, IHasName2
  {
    public string Name { get; set; }

    [Field]
    public int Tag { get; set; }
  }

  [Index("Age")]
  public class C : A
  {
    [Field, FieldMapping("MyAge")]
    public int Age { get; set; }
  }

  [Index("Tag")]
  public class D : C, ICreature
  {
    public string Name { get; set; }

    [Field(Length = 1000)]
    public virtual string Tag { get; set; }
  }

  public class E : D
  {
  }

  public class F : A, ICreature, IHasName2
  {
    string IHasName.Name
    {
      get { return Name; }
      set { Name = value; }
    }

    public string Name { get; set; }
  }

  [HierarchyRoot]
  public class X : Entity, IHasName
  {
    [Field, Key]
    public long ID { get; private set; }

    public string Name { get; set; }
  }

  public class Y : X
  {
  }

  public class Z : Y
  {
  }

  public abstract class DomainBuilderBase : IModule
  {
    protected abstract InheritanceSchema InheritanceSchema { get; }

    public void OnBuilt(Domain domain)
    {}

    public virtual void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
    {
      foreach (HierarchyDef hierarchyDef in model.Hierarchies)
        hierarchyDef.Schema = InheritanceSchema;
    }
  }

  public class SingleTableInheritanceBuilder : DomainBuilderBase
  {
    public static bool IsEnabled;

    public override void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
    {
      if (!IsEnabled)
        return;
      base.OnDefinitionsBuilt(context, model);
    }

    protected override InheritanceSchema InheritanceSchema
    {
      get { return InheritanceSchema.SingleTable; }
    }
  }

  public class ConcreteTableInheritanceBuilder : DomainBuilderBase
  {
    public static bool IsEnabled;

    public override void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
    {
      if (!IsEnabled)
        return;
      base.OnDefinitionsBuilt(context, model);
    }

    protected override InheritanceSchema InheritanceSchema
    {
      get { return InheritanceSchema.ConcreteTable; }
    }
  }
}

namespace Xtensive.Orm.Tests.Model.InheritanceSchemaTests
{
  public abstract class InheritanceSchemaTestBase : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var dc = base.BuildConfiguration();
      dc.Types.Register(typeof(A).Assembly, typeof(A).Namespace);
      dc.UpgradeMode = DomainUpgradeMode.Recreate;
      return dc;
    }

    public override void TestFixtureTearDown()
    {
      ConcreteTableInheritanceBuilder.IsEnabled = false;
      SingleTableInheritanceBuilder.IsEnabled = false;
      base.TestFixtureTearDown();
    }
  }

  public class ClassTableInheritanceTest : InheritanceSchemaTestBase
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var dc = base.BuildConfiguration();
      ConcreteTableInheritanceBuilder.IsEnabled = false;
      SingleTableInheritanceBuilder.IsEnabled = false;
      return dc;
    }

    [Test]
    public virtual void MainTest()
    {
      Domain.Model.Dump();
    }
  }

  public class ConcreteTableInheritanceTest : InheritanceSchemaTestBase
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration configuration = base.BuildConfiguration();
      ConcreteTableInheritanceBuilder.IsEnabled = true;
      SingleTableInheritanceBuilder.IsEnabled = false;
      return configuration;
    }

    [Test]
    public virtual void MainTest()
    {
      Domain.Model.Dump();
    }
  }

  public class SingleTableInheritanceTest : InheritanceSchemaTestBase
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration configuration = base.BuildConfiguration();
      ConcreteTableInheritanceBuilder.IsEnabled = false;
      SingleTableInheritanceBuilder.IsEnabled = true;
      return configuration;
    }

    [Test]
    public virtual void MainTest()
    {
      Domain.Model.Dump();
    }
  }
}