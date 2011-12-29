// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2011.09.06

using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.Issue_TypeCastInContain_Model;
using System.Collections.Generic;

namespace Xtensive.Storage.Tests.Issues.Issue_TypeCastInContain_Model
{
  [HierarchyRoot]
  public class Parent : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    public Parent(Session session)
      : base(session)
    {
    }
  }

  public class Child : Parent
  {
    [Field]
    public bool SomeBool { get; set; }

    [Field]
    public Parent Parent { get; set; }

    public Child(Session session)
      : base(session)
    {
    }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  public class Issue_TypeCastInContain : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Parent).Assembly, typeof (Parent).Namespace);
      return config;
    }

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      CreateSessionAndTransaction();
    }

    [Test]
    public void ParentsContainsChildWithImplicitCastTest()
    {
      var parents = Query.All<Parent>().ToArray();
      var result = Query.All<Child>().Where(child => parents.Contains(child)).ToArray();
    }

    [Test]
    public void ParentsContainsChildWithExplicitCastTest()
    {
      var parents = Query.All<Parent>().ToArray();
      var result = Query.All<Child>().Where(child => parents.Contains(child as Parent)).ToArray();
    }

    [Test]
    public void ChildInParentsTest()
    {
      var parents = Query.All<Parent>().ToArray();
      var result = Query.All<Child>().Where(child => child.In(parents)).ToArray();
    }

    [Test]
    public void ChildContainsBaseWithImplicitCast()
    {
      var children = Query.All<Child>().ToArray();
      var result = Query.All<Child>().Where(a => children.Contains(a.Parent)).ToArray();
    }

    [Test]
    public void ChildContainsBaseWithExplicitCast()
    {
      var children = Query.All<Child>().ToArray();
      var result = Query.All<Child>().Where(a => (children as IEnumerable<Parent>).Contains(a.Parent)).ToArray();
    }

    [Test]
    public void ParentInChildrenTest()
    {
      var children = Query.All<Child>().ToArray();
      var result = Query.All<Child>().Where(a => a.Parent.In(children)).ToArray();
    }
  }
}