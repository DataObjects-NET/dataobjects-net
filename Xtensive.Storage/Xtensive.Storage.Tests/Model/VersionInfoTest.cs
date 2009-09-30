// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.08.13

using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Model.VersionInfoTests.InvalidModel1
{
  [HierarchyRoot]
  public class Parent : Entity
  {
    [Key, Field]
    public int Id { get; private set; }
 
    [Field, Version]
    public string ParentVersionField { get; set; }
  }

  public class Child : Parent
  {
    [Field, Version]
    public string ChildVersionField { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Model.VersionInfoTests.InvalidModel2
{
  [HierarchyRoot]
  public class Parent : Entity
  {
    [Key, Field, Version]
    public int Id { get; private set; }
  }
}

namespace Xtensive.Storage.Tests.Model.VersionInfoTests.InvalidModel3
{
  [HierarchyRoot]
  public class Parent : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field, Version]
    public Parent ReferenceField { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Model.VersionInfoTests.InvalidModel4
{
  [HierarchyRoot]
  public class Parent : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field(LazyLoad = true), Version]
    public string ReferenceField { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Model.VersionInfoTests.ValidModel
{
  [HierarchyRoot]
  public class Parent : Entity
  {
    [Key, Field]
    public int Id { get; private set; }
 
    [Field, Version]
    public string ParentVersionField { get; set; }

    [Field]
    public string ParentNonVersionField { get; set; }
  }

  public class Child : Parent
  {
    [Field]
    public string ChildNonVersionField { get; set; }
  }

  [HierarchyRoot]
  public class Simple : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string NonLazyField1 { get; set; }

    [Field]
    public int NonLazyField2 { get; set; }

    [Field]
    public SimpleStructure StructureField { get; set; }

    [Field(LazyLoad = true)]
    public string LazyField { get; set; }

    [Field]
    public Simple ReferenceField { get; set; }

    [Field]
    public EntitySet<Simple> CollectionField { get; private set; }
  }

  public class SimpleStructure : Structure
  {
    [Field]
    public string NonLazyField { get; set; }

    [Field(LazyLoad = true)]
    public string LazyField { get; set; }

    [Field]
    public Simple ReferenceField { get; set; }
  }
  
}

namespace Xtensive.Storage.Tests.Model
{
  using VersionInfoTests.ValidModel;

  [TestFixture]
  public class VersionInfoTest
  {
    public Domain BuildDomain(string @namespace)
    {
      var configuration = DomainConfigurationFactory.Create("memory");
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(Assembly.GetExecutingAssembly(), @namespace);
      return Domain.Build(configuration);
    }

    [Test]
    public void RootOnlyVersionTest()
    {
      AssertEx.Throws<DomainBuilderException>(() => 
        BuildDomain("Xtensive.Storage.Tests.Model.VersionInfoTests.InvalidModel1"));
    }

    [Test]
    public void DenyKeyFieldsTest()
    {
      AssertEx.Throws<DomainBuilderException>(() => 
        BuildDomain("Xtensive.Storage.Tests.Model.VersionInfoTests.InvalidModel2"));
    }

    [Test]
    public void DenyEntityFieldsTest()
    {
      AssertEx.Throws<DomainBuilderException>(() => 
        BuildDomain("Xtensive.Storage.Tests.Model.VersionInfoTests.InvalidModel3"));
    }

    [Test]
    public void DenyLazyLoadFieldsTest()
    {
      AssertEx.Throws<DomainBuilderException>(() => 
        BuildDomain("Xtensive.Storage.Tests.Model.VersionInfoTests.InvalidModel4"));
    }

    [Test]
    public void VersionFieldsTest()
    {
      var domain = BuildDomain("Xtensive.Storage.Tests.Model.VersionInfoTests.ValidModel");
      var model = domain.Model;

      var parentType = model.Types[typeof (Parent)];
      Assert.IsTrue(parentType.Fields["ParentVersionField"].IsVersion);
      Assert.IsFalse(parentType.Fields["ParentNonVersionField"].IsVersion);
      
      var childType = model.Types[typeof (Child)];
      Assert.IsTrue(childType.Fields["ParentVersionField"].IsVersion);
      Assert.IsFalse(childType.Fields["ParentNonVersionField"].IsVersion);
      Assert.IsFalse(childType.Fields["ChildNonVersionField"].IsVersion);
      
      var simpleType = model.Types[typeof (Simple)];
      Assert.IsTrue(simpleType.Fields["NonLazyField1"].IsVersion);
      Assert.IsTrue(simpleType.Fields["NonLazyField2"].IsVersion);
      Assert.IsFalse(simpleType.Fields["Id"].IsVersion);
      Assert.IsFalse(simpleType.Fields["TypeId"].IsVersion);
      Assert.IsFalse(simpleType.Fields["LazyField"].IsVersion);
      Assert.IsFalse(simpleType.Fields["ReferenceField"].IsVersion);
      Assert.IsFalse(simpleType.Fields["CollectionField"].IsVersion);
      Assert.IsFalse(simpleType.Fields["StructureField"].IsVersion);
      Assert.IsTrue(simpleType.Fields["StructureField.NonLazyField"].IsVersion);
      Assert.IsFalse(simpleType.Fields["StructureField.LazyField"].IsVersion);
      Assert.IsFalse(simpleType.Fields["StructureField.ReferenceField"].IsVersion);
    }
  }
}