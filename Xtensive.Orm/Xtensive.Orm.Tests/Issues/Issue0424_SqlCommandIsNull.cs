// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.10.08

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0296_Model;

namespace Xtensive.Orm.Tests.Issues.Issue0296_Model
{
  [Serializable]
  [HierarchyRoot]
  public class TheParent : Entity
  {
    [Key, Field]
    public int Id { get; private set;}

    [Field]
    public TheChild Child { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class TheChild : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public int Value { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  [Serializable]
  public class Issue0424_SqlCommandIsNull : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (TheParent).Assembly, typeof (TheParent).Namespace);
      return configuration;
    }

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        new TheParent {Child = new TheChild()};
        t.Complete();
      }
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var parent = session.Query.All<TheParent>().Single();
        var result = session.Query.All<TheChild>().Single(child => child.Value==parent.Child.Value);
        t.Complete();
      }
    }
  }
}