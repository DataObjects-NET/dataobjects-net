// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.06.11

using System;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issue0690_Model;

namespace Xtensive.Orm.Tests.Issue0690_Model
{
  [HierarchyRoot]
  public class Book : Entity
  {
    [Key, Field]
    public long Id { get; private set; }

    [Field]
    public EntitySet<Review> Reviews { get; private set; }
  }

  [HierarchyRoot]
  public class Review : Entity
  {
    [Key, Field]
    public long Id { get; private set; }
  }

  [HierarchyRoot]
  public class Message : Entity
  {
    [Key, Field(Length = 50)]
    public string Id { get; private set; }

    [Field]
    public string Name { get; set; }

    public Message(string id)
      : base(id)
    {
      
    }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class Issue0690_RemoveAndCreate : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Book).Assembly, typeof (Book).Namespace);
      return configuration;
    }

    [Test]
    public void EntitySetTest()
    {
      using (var session = Domain.OpenSession())
      using (var ts = session.OpenTransaction()) {
        var book = new Book();
        var review = new Review();
        book.Reviews.Add(review);
        book.Reviews.Clear();
        book.Reviews.Add(review);
        session.SaveChanges();
      }
    }

    [Test]
    public void EntityWithCustomKeyTest()
    {
      using (var session = Domain.OpenSession())
      using (var ts = session.OpenTransaction()) {
        var id = Guid.NewGuid().ToString();
        var message = new Message(id);
        message.Remove();
        message = new Message(id);
        session.SaveChanges();
      }
    }

    [Test]
    public void RemoveCreateTest()
    {
      var id = Guid.NewGuid().ToString();
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var message = new Message(id) {Name = "Alfa"};
          t.Complete();
        }
        using (var t = session.OpenTransaction()) {
          var message = session.Query.Single<Message>(id);
          message.Remove();
          message = new Message(id) {Name = "Beta"};
          t.Complete();
        }
      }
    }

    [Test]
    public void CreateRemoveTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var id = Guid.NewGuid().ToString();
        var message = new Message(id) { Name = "Alfa" };
        message.Remove();
        t.Complete();
      }
    }

    [Test]
    public void CreateRemoveCreateTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction())
      {
        var id = Guid.NewGuid().ToString();
        var message = new Message(id) { Name = "Alfa" };
        message.Remove();
        message = new Message(id) {Name ="Beta"};
        t.Complete();
      }
    }

    [Test]
    public void RemoveCreateRemoveTest()
    {
      var id = Guid.NewGuid().ToString();
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var message = new Message(id) {Name = "Alfa"};
          t.Complete();
        }
        using (var t = session.OpenTransaction()) {
          var message = session.Query.Single<Message>(id);
          message.Remove();
          message = new Message(id) {Name = "Beta"};
          message.Remove();
          t.Complete();
        }
      }
    }
  }
}