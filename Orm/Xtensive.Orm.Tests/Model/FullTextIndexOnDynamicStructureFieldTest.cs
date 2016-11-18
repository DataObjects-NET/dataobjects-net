// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Julian Mamokin
// Created:    2016.11.17

using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Orm.Building;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Model.FullTextIndexOnDynamicStructureFieldTestModel;

namespace Xtensive.Orm.Tests.Model.FullTextIndexOnDynamicStructureFieldTestModel
{
  [HierarchyRoot]
  public class EntityWithDynamicallyDefinedStructureField : Entity
  {
    [Key, Field]
    public long Id { get; set; }
  }

  [HierarchyRoot]
  public class EntityWithStructureField : Entity
  {
    [Key, Field]
    public long Id { get; set; }

    [Field]
    public StructureWithDynamicallyDefinedField StructureWithDynamicallyDefinedFieldField { get; set; }
  }

  [HierarchyRoot]
  public class EntityWithDynamicallyAndExplicitlyDefinedStructureFields : Entity
  {
    [Key, Field]
    public long Id { get; set; }

    [Field]
    public StructureWithDynamicallyDefinedField StructureWithDynamicallyDefinedFieldField { get; set; }
  }

  [HierarchyRoot]
  public class EntityWithStringField : Entity
  {
    [Key, Field]
    public long Id { get; set; }

    [Field]
    public string SomeStringField { get; set; }
  }

  [HierarchyRoot]
  public class EntityWithDynamicallyDefinedStringField : Entity
  {
    [Key, Field]
    public long Id { get; set; }
  }

  [HierarchyRoot]
  public class EntityWithDynamicallyAndExplicitlyDefinedStringFields : Entity
  {
    [Key, Field]
    public long Id { get; set; }

    [Field]
    public string StringField { get; set; }
  }

  [HierarchyRoot]
  public class EntityWithMultipleStructureFields : Entity
  {
    [Key, Field]
    public long Id { get; set; }

    [Field]
    public StructureWithField StructureField { get; set; }

    [Field]
    public StructureWithDynamicallyDefinedField AnotherStructureField{ get; set; }

    [Field]
    public StructureWithDynamicallyAndExplicitlyDefinedFields OneMoreStructureField { get; set; }
  }

  public class StructureWithDynamicallyDefinedField : Structure
  {
  }

  public class StructureWithField : Structure
  {
    [Field]
    public string StringField { get; set; }
  }

  public class StructureWithDynamicallyAndExplicitlyDefinedFields : Structure
  {
    [Field]
    public string StringField { get; set; }
  }

  public class Module : IModule
  {
    public void OnBuilt(Domain domain)
    {
    }

    public void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
    {
      DefineDynamicFields(model);
      DefineFullTextIndex(model);
    }

    private void DefineDynamicFields(DomainModelDef model)
    {
      model.Types[typeof(StructureWithDynamicallyDefinedField)].DefineField("StringField", typeof(string));
      model.Types[typeof(StructureWithDynamicallyAndExplicitlyDefinedFields)].DefineField("SomeStringField", typeof (string));
      model.Types[typeof(EntityWithDynamicallyDefinedStringField)].DefineField("StringField", typeof(string));
      model.Types[typeof(EntityWithDynamicallyDefinedStructureField)].DefineField("HiddenField", typeof(StructureWithDynamicallyDefinedField));
      model.Types[typeof(EntityWithDynamicallyAndExplicitlyDefinedStringFields)].DefineField("SomeStringField", typeof(string));
      model.Types[typeof(EntityWithDynamicallyAndExplicitlyDefinedStructureFields)].DefineField("HiddenField", typeof(StructureWithDynamicallyDefinedField));
    }

