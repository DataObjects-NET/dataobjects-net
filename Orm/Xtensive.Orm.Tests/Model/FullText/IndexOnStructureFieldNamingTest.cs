// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Julian Mamokin
// Created:    2016.11.15

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Model.FullTextIndexOnStructureFieldNamingTestModel;

namespace Xtensive.Orm.Tests.Model.FullTextIndexOnStructureFieldNamingTestModel
{
  [HierarchyRoot]
  public class HierarchyWithFullTextIndex : BaseEntity
  {
    [Field]
    [FieldMapping("structureFieldMapping")]
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
    [FieldMapping("structureFieldMapping")]
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
    [FieldMapping("structureFieldMapping")]
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
    [FieldMapping("structureFieldMapping")]
    public StructureWithIndexedField StructureWithIndexedField1 { get; set; }

    [Field]
    public StructureWithIndexedField StructureWithIndexedField2 { get; set; }

    [Field]
    public AnotherStructure StructureWithAnotherIndexedField { get; set; }
  }

  public class StructureWithIndexedField : Structure
  {
    [Field, FullText("English")]
    [FieldMapping("IndexedStringFieldMapping")]
    public string IndexedStringField { get; set; }

    [Field, FullText("English")]
    [FieldMapping("IndexedStringFieldMapping2")]
    public string IndexedStringField1 { get; set; }

    [Field, FullText("English")]
    public string IndexedStringField2 { get; set; }

    [Field]
    public string UnindexedStringField { get; set; }
  }

  public class AnotherStructure : Structure
  {
    [Field, FullText("English")]
    [FieldMapping("AnotherStructureFieldMapping")]
    public string StringField { get; set; }

    [Field, FullText("Russian")]
    public string StringField1 { get; set; }

    [Field, FullText("Russian")]
    [FieldMapping("AnotherStructureFieldMapping1")]
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
  public class IndexOnStructureFieldNamingTest : AutoBuildTest
  {
    private static readonly List<string> UnderscoreRuleExpectedColumns = new List<string> {
      "structureFieldMapping_IndexedStringFieldMapping",
      "structureFieldMapping_IndexedStringFieldMapping2",
      "StructureWithIndexedField2_IndexedStringFieldMapping",
      "StructureWithIndexedField2_IndexedStringFieldMapping2",
      "StructureWithAnotherIndexedField_AnotherStructureFieldMapping",
      "StructureWithAnotherIndexedField_StringField1",
      "StructureWithAnotherIndexedField_AnotherStructureFieldMapping1"
    };

    [Test]
    public void FullTextIndexPresenceTest()
    {
      Assert.That(Domain.Model.FullTextIndexes.Count(), Is.EqualTo(4));
    }

    [Test]
    public void HierarchyWithFullTextIndexTest()
    {
      var hierarchy = Domain.Model.Types[typeof (HierarchyWithFullTextIndex)];
      var hierarchyTableColumnNames = hierarchy.Columns.Select(c => c.Name).ToList();
      var hierarchyIndexColumnNames = hierarchy.FullTextIndex.Columns.Select(c => c.Name).ToList();
      Assert.IsTrue(hierarchy.FullTextIndex.Columns.Count==10);
      Assert.IsTrue(hierarchyTableColumnNames.ContainsAll(hierarchyIndexColumnNames));
    }

    [Test]
    public void ClassTableIndexUnderscoreDotsRuleTest()
    {
      var classTable = Domain.Model.Types[typeof (ClassTableHierarchy)];
      var classTableColumnNames = classTable.Columns.Select(c => c.Name).ToList();
      var classtableIndexColumnNames = classTable.FullTextIndex.Columns.Select(c => c.Name).ToList();
      Assert.IsTrue(classTable.FullTextIndex.Columns.Count==9);
      Assert.IsTrue(classTableColumnNames.ContainsAll(classtableIndexColumnNames));
      foreach (var columnName in UnderscoreRuleExpectedColumns) {
        Assert.IsTrue(classtableIndexColumnNames.Contains(columnName));
      }
    }

    [Test]
    public void ConcreteTableIndexUnderscoreDotsRuleTest()
    {
      var concreteTable = Domain.Model.Types[typeof (ConcreteTableHierarchy)];
      var concreteTableColumnNames = concreteTable.Columns.Select(c => c.Name).ToList();
      var concreteTableIndexColumnNames = concreteTable.FullTextIndex.Columns.Select(c => c.Name).ToList();
      Assert.IsTrue(concreteTable.FullTextIndex.Columns.Count==9);
      Assert.IsTrue(concreteTableColumnNames.ContainsAll(concreteTableIndexColumnNames));
      foreach (var columnName in UnderscoreRuleExpectedColumns) {
        Assert.IsTrue(concreteTableIndexColumnNames.Contains(columnName));
      }
    }

    [Test]
    public void SingleTableUnderscoreDotsRuleTest()
    {
      var singleTable = Domain.Model.Types[typeof (SingleTableHierarchy)];
      var singleTableColumnNames = singleTable.Columns.Select(c => c.Name).ToList().ToList();
      var singleTableIndexColumnNames = singleTable.FullTextIndex.Columns.Select(c => c.Name).ToList();
      Assert.IsTrue(singleTable.FullTextIndex.Columns.Count==9);
      Assert.IsTrue(singleTableColumnNames.ContainsAll(singleTableIndexColumnNames));
      foreach (var columnName in UnderscoreRuleExpectedColumns) {
        Assert.IsTrue(singleTableIndexColumnNames.Contains(columnName));
      }
    }

    protected override void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.FullText);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (ClassTableHierarchy).Assembly, typeof (ClassTableHierarchy).Namespace);
      config.NamingConvention.NamingRules = NamingRules.UnderscoreDots;
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      return config;
    }
  }
}
