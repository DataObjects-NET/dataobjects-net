// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Groznov
// Created:    2016.05.16

using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0636_Model;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0636_Model
  {
    [HierarchyRoot]
    public abstract class BaseEntity : Entity
    {
      [Key, Field]
      public long Id { get; set; }
    }

    public class FirstSuccessor : BaseEntity
    {
      [Field]
      public string StringField1 { get; set; }
    }

    public class SecondSuccessor : BaseEntity
    {
      [Field]
      public string StringField2 { get; set; }
    }
  }

  public class IssueJira0636_SessionCacheTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (BaseEntity).Assembly, "Xtensive.Orm.Tests.Issues.IssueJira0636_Model");
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      long keyValue1;
      long keyValue2;
      using (var session = Domain.OpenSession())
      using (var trasnaction = session.OpenTransaction()) {
        var first = new FirstSuccessor();
        keyValue1 = first.Id;
        var second = new SecondSuccessor();
        keyValue2 = second.Id;
        trasnaction.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var firstSuccessor1 = Query.SingleOrDefault<FirstSuccessor>(keyValue1);
        Assert.That(firstSuccessor1, Is.Not.Null);
        var firstSuccessor2 = Query.SingleOrDefault<FirstSuccessor>(keyValue2);
        Assert.That(firstSuccessor2, Is.Null);

        var secondSuccessor1 = Query.SingleOrDefault<SecondSuccessor>(keyValue1);
        Assert.That(secondSuccessor1, Is.Null);
        var secondSuccessor2 = Query.SingleOrDefault<SecondSuccessor>(keyValue2);
        Assert.That(secondSuccessor2, Is.Not.Null);

        var baseEntity1 = Query.SingleOrDefault<BaseEntity>(keyValue1);
        Assert.That(baseEntity1, Is.Not.Null);
        var baseEntity2 = Query.SingleOrDefault<BaseEntity>(keyValue2);
        Assert.That(baseEntity2, Is.Not.Null);
      }
    }
  }
}
