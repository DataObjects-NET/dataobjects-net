// Copyright (C) 2003-2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Julian Mamokin
// Created:    2017.09.01

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0710_IndirectStructureChildFKAbsenceModel;

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
    public AnotherTestEntity AnotherTestEntity2 { get; set; }
  }

  [HierarchyRoot]
  public class AnotherTestEntity : Entity
  {
    [Field, Key]
    public long Id { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class IssueJira0710_IndirectStructureChildFKAbsence : AutoBuildTest
  {
    [Test]
    public void MainTest()
    {
      var testableType = Domain.Model.Types[typeof (TestEntity)];

      var expectedIndex = testableType.Indexes["TestEntity.FK_TestStructureChild.AnotherTestEntity"];
      Assert.NotNull(expectedIndex);
      Assert.IsTrue(expectedIndex.Columns.Any(c => c.Name=="TestStructureChild.AnotherTestEntity"));
      Assert.IsTrue(expectedIndex.IsSecondary);

      var expectedIndex2 = testableType.Indexes["TestEntity.FK_TestStructureChild.AnotherTestEntity2"];
      Assert.NotNull(expectedIndex2);
      Assert.IsTrue(expectedIndex2.Columns.Any(c => c.Name=="TestStructureChild.AnotherTestEntity2"));
      Assert.IsTrue(expectedIndex2.IsSecondary);
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
