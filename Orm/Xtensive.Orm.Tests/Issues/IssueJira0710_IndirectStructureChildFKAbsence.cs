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

      Assert.IsTrue(foreignKeys.Count==2);
      Assert.IsTrue(foreignKeys.All(c => c.Columns.Count==1));

      var foreignKey1Column = foreignKeys[0].Columns.Single();
      var expectedColumnName = testableType.Fields["TestStructureChild.AnotherTestEntity"].Columns.Single().Name;
      Assert.IsTrue(foreignKey1Column.Name==expectedColumnName);
      Assert.IsTrue(foreignKey1Column.Table.Name.Equals("TestEntity", StringComparison.InvariantCultureIgnoreCase));
      Assert.IsTrue(foreignKeys[0].ReferencedTable.Name.Equals("AnotherTestEntity", StringComparison.InvariantCultureIgnoreCase));

      var foreignKey2Column = foreignKeys[1].Columns.Single();
      expectedColumnName = testableType.Fields["TestStructureChild.AnotherTestEntity2"].Columns.Single().Name;
      Assert.IsTrue(foreignKey2Column.Name==expectedColumnName);
      Assert.IsTrue(foreignKey2Column.Table.Name.Equals("TestEntity", StringComparison.InvariantCultureIgnoreCase));
      Assert.IsTrue(foreignKeys[1].ReferencedTable.Name.Equals("OneMoreTestEntity", StringComparison.InvariantCultureIgnoreCase));
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.Register(typeof (TestEntity).Assembly, typeof (TestEntity).Namespace);
      return config;
    }
  }
}
