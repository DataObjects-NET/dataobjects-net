// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.12.02

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Xtensive.Testing;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Orm.Tests.Linq
{
  public class DataContextTest : NorthwindDOModelTest
  {

    private class UserException : Exception
    {
    }

    private class DataContext
    {
      public IQueryable<Order> Orders { get; private set; }
      public IQueryable<Customer> Customers { get; private set; }

      public IQueryable<Customer> CustomersException
      {
        get { throw new UserException(); }
      }

      public IQueryable<Customer> CustomersNull
      {
        get { return null; }
      }

      public IQueryable<Customer> CustomersNullQueryableExpression
      {
        get { throw new NotImplementedException(); }
      }

      public IQueryable<Customer> CustomersFakeQueryableExpression
      {
        get { throw new NotImplementedException(); }
      }

      public DataContext(Session session)
      {
        Orders = session.Query.All<Order>();
        Customers = session.Query.All<Customer>();
      }
    }

    [Test]
    public void JoinTest()
    {
      var context = new DataContext(Session.Current);
      var result = 
        from c in context.Customers
        from o in context.Orders
        select new {c, o};
      var expected = from c in Session.Query.All<Customer>().AsEnumerable()
      from o in Session.Query.All<Order>().AsEnumerable()
      select new {c, o};
      Assert.AreEqual(0, expected.Except(result).Count());
    }

    [Test]
    public void NullTest()
    {
      var context = new DataContext(Session.Current);
      AssertEx.Throws<ArgumentNullException>(()=>context.CustomersNull.SelectMany(c => context.Orders, (c, o) => new {c, o}));
    }

    [Test]
    public void ExceptionTest()
    {
      var context = new DataContext(Session.Current);
      AssertEx.Throws<UserException>(()=>context.CustomersException.SelectMany(c => context.Orders, (c, o) => new {c, o}));
    }
  }
}