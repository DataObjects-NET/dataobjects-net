// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2013.08.12

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Orm.Tests.Issues.IssueJira0471_LikeOperatorSupportModel;
using Xtensive.Core;

namespace Xtensive.Orm.Tests.Issues.IssueJira0471_LikeOperatorSupportModel
{
  [HierarchyRoot]
  public class Customer : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    public string FirstName { get; set; }

    [Field]
    public string LastName { get; set; }
  }
}

namespace Xtensive.Orm.Tests.Issues
{
  class IssueJira0471_LikeOperatorSupport : AutoBuildTest
  {
    protected override Configuration.DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof(Customer).Assembly, typeof(Customer).Namespace);
      return configuration;
    }

    protected override void PopulateData()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        new Customer {FirstName = "Alexey", LastName = "Kulakov"};
        new Customer {FirstName = "Ulexey", LastName = "Kerzhakov"};
        new Customer {FirstName = "Klexey", LastName = "Komarov"};
        new Customer { FirstName = "Klexey", LastName = "K%[maroff" };
        new Customer {FirstName = "Martha", LastName = "$mith"};
        new Customer {FirstName = "E%ric", LastName = "Cartman"};
        new Customer {FirstName = "Kyle", LastName = "Broflovski"};
        transaction.Complete();
      }
    }

    [Test]
    public void SimlpeOneSymbolTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var firstQuery = from a in session.Query.All<Customer>()
          where a.FirstName.Like("K_le")
          select a;
        Assert.That(firstQuery.First().FirstName,Is.EqualTo("Kyle"));
      }
    }

    [Test]
    public void SimpleSequenceTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var firstQuery = from a in session.Query.All<Customer>()
          where a.FirstName.Like("%xey")
          select a;
        Assert.That(firstQuery.Count(), Is.EqualTo(4));
      }
    }

    [Test]
    public void SimpleSpecialRegExpSimbolsTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var firstQuery = from a in session.Query.All<Customer>()
          where a.LastName.Like("$%")
          select a;
        Assert.That(firstQuery.First().LastName, Is.EqualTo("$mith"));
      }
    }

    [Test]
    public void SimpleEscapeSpecialSymbolsTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var firstQuery = from a in session.Query.All<Customer>()
          where a.FirstName.Like("E!%ric", '!')
          select a;
        Assert.That(firstQuery.First().FirstName, Is.EqualTo("E%ric"));
      }
    }
    [Test]
    public void AllInOneTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        var firstQuery = from a in session.Query.All<Customer>()
          where a.LastName.Like("K!%![m%f_",'!') 
          select a;
        Assert.That(firstQuery.First().LastName, Is.EqualTo("K%[maroff"));
      }
    }
  }
}
