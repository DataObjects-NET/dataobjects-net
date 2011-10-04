// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.06.11

using System;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issue0690_Model;

namespace Xtensive.Storage.Tests.Issue0690_Model
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

    public Message(string id)
      : base(id)
    {
      
    }
  }
}

namespace Xtensive.Storage.Tests.Issues
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
      using (var session = Session.Open(Domain))
      using (var ts = Transaction.Open()) {
        var book = new Book();
        var review = new Review();
        book.Reviews.Add(review);
        book.Reviews.Clear();
        book.Reviews.Add(review);
        session.Persist();
      }
    }

    [Test]
    public void EntityWithCustomKeyTest()
    {
      using (var session = Session.Open(Domain))
      using (var ts = Transaction.Open()) {
        var id = Guid.NewGuid().ToString();
        var message = new Message(id);
        message.Remove();
        message = new Message(id);
        session.Persist();
      }
    }
  }
}