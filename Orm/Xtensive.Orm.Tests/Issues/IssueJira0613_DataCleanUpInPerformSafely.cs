// Copyright (C) 2015 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.04.27

using System;
using NUnit.Framework;
using Xtensive.Orm.Model;
using source = Xtensive.Orm.Tests.Issues.IssueJira0613_DataCleanUpInPerformSafelyModel.Source;
using target = Xtensive.Orm.Tests.Issues.IssueJira0613_DataCleanUpInPerformSafelyModel.Target;

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class IssueJira0613_DataCleanUpInPerformSafely
  {
    [Test]
    public void MainTest()
    {
      var initialDomainConfiguration = DomainConfigurationFactory.Create();
      initialDomainConfiguration.UpgradeMode= DomainUpgradeMode.Recreate;
      initialDomainConfiguration.Types.Register(typeof (source.A1).Assembly, typeof (source.A1).Namespace);
      using (var initialDomain = Domain.Build(initialDomainConfiguration))
      using (var session = initialDomain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        for (int i = 0; i < 10; i++) {
          new source.A1 {Date = DateTime.Now, Text = "A1 " + i};
        }
        for (int i = 0; i < 10; i++) {
          new source.A2 {Length = i, Text = "A2 " + i};
        }
        transaction.Complete();
      }

      var performedDomainConfiguration = DomainConfigurationFactory.Create();
      performedDomainConfiguration.UpgradeMode= DomainUpgradeMode.PerformSafely;
      performedDomainConfiguration.Types.Register(typeof (target.A1).Assembly, typeof (target.A1).Namespace);

      using (var performedDomain = Domain.Build(performedDomainConfiguration))
      using (var session = performedDomain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var count = session.Query.All<target.A1>().Count();
        Assert.That(count, Is.EqualTo(10));

        count = session.Query.All<target.A2>().Count();
        Assert.That(count, Is.EqualTo(10));
      }
    }
  }
}

namespace Xtensive.Orm.Tests.Issues.IssueJira0613_DataCleanUpInPerformSafelyModel
{
  namespace Source
  {
    public abstract class BaseType : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }
    }

    [HierarchyRoot]
    public class A1 : BaseType
    {
      [Field]
      public DateTime Date { get; set; }
    }

    [HierarchyRoot]
    public class A2 : BaseType
    {
      [Field]
      public int Length { get; set; }
    }
  }

  namespace Target
  {
    [HierarchyRoot(InheritanceSchema.ConcreteTable)]
    public abstract class BaseType : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }
    }

    public class A1 : BaseType
    {
      [Field]
      public DateTime Date { get; set; }
    }

    public class A2 : BaseType
    {
      [Field]
      public int Length { get; set; }
    }
  }
}
