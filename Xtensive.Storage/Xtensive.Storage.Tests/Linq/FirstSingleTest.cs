// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.04

using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  [TestFixture]
  public class FirstSingleTest : NorthwindDOModelTest
  {
    [Test]
    public void FirstTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var customer = Query<Customer>.All.First();
        Assert.IsNotNull(customer);
        t.Complete();
      }
    }

    [Test]
    public void FirstPredicateTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var customer = Query<Customer>.All.First(c => c.Id=="ALFKI");
        Assert.IsNotNull(customer);
        t.Complete();
      }
    }

    [Test]
    public void WhereFirstTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var customer = Query<Customer>.All.Where(c => c.Id=="ALFKI").First();
        Assert.IsNotNull(customer);
        t.Complete();
      }
    }

    [Test]
    public void FirstOrDefaultTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var customer = Query<Customer>.All.FirstOrDefault();
        Assert.IsNotNull(customer);
        t.Complete();
      }
    }

    [Test]
    public void FirstOrDefaultPredicateTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        Query<Customer>.All.FirstOrDefault(c => c.Id == "ALFKI");
        t.Complete();
      }
    }

    [Test]
    public void WhereFirstOrDefaultTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var customer = Query<Customer>.All.Where(c => c.Id == "ALFKI").FirstOrDefault();
        Assert.IsNotNull(customer);
        t.Complete();
      }
    }

    [Test]
    public void SingleTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        AssertEx.ThrowsInvalidOperationException(() => Query<Customer>.All.Single());
        t.Complete();
      }
    }

    [Test]
    public void SinglePredicateTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var customer = Query<Customer>.All.Single(c => c.Id == "ALFKI");
        Assert.IsNotNull(customer);
        t.Complete();
      }
    }

    [Test]
    public void WhereSingleTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var customer = Query<Customer>.All.Where(c => c.Id == "ALFKI").Single();
        Assert.IsNotNull(customer);
        t.Complete();
      }
    }

    [Test]
    public void SingleOrDefaultTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        AssertEx.ThrowsInvalidOperationException(() => Query<Customer>.All.SingleOrDefault());
        t.Complete();
      }
    }

    [Test]
    public void SingleOrDefaultPredicateTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var customer = Query<Customer>.All.SingleOrDefault(c => c.Id=="ALFKI");
        Assert.IsNotNull(customer);
        t.Complete();
      }
    }

    [Test]
    public void WhereSingleOrDefaultTest()
    {
      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        var customer = Query<Customer>.All.Where(c => c.Id == "ALFKI").SingleOrDefault();
        Assert.IsNotNull(customer);
        t.Complete();
      }
    }
  }
}