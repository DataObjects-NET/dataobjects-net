// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.11.30

using System;
using NUnit.Framework;
using Xtensive.Storage.Building;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Tests.Model.InheritanceSchemaModel;
using System.Linq;

namespace Xtensive.Storage.Tests.Model.InheritanceSchemaModel
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

  [Serializable]
  [TableMapping("A-Root")]
  [HierarchyRoot]
  public class A : Entity
  {
    [Field, Key]
    public long ID { get; private set; }
  }

  [Serializable]
  [Index("Tag")]
//  [Index("Name")]
  // TODO: Alex Kochetov: Log error if duplicate index is specified.
  public class B : A, IHasName, IHasName2
  {
    public string Name { get; set; }

    [Field]
    public int Tag { get; set; }
  }

  [Serializable]
  [Index("Age")]
  public class C : A
  {
    [Field, FieldMapping("MyAge")]
    public int Age { get; set; }
  }

  [Serializable]
  [Index("Tag")]
  public class D : C, ICreature
  {
    public string Name { get; set; }

    [Field(Length = 1000)]
    public virtual string Tag { get; set; }
  }

  [Serializable]
  public class E : D
  {
  }

  [Serializable]
  public class F : A, ICreature, IHasName2
  {
    string IHasName.Name
    {
      get { return Name; }
      set { Name = value; }
    }

    public string Name { get; set; }
  }

  [Serializable]
  //  [Index("Name")]
  // TODO: Alex Kochetov: Log error if duplicate index is specified.
  [HierarchyRoot]
  public class X : Entity, IHasName
  {
    [Field, Key]
    public long ID { get; private set; }

    public string Name { get; set; }
  }

  [Serializable]
  public class Y : X
  {
  }

  [Serializable]
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

namespace Xtensive.Storage.Tests.Model.InheritanceSchemaTests
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

    [Test]
    public virtual void MainTest()
    {
      Domain.Model.Dump();
    }
  }

  public class ClassTableInheritanceTest : InheritanceSchemaTestBase
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var dc = base.BuildConfiguration();
//      dc.NamingConvention.LetterCasePolicy = LetterCasePolicy.Uppercase;
//      dc.NamingConvention.NamingRules = NamingRules.UnderscoreDots | NamingRules.UnderscoreHyphens;
//      dc.NamingConvention.NamespacePolicy = NamespacePolicy.Hash;
//      dc.NamingConvention.NamespaceSynonyms.Add("Xtensive.Storage.Tests.Model.DefaultPlacement", "X");
      ConcreteTableInheritanceBuilder.IsEnabled = false;
      SingleTableInheritanceBuilder.IsEnabled = false;
      return dc;
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
  }
}