// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.11.10

using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.ManyToManyValidationTest_Model;

namespace Xtensive.Orm.Tests.Storage
{
  namespace ManyToManyValidationTest_Model
  {
    [HierarchyRoot]
    public class Book : Entity
    {
      public int ValidationCount { get; set; }

      [Field, Key]
      public int Id { get; private set; }

      [Field]
      public string Title { get; set; }

      [Field]
      public EntitySet<Author> Authors { get; private set; }

      protected override void OnValidate()
      {
        ValidationCount++;
      }
    }

    [HierarchyRoot]
    public class Author: Entity
    {
      public int ValidationCount { get; set; }

      [Field, Key]
      public int Id { get;  private set; }

      [Field]
      public string Name { get; set; }

      [Field]
      public EntitySet<Book> Books { get; private set; }

      protected override void OnValidate()
      {
        ValidationCount++;
      }
    }
  }

  [Serializable]
  public class ManyToManyValidationTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Author).Assembly, typeof (Author).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      Author author;
      Book book;
      using (var session = Session.Open(Domain))
      using (var t = Transaction.Open()) {
        author = new Author {Name = "Vasya Pupkin"};
        Assert.AreEqual(2, author.ValidationCount);

        book = new Book {Title = "Mathematics"};
        book.Authors.Add(author);

        Assert.AreEqual(3, author.ValidationCount);
        
        t.Complete();
      }
      Assert.AreEqual(4, author.ValidationCount);
      Assert.AreEqual(4, book.ValidationCount);

      using (var session = Session.Open(Domain))
      using (var t = Transaction.Open()) {
        author = Query.All<Author>().First(a => a.Name == "Vasya Pupkin");
        Assert.IsNotNull(author);
        Assert.AreEqual(0, author.ValidationCount);
        
        t.Complete();
      }
      Assert.AreEqual(0, author.ValidationCount);

      using (var session = Session.Open(Domain))
      using (var t = Transaction.Open()) {
        author = Query.All<Author>().First(a => a.Name == "Vasya Pupkin");
        Assert.IsNotNull(author);
        Assert.AreEqual(0, author.ValidationCount);

        author.Books.Add(new Book {Title = "Mathematics 1-3"});
        Assert.AreEqual(1, author.ValidationCount);
        
        t.Complete();
      }
      Assert.AreEqual(1, author.ValidationCount);

    }
  }
}