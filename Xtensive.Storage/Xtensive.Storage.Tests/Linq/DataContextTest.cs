// Copyright (C) 2009 Xtensive LLC.
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
using Xtensive.Core.Testing;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  public class DataContextTest : NorthwindDOModelTest
  {
    private class Queryabe<T> : IQueryable<T>
  {
      public IEnumerator<T> GetEnumerator()
      {
        throw new NotImplementedException();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return GetEnumerator();
      }

      public Expression Expression
      {
        get { throw new NotImplementedException(); }
      }

      public Type ElementType
      {
        get { throw new NotImplementedException(); }
      }

      public IQueryProvider Provider
      {
        get { throw new NotImplementedException(); }
      }
  }

    private class UserException : Exception
    {
    }

    private static class DataContext
    {
      public static IQueryable<Order> Orders { get; private set; }
      public static IQueryable<Customer> Customers { get; private set; }

      public static IQueryable<Customer> CustomersException
      {
        get { throw new UserException(); }
      }

      public static IQueryable<Customer> CustomersNull
      {
        get { return null; }
      }

      public static IQueryable<Customer> CustomersNullQueryableExpression
      {
        get { throw new NotImplementedException(); }
      }

      public static IQueryable<Customer> CustomersFakeQueryableExpression
      {
        get { throw new NotImplementedException(); }
      }

      static DataContext()
      {
        Orders = Query<Order>.All;
        Customers = Query<Customer>.All;
      }
    }

    [Test]
    public void JoinTest()
    {
      var result = from c in DataContext.Customers
      from o in DataContext.Orders
      select new {c, o};
      var expected = from c in Query<Customer>.All.AsEnumerable()
      from o in Query<Order>.All.AsEnumerable()
      select new {c, o};
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void NullTest()
    {
      AssertEx.Throws<ArgumentNullException>(()=>DataContext.CustomersNull.SelectMany(c => DataContext.Orders, (c, o) => new {c, o}));
    }

    [Test]
    public void ExceptionTest()
    {
      AssertEx.Throws<UserException>(()=>DataContext.CustomersException.SelectMany(c => DataContext.Orders, (c, o) => new {c, o}));
    }
  }
}