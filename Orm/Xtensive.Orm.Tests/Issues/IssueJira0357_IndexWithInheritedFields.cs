// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.12.30

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using concreteTableModel = Xtensive.Orm.Tests.Issues.IssueJira0357_IndexWithInheritedFields1;
using singleTableModel = Xtensive.Orm.Tests.Issues.IssueJira0357_IndexWithInheritedFields2;
using classTableModel = Xtensive.Orm.Tests.Issues.IssueJira0357_IndexWithInheritedFields3;


namespace Xtensive.Orm.Tests.Issues.IssueJira0357_IndexWithInheritedFields1
{
  [HierarchyRoot]
  public class BaseEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  [HierarchyRoot(InheritanceSchema = InheritanceSchema.ConcreteTable)]
  [Index("TestField", "TestLink", Unique = true, Name = "DescendantA.IX_TestFieldTestLink")]
  public class EntityA : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string TestField { get; set; }

    [Field(Nullable = false)]
    public EntityB TestLink { get; set; }
  }

  [HierarchyRoot]
  public class EntityB : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string TestField { get; set; }
  }

  [Index("Field", "TestField", Unique = true, Name = "DescendantA.IX_FieldTestField")]
  [Index("Field", Unique = true, Name = "DescendantA.IX_Field")]
  [Index("TestField", Unique = true, Name = "DescendantA.IX_TestField")]
  public class DescendantA : EntityA
  {
    [Field]
    public int Field { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Issues.IssueJira0357_IndexWithInheritedFields2
{
  [HierarchyRoot]
  public class BaseEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  [HierarchyRoot(InheritanceSchema = InheritanceSchema.SingleTable)]
  [Index("TestField", "TestLink", Unique = true, Name = "DescendantA.IX_TestFieldTestLink")]
  [Index("TestField", Unique = true, Name = "DescendantA.IX_TestField")]
  public class EntityA : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string TestField { get; set; }

    [Field(Nullable = false)]
    public EntityB TestLink { get; set; }
  }

  [HierarchyRoot]
  public class EntityB : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string TestField { get; set; }
  }

  [Index("Field", "TestField", Unique = true, Name = "DescendantA.IX_FieldTestField")]
  [Index("Field", Unique = true, Name = "DescendantA.IX_Field")]
  public class DescendantA : EntityA
  {
    [Field]
    public int Field { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Issues.IssueJira0357_IndexWithInheritedFields3
{
  [HierarchyRoot]
  public class BaseEntity : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  [HierarchyRoot(InheritanceSchema = InheritanceSchema.ClassTable)]
  [Index("TestField", "TestLink", Unique = true, Name = "DescendantA.IX_TestFieldTestLink")]
  public class EntityA : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string TestField { get; set; }

    [Field(Nullable = false)]
    public EntityB TestLink { get; set; }
  }

  [HierarchyRoot]
  public class EntityB : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string TestField { get; set; }
  }

  [Index("Field", Unique = true, Name = "DescendantA.IX_Field")]
  public class DescendantA : EntityA
  {
    [Field]
    public int Field { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class IssueJira0357_IndexWithInheritedFields : AutoBuildTest
  {
    protected DomainConfiguration BuildConfiguration(Type type)
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(type.Assembly, type.Namespace);
      return configuration;
    }

    [Test]
    public void ConcreteTableTest()
    {
      using (var domain = BuildDomain(BuildConfiguration(typeof (concreteTableModel.DescendantA)))) {
        var descendantType = domain.Model.Types[typeof (concreteTableModel.DescendantA)];
        var secondaryIndexes = descendantType.Indexes.Where(index => index.IsSecondary && !index.IsVirtual && index.IsUnique);
        Assert.AreEqual(4, secondaryIndexes.Count());
        var entityAType = domain.Model.Types[typeof (concreteTableModel.EntityA)];
        secondaryIndexes = entityAType.Indexes.Where(index => index.IsSecondary && !index.IsVirtual && index.IsUnique);
        Assert.AreEqual(1, secondaryIndexes.Count());
      }
    }

    [Test]
    public void SingleTableTest()
    {
      using (var domain = BuildDomain(BuildConfiguration(typeof (singleTableModel.DescendantA)))) {
        var type = domain.Model.Types[typeof (singleTableModel.EntityA)];
        var secondaryIndexes = type.Indexes.Where(index => index.IsSecondary && !index.IsVirtual && index.IsUnique);
        Assert.AreEqual(4, secondaryIndexes.Count());
      }
    }

    [Test]
    public void ClassTableTest()
    {
      using (var domain = BuildDomain(BuildConfiguration(typeof (classTableModel.DescendantA)))) {
        var type = domain.Model.Types[typeof (classTableModel.DescendantA)];
        var secondaryIndexes = type.Indexes.Where(index => index.IsSecondary && !index.IsVirtual);
        Assert.AreEqual(1, secondaryIndexes.Count());
        var entityAType = domain.Model.Types[typeof(classTableModel.EntityA)];
        secondaryIndexes = entityAType.Indexes.Where(index => index.IsSecondary && !index.IsVirtual && index.IsUnique);
        Assert.AreEqual(1, secondaryIndexes.Count());
      }
    }
  }
}
