// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.12.19

using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using model = Xtensive.Orm.Tests.Storage.BatchingWhenValidateEntityVersionsIsEnabledTestModel;

namespace Xtensive.Orm.Tests.Storage.BatchingWhenValidateEntityVersionsIsEnabledTestModel
{
  [HierarchyRoot]
  public class Store : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    public EntitySet<Book> StoredBooks { get; set; } 
  }

  [HierarchyRoot]
  public class Book : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Title { get; set; }

    [Field]
    public EntitySet<Author> Authors { get; set; }

    [Field]
    [Association(PairTo = "StoredBooks")]
    public EntitySet<Store> Stores { get; set; }
  }

  [HierarchyRoot]
  public class Author : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Length = 50)]
    public string FirstName { get; set; }

    [Field(Length = 50)]
    public string LastName { get; set; }

    [Field]
    [Association(PairTo = "Authors")]
    public EntitySet<Book> Books { get; set; }
  }

}

namespace Xtensive.Orm.Tests.Storage.BatchingWhenValidateEntityVersionsIsEnabledTest
{
  [TestFixture]
  public class BatchingWhenValidateEntityVersionsIsEnabledTest : AutoBuildTest
  {
    protected override void CheckRequirements()
    {
      ProviderInfo.Supports(ProviderFeatures.Batches);
    }

    public override void TestFixtureSetUp()
    {
      try {
        RebuildDomain();
        CheckRequirements();
      }
      catch (IgnoreException) {
        throw;
      }
      catch (Exception e) {
        Debug.WriteLine("Error in TestFixtureSetUp:\r\n{0}".FormatWith(e));
        throw;
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = DomainConfigurationFactory.Create();
      var defaultSession = configuration.Sessions.Default;
      defaultSession.Options = defaultSession.Options | SessionOptions.ValidateEntityVersions;
      configuration.Types.Register(typeof (model.Book).Assembly, typeof (model.Book).Namespace);
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var author = new model.Author {
          FirstName = "William",
          LastName = "Shakespeare"
        };
        var store = new model.Store {Name = "Store"};
        new model.Store {Name = "RemovedStore"};
        var book = new model.Book {Title = "Romeo and Juliet"};
        book.Authors.Add(author);
        book.Stores.Add(store);
        transaction.Complete();
      }
    }

    [Test]
    public void Test01()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var store = session.Query.All<model.Store>().Single(el => el.Name=="Store");
        var book = session.Query.All<model.Book>().Single(el => el.Title=="Romeo and Juliet");
        var newBook = new model.Book {Title = "Hamlet"};
        newBook.Authors.Add(session.Query.All<model.Author>().Single());
        newBook.Stores.Add(store);
        var newStore = new model.Store {Name = "AnotherStore"};
        var oldBookStore = book.Stores.Single(el => el.Name=="Store");
        oldBookStore.Name = "RenamedStore";
        newStore.Remove();
        session.SaveChanges();
      }
    }

    [Test]
    public void Test02()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var store1 = new model.Store {Name = "store1"};
        var store2 = new model.Store {Name = "store2"};
        var store3 = new model.Store {Name = "store3"};
        store1.Remove();
        store2.Remove();
        store3.Remove();
        session.SaveChanges();
      }
    }

    [Test]
    public void Test03()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var store1 = new model.Store {Name = "store1"};
        var store2 = new model.Store {Name = "store2"};
        var store3 = new model.Store {Name = "store3"};
        transaction.Complete();
      }
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var stores = session.Query.All<model.Store>().Where(el => el.Name.StartsWith("store")).ToList();
        foreach (var store in stores) {
          store.Name += "Changed";
        }
        session.SaveChanges();
      }
    }

    [Test]
    public void Test04()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var storeForRemove = session.Query.All<model.Store>().Single(el => el.Name=="RemovedStore");
        var storeForUpgrade = session.Query.All<model.Store>().Single(el => el.Name=="Store");
        var author = new model.Author() {LastName = "LastName", FirstName = "FirstName"};
        var book = new model.Book() {Title = "The book withot title" };
        book.Stores.Add(storeForUpgrade);
        book.Authors.Add(author);
        storeForUpgrade.Name = "UpgradedStore";
        storeForRemove.Remove();
        var allStores = session.Query.All<model.Store>().ToList();
      }
    }
  }
}
