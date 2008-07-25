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
using Xtensive.Storage.Tests.LazyLoadTests;

namespace Xtensive.Storage.Tests.LazyLoadTests
{
  [HierarchyRoot(typeof(DefaultGenerator), "ID")]
  public class Book : Entity
  {
    [Field]
    public int ID { get; set; }

    [Field(LazyLoad = true)]
    public string Text { get; set; }
  }
}

namespace Xtensive.Storage.Tests.Storage
{
  [TestFixture]
  public class LazyLoadTests
  {
    [Test]
    public void MainTest()
    {
      DomainConfiguration config = new DomainConfiguration("memory://localhost/LazyLoadTests");
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.LazyLoadTests");
      Domain domain = Domain.Build(config);
      Key key;
      string text = "Text";
      using (domain.OpenSession()) {
        Book b = new Book();
        key = b.Key;
        b.Text = text;
      }
      using (domain.OpenSession()) {
        Book b = key.Resolve<Book>();
        Tuple tuple = b.Tuple;
        Assert.IsFalse(tuple.IsAvailable(2));
        Assert.AreEqual(text, b.Text);
      }
    }
  }
}