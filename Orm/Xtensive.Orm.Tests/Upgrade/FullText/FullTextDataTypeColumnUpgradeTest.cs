// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.11.14

using System;
using System.Linq;
using NUnit.Framework;
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
  [HierarchyRoot]
  public class Document : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field, FullText("English")]
    public string Title { get; set; }

    [Field(Length = 8001)]
    public byte[] DocumentBody { get; set; }

    [Field(Length = 50)]
    public string Extension { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel3
{
  [HierarchyRoot]
  public class Document : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Title { get; set; }

    [Field(Length = 8001)]
    [FullText("English", DataTypeField = "Extension")]
    public byte[] DocumentBody { get; set; }

    [Field(Length = 50)]
    public string Extension { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel4
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

    [Field(Length = 50)]
    public string Extension { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Upgrade.FullTextDataTypeColumnUpgrageTestModel5
{
  [HierarchyRoot]
  public class Document : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field(Length = 8001)]
    [FullText("English", DataTypeField = "Extension")]
    public byte[] DocumentBody { get; set; }

    [Field(Length = 50)]
    public string Extension { get; set; }

    [Field, FullText("English")]
    public string Title { get; set; }
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
  [HierarchyRoot]
  public class Document : Entity
  {
    [Field, Key]
    public int Id { get; set; }

    [Field]
    public string Title { get; set; }

    [Field(Length = 8001)]
    [FullText("English", DataTypeField = "RealExtension")]
    public byte[] DocumentBody { get; set; }

    [Field(Length = 50)]
    public string Extension { get; set; }

    [Field(Length = 50)]
    public string RealExtension { get; set; }
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
      using (var domain = BuildDomain(DomainUpgradeMode.Recreate, typeof (noFTModel.Document))) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(0));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (noFTModel.Document)]);
        Assert.That(typeInfo.FullTextIndex, Is.Null);
      }
    }

    [Test]
    public void RecreateModeOneSimpleFTIndexTest()
    {
      using (var domain = BuildDomain(DomainUpgradeMode.Recreate, typeof (simpleFTModel.Document))) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (simpleFTModel.Document)]);
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
      using (var domain = BuildDomain(DomainUpgradeMode.Recreate, typeof (complexFTModel.Document))) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (complexFTModel.Document)]);
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
      using (var domain = BuildDomain(DomainUpgradeMode.Recreate, typeof (twoFTIndexesModel1.Document)))       {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (twoFTIndexesModel1.Document)]);
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
      using (var domain = BuildDomain(DomainUpgradeMode.Recreate, typeof (twoFTIndexesModel2.Document))) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (twoFTIndexesModel2.Document)]);
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
      BuildDomain(DomainUpgradeMode.Recreate, typeof (simpleFTModel.Document)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.Validate, typeof (simpleFTModel.Document)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (simpleFTModel.Document)]);
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
      BuildDomain(DomainUpgradeMode.Recreate, typeof (complexFTModel.Document)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.Validate, typeof (complexFTModel.Document)));

      using (domain)       {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (complexFTModel.Document)]);
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
      BuildDomain(DomainUpgradeMode.Recreate, typeof (twoFTIndexesModel1.Document)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.Validate, typeof (twoFTIndexesModel1.Document)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (twoFTIndexesModel1.Document)]);
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
      BuildDomain(DomainUpgradeMode.Recreate, typeof (twoFTIndexesModel2.Document)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.Validate, typeof (twoFTIndexesModel2.Document)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (twoFTIndexesModel2.Document)]);
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
      BuildDomain(DomainUpgradeMode.Recreate, typeof (simpleFTModel.Document)).Dispose();
      Assert.DoesNotThrow(() =>
      {
        BuildDomain(DomainUpgradeMode.Validate, typeof (noFTModel.Document)).Dispose();
      });
    }

    [Test]
    public void ValidateModeRemoveComplexFTIndexTest()
    {
      BuildDomain(DomainUpgradeMode.Recreate, typeof (complexFTModel.Document)).Dispose();
      Assert.DoesNotThrow(() =>
      {
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
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.Perform, typeof (simpleFTModel.Document)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (simpleFTModel.Document)]);
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
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.Perform, typeof (complexFTModel.Document)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (complexFTModel.Document)]);
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
      BuildDomain(DomainUpgradeMode.Recreate, typeof (simpleFTModel.Document)).Dispose();

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
      BuildDomain(DomainUpgradeMode.Recreate, typeof (simpleFTModel.Document)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.Perform, typeof (simpleFTModel.Document)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (simpleFTModel.Document)]);
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
      BuildDomain(DomainUpgradeMode.Recreate, typeof (simpleFTModel.Document)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.Perform, typeof (complexFTModel.Document)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (complexFTModel.Document)]);
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
      BuildDomain(DomainUpgradeMode.Recreate, typeof (complexFTModel.Document)).Dispose();

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
      BuildDomain(DomainUpgradeMode.Recreate, typeof (complexFTModel.Document)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.Perform, typeof (simpleFTModel.Document)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (simpleFTModel.Document)]);
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
      BuildDomain(DomainUpgradeMode.Recreate, typeof (complexFTModel.Document)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.Perform, typeof (complexFTModel.Document)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (complexFTModel.Document)]);
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
      BuildDomain(DomainUpgradeMode.Recreate, typeof (complexFTModel.Document)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.Perform, typeof (newTypeColumnModel.Document)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (newTypeColumnModel.Document)]);
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
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.PerformSafely, typeof (simpleFTModel.Document)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (simpleFTModel.Document)]);
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
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.PerformSafely, typeof (complexFTModel.Document)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (complexFTModel.Document)]);
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
      BuildDomain(DomainUpgradeMode.Recreate, typeof (simpleFTModel.Document)).Dispose();

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
      BuildDomain(DomainUpgradeMode.Recreate, typeof (simpleFTModel.Document)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.PerformSafely, typeof (simpleFTModel.Document)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (simpleFTModel.Document)]);
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
      BuildDomain(DomainUpgradeMode.Recreate, typeof (simpleFTModel.Document)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.PerformSafely, typeof (complexFTModel.Document)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (complexFTModel.Document)]);
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
      BuildDomain(DomainUpgradeMode.Recreate, typeof (complexFTModel.Document)).Dispose();

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
      BuildDomain(DomainUpgradeMode.Recreate, typeof (complexFTModel.Document)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.PerformSafely, typeof (simpleFTModel.Document)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (simpleFTModel.Document)]);
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
      BuildDomain(DomainUpgradeMode.Recreate, typeof (complexFTModel.Document)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.PerformSafely, typeof (complexFTModel.Document)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (complexFTModel.Document)]);
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
      BuildDomain(DomainUpgradeMode.Recreate, typeof (complexFTModel.Document)).Dispose();

      Domain domain = null;
      Assert.DoesNotThrow(() => domain = BuildDomain(DomainUpgradeMode.PerformSafely, typeof (newTypeColumnModel.Document)));

      using (domain) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (newTypeColumnModel.Document)]);
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
      BuildDomain(DomainUpgradeMode.Recreate, typeof (simpleFTModel.Document)).Dispose();

      using (var domain = BuildDomain(DomainUpgradeMode.Skip, typeof (simpleFTModel.Document))) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (simpleFTModel.Document)]);
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
      BuildDomain(DomainUpgradeMode.Recreate, typeof (complexFTModel.Document)).Dispose();

      using (var domain = BuildDomain(DomainUpgradeMode.Skip, typeof (complexFTModel.Document))) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (complexFTModel.Document)]);
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
      BuildDomain(DomainUpgradeMode.Recreate, typeof (twoFTIndexesModel1.Document)).Dispose();

      using (var domain = BuildDomain(DomainUpgradeMode.Skip, typeof (twoFTIndexesModel1.Document))) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (twoFTIndexesModel1.Document)]);
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
      BuildDomain(DomainUpgradeMode.Recreate, typeof (twoFTIndexesModel2.Document)).Dispose();

      using (var domain = BuildDomain(DomainUpgradeMode.Skip, typeof (twoFTIndexesModel2.Document))) {
        Assert.That(domain.Model.FullTextIndexes.Count(), Is.EqualTo(1));
        TypeInfo typeInfo = null;
        Assert.DoesNotThrow(() => typeInfo = domain.Model.Types[typeof (twoFTIndexesModel2.Document)]);
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

    private Domain BuildDomain(DomainUpgradeMode mode, Type type)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = mode;
      configuration.Types.Register(type);
      return Domain.Build(configuration);
    }

    private void CheckRequirements()
    {
      Require.AnyFeatureSupported(ProviderFeatures.FullTextColumnDataTypeSpecification);
    }
  }
}
