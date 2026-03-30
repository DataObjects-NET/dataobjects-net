// Copyright (C) 2003-2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Julian Mamokin
// Created:    2017.09.01

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Issues.IssueJira0710_IndirectStructureChildFKAbsenceModel;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Tests.Issues.IssueJira0710_IndirectStructureChildFKAbsenceModel
{
  [HierarchyRoot]
  public class TestEntity : Entity
  {
    [Field, Key]
    public long Id { get; set; }

    [Field]
    public TestStructureChild TestStructureChild { get; set; }
  }

  public class TestStructure : Structure
  {
    [Field]
    public AnotherTestEntity AnotherTestEntity { get; set; }
  }

  public class TestStructureChild : TestStructure
  {
    [Field]
    public OneMoreTestEntity AnotherTestEntity2 { get; set; }
  }

  [HierarchyRoot]
  public class AnotherTestEntity : Entity
  {
    [Field, Key]
    public long Id { get; set; }
  }

  [HierarchyRoot]
  public class OneMoreTestEntity : Entity
  {
    [Field, Key]
    public long Id { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0710_IndirectStructureChildFKAbsence : AutoBuildTest
  {
    protected override void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.ForeignKeyConstraints);
    }

    [Test]
    public void MainTest()
    {
      var testableType = Domain.Model.Types[typeof (TestEntity)];
      var foreignKeys = Domain.StorageNodeManager.GetNode(WellKnown.DefaultNodeId).Mapping[testableType]
        .TableConstraints.OfType<ForeignKey>()
        .OrderBy(i => i.ReferencedTable.Name).ToList();

      Assert.That(foreignKeys.Count==2, Is.True);
      Assert.That(foreignKeys.All(c => c.Columns.Count==1), Is.True);

      var foreignKey1Column = foreignKeys[0].Columns.Single();
      var expectedColumnName = testableType.Fields["TestStructureChild.AnotherTestEntity"].Columns.Single().Name;
      Assert.That(foreignKey1Column.Name==expectedColumnName, Is.True);
      Assert.That(foreignKey1Column.Table.Name.Equals("TestEntity", StringComparison.InvariantCultureIgnoreCase), Is.True);
      Assert.That(foreignKeys[0].ReferencedTable.Name.Equals("AnotherTestEntity", StringComparison.InvariantCultureIgnoreCase), Is.True);

      var foreignKey2Column = foreignKeys[1].Columns.Single();
      expectedColumnName = testableType.Fields["TestStructureChild.AnotherTestEntity2"].Columns.Single().Name;
      Assert.That(foreignKey2Column.Name==expectedColumnName, Is.True);
      Assert.That(foreignKey2Column.Table.Name.Equals("TestEntity", StringComparison.InvariantCultureIgnoreCase), Is.True);
      Assert.That(foreignKeys[1].ReferencedTable.Name.Equals("OneMoreTestEntity", StringComparison.InvariantCultureIgnoreCase), Is.True);
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.RegisterCaching(typeof (TestEntity).Assembly, typeof (TestEntity).Namespace);
      return config;
    }
  }
}
