// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.11.30

using NUnit.Framework;
using Xtensive.Storage.Building;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Tests.Model.InheritanceSchemaModel;  

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

  [MaterializedView, Mapping("Creatures")]
  public interface ICreature : IHasName
  {
  }

  [Mapping("A-Root")]
  [HierarchyRoot]
  public class A : Entity
  {
    [Field, Key]
    public long ID { get; private set; }
  }

  [Index("Tag")]
//  [Index("Name")]
  // TODO: Alex Kochetov: Log error if duplicate index is specified.
  public class B : A, IHasName, IHasName2
  {
    public string Name { get; set; }

    [Field]
    public int Tag { get; set; }
  }

  [Index("Age")]
  public class C : A
  {
    [Field, Mapping("MyAge")]
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
    public override string Tag
    {
      get { return base.Tag; }
      set { base.Tag = value; }
    }
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

  [Index("Name")]
  [HierarchyRoot]
  public class X : Entity
  {
    [Field, Key]
    public long ID { get; private set; }

    [Field(Length = 1000)]
    public string Name { get; set; }
  }

  public class Y : X
  {
  }

  public class Z : Y
  {
  }

  internal abstract class DomainBuilderBase : IModule
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

  internal class SingleTableInheritanceBuilder : DomainBuilderBase
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

  internal class ConcreteTableInheritanceBuilder : DomainBuilderBase
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
      dc.Types.Register(typeof (A).Assembly, typeof(A).Namespace);
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
//      Domain.Model.Types.Dump();

      foreach (var type in Domain.Model.Types) {
        foreach (var indexInfo in type.Indexes) {
          var keyComplexity = type.Hierarchy.KeyInfo.Columns.Count;
          if (indexInfo.IsPrimary)
            Assert.AreEqual(keyComplexity, indexInfo.KeyColumns.Count, "Type: {0}; index: {1}", indexInfo.ReflectedType.Name,
              indexInfo.Name, type.Name);
          else
            Assert.AreEqual(2, indexInfo.ValueColumns.Count, "Type: {0}; index: {1}", indexInfo.ReflectedType.Name,
              indexInfo.Name, type.Name);
        }
      }
    }
  }

  public class ClassTableInheritanceTest : InheritanceSchemaTestBase
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var dc = base.BuildConfiguration();
      dc.NamingConvention.LetterCasePolicy = LetterCasePolicy.Uppercase;
      dc.NamingConvention.NamingRules = NamingRules.UnderscoreDots | NamingRules.UnderscoreHyphens;
      dc.NamingConvention.NamespacePolicy = NamespacePolicy.Hash;
      dc.NamingConvention.NamespaceSynonyms.Add("Xtensive.Storage.Tests.Model.DefaultPlacement", "X");
      ConcreteTableInheritanceBuilder.IsEnabled = false;
      SingleTableInheritanceBuilder.IsEnabled = false;
      return dc;
    }

    public override void MainTest()
    {
      base.MainTest();

//      TypeInfo typeInfo = Domain.Model.Types[typeof (A)];
//      Assert.IsNotNull(typeInfo);
//      Assert.AreEqual(1, typeInfo.Indexes.Count);
//      Assert.IsTrue(typeInfo.AffectedIndexes.Count > 0);
//      Assert.IsNotNull(typeInfo.Indexes.PrimaryIndex);
//      Assert.IsTrue(typeInfo.Indexes.PrimaryIndex.IsVirtual);
//      Assert.IsTrue((typeInfo.Indexes.PrimaryIndex.Attributes & IndexAttributes.Join) > 0);
//      Assert.AreEqual(8, typeInfo.Indexes.PrimaryIndex.ValueColumns.Count);
//
//
//      typeInfo = Domain.Model.Types[typeof (B)];
//      Assert.IsNotNull(typeInfo);
//      Assert.AreEqual(4, typeInfo.Indexes.Count);
//      Assert.IsTrue(typeInfo.AffectedIndexes.Count > 0);
//      Assert.IsNotNull(typeInfo.Indexes.PrimaryIndex);
//      Assert.IsTrue(typeInfo.Indexes.PrimaryIndex.IsVirtual);
//      Assert.IsTrue((typeInfo.Indexes.PrimaryIndex.Attributes & IndexAttributes.Join) > 0);
//      Assert.AreEqual(3, typeInfo.Indexes.PrimaryIndex.ValueColumns.Count);
//
//      typeInfo = Domain.Model.Types[typeof (C)];
//      Assert.IsNotNull(typeInfo);
//      Assert.AreEqual(3, typeInfo.Indexes.Count);
//      Assert.IsTrue(typeInfo.AffectedIndexes.Count > 0);
//      Assert.IsNotNull(typeInfo.Indexes.PrimaryIndex);
//      Assert.IsTrue(typeInfo.Indexes.PrimaryIndex.IsVirtual);
//      Assert.IsTrue((typeInfo.Indexes.PrimaryIndex.Attributes & IndexAttributes.Join) > 0);
//      Assert.AreEqual(4, typeInfo.Indexes.PrimaryIndex.ValueColumns.Count);
//
//      typeInfo = Domain.Model.Types[typeof (D)];
//      Assert.IsNotNull(typeInfo);
//      Assert.AreEqual(5, typeInfo.Indexes.Count);
//      Assert.IsTrue(typeInfo.AffectedIndexes.Count > 0);
//      Assert.IsNotNull(typeInfo.Indexes.PrimaryIndex);
//      Assert.IsTrue(typeInfo.Indexes.PrimaryIndex.IsVirtual);
//      Assert.IsTrue((typeInfo.Indexes.PrimaryIndex.Attributes & IndexAttributes.Join) > 0);
//      Assert.AreEqual(4, typeInfo.Indexes.PrimaryIndex.ValueColumns.Count);
//
//      typeInfo = Domain.Model.Types[typeof (IHasName)];
//      Assert.IsNotNull(typeInfo);
//      Assert.AreEqual(2, typeInfo.Indexes.Count);
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

    public override void MainTest()
    {
      base.MainTest();
//
//      TypeInfo typeInfo = Domain.Model.Types[typeof (A)];
//      Assert.IsNotNull(typeInfo);
//      Assert.AreEqual(2, typeInfo.Indexes.Count);
//      Assert.IsTrue(typeInfo.AffectedIndexes.Count > 0);
//      Assert.IsNotNull(typeInfo.Indexes.PrimaryIndex);
//      Assert.IsTrue(typeInfo.Indexes.PrimaryIndex.IsVirtual);
//      Assert.AreEqual(8, typeInfo.Indexes.PrimaryIndex.ValueColumns.Count);
//
//
//      typeInfo = Domain.Model.Types[typeof (B)];
//      Assert.IsNotNull(typeInfo);
//      Assert.AreEqual(3, typeInfo.Indexes.Count);
//      Assert.IsTrue(typeInfo.AffectedIndexes.Count > 0);
//      Assert.IsNotNull(typeInfo.Indexes.PrimaryIndex);
//      Assert.IsFalse(typeInfo.Indexes.PrimaryIndex.IsVirtual);
//      Assert.AreEqual(3, typeInfo.Indexes.PrimaryIndex.ValueColumns.Count);
//
//      typeInfo = Domain.Model.Types[typeof (C)];
//      Assert.IsNotNull(typeInfo);
//      Assert.AreEqual(4, typeInfo.Indexes.Count);
//      Assert.IsTrue(typeInfo.AffectedIndexes.Count > 0);
//      Assert.IsNotNull(typeInfo.Indexes.PrimaryIndex);
//      Assert.IsTrue(typeInfo.Indexes.PrimaryIndex.IsVirtual);
//      Assert.AreEqual(4, typeInfo.Indexes.PrimaryIndex.ValueColumns.Count);
//
//      typeInfo = Domain.Model.Types[typeof (D)];
//      Assert.IsNotNull(typeInfo);
//      Assert.AreEqual(8, typeInfo.Indexes.Count);
//      Assert.IsTrue(typeInfo.AffectedIndexes.Count > 0);
//      Assert.IsNotNull(typeInfo.Indexes.PrimaryIndex);
//      Assert.IsTrue(typeInfo.Indexes.PrimaryIndex.IsVirtual);
//      Assert.AreEqual(4, typeInfo.Indexes.PrimaryIndex.ValueColumns.Count);
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

    public override void MainTest()
    {
      base.MainTest();

//      TypeInfo typeInfo = Domain.Model.Types[typeof (A)];
//      Assert.IsNotNull(typeInfo);
//      Assert.AreEqual(9, typeInfo.Indexes.Count);
//      Assert.IsTrue(typeInfo.AffectedIndexes.Count > 0);
//      Assert.IsNotNull(typeInfo.Indexes.PrimaryIndex);
//      Assert.IsFalse(typeInfo.Indexes.PrimaryIndex.IsVirtual);
//      Assert.AreEqual(8, typeInfo.Indexes.PrimaryIndex.ValueColumns.Count);
//
//      typeInfo = Domain.Model.Types[typeof (B)];
//      Assert.IsNotNull(typeInfo);
//      Assert.AreEqual(3, typeInfo.Indexes.Count);
//      Assert.IsTrue(typeInfo.AffectedIndexes.Count > 0);
//      Assert.IsNotNull(typeInfo.Indexes.PrimaryIndex);
//      Assert.IsTrue(typeInfo.Indexes.PrimaryIndex.IsVirtual);
//      Assert.IsTrue((typeInfo.Indexes.PrimaryIndex.Attributes & IndexAttributes.Filtered) > 0);
//      Assert.AreEqual(3, typeInfo.Indexes.PrimaryIndex.ValueColumns.Count);
//      Assert.IsNotNull(typeInfo.Indexes.GetIndex("Name"));
//      Assert.IsTrue(typeInfo.Indexes.GetIndex("Name").IsVirtual);
//
//
//      typeInfo = Domain.Model.Types[typeof (C)];
//      Assert.IsNotNull(typeInfo);
//      Assert.AreEqual(2, typeInfo.Indexes.Count);
//      Assert.IsTrue(typeInfo.AffectedIndexes.Count > 0);
//      Assert.IsNotNull(typeInfo.Indexes.PrimaryIndex);
//      Assert.IsTrue(typeInfo.Indexes.PrimaryIndex.IsVirtual);
//      Assert.IsTrue((typeInfo.Indexes.PrimaryIndex.Attributes & IndexAttributes.Filtered) > 0);
//      Assert.AreEqual(4, typeInfo.Indexes.PrimaryIndex.ValueColumns.Count);
//
//      typeInfo = Domain.Model.Types[typeof (D)];
//      Assert.IsNotNull(typeInfo);
//      Assert.AreEqual(3, typeInfo.Indexes.Count);
//      Assert.IsTrue(typeInfo.AffectedIndexes.Count > 0);
//      Assert.IsNotNull(typeInfo.Indexes.PrimaryIndex);
//      Assert.IsTrue(typeInfo.Indexes.PrimaryIndex.IsVirtual);
//      Assert.IsTrue((typeInfo.Indexes.PrimaryIndex.Attributes & IndexAttributes.Filtered) > 0);
//      Assert.AreEqual(4, typeInfo.Indexes.PrimaryIndex.ValueColumns.Count);
    }
  }
}