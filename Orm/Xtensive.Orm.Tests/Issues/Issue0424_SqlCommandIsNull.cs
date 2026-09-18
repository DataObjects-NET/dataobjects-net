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
  [HierarchyRoot]
  public class Parent : Entity
  {
    [Key, Field]
    public int Id { get; private set;}

    [Field]
    public Child Child { get; set; }
  }

  [HierarchyRoot]
  public class Child : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public int Value { get; private set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0424_SqlCommandIsNull : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Parent));
      configuration.Types.Register(typeof (Child));
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        _ = new Parent {Child = new Child()};
        t.Complete();
      }
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var parent = session.Query.All<Parent>().Single();
        var result = session.Query.All<Child>().Single(child => child.Value==parent.Child.Value);
        t.Complete();
      }
    }
  }
}