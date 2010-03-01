// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.26

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Issues.Issue0009_Model;

namespace Xtensive.Storage.Tests.Issues.Issue0009_Model
{
  [Serializable]
  [HierarchyRoot]
  public class Book : Entity
  {
    [Field, Key]
    public int ID { get; private set; }

    [Field]
    public EntitySet<Author> Authors { get; private set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Author : Entity
  {
    [Field, Key]
    public Guid ID { get; private set; }

    [Field, Association(PairTo = "Authors")]
    public EntitySet<Book> Books { get; private set; }
  }

}

namespace Xtensive.Storage.Tests.Issues
{
  public class Issue0009_2SymmetricTablesForMNRelationship : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), typeof(Book).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (Session.Open(Domain)) {
        using (var t = Transaction.Open()) {
          var book = new Book();
          var author = new Author();
          book.Authors.Add(author);

          Assert.AreEqual(1, book.Authors.Count);
          Assert.AreEqual(1, author.Books.Count);
          Assert.IsTrue(book.Authors.Contains(author));
          Assert.IsTrue(author.Books.Contains(book));
          t.Complete();
        }
      }
    }
  }
}