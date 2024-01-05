﻿// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2012.07.25

using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.SessionSaveChangesPerformanceTestModel;

namespace Xtensive.Orm.Tests.Storage
{
  namespace SessionSaveChangesPerformanceTestModel
  {
    [HierarchyRoot]
    public class MyEntity : Entity
    {
      [Key, Field]
      public int Id { get; private set; }

      [Field]
      public string Title { get; set; }

      public MyEntity(Session session)
        : base(session)
      {}
    }
  }

  public class SessionSaveChangesPerformanceTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (MyEntity));
      return configuration;
    }

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        new MyEntity(session);
        new MyEntity(session);
        new MyEntity(session);
        t.Complete();
      }
    }

    [Test]
    public void MainTest()
    {
      var clientProfile = new SessionConfiguration(SessionOptions.ClientProfile);
      using (var session = Domain.OpenSession(clientProfile)) {
        var items = session.Query.All<MyEntity>().ToList();
        items[0].Title = "Some text";
        session.SaveChanges();
      }
    }

    [Test]
    public void ConcurrentUpdateTest()
    {
      var clientProfile = new SessionConfiguration(SessionOptions.ClientProfile);
      
      using (var session1 = Domain.OpenSession(clientProfile)) {
        var item1 = session1.Query.All<MyEntity>().OrderBy(e => e.Id).Take(1).First();
        item1.Title = "Hello from session 1";
        session1.SaveChanges();

        using (var session2 = Domain.OpenSession(clientProfile)) {
          var item2 = session2.Query.All<MyEntity>().OrderBy(e => e.Id).Take(1).First();
          Assert.AreEqual("Hello from session 1", item2.Title);
          item2.Title = "Hello from session 2";
          session2.SaveChanges();
        }

        item1 = session1.Query.All<MyEntity>().OrderBy(e => e.Id).Take(1).First();
        Assert.AreEqual("Hello from session 2", item1.Title);
      }
    }
  }
}