// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.26

using System;
using System.Reflection;
using NUnit.Framework;
using System.Linq;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Tests.Issues.Issue0009_Model;

namespace Xtensive.Storage.Tests.Issues.Issue0009_Model
{
  [HierarchyRoot(typeof(KeyGenerator), "ID")]
  public class Book : Entity
  {
    [Field]
    public int ID { get; private set; }

    [Field]
    public EntitySet<Author> Authors { get; private set; }
  }

  [HierarchyRoot(typeof(KeyGenerator), "ID")]
  public class Author : Entity
  {
    [Field]
    public Guid ID { get; private set; }

    [Field]
    public int Name { get; set; }

    [Field(PairTo = "Authors")]
    public EntitySet<Book> Books { get; private set; }
  }

}

namespace Xtensive.Storage.Tests.Issues
{
  public class Issue0009_2SymmetricTablesForMNRelationship : AutoBuildTest
  {
    protected override Xtensive.Storage.Configuration.DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof(Book).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var book = new Book();
          var author = new Author(){Name = 1};
          book.Authors.Add(author);

          Assert.AreEqual(1, book.Authors.Count);
          Assert.AreEqual(1, author.Books.Count);
          Assert.IsTrue(book.Authors.Contains(author));
          Assert.IsTrue(author.Books.Contains(book));

          var author2 = new Author {Name = 2};
          var author3 = new Author { Name = 3 };
          book.Authors.Add(author2);
          book.Authors.Add(author3);
          var c = book.Authors.Where(n => n.Name <= 2).Select(n => n.Name).ToList();
          Assert.AreEqual(2, c.Count());

          t.Complete();
        }
      }
    }
  }
}