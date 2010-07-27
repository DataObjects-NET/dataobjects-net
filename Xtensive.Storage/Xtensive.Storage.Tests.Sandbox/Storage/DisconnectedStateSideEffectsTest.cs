// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.06.24

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Tests.Storage.DisconnectedStateSideEffectsTest
{
  [Serializable]
  [HierarchyRoot]
  public class Book : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public int Version { get; set; }

    [Field]
    public string Title { get; set; }

    protected override void OnSetFieldValue(FieldInfo field, object oldValue, object newValue)
    {
      base.OnSetFieldValue(field, oldValue, newValue);
      if (field.Name=="Version")
        return;
      if (!Session.IsDisconnected)
        Version++;
    }

    public override string ToString()
    {
      return Title;
    }
  }

  [Serializable]
  [HierarchyRoot]
  public class Author : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public int Version { get; set; }

    [Field]
    public string Title { get; set; }

    protected override void OnInitialize()
    {
      base.OnInitialize();
      var book = Query.All<Book>().FirstOrDefault();
    }

    public override string ToString()
    {
      return Title;
    }
  }

  [TestFixture]
  public class DisconnectedStateSideEffectsTest : AutoBuildTest
  {
    private const string NewBookTitle = "New Book";

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
    public void SequentialApplyChangesTest()
    {
      var ds = new DisconnectedState();
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        using (ds.Attach(session))
        using (ds.Connect()) {
          var book = Query.All<Book>().First();
          book.Title += " Changed";
          ds.ApplyChanges();
          book.Title += " Changed";
          ds.ApplyChanges();
        }
        // tx.Complete();
      }
    }

    [Test]
    public void SequentialApplyChangesTest_DisconnectedStateAsVersionProvider()
    {
      var ds = new DisconnectedState();
      ds.VersionsProviderType = VersionsProviderType.DisconnectedState;
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        using (ds.Attach(session))
        using (ds.Connect()) {
          var book = Query.All<Book>().First();
          book.Title += " Changed";
          ds.ApplyChanges();
          book.Title += " Changed";
          AssertEx.Throws<VersionConflictException>(() => ds.ApplyChanges());
        }
        // tx.Complete();
      }
    }

    [Test]
    public void OverridenOnInitializeTest()
    {
      var ds = new DisconnectedState();
      using (var session = Session.Open(Domain))
      using (ds.Attach(session))
      using (ds.Connect()) {
        var author = new Author();
      }
    }
  }
}