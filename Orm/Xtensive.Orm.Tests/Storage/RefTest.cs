// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.10.01

using System;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using System.Linq;

namespace Xtensive.Orm.Tests.Storage.RefTest
{
  [HierarchyRoot]
  public class Author : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }

  [TestFixture]
  public class RefTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Author));
      return config;
    }

    [Test]
    public void CombinedTest()
    {
      Key authorKey;
      Ref<Author> authorRef;

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var author = new Author();
        authorKey = author.Key;
        authorRef = (Ref<Author>) author;
        tx.Complete();
      }

      authorRef = Cloner.CloneViaBinarySerialization(authorRef);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        Assert.That(authorRef.Key, Is.EqualTo(authorKey));
        var author = authorRef.Value;
        Assert.That(author, Is.Not.Null);
        Assert.That(author.Key, Is.EqualTo(authorRef.Key));
        tx.Complete();
      }

    }
  }
}