    private void DefineFullTextIndex(DomainModelDef model)
    {
      var structureWithField = model.Types[typeof (StructureWithField)];
      var structureWithFieldFtIndex = new FullTextIndexDef(structureWithField);
      structureWithFieldFtIndex.Fields.Add(new FullTextFieldDef("StringField",true) {
        Configuration = "english"
      });
      var structureWithDynamicallyDefinedField = model.Types[typeof(StructureWithDynamicallyDefinedField)];
      var structureFtIndex = new FullTextIndexDef(structureWithDynamicallyDefinedField);
      structureFtIndex.Fields.Add(new FullTextFieldDef("StringField", true) {
        Configuration = "English"
      });
      var structureWithDynamicallyAndExplicitlyDefinedField = model.Types[typeof (StructureWithDynamicallyAndExplicitlyDefinedFields)];
      var structureWithDynamicallyAndExplicitlyDefinedFieldstructureFtIndex = new FullTextIndexDef(structureWithDynamicallyAndExplicitlyDefinedField);
      structureWithDynamicallyAndExplicitlyDefinedFieldstructureFtIndex.Fields.AddRange(new List<FullTextFieldDef> {
        new FullTextFieldDef("StringField", true) {Configuration = "English"},
        new FullTextFieldDef("SomeStringField", true) {Configuration = "English"}
      });
      var entityWithStringField = model.Types[typeof(EntityWithStringField)];
      var entityWithStringFieldFtIndex = new FullTextIndexDef(entityWithStringField);
      entityWithStringFieldFtIndex.Fields.Add(new FullTextFieldDef("SomeStringField", true) {
        Configuration = "English"
      });
      var entityWithDynamicallyDefinedStringField = model.Types[typeof (EntityWithDynamicallyDefinedStringField)];
      var entityWithDynamicallyDefinedStringFieldFtIndex = new FullTextIndexDef(entityWithDynamicallyDefinedStringField);
      entityWithDynamicallyDefinedStringFieldFtIndex.Fields.Add(new FullTextFieldDef("StringField", true) {
        Configuration = "English"
      });
      var entityWithDynamicallyAndExplicitlyDefinedStringFields = model.Types[typeof(EntityWithDynamicallyAndExplicitlyDefinedStringFields)];
      var entityWithDynamicallyAndExplicitlyDefinedStringFieldsFtIndex = new FullTextIndexDef(entityWithDynamicallyAndExplicitlyDefinedStringFields);
      entityWithDynamicallyAndExplicitlyDefinedStringFieldsFtIndex.Fields.AddRange(new List<FullTextFieldDef> {
        new FullTextFieldDef("SomeStringField", true) {Configuration = "English"},
        new FullTextFieldDef("StringField", true) {Configuration = "English"}
      });
      model.FullTextIndexes.Add(structureFtIndex);
      model.FullTextIndexes.Add(structureWithFieldFtIndex);
      model.FullTextIndexes.Add(entityWithStringFieldFtIndex);
      model.FullTextIndexes.Add(entityWithDynamicallyDefinedStringFieldFtIndex);
      model.FullTextIndexes.Add(entityWithDynamicallyAndExplicitlyDefinedStringFieldsFtIndex);
      model.FullTextIndexes.Add(structureWithDynamicallyAndExplicitlyDefinedFieldstructureFtIndex);
    }
  }
}

namespace Xtensive.Orm.Tests.Model
{
  public class FullTextIndexOnDynamicStructureFieldTest : AutoBuildTest
  {
    [Test]
    public void EntityWithStringFieldTest()
    {
      var hierarchy = Domain.Model.Types[typeof(EntityWithStringField)];
      var hierarchyIndexColumns = hierarchy.FullTextIndex.Columns;
      Assert.IsTrue(hierarchy.FullTextIndex.Columns.Count==1);
      foreach (var column in hierarchyIndexColumns) {
        FieldInfo correspondingField;
        hierarchy.Fields.TryGetValue(column.Name, out correspondingField);
        Assert.IsNotNull(correspondingField);
        Assert.IsTrue(correspondingField.Columns.Contains(column.Name));
      }
    }

