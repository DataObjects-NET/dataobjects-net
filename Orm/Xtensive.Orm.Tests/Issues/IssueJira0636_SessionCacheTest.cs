// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Groznov
// Created:    2016.05.16

using System.Linq;
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

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var trasnaction = session.OpenTransaction()) {
        new FirstSuccessor();
        new SecondSuccessor();
        trasnaction.Complete();
      }
    }

    [Test]
    public void SelectBaseEntityFirstTest()
    {
      long keyValue1;
      long keyValue2;
      GetKeyValues(out keyValue1, out keyValue2);

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        GetEntity<BaseEntity>(keyValue1, false);
        GetEntity<BaseEntity>(keyValue2, false);

        GetEntity<FirstSuccessor>(keyValue1, false);
        GetEntity<FirstSuccessor>(keyValue2, true);

        GetEntity<SecondSuccessor>(keyValue1, true);
        GetEntity<SecondSuccessor>(keyValue2, false);
      }
    }

    [Test]
    public void SelectBaseEntityInTheMiddleTest()
    {
      long keyValue1;
      long keyValue2;
      GetKeyValues(out keyValue1, out keyValue2);

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        GetEntity<FirstSuccessor>(keyValue1, false);
        GetEntity<FirstSuccessor>(keyValue2, true);

        GetEntity<BaseEntity>(keyValue1, false);
        GetEntity<BaseEntity>(keyValue2, false);

        GetEntity<SecondSuccessor>(keyValue1, true);
        GetEntity<SecondSuccessor>(keyValue2, false);
      }
    }

    [Test]
    public void SelectBaseEntityLastTest()
    {
      long keyValue1;
      long keyValue2;
      GetKeyValues(out keyValue1, out keyValue2);

      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        GetEntity<FirstSuccessor>(keyValue1, false);
        GetEntity<FirstSuccessor>(keyValue2, true);

        GetEntity<SecondSuccessor>(keyValue1, true);
        GetEntity<SecondSuccessor>(keyValue2, false);

        GetEntity<BaseEntity>(keyValue1, false);
        GetEntity<BaseEntity>(keyValue2, false);
      }
    }

    private void GetKeyValues(out long keyValue1, out long keyValue2)
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        keyValue1 = Query.All<FirstSuccessor>().Single().Id;
        keyValue2 = Query.All<SecondSuccessor>().Single().Id;
      }
    }

    private T GetEntity<T>(long keyValue, bool expectedNull)
      where T : Entity
    {
      var entity = Query.SingleOrDefault<T>(keyValue);
      if (expectedNull)
        Assert.IsNull(entity);
      else
        Assert.IsNotNull(entity);
      return entity;
    }
  }
}
