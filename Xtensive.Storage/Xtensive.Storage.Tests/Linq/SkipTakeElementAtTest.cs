// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.10.20

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;
using Xtensive.Storage.Linq;

namespace Xtensive.Storage.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class SkipTakeElementAtTest : NorthwindDOModelTest
  {

    [Test]
    [ExpectedException(typeof (InvalidOperationException))]
    public void ReuseTake1Test()
    {
      var result1 = TakeCustomersIncorrect(1).Count();
      Assert.AreEqual(1, result1);
      var result2 = TakeCustomersIncorrect(2).Count();
      Assert.AreEqual(2, result2);
    }

    [Test]
    public void ReuseTake2Test()
    {
      var result1 = TakeCustomersCorrect(1).Count();
      Assert.AreEqual(1, result1);
      var result2 = TakeCustomersCorrect(2).Count();
      Assert.AreEqual(2, result2);
    }

    [Test]
    [ExpectedException(typeof (InvalidOperationException))]
    public void ReuseSkipTest()
    {
      var totalCount = Query<Customer>.All.Count();
      var result1 = SkipCustomersIncorrect(1).Count();
      Assert.AreEqual(totalCount - 1, result1);
      var result2 = SkipCustomersIncorrect(2).Count();
      Assert.AreEqual(totalCount - 2, result2);
    }

    [Test]
    public void ReuseSkip2Test()
    {
      var totalCount = Query<Customer>.All.Count();
      var result1 = SkipCustomersCorrect(1).Count();
      Assert.AreEqual(totalCount - 1, result1);
      var result2 = SkipCustomersCorrect(2).Count();
      Assert.AreEqual(totalCount - 2, result2);
    }

    [Test]
    [ExpectedException(typeof (InvalidOperationException))]
    public void ReuseElementAtTest()
    {
      var customers = Query<Customer>.All.OrderBy(customer=>customer.Id).ToList();
      Assert.IsTrue(customers.Count>0);
      for (int i = 0; i < customers.Count; i++)
        Assert.AreEqual(customers[i], ElementAtIncorrect(i));
    }

    [Test]
    public void ReuseElementAt2Test()
    {
      var customers = Query<Customer>.All.OrderBy(customer=>customer.Id).ToList();
      Assert.IsTrue(customers.Count>0);
      for (int i = -100; i < customers.Count + 100; i++)
        if (i < 0) {
          int index = i;
          AssertEx.ThrowsInvalidOperationException(()=> ElementAtCorrect(index));
        }
        else if  (i >= customers.Count) {
          int index = i;
          AssertEx.ThrowsInvalidOperationException(()=> ElementAtCorrect(index));
        }
        else
          Assert.AreEqual(customers[i], ElementAtCorrect(i));

    }


    [Test]
    public void ReuseElementAtOrDefaultTest()
    {
      var customers = Query<Customer>.All.OrderBy(customer=>customer.Id).ToList();
      Assert.IsTrue(customers.Count>0);
      for (int i = -100; i < customers.Count + 100; i++) {
        if (i < 0 || i >= customers.Count)
          Assert.IsNull(ElementAtOrDefaultCorrect(i));
        else
          Assert.AreEqual(customers[i], ElementAtOrDefaultCorrect(i));
      }
    }

    [Test]
    [ExpectedException(typeof (InvalidOperationException))]
    public void ReuseElementAtOrDefault2Test()
    {
      var customers = Query<Customer>.All.OrderBy(customer=>customer.Id).ToList();
      Assert.IsTrue(customers.Count>0);
      for (int i = -100; i < customers.Count + 100; i++) {
        if (i < 0 || i >= customers.Count)
          Assert.IsNull(ElementAtOrDefaultIncorrect(i));
        else
          Assert.AreEqual(customers[i], ElementAtOrDefaultIncorrect(i));
      }
    }

    [Test]
    public void ElementAtOrDefaultIsNotRootTest()
    {
      var customers = Query<Customer>.All.OrderBy(customer=>customer.Id).ToList();
      Assert.IsTrue(customers.Count>0);
      for (int i = -100; i < customers.Count + 100; i++) {
        if (i < 0 || i >= customers.Count)
          Assert.IsNull(Query<Customer>.All.OrderBy(customer=>customer.Id).ElementAtOrDefault(i));
        else
          Assert.AreEqual(customers[i], Query<Customer>.All.OrderBy(customer=>customer.Id).ElementAtOrDefault(i));
      }
    }

    [Test]
    public void ElementAtOrDefaultIsRootTest()
    {
      var customers = Query<Customer>.All.ToList();
      Assert.IsTrue(customers.Count>0);
      for (int i = -100; i < customers.Count + 100; i++) {
        if (i < 0 || i >= customers.Count)
          Assert.IsNull(Query<Customer>.All.ElementAtOrDefault(i));
        else
          Assert.AreEqual(customers[i], Query<Customer>.All.ElementAtOrDefault(i));
      }
    }

    [Test]
    public void ElementAtIsNotRootTest()
    {
      var customers = Query<Customer>.All.OrderBy(customer=>customer.Id).ToList();
      Assert.IsTrue(customers.Count>0);
      for (int i = -100; i < customers.Count + 100; i++) {
        if (i < 0 ) {
          int index = i;
          AssertEx.ThrowsArgumentOutOfRangeException(()=> Query<Customer>.All.OrderBy(customer => customer.Id).ElementAt(index));
        }
        else if (i >= customers.Count) {
          int index = i;
          AssertEx.ThrowsInvalidOperationException(()=> Query<Customer>.All.OrderBy(customer => customer.Id).ElementAt(index));
        }
        else
          Assert.AreEqual(customers[i], Query<Customer>.All.OrderBy(customer=>customer.Id).ElementAt(i));
      }
    }

    [Test]
    public void ElementAtIsRootTest()
    {
      var customers = Query<Customer>.All.ToList();
      Assert.IsTrue(customers.Count>0);
      for (int i = -100; i < customers.Count + 100; i++) {
        if (i < 0) {
          int index = i;
          AssertEx.ThrowsArgumentOutOfRangeException(()=> Query<Customer>.All.ElementAt(index));
        }
        else if  (i >= customers.Count) {
          int index = i;
          AssertEx.ThrowsInvalidOperationException(()=> Query<Customer>.All.ElementAt(index));
        }
        else
          Assert.AreEqual(customers[i], Query<Customer>.All.ElementAt(i));
      }
    }

    private IEnumerable<Customer> TakeCustomersIncorrect(int amount)
    {
      return Query.Execute(() => Query<Customer>.All.Take(amount));
    }

    private IEnumerable<Customer> TakeCustomersCorrect(int amount)
    {
      return Query.Execute(() => Query<Customer>.All.Take(()=>amount));
    }

    private IEnumerable<Customer> SkipCustomersIncorrect(int skip)
    {
      return Query.Execute(() => Query<Customer>.All.Skip(skip));
    }

    private IEnumerable<Customer> SkipCustomersCorrect(int skip)
    {
      return Query.Execute(() => Query<Customer>.All.Skip(()=>skip));
    }

    private Customer ElementAtIncorrect(int index)
    {
      return Query.Execute(() => Query<Customer>.All.OrderBy(customer=>customer.Id).ElementAt(index));
    }

    private Customer ElementAtCorrect(int index)
    {
      return Query.Execute(() => Query<Customer>.All.OrderBy(customer=>customer.Id).ElementAt(()=>index));
    }

    private Customer ElementAtOrDefaultIncorrect(int index)
    {
      return Query.Execute(() => Query<Customer>.All.OrderBy(customer=>customer.Id).ElementAtOrDefault(index));
    }

    private Customer ElementAtOrDefaultCorrect(int index)
    {
      return Query.Execute(() => Query<Customer>.All.OrderBy(customer=>customer.Id).ElementAtOrDefault(()=>index));
    }
  }
}