    [Test]
    public void EntityWithDinamicallyDefinedStringFieldTest()
    {
      var hierarchy = Domain.Model.Types[typeof (EntityWithDynamicallyDefinedStringField)];
      var hierarchyIndexColumns = hierarchy.FullTextIndex.Columns;
      Assert.IsTrue(hierarchy.FullTextIndex.Columns.Count==1);
      foreach (var column in hierarchyIndexColumns) {
        FieldInfo correspondingField;
        hierarchy.Fields.TryGetValue(column.Name, out correspondingField);
        Assert.IsNotNull(correspondingField);
        Assert.IsTrue(correspondingField.Columns.Contains(column.Name));
      }
    }

    [Test]
    public void EntityWithDynamicallyAndExplicitlyDefinedStringFieldsTest()
    {
      var hierarchy = Domain.Model.Types[typeof(EntityWithDynamicallyAndExplicitlyDefinedStringFields)];
      var hierarchyIndexColumns = hierarchy.FullTextIndex.Columns;
      Assert.IsTrue(hierarchy.FullTextIndex.Columns.Count==2);
      foreach (var column in hierarchyIndexColumns) {
        FieldInfo correspondingField;
        hierarchy.Fields.TryGetValue(column.Name, out correspondingField);
        Assert.IsNotNull(correspondingField);
        Assert.IsTrue(correspondingField.Columns.Contains(column.Name));
      }
    }

    [Test]
    public void EntityWithStructureFieldTest()
    {
      var hierarchy = Domain.Model.Types[typeof (EntityWithStructureField)];
      var hierarchyIndexColumns = hierarchy.FullTextIndex.Columns;
      Assert.IsTrue(hierarchy.FullTextIndex.Columns.Count==1);
      foreach (var column in hierarchyIndexColumns) {
        FieldInfo correspondingField;
        hierarchy.Fields.TryGetValue(column.Name, out correspondingField);
        Assert.IsNotNull(correspondingField);
        Assert.IsTrue(correspondingField.Columns.Contains(column.Name));
      }
    }

    [Test]
    public void EntityWithDynamicalyDefinedStructureFieldTest()
    {
      var hierarchy = Domain.Model.Types[typeof (EntityWithDynamicallyDefinedStructureField)];
      var hierarchyIndexColumns = hierarchy.FullTextIndex.Columns;
      Assert.IsTrue(hierarchy.FullTextIndex.Columns.Count==1);
      foreach (var column in hierarchyIndexColumns) {
        FieldInfo correspondingField;
        hierarchy.Fields.TryGetValue(column.Name, out correspondingField);
        Assert.IsNotNull(correspondingField);
        Assert.IsTrue(correspondingField.Columns.Contains(column.Name));
      }
    }

    [Test]
    public void EntityWithDynamicallyAndExplicitlyDefinedStructureFieldsTest()
    {
      var hierarchy = Domain.Model.Types[typeof(EntityWithDynamicallyAndExplicitlyDefinedStructureFields)];
      var hierarchyIndexColumns = hierarchy.FullTextIndex.Columns;
      Assert.IsTrue(hierarchy.FullTextIndex.Columns.Count==2);
      foreach (var column in hierarchyIndexColumns) {
        FieldInfo correspondingField;
        hierarchy.Fields.TryGetValue(column.Name, out correspondingField);
        Assert.IsNotNull(correspondingField);
        Assert.IsTrue(correspondingField.Columns.Contains(column.Name));
      }
    }

    [Test]
    public void EntityWithMultipleStructureFieldsTest()
    {
      var hierarchy = Domain.Model.Types[typeof (EntityWithMultipleStructureFields)];
      var hierarchyIndexColumns = hierarchy.FullTextIndex.Columns;
      Assert.IsTrue(hierarchy.FullTextIndex.Columns.Count==4);
      foreach (var column in hierarchyIndexColumns) {
        FieldInfo correscpondingField;
        hierarchy.Fields.TryGetValue(column.Name, out correscpondingField);
        Assert.IsNotNull(correscpondingField);
        Assert.IsTrue(correscpondingField.Columns.Contains(column.Name));
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (EntityWithStructureField).Assembly, typeof (EntityWithStructureField).Namespace);
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      return configuration;
    }
  }
}




  

