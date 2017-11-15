// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.06.24

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Services;

namespace Xtensive.Orm.Tests.Storage.NotifyXxxTests
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

    protected override void OnInitialize()
    {
      base.OnInitialize();

      if(IsMaterializing)
        Console.WriteLine("On initialize");
    }
  }

  [TestFixture]
  public class NotifyXxxTest : AutoBuildTest
  {
    private NotifyCollectionChangedAction lastChangeAction;
    private string lastChangedProperty;
    private object lastSenderObject;
    private object lastSenderCollection;

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(Book).Assembly, typeof(Book).Namespace);
      return configuration;
    }

    [Test]
    public void StandardTest()
    {
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var sessionStateAccessor = DirectStateAccessor.Get(session);
        var book1 = new Book() {Title = "Book 1"};
        var book2 = new Book() {Title = "Book"};
        book1.RelatedBooks.CollectionChanged += RelatedBooks_CollectionChanged;
        book2.PropertyChanged += Book_PropertyChanged;

        ResetLastXxx();
        book2.Title = "Book 2";
        Assert.AreEqual("Title", lastChangedProperty);
        Assert.AreSame(book2, lastSenderObject);

        ResetLastXxx();
        book1.RelatedBooks.Add(book2);
        Assert.AreEqual(NotifyCollectionChangedAction.Add, lastChangeAction);
        Assert.AreSame(book1.RelatedBooks, lastSenderCollection);

        { // Test 1
          ResetLastXxx();
          book1.RelatedBooks.Remove(book2);
          // "Reset", coz collection is considered as not fully loaded
#if DEBUG
          Assert.AreEqual(NotifyCollectionChangedAction.Reset, lastChangeAction);
#else
          Assert.AreEqual(NotifyCollectionChangedAction.Remove, lastChangeAction);
#endif
          Assert.AreSame(book1.RelatedBooks, lastSenderCollection);
        }

        // Restoring removed item
        book1.RelatedBooks.Add(book2);
        // Let's fully load the collection
        var bookCount = book1.RelatedBooks.Count;

        { // Test 2
          ResetLastXxx();
          book1.RelatedBooks.Remove(book2);
          // Now we must get "Remove" event, since item index can be found
          Assert.AreEqual(NotifyCollectionChangedAction.Remove, lastChangeAction);
          Assert.AreSame(book1.RelatedBooks, lastSenderCollection);
        }

        ResetLastXxx();
        book1.RelatedBooks.Clear();
        Assert.AreEqual(NotifyCollectionChangedAction.Reset, lastChangeAction);
        Assert.AreSame(book1.RelatedBooks, lastSenderCollection);

        ResetLastXxx();
        session.NotifyChanged();
        Assert.AreEqual(null, lastChangedProperty);
        Assert.AreSame(book2, lastSenderObject);
        Assert.AreEqual(NotifyCollectionChangedAction.Reset, lastChangeAction);
        Assert.AreSame(book1.RelatedBooks, lastSenderCollection);
        // tx.Complete();
      }
    }

    private void ResetLastXxx()
    {
      lastChangeAction = NotifyCollectionChangedAction.Move; // Since it is never used by DO4
      lastChangedProperty = "@None@";
      lastSenderObject = new object();
      lastSenderCollection = new object();
    }

    private void Book_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      TestLog.Info("PropertyChanged: Sender = {0}, Property = {1}", sender, e.PropertyName);
      lastSenderObject = sender;
      lastChangedProperty = e.PropertyName;
    }

    private void RelatedBooks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      TestLog.Info("CollectionChanged: Sender = {0}, Action = {1}", sender, e.Action);
      lastSenderCollection = sender;
      lastChangeAction = e.Action;
    }

    [Test]
    public void OnInitializeTest()
    {
      Key key;

      using (Domain.OpenSession()) {
        using (var t = Session.Current.OpenTransaction()) {

          var b = new Book();
          key = b.Key;
          t.Complete();
        }
      }

      using (Domain.OpenSession()) {
        using (Session.Current.OpenTransaction()) {

          //var b = Query.All<Book>().First();
          var c = Query.Single<Book>(key);
        }
      }
    }
  }
}
