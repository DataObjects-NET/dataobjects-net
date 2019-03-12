// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.11.14

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using noFTModel = Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel1;
using simpleFTModel = Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel2;
using complexFTModel = Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel3;
using twoFTIndexesModel1 = Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel4;
using twoFTIndexesModel2 = Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel5;
using newTypeColumnModel = Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel10;
using wrongModel1 = Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel6;
using wrongModel2 = Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel7;
using wrongModel3 = Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel8;
using wrongModel4 = Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel9;

namespace Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel1
{
  [HierarchyRoot]
  public class Document : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Title { get; set; }

    [Field(Length = 8001)]
    public byte[] DocumentBody { get; set; }

    [Field(Length = 50)]
    public string Extension { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel2
{
  public class DefinitionCorrector : IModule
  {
    public void OnBuilt(Domain domain)
    {
    }

    public void OnDefinitionsBuilt(Building.BuildingContext context, Building.Definitions.DomainModelDef model)
    {
      var documentDef = model.Types[typeof (noFTModel.Document)];
      var ftIndexDef = new FullTextIndexDef(documentDef);
      ftIndexDef.Fields.Add(new FullTextFieldDef("Title", false) {Configuration = "English"});
      model.FullTextIndexes.Add(ftIndexDef);
    }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel3
{
  public class DefinitionCorrector : IModule
  {
    public void OnBuilt(Domain domain)
    {
    }

    public void OnDefinitionsBuilt(Building.BuildingContext context, DomainModelDef model)
    {
      var documentDef = model.Types[typeof (noFTModel.Document)];
      var ftIndexDef = new FullTextIndexDef(documentDef);
      ftIndexDef.Fields.Add(
        new FullTextFieldDef("DocumentBody", false) {Configuration = "English", TypeFieldName = "Extension"});
      model.FullTextIndexes.Add(ftIndexDef);
    }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel4
{
  public class DefinitionCorrector : IModule
  {
    public void OnBuilt(Domain domain)
    {
    }

    public void OnDefinitionsBuilt(Building.BuildingContext context, DomainModelDef model)
    {
      var documentDef = model.Types[typeof (noFTModel.Document)];
      var ftIndexDef = new FullTextIndexDef(documentDef);
      ftIndexDef.Fields.Add(new FullTextFieldDef("Title", false) {Configuration = "English"});
      ftIndexDef.Fields.Add(
        new FullTextFieldDef("DocumentBody", false) {Configuration = "English", TypeFieldName = "Extension"});
      model.FullTextIndexes.Add(ftIndexDef);
    }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel5
{
  public class DefinitionCorrector : IModule
  {
    public void OnBuilt(Domain domain)
    {
    }

    public void OnDefinitionsBuilt(Building.BuildingContext context, DomainModelDef model)
    {
      var documentDef = model.Types[typeof (noFTModel.Document)];
      var title = documentDef.Fields["Title"];
      var documentBody = documentDef.Fields["DocumentBody"];
      var titleIndex = documentDef.Fields.IndexOf(title);
      var documentBodyIndex = documentDef.Fields.IndexOf(documentBody);
      var oldFieldsMap = documentDef.Fields.ToArray();
      documentDef.Fields.Clear();
      oldFieldsMap[titleIndex] = documentBody;
      oldFieldsMap[documentBodyIndex] = title;
      foreach (var fieldDef in oldFieldsMap)
        documentDef.Fields.Add(fieldDef);

      var ftIndexDef = new FullTextIndexDef(documentDef);

      ftIndexDef.Fields.Add(
        new FullTextFieldDef("DocumentBody", false) {Configuration = "English", TypeFieldName = "Extension"});
      ftIndexDef.Fields.Add(new FullTextFieldDef("Title", false) {Configuration = "English"});
      model.FullTextIndexes.Add(ftIndexDef);
    }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel6
{
  [HierarchyRoot]
  public class Document : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, Key(Position = 1)]
    public int Id2 { get; set; }

    [Field, FullText("English")]
    public string Title { get; set; }

    [Field(Length = 8001)]
    [FullText("English", DataTypeField = "Extension")]
    public byte[] DocumentBody { get; set; }

    [Field(Length = 50)]
    public string Extension { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel7
{
  [HierarchyRoot]
  public class Document : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, FullText("English")]
    public string Title { get; set; }

    [Field(Length = 8001)]
    [FullText("English", DataTypeField = "Extension")]
    public string DocumentBody { get; set; }

    [Field(Length = 50)]
    public string Extension { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel8
{
  [HierarchyRoot]
  public class Document : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, FullText("English")]
    public string Title { get; set; }

    [Field(Length = 8001)]
    [FullText("English", DataTypeField = "Extension")]
    public byte[] DocumentBody { get; set; }

    [Field]
    public string Extension { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel9
{
  [HierarchyRoot]
  public class Document : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, FullText("English")]
    public string Title { get; set; }

    [Field(Length = 8001)]
    [FullText("English", DataTypeField = "Extension")]
    public byte[] DocumentBody { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel10
{
  public class DefinitionCorrector : IModule
  {
    public void OnBuilt(Domain domain)
    {
    }

    public void OnDefinitionsBuilt(Building.BuildingContext context, DomainModelDef model)
    {
      var documentDef = model.Types[typeof (noFTModel.Document)];
      var realExtensionField = documentDef.DefineField("RealExtension", typeof (string));
      realExtensionField.Length = 50;

      var ftIndexDef = new FullTextIndexDef(documentDef);
      ftIndexDef.Fields.Add(
        new FullTextFieldDef("DocumentBody", false) {Configuration = "English", TypeFieldName = "RealExtension"});
      model.FullTextIndexes.Add(ftIndexDef);
    }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTest
{
  [TestFixture]
  public class FullTextDataTypeColumnUpgradeTest
  {
    [OneTimeSetUp]
    public void TestFixtureSetUp()
    {
      CheckRequirements();
    }

    [Test]
    public void RecreateModeNoFullTextIndexesTest()
    {
      using (var domain = BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document)))
      {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(0));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (noFTModel.Document)]);
        Assert.That(typeInfo.FullTextIndex, Is.Null);
      }
    }

    [Test]
    public void RecreateModeOneSimpleFTIndexTest()
    {
      using (var domain = BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document), typeof (simpleFTModel.DefinitionCorrector))) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (noFTModel.Document)]);
        Assert.That(typeInfo.FullTextIndex, Is.Not.Null);
        var fullTextIndex = typeInfo.FullTextIndex;
        Assert.That(fullTextIndex.Columns.Count, Is.EqualTo(1));
        var column = fullTextIndex.Columns[0];
        Assert.That(column.Configuration, Is.EqualTo("English"));
        Assert.That(column.TypeColumn, Is.Null);
        Assert.That(column.Column.Name, Is.EqualTo("Title"));
      }
    }

    [Test]
    public void RecreateModeOneComplexFTIndexTest()
    {
      using (var domain = BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document), typeof (complexFTModel.DefinitionCorrector))) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (noFTModel.Document)]);
        Assert.That(typeInfo.FullTextIndex, Is.Not.Null);
        var fullTextIndex = typeInfo.FullTextIndex;
        Assert.That(fullTextIndex.Columns.Count, Is.EqualTo(1));
        var column = fullTextIndex.Columns[0];
        Assert.That(column.Configuration, Is.EqualTo("English"));
        Assert.That(column.TypeColumn, Is.Not.Null);
        Assert.That(column.TypeColumn.Name, Is.EqualTo("Extension"));
        Assert.That(column.Column.Name, Is.EqualTo("DocumentBody"));
      }
    }

    [Test]
    public void RecreateModeSimplePlusComplexFTIndexesTest()
    {
      using (var domain = BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document), typeof (twoFTIndexesModel1.DefinitionCorrector))) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (noFTModel.Document)]);
        Assert.That(typeInfo.FullTextIndex, Is.Not.Null);
        var fullTextIndex = typeInfo.FullTextIndex;
        Assert.That(fullTextIndex.Columns.Count, Is.EqualTo(2));

        var column = fullTextIndex.Columns[0];
        Assert.That(column.Configuration, Is.EqualTo("English"));
        Assert.That(column.TypeColumn, Is.Null);
        Assert.That(column.Column.Name, Is.EqualTo("Title"));

        column = fullTextIndex.Columns[1];
        Assert.That(column.Configuration, Is.EqualTo("English"));
        Assert.That(column.TypeColumn.Name, Is.EqualTo("Extension"));
        Assert.That(column.Column.Name, Is.EqualTo("DocumentBody"));
      }
    }

    [Test]
    public void RecreateModeComplexPlusSimpleFTIndexesTest()
    {
      using (var domain = BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document), typeof (twoFTIndexesModel2.DefinitionCorrector))) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (noFTModel.Document)]);
        Assert.That(typeInfo.FullTextIndex, Is.Not.Null);
        var fullTextIndex = typeInfo.FullTextIndex;
        Assert.That(fullTextIndex.Columns.Count, Is.EqualTo(2));

        var column = fullTextIndex.Columns[0];
        Assert.That(column.Configuration, Is.EqualTo("English"));
        Assert.That(column.TypeColumn, Is.Not.Null);
        Assert.That(column.TypeColumn.Name, Is.EqualTo("Extension"));
        Assert.That(column.Column.Name, Is.EqualTo("DocumentBody"));

        column = fullTextIndex.Columns[1];
        Assert.That(column.Configuration, Is.EqualTo("English"));
        Assert.That(column.TypeColumn, Is.Null);
        Assert.That(column.Column.Name, Is.EqualTo("Title"));
      }
    }

    [Test]
    public void ValidateModeNoFTIndexTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.Validate, typeof (noFTModel.Document)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(0));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (noFTModel.Document)]);
        Assert.That(typeInfo.FullTextIndex, Is.Null);
      }
    }

    [Test]
    public void ValidateModeSimpleFTIndexTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document), typeof (simpleFTModel.DefinitionCorrector)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.Validate, typeof (noFTModel.Document), typeof (simpleFTModel.DefinitionCorrector)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (noFTModel.Document)]);
        Assert.That(typeInfo.FullTextIndex, Is.Not.Null);
        var fullTextIndex = typeInfo.FullTextIndex;
        Assert.That(fullTextIndex.Columns.Count, Is.EqualTo(1));
        var column = fullTextIndex.Columns[0];
        Assert.That(column.Configuration, Is.EqualTo("English"));
        Assert.That(column.TypeColumn, Is.Null);
        Assert.That(column.Column.Name, Is.EqualTo("Title"));
      }
    }

    [Test]
    public void ValidateModeComplexFTIndexTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document), typeof (complexFTModel.DefinitionCorrector)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.Validate, typeof (noFTModel.Document), typeof (complexFTModel.DefinitionCorrector)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (noFTModel.Document)]);
        Assert.That(typeInfo.FullTextIndex, Is.Not.Null);
        var fullTextIndex = typeInfo.FullTextIndex;
        Assert.That(fullTextIndex.Columns.Count, Is.EqualTo(1));
        var column = fullTextIndex.Columns[0];
        Assert.That(column.Configuration, Is.EqualTo("English"));
        Assert.That(column.TypeColumn, Is.Not.Null);
        Assert.That(column.TypeColumn.Name, Is.EqualTo("Extension"));
        Assert.That(column.Column.Name, Is.EqualTo("DocumentBody"));
      }
    }

    [Test]
    public void ValidateModeSimplePlusComplexFTIndexTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document), typeof (twoFTIndexesModel1.DefinitionCorrector)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.Validate, typeof (noFTModel.Document), typeof (twoFTIndexesModel1.DefinitionCorrector)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (noFTModel.Document)]);
        Assert.That(typeInfo.FullTextIndex, Is.Not.Null);
        var fullTextIndex = typeInfo.FullTextIndex;
        Assert.That(fullTextIndex.Columns.Count, Is.EqualTo(2));

        var column = fullTextIndex.Columns[0];
        Assert.That(column.Configuration, Is.EqualTo("English"));
        Assert.That(column.TypeColumn, Is.Null);
        Assert.That(column.Column.Name, Is.EqualTo("Title"));

        column = fullTextIndex.Columns[1];
        Assert.That(column.Configuration, Is.EqualTo("English"));
        Assert.That(column.TypeColumn.Name, Is.EqualTo("Extension"));
        Assert.That(column.Column.Name, Is.EqualTo("DocumentBody"));
      }
    }

    [Test]
    public void ValidateModeComplexPlusSimpleFTIndexTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document), typeof (twoFTIndexesModel2.DefinitionCorrector)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.Validate, typeof (noFTModel.Document), typeof (twoFTIndexesModel2.DefinitionCorrector)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (noFTModel.Document)]);
        Assert.That(typeInfo.FullTextIndex, Is.Not.Null);
        var fullTextIndex = typeInfo.FullTextIndex;
        Assert.That(fullTextIndex.Columns.Count, Is.EqualTo(2));

        var column = fullTextIndex.Columns[0];
        Assert.That(column.Configuration, Is.EqualTo("English"));
        Assert.That(column.TypeColumn, Is.Not.Null);
        Assert.That(column.TypeColumn.Name, Is.EqualTo("Extension"));
        Assert.That(column.Column.Name, Is.EqualTo("DocumentBody"));

        column = fullTextIndex.Columns[1];
        Assert.That(column.Configuration, Is.EqualTo("English"));
        Assert.That(column.TypeColumn, Is.Null);
        Assert.That(column.Column.Name, Is.EqualTo("Title"));
      }
    }

    [Test]
    public void ValidateModeRemoveSimpleFTIndexTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document), typeof (simpleFTModel.DefinitionCorrector)).Dispose();
      Assert.DoesNotThrow(() => {
        BuildDomain(DomainUpgradeMode.Validate, typeof (noFTModel.Document)).Dispose();
      });
    }

    [Test]
    public void ValidateModeRemoveComplexFTIndexTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document), typeof (complexFTModel.DefinitionCorrector)).Dispose();
      Assert.DoesNotThrow(() => {
        BuildDomain(DomainUpgradeMode.Validate, typeof (noFTModel.Document)).Dispose();
      });
    }

    [Test]
    public void PerformModeNoFTIndexTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.Perform, typeof (noFTModel.Document)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(0));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (noFTModel.Document)]);
        Assert.That(typeInfo.FullTextIndex, Is.Null);
      }
    }

    [Test]
    public void PerformModeAddSimpleFTIndexTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.Perform, typeof (noFTModel.Document), typeof (simpleFTModel.DefinitionCorrector)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (noFTModel.Document)]);
        Assert.That(typeInfo.FullTextIndex, Is.Not.Null);
        var fullTextIndex = typeInfo.FullTextIndex;
        Assert.That(fullTextIndex.Columns.Count, Is.EqualTo(1));
        var column = fullTextIndex.Columns[0];
        Assert.That(column.Configuration, Is.EqualTo("English"));
        Assert.That(column.TypeColumn, Is.Null);
        Assert.That(column.Column.Name, Is.EqualTo("Title"));
      }
    }

    [Test]
    public void PerformModeAddComplexFTIndexTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.Perform, typeof (noFTModel.Document), typeof (complexFTModel.DefinitionCorrector)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (noFTModel.Document)]);
        Assert.That(typeInfo.FullTextIndex, Is.Not.Null);
        var fullTextIndex = typeInfo.FullTextIndex;
        Assert.That(fullTextIndex.Columns.Count, Is.EqualTo(1));
        var column = fullTextIndex.Columns[0];
        Assert.That(column.Configuration, Is.EqualTo("English"));
        Assert.That(column.TypeColumn, Is.Not.Null);
        Assert.That(column.TypeColumn.Name, Is.EqualTo("Extension"));
        Assert.That(column.Column.Name, Is.EqualTo("DocumentBody"));
      }
    }

    [Test]
    public void PerformModeDropSimpleFTIndexTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document), typeof (simpleFTModel.DefinitionCorrector)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.Perform, typeof (noFTModel.Document)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(0));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (noFTModel.Document)]);
        Assert.That(typeInfo.FullTextIndex, Is.Null);
      }
    }

    [Test]
    public void PerformModeSimpleFTIndexTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document), typeof (simpleFTModel.DefinitionCorrector)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.Perform, typeof (noFTModel.Document), typeof (simpleFTModel.DefinitionCorrector)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (noFTModel.Document)]);
        Assert.That(typeInfo.FullTextIndex, Is.Not.Null);
        var fullTextIndex = typeInfo.FullTextIndex;
        Assert.That(fullTextIndex.Columns.Count, Is.EqualTo(1));
        var column = fullTextIndex.Columns[0];
        Assert.That(column.Configuration, Is.EqualTo("English"));
        Assert.That(column.TypeColumn, Is.Null);
        Assert.That(column.Column.Name, Is.EqualTo("Title"));
      }
    }

    [Test]
    public void PerformModeRebuildSimpleFTIndexByComplexOneTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document), typeof (simpleFTModel.DefinitionCorrector)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.Perform, typeof (noFTModel.Document), typeof (complexFTModel.DefinitionCorrector)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (noFTModel.Document)]);
        Assert.That(typeInfo.FullTextIndex, Is.Not.Null);
        var fullTextIndex = typeInfo.FullTextIndex;
        Assert.That(fullTextIndex.Columns.Count, Is.EqualTo(1));
        var column = fullTextIndex.Columns[0];
        Assert.That(column.Configuration, Is.EqualTo("English"));
        Assert.That(column.TypeColumn, Is.Not.Null);
        Assert.That(column.TypeColumn.Name, Is.EqualTo("Extension"));
        Assert.That(column.Column.Name, Is.EqualTo("DocumentBody"));
      }
    }

    [Test]
    public void PerformModeDropComplexFTIndexTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document), typeof (complexFTModel.DefinitionCorrector)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.Perform, typeof (noFTModel.Document)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(0));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (noFTModel.Document)]);
        Assert.That(typeInfo.FullTextIndex, Is.Null);
      }
    }

    [Test]
    public void PerformModeRebuildComplexFTIndexTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document), typeof (complexFTModel.DefinitionCorrector)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.Perform, typeof (noFTModel.Document), typeof (simpleFTModel.DefinitionCorrector)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (noFTModel.Document)]);
        Assert.That(typeInfo.FullTextIndex, Is.Not.Null);
        var fullTextIndex = typeInfo.FullTextIndex;
        Assert.That(fullTextIndex.Columns.Count, Is.EqualTo(1));
        var column = fullTextIndex.Columns[0];
        Assert.That(column.Configuration, Is.EqualTo("English"));
        Assert.That(column.TypeColumn, Is.Null);
        Assert.That(column.Column.Name, Is.EqualTo("Title"));
      }
    }

    [Test]
    public void PerformModeComplexFTIndexTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document), typeof (complexFTModel.DefinitionCorrector)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.Perform, typeof (noFTModel.Document), typeof (complexFTModel.DefinitionCorrector)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (noFTModel.Document)]);
        Assert.That(typeInfo.FullTextIndex, Is.Not.Null);
        var fullTextIndex = typeInfo.FullTextIndex;
        Assert.That(fullTextIndex.Columns.Count, Is.EqualTo(1));
        var column = fullTextIndex.Columns[0];
        Assert.That(column.Configuration, Is.EqualTo("English"));
        Assert.That(column.TypeColumn, Is.Not.Null);
        Assert.That(column.TypeColumn.Name, Is.EqualTo("Extension"));
        Assert.That(column.Column.Name, Is.EqualTo("DocumentBody"));
      }
    }

    [Test]
    public void PerformModeChangedTypeColumnOfFTIndexTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document), typeof (complexFTModel.DefinitionCorrector)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.Perform, typeof (noFTModel.Document), typeof (newTypeColumnModel.DefinitionCorrector)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (noFTModel.Document)]);
        Assert.That(typeInfo.FullTextIndex, Is.Not.Null);
        var fullTextIndex = typeInfo.FullTextIndex;
        Assert.That(fullTextIndex.Columns.Count, Is.EqualTo(1));
        var column = fullTextIndex.Columns[0];
        Assert.That(column.Configuration, Is.EqualTo("English"));
        Assert.That(column.TypeColumn, Is.Not.Null);
        Assert.That(column.TypeColumn.Name, Is.EqualTo("RealExtension"));
        Assert.That(column.Column.Name, Is.EqualTo("DocumentBody"));
      }
    }

    [Test]
    public void PerformSafelyModeNoFTIndexTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.PerformSafely, typeof (noFTModel.Document)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(0));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (noFTModel.Document)]);
        Assert.That(typeInfo.FullTextIndex, Is.Null);
      }
    }

    [Test]
    public void PerformSafelyModeAddSimpleFTIndexTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.PerformSafely, typeof (noFTModel.Document), typeof (simpleFTModel.DefinitionCorrector)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (noFTModel.Document)]);
        Assert.That(typeInfo.FullTextIndex, Is.Not.Null);
        var fullTextIndex = typeInfo.FullTextIndex;
        Assert.That(fullTextIndex.Columns.Count, Is.EqualTo(1));
        var column = fullTextIndex.Columns[0];
        Assert.That(column.Configuration, Is.EqualTo("English"));
        Assert.That(column.TypeColumn, Is.Null);
        Assert.That(column.Column.Name, Is.EqualTo("Title"));
      }
    }

    [Test]
    public void PerformSafelyModeAddComplexFTIndexTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.PerformSafely, typeof (noFTModel.Document), typeof (complexFTModel.DefinitionCorrector)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (noFTModel.Document)]);
        Assert.That(typeInfo.FullTextIndex, Is.Not.Null);
        var fullTextIndex = typeInfo.FullTextIndex;
        Assert.That(fullTextIndex.Columns.Count, Is.EqualTo(1));
        var column = fullTextIndex.Columns[0];
        Assert.That(column.Configuration, Is.EqualTo("English"));
        Assert.That(column.TypeColumn, Is.Not.Null);
        Assert.That(column.TypeColumn.Name, Is.EqualTo("Extension"));
        Assert.That(column.Column.Name, Is.EqualTo("DocumentBody"));
      }
    }

    [Test]
    public void PerformSafelyModeDropSimpleFTIndexTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document), typeof (simpleFTModel.DefinitionCorrector)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.PerformSafely, typeof (noFTModel.Document)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(0));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (noFTModel.Document)]);
        Assert.That(typeInfo.FullTextIndex, Is.Null);
      }
    }

    [Test]
    public void PerformSafelyModeSimpleFTIndexTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document), typeof (noFTModel.Document)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.PerformSafely, typeof (noFTModel.Document), typeof (simpleFTModel.DefinitionCorrector)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (noFTModel.Document)]);
        Assert.That(typeInfo.FullTextIndex, Is.Not.Null);
        var fullTextIndex = typeInfo.FullTextIndex;
        Assert.That(fullTextIndex.Columns.Count, Is.EqualTo(1));
        var column = fullTextIndex.Columns[0];
        Assert.That(column.Configuration, Is.EqualTo("English"));
        Assert.That(column.TypeColumn, Is.Null);
        Assert.That(column.Column.Name, Is.EqualTo("Title"));
      }
    }

    [Test]
    public void PerformSafelyModeRebuildSimpleFTIndexByComplexOneTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document), typeof (simpleFTModel.DefinitionCorrector)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.PerformSafely, typeof (noFTModel.Document), typeof (complexFTModel.DefinitionCorrector)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (noFTModel.Document)]);
        Assert.That(typeInfo.FullTextIndex, Is.Not.Null);
        var fullTextIndex = typeInfo.FullTextIndex;
        Assert.That(fullTextIndex.Columns.Count, Is.EqualTo(1));
        var column = fullTextIndex.Columns[0];
        Assert.That(column.Configuration, Is.EqualTo("English"));
        Assert.That(column.TypeColumn, Is.Not.Null);
        Assert.That(column.TypeColumn.Name, Is.EqualTo("Extension"));
        Assert.That(column.Column.Name, Is.EqualTo("DocumentBody"));
      }
    }

    [Test]
    public void PerformSafelyModeDropComplexFTIndexTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document), typeof (complexFTModel.DefinitionCorrector)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.PerformSafely, typeof (noFTModel.Document)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(0));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (noFTModel.Document)]);
        Assert.That(typeInfo.FullTextIndex, Is.Null);
      }
    }

    [Test]
    public void PerformSafelyModeRebuildComplexFTIndexTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document), typeof (complexFTModel.DefinitionCorrector)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.PerformSafely, typeof (noFTModel.Document), typeof (simpleFTModel.DefinitionCorrector)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (noFTModel.Document)]);
        Assert.That(typeInfo.FullTextIndex, Is.Not.Null);
        var fullTextIndex = typeInfo.FullTextIndex;
        Assert.That(fullTextIndex.Columns.Count, Is.EqualTo(1));
        var column = fullTextIndex.Columns[0];
        Assert.That(column.Configuration, Is.EqualTo("English"));
        Assert.That(column.TypeColumn, Is.Null);
        Assert.That(column.Column.Name, Is.EqualTo("Title"));
      }
    }

    [Test]
    public void PerformSafelyModeComplexFTIndexTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document), typeof (complexFTModel.DefinitionCorrector)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.PerformSafely, typeof (noFTModel.Document), typeof (complexFTModel.DefinitionCorrector)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (noFTModel.Document)]);
        Assert.That(typeInfo.FullTextIndex, Is.Not.Null);
        var fullTextIndex = typeInfo.FullTextIndex;
        Assert.That(fullTextIndex.Columns.Count, Is.EqualTo(1));
        var column = fullTextIndex.Columns[0];
        Assert.That(column.Configuration, Is.EqualTo("English"));
        Assert.That(column.TypeColumn, Is.Not.Null);
        Assert.That(column.TypeColumn.Name, Is.EqualTo("Extension"));
        Assert.That(column.Column.Name, Is.EqualTo("DocumentBody"));
      }
    }

    [Test]
    public void PerformSafelyModeChangedTypeColumnOfFTIndexTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document), typeof (complexFTModel.DefinitionCorrector)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.PerformSafely, typeof (noFTModel.Document), typeof (newTypeColumnModel.DefinitionCorrector)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (noFTModel.Document)]);
        Assert.That(typeInfo.FullTextIndex, Is.Not.Null);
        var fullTextIndex = typeInfo.FullTextIndex;
        Assert.That(fullTextIndex.Columns.Count, Is.EqualTo(1));
        var column = fullTextIndex.Columns[0];
        Assert.That(column.Configuration, Is.EqualTo("English"));
        Assert.That(column.TypeColumn, Is.Not.Null);
        Assert.That(column.TypeColumn.Name, Is.EqualTo("RealExtension"));
        Assert.That(column.Column.Name, Is.EqualTo("DocumentBody"));
      }
    }

    [Test]
    public void SkipModeNoFullTextIndexesTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document)).Dispose();

      using (var domain = BuildDomain(DomainUpgradeMode.Skip, typeof (noFTModel.Document))) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(0));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (noFTModel.Document)]);
        Assert.That(typeInfo.FullTextIndex, Is.Null);
      }
    }

    [Test]
    public void SkipModeOneSimpleFTIndexTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document), typeof (simpleFTModel.DefinitionCorrector)).Dispose();

      using (var domain = BuildDomain(DomainUpgradeMode.Skip, typeof (noFTModel.Document), typeof (simpleFTModel.DefinitionCorrector))) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (noFTModel.Document)]);
        Assert.That(typeInfo.FullTextIndex, Is.Not.Null);
        var fullTextIndex = typeInfo.FullTextIndex;
        Assert.That(fullTextIndex.Columns.Count, Is.EqualTo(1));
        var column = fullTextIndex.Columns[0];
        Assert.That(column.Configuration, Is.EqualTo("English"));
        Assert.That(column.TypeColumn, Is.Null);
        Assert.That(column.Column.Name, Is.EqualTo("Title"));
      }
    }

    [Test]
    public void SkipModeOneComplexFTIndexTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document), typeof (complexFTModel.DefinitionCorrector)).Dispose();

      using (var domain = BuildDomain(DomainUpgradeMode.Skip, typeof (noFTModel.Document), typeof (complexFTModel.DefinitionCorrector))) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (noFTModel.Document)]);
        Assert.That(typeInfo.FullTextIndex, Is.Not.Null);
        var fullTextIndex = typeInfo.FullTextIndex;
        Assert.That(fullTextIndex.Columns.Count, Is.EqualTo(1));
        var column = fullTextIndex.Columns[0];
        Assert.That(column.Configuration, Is.EqualTo("English"));
        Assert.That(column.TypeColumn, Is.Not.Null);
        Assert.That(column.TypeColumn.Name, Is.EqualTo("Extension"));
        Assert.That(column.Column.Name, Is.EqualTo("DocumentBody"));
      }
    }

    [Test]
    public void SkipModeSimplePlusComplexFTIndexesTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document), typeof (twoFTIndexesModel1.DefinitionCorrector)).Dispose();

      using (var domain = BuildDomain(DomainUpgradeMode.Skip, typeof (noFTModel.Document), typeof (twoFTIndexesModel1.DefinitionCorrector))) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (noFTModel.Document)]);
        Assert.That(typeInfo.FullTextIndex, Is.Not.Null);
        var fullTextIndex = typeInfo.FullTextIndex;
        Assert.That(fullTextIndex.Columns.Count, Is.EqualTo(2));

        var column = fullTextIndex.Columns[0];
        Assert.That(column.Configuration, Is.EqualTo("English"));
        Assert.That(column.TypeColumn, Is.Null);
        Assert.That(column.Column.Name, Is.EqualTo("Title"));

        column = fullTextIndex.Columns[1];
        Assert.That(column.Configuration, Is.EqualTo("English"));
        Assert.That(column.TypeColumn.Name, Is.EqualTo("Extension"));
        Assert.That(column.Column.Name, Is.EqualTo("DocumentBody"));
      }
    }

    [Test]
    public void SkipModeComplexPlusSimpleFTIndexesTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document), typeof (twoFTIndexesModel2.DefinitionCorrector)).Dispose();

      using (var domain = BuildDomain(DomainUpgradeMode.Skip, typeof (noFTModel.Document), typeof (twoFTIndexesModel2.DefinitionCorrector))) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (noFTModel.Document)]);
        Assert.That(typeInfo.FullTextIndex, Is.Not.Null);
        var fullTextIndex = typeInfo.FullTextIndex;
        Assert.That(fullTextIndex.Columns.Count, Is.EqualTo(2));

        var column = fullTextIndex.Columns[0];
        Assert.That(column.Configuration, Is.EqualTo("English"));
        Assert.That(column.TypeColumn, Is.Not.Null);
        Assert.That(column.TypeColumn.Name, Is.EqualTo("Extension"));
        Assert.That(column.Column.Name, Is.EqualTo("DocumentBody"));

        column = fullTextIndex.Columns[1];
        Assert.That(column.Configuration, Is.EqualTo("English"));
        Assert.That(column.TypeColumn, Is.Null);
        Assert.That(column.Column.Name, Is.EqualTo("Title"));
      }
    }

    [Test]
    public void PrimaryIndexContainsTwoColumnsTest()
    {
      Assert.Throws<StorageException>(
        () => BuildDomain(DomainUpgradeMode.Recreate, typeof (wrongModel1.Document)).Dispose());
    }

    [Test]
    public void WrongTypeOfFullTextColumnTest()
    {
      Assert.Throws<StorageException>(
        () => BuildDomain(DomainUpgradeMode.Recreate, typeof (wrongModel2.Document)).Dispose());
    }

    [Test]
    public void WrongTypeOfTypeColumnTest()
    {
      Assert.Throws<StorageException>(
        () => BuildDomain(DomainUpgradeMode.Recreate, typeof (wrongModel3.Document)).Dispose());
    }

    [Test]
    public void TypeColumnIsNotExistsInEntity()
    {
      Assert.Throws<DomainBuilderException>(
        () => BuildDomain(DomainUpgradeMode.Recreate, typeof (wrongModel4.Document)).Dispose());
    }

    private Domain BuildDomain(DomainUpgradeMode mode, Type baseType, Type additionalType = null)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = mode;
      configuration.Types.Register(baseType);
      if (additionalType!=null)
        configuration.Types.Register(additionalType);
      return Domain.Build(configuration);
    }

    private void CheckRequirements()
    {
      Require.AnyFeatureSupported(ProviderFeatures.FullTextColumnDataTypeSpecification);
    }
  }
}
