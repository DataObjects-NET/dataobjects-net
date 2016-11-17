// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Julian Mamokin
// Created:    2016.11.17

using NUnit.Framework;
using Xtensive.Orm.Building;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Model.FullTextIndexOnDynamicStructureFieldTest.FullTextIndexOnDynamicStructureFieldTestModel;

namespace Xtensive.Orm.Tests.Model
{
  namespace FullTextIndexOnDynamicStructureFieldTest
  {
    namespace FullTextIndexOnDynamicStructureFieldTestModel
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
        public SomeStructureClass StructureField { get; set; }
      }

      [HierarchyRoot]
      public class EntityWithTwoStructureFields : Entity
      {
        [Key, Field]
        public long Id { get; set; }

        [Field]
        public SomeStructureClass StructureField { get; set; }
      }

      public class SomeStructureClass : Structure
      {
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
          var structure = model.Types[typeof(SomeStructureClass)];
          structure.DefineField("StringField", typeof(string));
          var entityWithDynamicallyDefinedStructureField = model.Types[typeof(EntityWithDynamicallyDefinedStructureField)];
          entityWithDynamicallyDefinedStructureField.DefineField("HiddenField", typeof(SomeStructureClass));
          var entityWithTwoStructureFields = model.Types[typeof(EntityWithTwoStructureFields)];
          entityWithTwoStructureFields.DefineField("HiddenField", typeof(SomeStructureClass));
        }

        private void DefineFullTextIndex(DomainModelDef model)
        {
          var structure = model.Types[typeof(SomeStructureClass)];
          var ftIndex = new FullTextIndexDef(structure);
          ftIndex.Fields.Add(new FullTextFieldDef("StringField", true) {
            Configuration = "English"
          });
          model.FullTextIndexes.Add(ftIndex);
        }
      }
    }

    public class FullTextIndexOnDynamicStructureFieldTest : AutoBuildTest
    {
      [Test]
      public void EntityWithStructureFieldTest()
      {
        var hierarchy = Domain.Model.Types[typeof(EntityWithStructureField)];
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
      public void EntityWithoutDynamicalyDefinedStructureFieldTest()
      {
        var hierarchy = Domain.Model.Types[typeof(EntityWithDynamicallyDefinedStructureField)];
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
      public void EntityWithTwoStructureFieldsTest()
      {
        var hierarchy = Domain.Model.Types[typeof(EntityWithTwoStructureFields)];
        var hierarchyIndexColumns = hierarchy.FullTextIndex.Columns;
        Assert.IsTrue(hierarchy.FullTextIndex.Columns.Count==2);
        foreach (var column in hierarchyIndexColumns) {
          FieldInfo correspondingField;
          hierarchy.Fields.TryGetValue(column.Name, out correspondingField);
          Assert.IsNotNull(correspondingField);
          Assert.IsTrue(correspondingField.Columns.Contains(column.Name));
        }
      }

      protected override DomainConfiguration BuildConfiguration()
      {
        var configuration = base.BuildConfiguration();
        configuration.Types.Register(typeof(EntityWithStructureField).Assembly, typeof(EntityWithStructureField).Namespace);
        configuration.UpgradeMode = DomainUpgradeMode.Recreate;
        return configuration;
      }
    }
  }
}





  

