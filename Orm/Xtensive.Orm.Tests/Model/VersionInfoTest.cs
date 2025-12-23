// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.08.13

using System;
using System.Reflection;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Tests.Model.VersionInfoTests.ValidModel;

#region Models

namespace Xtensive.Orm.Tests.Model.VersionInfoTests.InvalidModel1
{
  [Serializable]
  [HierarchyRoot]
  public class Parent : Entity
  {
    [Key, Field]
    public int Id { get; private set; }
 
    [Field, Version]
    public string ParentVersionField { get; set; }
  }

  [Serializable]
  public class Child : Parent
  {
    [Field, Version]
    public string ChildVersionField { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Model.VersionInfoTests.InvalidModel2
{
  [Serializable]
  [HierarchyRoot]
  public class Parent : Entity
  {
    [Key, Field, Version]
    public int Id { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Model.VersionInfoTests.InvalidModel3
{
  [Serializable]
  [HierarchyRoot]
  public class Parent : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field(LazyLoad = true), Version]
    public string ReferenceField { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Model.VersionInfoTests.ValidModel
{
  [Serializable]
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

  [Serializable]
  public class Child : Parent
  {
    [Field]
    public string ChildNonVersionField { get; set; }
  }

  [Serializable]
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

    [Field]
    public byte[] ByteArrayField { get; set; }
  }

  [Serializable]
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

#endregion

namespace Xtensive.Orm.Tests.Model
{
  [TestFixture]
  public class VersionInfoTest
  {
    [OneTimeSetUp]
    public void TestFixtureSetUp()
    {
      Require.ProviderIs(StorageProvider.SqlServer);
    }

    public Domain BuildDomain(string @namespace)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.RegisterCaching(Assembly.GetExecutingAssembly(), @namespace);
      return Domain.Build(configuration);
    }

    [Test]
    public void DenyKeyFieldsTest()
    {
      AssertEx.Throws<DomainBuilderException>(() => 
        BuildDomain("Xtensive.Orm.Tests.Model.VersionInfoTests.InvalidModel2"));
    }

    [Test]
    public void DenyLazyLoadFieldsTest()
    {
      AssertEx.Throws<DomainBuilderException>(() => 
        BuildDomain("Xtensive.Orm.Tests.Model.VersionInfoTests.InvalidModel3"));
    }

    [Test]
    public void VersionFieldsTest()
    {
      using var domain = BuildDomain("Xtensive.Orm.Tests.Model.VersionInfoTests.ValidModel");
      var model = domain.Model;

      var parentType = model.Types[typeof (Parent)];
      var versionFields = parentType.GetVersionFields();
      Assert.That(versionFields.Any(field => field == parentType.Fields["ParentVersionField"]), Is.True);
      Assert.That(versionFields.Any(field => field == parentType.Fields["ParentNonVersionField"]), Is.False);
      
      var childType = model.Types[typeof (Child)];
      versionFields = childType.GetVersionFields();
      Assert.That(versionFields.Any(field => field == childType.Fields["ParentVersionField"]), Is.True);
      Assert.That(versionFields.Any(field => field == childType.Fields["ParentNonVersionField"]), Is.False);
      Assert.That(versionFields.Any(field => field == childType.Fields["ChildNonVersionField"]), Is.False);
      
      var simpleType = model.Types[typeof (Simple)];
      var versionColumns = simpleType.GetVersionColumns();
      Assert.That(versionColumns.Any(pair => pair.Field == simpleType.Fields["NonLazyField1"]), Is.True);
      Assert.That(versionColumns.Any(pair => pair.Field == simpleType.Fields["NonLazyField2"]), Is.True);
      Assert.That(versionColumns.Any(pair => pair.Field == simpleType.Fields["ReferenceField.Id"]), Is.True);
      Assert.That(versionColumns.Any(pair => pair.Field == simpleType.Fields["Id"]), Is.False);
      Assert.That(versionColumns.Any(pair => pair.Field == simpleType.Fields["TypeId"]), Is.False);
      Assert.That(versionColumns.Any(pair => pair.Field == simpleType.Fields["LazyField"]), Is.False);
      Assert.That(versionColumns.Any(pair => pair.Field == simpleType.Fields["CollectionField"]), Is.False);
      Assert.That(versionColumns.Any(pair => pair.Field == simpleType.Fields["StructureField"]), Is.False);
      Assert.That(versionColumns.Any(pair => pair.Field == simpleType.Fields["StructureField.NonLazyField"]), Is.True);
      Assert.That(versionColumns.Any(pair => pair.Field == simpleType.Fields["StructureField.LazyField"]), Is.False);
      Assert.That(versionColumns.Any(pair => pair.Field == simpleType.Fields["StructureField.ReferenceField.Id"]), Is.True);
      Assert.That(versionColumns.Any(pair => pair.Field == simpleType.Fields["ByteArrayField"]), Is.False);
    }
    
    [Test]
    public void SerializeVersionInfoTest()
    {
      using var domain = BuildDomain("Xtensive.Orm.Tests.Model.VersionInfoTests.ValidModel");
      VersionInfo version;

      using (var session = domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var instance = new Simple();
          instance.NonLazyField1 = "Value";
          instance.NonLazyField2 = 123;
          instance.StructureField = new SimpleStructure {NonLazyField = "Value"};
          instance.ReferenceField = instance;
          version = instance.VersionInfo;
          transactionScope.Complete();
        }
      }

      Assert.That(version.IsVoid, Is.False);
      var versionClone = Cloner.CloneViaBinarySerialization(version);
      Assert.That(versionClone.IsVoid, Is.False);
      Assert.That(versionClone, Is.EqualTo(version));
    }
  }
}