// Copyright (C) 2015 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.04.27

using System;
using NUnit.Framework;
using Xtensive.Orm.Upgrade;
using Xtensive.Orm.Model;
using source = Xtensive.Orm.Tests.Issues.IssueJira0613_DataCleanUpInPerformSafelyModel.Source;
using badCase = Xtensive.Orm.Tests.Issues.IssueJira0613_DataCleanUpInPerformSafelyModel.BadCase;
using goodCase = Xtensive.Orm.Tests.Issues.IssueJira0613_DataCleanUpInPerformSafelyModel.GoodCase;


namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class IssueJira0613_DataCleanUpInPerformSafely
  {
    [Test]
    public void IncorrectCrossHierarchicalMovementTest()
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
      performedDomainConfiguration.Types.Register(typeof (badCase.A1).Assembly, typeof (badCase.A1).Namespace);

      //Assert.Throws<SchemaSynchronizationException>(() => Domain.Build(performedDomainConfiguration));
    }

    [Test]
    public void CorrectCrossHierarchicalMovements()
    {
      var initialDomainConfiguration = DomainConfigurationFactory.Create();
      initialDomainConfiguration.UpgradeMode = DomainUpgradeMode.Recreate;
      initialDomainConfiguration.Types.Register(typeof(source.A1).Assembly, typeof(source.A1).Namespace);
      using (var initialDomain = Domain.Build(initialDomainConfiguration))
      using (var session = initialDomain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        for (int i = 0; i < 10; i++) {
          new source.A1 { Date = DateTime.Now, Text = "A1 " + i };
        }
        for (int i = 0; i < 10; i++) {
          new source.A2 { Length = i, Text = "A2 " + i };
        }
        transaction.Complete();
      }

      var performedDomainConfiguration = DomainConfigurationFactory.Create();
      performedDomainConfiguration.UpgradeMode= DomainUpgradeMode.PerformSafely;
      performedDomainConfiguration.Types.Register(typeof (goodCase.A1).Assembly, typeof (goodCase.A1).Namespace);

      using (var performedDomain = Domain.Build(performedDomainConfiguration))
      using (var session = performedDomain.OpenSession())
      using (session.Activate())
      using (var transaction = session.OpenTransaction()) {
        var count = session.Query.All<goodCase.A1>().Count();
        Assert.That(count, Is.EqualTo(10));

        count = session.Query.All<goodCase.A2>().Count();
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

  namespace BadCase
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

  namespace GoodCase
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

    public class RecycledBaseType : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }
    }

    [HierarchyRoot]
    [Recycled(OriginalName = "Xtensive.Orm.Tests.Issues.IssueJira0613_DataCleanUpInPerformSafelyModel.Source.A1")]
    public class RecycledA1 : RecycledBaseType
    {
      [Field]
      public DateTime Date { get; set; }
    }

    [HierarchyRoot]
    [Recycled(OriginalName = "Xtensive.Orm.Tests.Issues.IssueJira0613_DataCleanUpInPerformSafelyModel.Source.A2")]
    public class RecycledA2 : RecycledBaseType
    {
      [Field]
      public int Length { get; set; }
    }

    public class Upgrader : UpgradeHandler
    {
      public override bool CanUpgradeFrom(string oldVersion)
      {
        return true;
      }

      public override void OnUpgrade()
      {
        using (var tx = UpgradeContext.Session.OpenTransaction(TransactionOpenMode.New)) {
          var oldA1Values = UpgradeContext.Session.Query.All<RecycledA1>();
          var oldA2Values = UpgradeContext.Session.Query.All<RecycledA2>();
          foreach (var recycledA2 in oldA2Values) {
            new A2 {
              Text = recycledA2.Text,
              Length = recycledA2.Length,
            };
          }
          foreach (var recycledA1 in oldA1Values) {
            new A1 {
              Text = recycledA1.Text,
              Date = recycledA1.Date
            };
          }
          tx.Complete();
        }
      }
    }
  }
}
