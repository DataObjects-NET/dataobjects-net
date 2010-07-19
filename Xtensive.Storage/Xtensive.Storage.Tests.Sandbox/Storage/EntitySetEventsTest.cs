// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.06.24

using System;
using System.Collections.Specialized;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Services;

namespace Xtensive.Storage.Tests.Storage.EntitySetEventsTest
{
  [Serializable]
  [HierarchyRoot]
  public class Book : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string Title { get; set; }

    [Field]
    public EntitySet<Book> RelatedBooks { get; private set; }

    public override string ToString()
    {
      return Title;
    }
  }

  [TestFixture]
  public class EntitySetEventsTest : AutoBuildTest
  {
    private NotifyCollectionChangedAction lastAction;

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(Book).Assembly, typeof(Book).Namespace);
      return configuration;
    }

    [Test]
    public void CombinedTest()
    {
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var sessionStateAccessor = DirectStateAccessor.Get(session);
        var book1 = new Book() {Title = "Book 1"};
        var book2 = new Book() {Title = "Book 2"};
        book1.RelatedBooks.CollectionChanged += RelatedBooks_CollectionChanged;

        book1.RelatedBooks.Add(book2);
        Assert.AreEqual(NotifyCollectionChangedAction.Add, lastAction);

        book1.RelatedBooks.Remove(book2);
        Assert.AreEqual(NotifyCollectionChangedAction.Remove, lastAction);

        book1.RelatedBooks.Clear();
        Assert.AreEqual(NotifyCollectionChangedAction.Reset, lastAction);

        sessionStateAccessor.Invalidate();
        Assert.AreEqual(NotifyCollectionChangedAction.Reset, lastAction);
        // tx.Complete();
      }
    }

    private void RelatedBooks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      Log.Info("CollectionChanged: Sender = {0}, Action = {1}, ", sender, e.Action);
      lastAction = e.Action;
    }
  }
}
