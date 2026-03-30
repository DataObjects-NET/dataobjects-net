// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.11.26

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.Issue0009_Model;

namespace Xtensive.Orm.Tests.Issues.Issue0009_Model
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

namespace Xtensive.Orm.Tests.Issues
{
  public class Issue0009_2SymmetricTablesForMNRelationship : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.RegisterCaching(Assembly.GetExecutingAssembly(), typeof(Book).Namespace);
      return config;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var book = new Book();
          var author = new Author();
          book.Authors.Add(author);

          Assert.That(book.Authors.Count, Is.EqualTo(1));
          Assert.That(author.Books.Count, Is.EqualTo(1));
          Assert.That(book.Authors.Contains(author), Is.True);
          Assert.That(author.Books.Contains(book), Is.True);
          t.Complete();
        }
      }
    }
  }
}