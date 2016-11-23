// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Julian Mamokin
// Created:    2016.11.15

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Model.FullTextIndexOnStructureFieldTestModel;

namespace Xtensive.Orm.Tests.Model.FullTextIndexOnStructureFieldTestModel
{
  [HierarchyRoot]
  public class HierarchyWithFullTextIndex : BaseEntity
  {
    [Field]
    public StructureWithIndexedField StructureWithIndexedField1 { get; set; }

    [Field]
    public StructureWithIndexedField StructureWithIndexedField2 { get; set; }

    [Field]
    public AnotherStructure StructureWithAnotherIndexedField { get; set; }

    [Field, FullText("English")]
    public string IndexedStringField { get; set; }
  }

  [HierarchyRoot(InheritanceSchema.ClassTable)]
  public class ClassTableHierarchy : BaseEntity
  {
    [Field]
    public StructureWithIndexedField StructureWithIndexedField1 { get; set; }

    [Field]
    public StructureWithIndexedField StructureWithIndexedField2 { get; set; }

    [Field]
    public AnotherStructure StructureWithAnotherIndexedField { get; set; }
  }

  [HierarchyRoot(InheritanceSchema.ConcreteTable)]
  public class ConcreteTableHierarchy : BaseEntity
  {
    [Field]
    public StructureWithIndexedField StructureWithIndexedField1 { get; set; }

    [Field]
    public StructureWithIndexedField StructureWithIndexedField2 { get; set; }

    [Field]
    public AnotherStructure StructureWithAnotherIndexedField { get; set; }
  }

  [HierarchyRoot(InheritanceSchema.SingleTable)]
  public class SingleTableHierarchy : BaseEntity
  {
    [Field]
    public StructureWithIndexedField StructureWithIndexedField1 { get; set; }

    [Field]
    public StructureWithIndexedField StructureWithIndexedField2 { get; set; }

    [Field]
    public AnotherStructure StructureWithAnotherIndexedField { get; set; }
  }

  public class StructureWithIndexedField : Structure
  {
    [Field, FullText("English")]
    public string IndexedStringField { get; set; }

    [Field, FullText("English")]
    public string IndexedStringField1 { get; set; }

    [Field, FullText("English")]
    public string IndexedStringField2 { get; set; }

    [Field]
    public string UnindexedStringField { get; set; }
  }

  public class AnotherStructure : Structure
  {
    [Field, FullText("English")]
    public string StringField { get; set; }

    [Field, FullText("Russian")]
    public string StringField1 { get; set; }

    [Field, FullText("Russian")]
    public string StringField2 { get; set; }
  }

  public class BaseEntity : Entity
  {
    [Key, Field]
    public long Id { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Model
{
  public class FullTextIndexOnStructureFieldTest : AutoBuildTest
  {
    [Test]
    public void FullTextIndexPresenceTest()
    {
      Assert.That(Domain.Model.FullTextIndexes.Count(), Is.EqualTo(4));
    }

    [Test]
    public void HierarchyWithFullTextIndexTest()
    {
      var hierarchy = Domain.Model.Types[typeof(HierarchyWithFullTextIndex)];
      var hierarchyIndexColumns = hierarchy.FullTextIndex.Columns;
      foreach (var column in hierarchyIndexColumns){
        FieldInfo correspondingField;
        hierarchy.Fields.TryGetValue(column.Name, out correspondingField);
        Assert.IsNotNull(correspondingField);
        Assert.IsTrue(correspondingField.Columns.Contains(column.Name));
      }
    }

    [Test]
    public void ClassTableFullTextIndexTest()
    {
      var simpleClassTable = Domain.Model.Types[typeof(ClassTableHierarchy)];
      var simpleClassTableIndexColumns = simpleClassTable.FullTextIndex.Columns;
      foreach (var column in simpleClassTableIndexColumns) {
        FieldInfo correspondingField;
        simpleClassTable.Fields.TryGetValue(column.Name, out correspondingField);
        Assert.IsNotNull(correspondingField);
        Assert.IsTrue(correspondingField.Columns.Contains(column.Name));
      }
    }

    [Test]
    public void ConcreteTableFullTextIndexTest()
    {
      var simpleConcreteTable = Domain.Model.Types[typeof(ConcreteTableHierarchy)];
      foreach (var column in simpleConcreteTable.FullTextIndex.Columns) {
        FieldInfo correspondingField;
        simpleConcreteTable.Fields.TryGetValue(column.Name, out correspondingField);
        Assert.IsNotNull(correspondingField);
        Assert.IsTrue(correspondingField.Columns.Contains(column.Name));
      }
    }

    [Test]
    public void SingleTableFullTextIndexTest()
    {
      var simpleSingleTable = Domain.Model.Types[typeof(SingleTableHierarchy)];
      foreach (var column in simpleSingleTable.FullTextIndex.Columns) {
        FieldInfo correspondingField;
        simpleSingleTable.Fields.TryGetValue(column.Name, out correspondingField);
        Assert.IsNotNull(correspondingField);
        Assert.IsTrue(correspondingField.Columns.Contains(column.Name));
      }
    }

    protected override void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.FullText);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(typeof (ClassTableHierarchy).Assembly, typeof (ClassTableHierarchy).Namespace);
      return configuration;
    }
  }
}
