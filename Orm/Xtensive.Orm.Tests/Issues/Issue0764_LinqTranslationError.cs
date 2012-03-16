// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.07.12

using System;
using System.Diagnostics;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0764_LinqTranslationError_Model;
using System.Linq;

namespace Xtensive.Orm.Tests.Issues
{
  namespace Issue0764_LinqTranslationError_Model
  {
    [Serializable]
    [HierarchyRoot]
    public class MyEntity : Entity
    {
      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Text { get; set; }

      [Field, Association(PairTo = "LinkSource", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      public EntitySet<Link> LinksFromThis { get; private set; }

      [Field, Association(PairTo = "LinkDestination", OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
      public EntitySet<Link> LinksToThis { get; private set; }
    }

    [HierarchyRoot]
    public class Link : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      public MyEntity LinkSource { get; set; }

      [Field]
      public MyEntity LinkDestination { get; set; }
    }
  }

  [Serializable]
  public class Issue0764_LinqTranslationError : AutoBuildTest
  {

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(typeof(MyEntity).Assembly, typeof(MyEntity).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var source = new MyEntity() { Text = "Source" };
        var destination = new MyEntity() { Text = "Destination" };
        var link = new Link() {LinkSource = source, LinkDestination = destination};

        var query = 
          from l in session.Query.All<Link>()
          where l.LinkSource == source && l.LinkDestination.Text == "Destination"
          select l.LinkDestination;

        query.ToList();
      }
    }
  }
}