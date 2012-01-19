// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.06.24

using System;
using NUnit.Framework;
using Xtensive.Orm;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0785.Model;
using Xtensive.Core;

namespace Xtensive.Orm.Tests.Issues.Issue0785.Model
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
    public RefHolder<Book> BookRef { get; set; }

    public override string ToString()
    {
      return Title;
    }
  }

  [HierarchyRoot]
  public abstract class ItemInfo<T> : Entity where T: Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public RefHolder<T> Reference { get; set; }
  }

  public class BookInfo : ItemInfo<Book>
  {
    
  }

  [Serializable]
  public class RefHolder<T> : Structure
  {
    [Field]
    public T Ref { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  [TestFixture]
  public class Issue0785_GenericStructureBugs : AutoBuildTest
  {
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
        var book = new Book() { Title = "Book" };
        var bookRef = new RefHolder<Book>();
        bookRef.Ref = book;
        book.BookRef = bookRef;
        Assert.AreEqual(book, book.BookRef.Ref);
      }
    }
  }
}