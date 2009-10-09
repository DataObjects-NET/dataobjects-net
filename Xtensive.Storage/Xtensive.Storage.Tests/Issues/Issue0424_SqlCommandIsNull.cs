// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.10.08

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.Issue0296_Model;

namespace Xtensive.Storage.Tests.Issues.Issue0296_Model
{
  [HierarchyRoot]
  public class TheParent : Entity
  {
    [Key, Field]
    public int Id { get; private set;}

    [Field]
    public TheChild Child { get; set; }
  }

  [HierarchyRoot]
  public class TheChild : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public int Value { get; private set; }
  }
}

namespace Xtensive.Storage.Tests.Issues
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
      using (Session.Open(Domain))
      using (var t = Transaction.Open()) {
        new TheParent {Child = new TheChild()};
        t.Complete();
      }
    }

    [Test]
    public void MainTest()
    {
      using (Session.Open(Domain))
      using (var t = Transaction.Open()) {
        var parent = Query<TheParent>.All.Single();
        var result = Query<TheChild>.All.Single(child => child.Value==parent.Child.Value);
        t.Complete();
      }
    }
  }
}