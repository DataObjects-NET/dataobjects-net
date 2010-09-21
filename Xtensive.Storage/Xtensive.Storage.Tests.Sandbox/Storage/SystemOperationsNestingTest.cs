// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.06.24

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Operations;

namespace Xtensive.Storage.Tests.Storage.SystemOperationsNestingTest
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
    public Author Author { get; set; }

    public override string ToString()
    {
      return Title;
    }
  }

  [Serializable]
  [HierarchyRoot]
  public class Author : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string Title { get; set; }

    [Field]
    [Association(PairTo = "Author")]
    public EntitySet<Book> Books { get; private set; }

    public override string ToString()
    {
      return Title;
    }
  }

  [TestFixture]
  public class SystemOperationsNestingTest : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(Book).Assembly, typeof(Book).Namespace);
      return configuration;
    }

    /// <summary>
    /// Mains the test.
    /// </summary>
    [Test]
    public void MainTest()
    {
      var ds = new DisconnectedState();
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        using (ds.Attach(session))
        using (ds.Connect()) {

        }
        // tx.Complete();
      }
    }
  }
}