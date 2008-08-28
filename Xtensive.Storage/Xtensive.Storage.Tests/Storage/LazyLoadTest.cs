// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.06.26

using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Tests.Storage.BookAuthorModel;

namespace Xtensive.Storage.Tests.Storage.BookAuthorModel
{
  [HierarchyRoot(typeof(Generator), "ID")]
  public class Book : Entity
  {
    [Field]
    public int ID { get; set; }

    [Field(Length = 10)]
    public string Title { get; set; }

    [Field(LazyLoad = true, IsNullable = true)]
    public string Text { get; set; }

    [Field]
    public Author Author { get; set; }
  }

  [HierarchyRoot(typeof(Generator), "ID")]
  public class Author : Entity
  {
    [Field]
    public int ID { get; set; }

    [Field]
    public string Name { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Storage
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
      using (Domain.OpenSession()) {
        using (var t = Session.Current.OpenTransaction()) {
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
    public void RegularFieldTest()
    {
      using (Domain.OpenSession()) {
        using (var t = Session.Current.OpenTransaction()) {
          Book b = key.Resolve<Book>();
          Tuple tuple = b.Tuple;

          // Assert that fields are not loaded
          Assert.IsFalse(tuple.IsAvailable(2));
          Assert.IsFalse(tuple.IsAvailable(3));

          // This should load all not lazy load fields.
          Assert.AreEqual(TITLE, b.Title);
          Assert.IsTrue(tuple.IsAvailable(2));
          Assert.IsFalse(tuple.IsAvailable(3));

          // Fetching lazy load field
          Assert.AreEqual(TEXT, b.Text);
          t.Complete();
        }
      }
    }

    [Test]
    public void LazyLoadFieldTest()
    {
      using (Domain.OpenSession()) {
        Book b = key.Resolve<Book>();
        Tuple tuple = b.Tuple;

        // Assert that fields are not loaded
        Assert.IsFalse(tuple.IsAvailable(2));
        Assert.IsFalse(tuple.IsAvailable(3));

        // This should load all not lazy load fields + selected lazy load field.
        Assert.AreEqual(TEXT, b.Text);
        Assert.IsTrue(tuple.IsAvailable(2));
        Assert.IsTrue(tuple.IsAvailable(3));
        Assert.AreEqual(TEXT, b.Text);
      }
    }
  }
}