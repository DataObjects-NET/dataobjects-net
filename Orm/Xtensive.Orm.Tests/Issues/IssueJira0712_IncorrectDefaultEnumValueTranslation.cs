// Copyright (C) 2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Julian Mamokin
// Created:    2017.09.15

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;
using model = Xtensive.Orm.Tests.Issues.IssueJira0712_IncorrectDefaultEnumValueTranslationModel;

namespace Xtensive.Orm.Tests.Issues.IssueJira0712_IncorrectDefaultEnumValueTranslationModel
{
  namespace ConcreteTable
  {
    [HierarchyRoot(InheritanceSchema.ConcreteTable)]
    public class TestEntity : Entity
    {
      [Key, Field]
      public Guid Id { get; set; }

      [Field(DefaultValue = OneTwo.One)]
      public OneTwo Num { get; set; }
    }

    public class EntDerived : TestEntity
    {
    }
  }

  namespace SingleTable
  {
    [HierarchyRoot(InheritanceSchema.SingleTable)]
    public class TestEntity : Entity
    {
      [Key, Field]
      public Guid Id { get; set; }

      [Field(DefaultValue = OneTwo.One)]
      public OneTwo Num { get; set; }
    }

    public class EntDerived : TestEntity
    {
    }
  }

  namespace ClassTable
  {
    [HierarchyRoot(InheritanceSchema.ClassTable)]
    public class TestEntity : Entity
    {
      [Key, Field]
      public Guid Id { get; set; }

      [Field(DefaultValue = OneTwo.One)]
      public OneTwo Num { get; set; }
    }

    public class EntDerived : TestEntity
    {
    }
  }

  public enum OneTwo
  {
    One = 1,
    Two = 2,
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class IssueJira0712_IncorrectDefaultEnumValueTranslation
  {
    [Test]
    public void ConcreteTableTest()
    {
      var configuration = BuildConfiguration(typeof (model.ConcreteTable.TestEntity));
      Assert.DoesNotThrow(() => Domain.Build(configuration));
    }

    [Test]
    public void ClassTableTest()
    {
      var configuration = BuildConfiguration(typeof (model.ClassTable.TestEntity));
      Assert.DoesNotThrow(() => Domain.Build(configuration));
    }

    [Test]
    public void SingleTableTest()
    {
      var configuration = BuildConfiguration(typeof (model.SingleTable.TestEntity));
      Assert.DoesNotThrow(() => Domain.Build(configuration));
    }

    private DomainConfiguration BuildConfiguration(Type t)
    {
      var config = DomainConfigurationFactory.Create();
      config.Types.Register(t.Assembly, t.Namespace);
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      return config;
    }
  }
}
