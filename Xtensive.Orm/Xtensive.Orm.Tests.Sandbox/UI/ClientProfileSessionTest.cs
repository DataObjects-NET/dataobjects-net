// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.09.14

using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.UI.Model;

namespace Xtensive.Orm.Tests.UI
{
  namespace Model
  {
    [HierarchyRoot]
    public class Author : Entity
    {
      [Field,Key]
      public int Id { get; private set; }

      [Field]
      public string Name { get; set; }

      public Author(Session session) : base(session)
      {
      }
    }
  }

  [TestFixture]
  public class ClientProfileSessionTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(Author).Assembly, typeof(Author).Namespace);
      return configuration;
    }

    [Test]
    public void CreateTest()
    {
      var config = Domain.Configuration.Sessions.Default.Clone();
      config.Options = SessionOptions.ClientProfile;
      using (var session = Domain.OpenSession(config)) {
        new Author(session) {Name = "Alex"};
      }
      using (var session = Domain.OpenSession(config)) {
        Assert.IsFalse(session.Query.All<Author>().Any(a => a.Name == "Alex"));
      }
      using (var session = Domain.OpenSession(config)) {
        new Author(session) { Name = "Alex" };
        session.SaveChanges();
        Assert.IsTrue(session.Query.All<Author>().Any(a => a.Name == "Alex"));
      }
    }

    [Test]
    public void QueryTest()
    {
      var config = Domain.Configuration.Sessions.Default.Clone();
      config.Options = SessionOptions.ClientProfile;
      using (var session = Domain.OpenSession(config)) {
        session.Query.All<Author>().ToList();
      }
      using (var session = Domain.OpenSession(config)) {
        new Author(session) { Name = "Alex" };
        session.SaveChanges();
        var authors = session.Query.All<Author>().Where(a => a.Name == "Alex").ToList();
        Assert.Greater(authors.Count, 0);
      }
    }
  }
}