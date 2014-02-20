// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alena Mikshina
// Created:    2014.02.13

using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Orm.Model;
using Xtensive.Orm.Tests.Issues.IssueJira0516_PartialIndexConstructionModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0516_PartialIndexConstructionModel
  {
    [HierarchyRoot(InheritanceSchema.ConcreteTable)]
    [Index("Number", Unique = true, Filter = "NumberIndexFilter")]
    public abstract class Basic : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public int Number { get; set; }

      [Field]
      public bool NumberIsIndexed { get; set; }

      private static Expression<Func<Basic, bool>> NumberIndexFilter()
      {
        return entity => entity.NumberIsIndexed;
      }
    }

    public abstract class Derived : Basic
    {
    }
  }

  [TestFixture]
  internal class IssueJira0516_PartialIndexConstruction
  {
    [Test]
    public void MainTest()
    {
      RunTest(typeof (Derived));
    }

    private void RunTest(Type type)
    {
      BuildDomain(type, DomainUpgradeMode.Recreate);
      BuildDomain(type, DomainUpgradeMode.Validate);
    }

    private void BuildDomain(Type type, DomainUpgradeMode upgradeMode)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(type);
      configuration.UpgradeMode = upgradeMode;
      Domain.Build(configuration).Dispose();
    }
  }
}
