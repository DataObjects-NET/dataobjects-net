// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.03.15

using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Storage.CustomEntityConnectionModel;

namespace Xtensive.Orm.Tests.Storage.CustomEntityConnectionModel
{
  [HierarchyRoot]
  public class Book : Entity
  {
    [Key, Field]
    public int Id { get; private set; }
    
    [Field, Association(PairTo = "Right",
      OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
    public EntitySet<AuthorBookLink> Links { get; private set; }
  }

  [HierarchyRoot]
  public class Author : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field, Association(PairTo = "Left",
      OnOwnerRemove = OnRemoveAction.Cascade, OnTargetRemove = OnRemoveAction.Clear)]
    public EntitySet<AuthorBookLink> Links { get; private set; }
  }

  public abstract class Link<TLeft, TRight> : Entity
    where TLeft : IEntity
    where TRight : IEntity
  {
    [Key(0), Field]
    public TLeft Left { get; private set; }

    [Key(1), Field]
    public TRight Right { get; private set; }

    public Link(TLeft left, TRight right)
      : base(left, right)
    {
    }
  }

  [HierarchyRoot, KeyGenerator(KeyGeneratorKind.None)]
  public class AuthorBookLink : Link<Author, Book>
  {
    [Field]
    public string Comment { get; set; }

    public AuthorBookLink(Author left, Book right)
      : base(left, right)
    {
    }
  }
}

namespace Xtensive.Orm.Tests.Storage
{
  [TestFixture]
  public class CustomEntityConnectionTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Book).Assembly, typeof (Book).Namespace);
      return configuration;
    }

    [Test]
    public void CombinedTest()
    {
      using (var session = Domain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var book = new Book();
        var author = new Author();
        new AuthorBookLink(author, book);
        // Session.Current.Persist();
        Assert.AreEqual(1, book.Links.Count);
        Assert.AreEqual(1, author.Links.Count);
        t.Complete();
      }
    }
  }
}