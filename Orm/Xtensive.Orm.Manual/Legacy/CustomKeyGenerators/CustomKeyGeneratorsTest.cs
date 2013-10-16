// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.01.28

using System;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Manual.Legacy.CustomKeyGenerators
{
  #region Model

  [Serializable]
  [HierarchyRoot]
  [KeyGenerator(Name = "Author")]
  public class Author : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field(Length = 200)]
    public string Name { get; set; }

    [Field]
    public EntitySet<Book> Books { get; private set; }

    public override string ToString()
    {
      return Name;
    }

    // Constructors

    public Author(Session session)
      : base(session)
    {
    }

    public Author(Session session, int id)
      : base(session, id)
    {
    }
  }

  [Serializable]
  [HierarchyRoot]
  [KeyGenerator(Name = "Book")]
  public class Book : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field(Length = 200)]
    public string Name { get; set; }

    [Field]
    [Association(PairTo = "Books")]
    public EntitySet<Author> Authors { get; private set; }

    public override string ToString()
    {
      return string.Format("{0} by {1}",
        Name, Authors.ToCommaDelimitedString());
    }

    // Constructors

    public Book(Session session)
      : base (session)
    {
    }

    public Book(Session session, int id)
      : base(session, id)
    {
    }
  }

  #endregion

  [TestFixture]
  public class CustomKeyGeneratorsTest
  {
    [Test]
    public void CombinedTest()
    {
      // Creating new Domain configuration
      var config = DomainConfigurationFactory.CreateWithoutSessionConfigurations();
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      // Registering all types in the specified assembly and namespace
      config.Types.Register(typeof (Author).Assembly, typeof(Author).Namespace);
      // And finally building the domain
      var domain = Domain.Build(config);

      using (var session = domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {

          // Creating two authors
          var joseph = new Author(session) {Name = "Joseph Albahari"};
          var ben    = new Author(session) {Name = "Ben Albahari"};
          
          // Creating the Book book with book.Id = joseph.Id
          var book = new Book (session, joseph.Id) {Name = "C# 4.0 in a Nutshell"};
          book.Authors.Add(joseph);
          book.Authors.Add(ben);

          // Testing ProxyKeyGenerator
          Assert.AreSame(joseph, session.Query.SingleOrDefault(joseph.Key));
          Assert.AreSame(ben, session.Query.SingleOrDefault(ben.Key));
          // Must fail, if [KeyGenerator(typeof(ProxyKeyGenerator<Book, Author>))]
          // line is commented
          Assert.AreSame(book, session.Query.SingleOrDefault(book.Key));

          // Let's finally print the Book 
          Console.WriteLine(book);

          transactionScope.Complete();
        }
      }
    }
  }
}