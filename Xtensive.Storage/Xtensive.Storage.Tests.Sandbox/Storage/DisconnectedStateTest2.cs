// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.06.24

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Storage.DisconnectedStateTest2
{
  [Serializable]
  [HierarchyRoot]
  public class Book : Entity
  {
    [Key, Field]
    public Guid Id { get; private set; }

    [Field]
    public string Title { get; set; }

    public override string ToString()
    {
      return Title;
    }
  }

  [TestFixture]
  public class DiscinnectedStateTest2 : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(Book).Assembly, typeof(Book).Namespace);
      return configuration;
    }

    protected override Domain  BuildDomain(DomainConfiguration configuration)
    {
      var domain = base.BuildDomain(configuration);
      using (var session = Session.Open(domain))
      using (var tx = Transaction.Open()) {
        new Book() { Title = "Book" };
        tx.Complete();
      }
      return domain;
    }

    [Test]
    public void ConnectInsideTransactionTest()
    {
      var ds = new DisconnectedState();
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        using (ds.Attach(session))
        using (ds.Connect()) {
          var book = Query.All<Book>().SingleOrDefault();
          var book2 = new Book() { Title = "Book2" };
          Assert.IsNotNull(book);
          Assert.IsNotNull(book2);
          ds.ApplyChanges();
        }
        Assert.AreEqual(2, Query.All<Book>().Count());
        tx.Complete();
      }
    }
  }
}