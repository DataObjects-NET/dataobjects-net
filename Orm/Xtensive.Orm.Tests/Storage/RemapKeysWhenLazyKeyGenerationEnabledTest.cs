// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.04.01

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.RemapKeysWhenLazyKeyGenerationEnabledModel;

namespace Xtensive.Orm.Tests.Storage.RemapKeysWhenLazyKeyGenerationEnabledModel
{
  [HierarchyRoot]
  public class Author : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field, Association(PairTo = "Authors")]
    public EntitySet<Book> Books { get; private set; }
  }

  [HierarchyRoot]
  public class Book : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string Title { get; set; }

    [Field]
    public EntitySet<Author> Authors { get; private set; }

    [Field, Association(PairTo = "Book")]
    public EntitySet<Comment> Comments { get; private set; }

    [Field]
    public EntitySet<Store> Stores { get; private set; }
  }

  [HierarchyRoot]
  public class Comment : Entity
  {
    [Key, Field]
    public long Id { get; private set; }

    [Field]
    public Book Book { get; set; }
  }

  [HierarchyRoot]
  public class Store : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public EntitySet<Book> Books { get; private set; } 
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class RemapKeysWhenLazyKeyGenerationEnabledTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.UpgradeMode = DomainUpgradeMode.Recreate;
      configuration.Types.Register(typeof (Book).Assembly, typeof (Book).Namespace);
      configuration.Sessions.Default.Options = SessionOptions.LazyKeyGeneration | SessionOptions.AutoActivation;
      return configuration;
    }

    [Test]
    public void Test01()
    {
      var expectedAuthorsCount = 5;
      var booksPerAuthor = 2;

      using (var session = Domain.OpenSession()) {
        using (var transaction = session.OpenTransaction()) {
          var authors = new List<Author>();
          var stores = new List<Store>();
          var store = new Store();
          for (var i = 0; i < expectedAuthorsCount; i++) {
            var author = new Author { Name = string.Format("Author {0}", i) };
            for (var j = 0; j < booksPerAuthor; j++)
              author.Books.Add(new Book { Title = string.Format("Book {0} of Author {1}", j, i) });

            foreach (var book in author.Books) {
              new Comment { Book = book };
              store.Books.Add(book);
            }
            authors.Add(author);
          }
          var states = session.EntityChangeRegistry.GetItems(PersistenceState.New).ToList();

          foreach (var state in states.Where(state => !state.Type.IsAuxiliary))
            Assert.IsTrue(state.Key.IsTemporary(Domain));

          session.SaveChanges();

          Assert.AreEqual(store.Books.Count, expectedAuthorsCount * booksPerAuthor);

          foreach (var author in authors) {
            Assert.IsFalse(author.Key.IsTemporary(Domain));
            Assert.Greater(author.Id, 0);
            foreach (var book in author.Books) {
              Assert.IsFalse(book.Key.IsTemporary(Domain));
              Assert.Greater(book.Id, 0);
              Assert.AreEqual(book.Stores.Count, 0);
              foreach (var comment in book.Comments) {
                Assert.IsFalse(comment.Key.IsTemporary(Domain));
                Assert.Greater(comment.Id, 0);
              }
            }
          }
          transaction.Complete();
        }
      }
    }
  }
}
