// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.06.26

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.BookAuthorModel;

namespace Xtensive.Orm.Tests.Storage.BookAuthorModel
{
  [Serializable]
  [HierarchyRoot]
  public class Book : Entity
  {
    [Field, Key]
    public int ID { get; private set; }

    [Field(Length = 10)]
    public string Title { get; set; }

    [Field(LazyLoad = true)]
    public string Text { get; set; }

    [Field]
    public Author Author { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Author : Entity
  {
    [Field, Key]
    public int ID { get; private set; }

    [Field]
    public string Name { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  public class LazyLoadTest : AutoBuildTest
  {
    private const string TEXT = "Text";
    private const string TITLE = "Title";
    private Key key;

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();

      // Creating a book
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          Book b = new Book();
          key = b.Key;
          b.Title = TITLE;
          b.Text = TEXT;
          t.Complete();
        }
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Storage.BookAuthorModel");
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          Book b = session.Query.SingleOrDefault<Book>(key);
          Tuple tuple = b.Tuple;

          Assert.IsTrue(tuple.GetFieldState(2).IsAvailable());
          Assert.IsFalse(tuple.GetFieldState(3).IsAvailable());
          Assert.AreEqual(TITLE, b.Title);
          Assert.IsFalse(tuple.GetFieldState(3).IsAvailable());

          // Fetching lazy load field
          Assert.AreEqual(TEXT, b.Text);
          t.Complete();
        }
      }
    }
  }
}