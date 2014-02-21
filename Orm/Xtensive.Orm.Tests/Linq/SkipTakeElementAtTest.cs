// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.10.20

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Linq;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.ObjectModel;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Orm.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class SkipTakeElementAtTest : NorthwindDOModelTest
  {
    [Test]
    [ExpectedException(typeof (QueryTranslationException))]
    public void ReuseTake1Test()
    {
      var result1 = TakeCustomersIncorrect(1).Count();
      Assert.AreEqual(1, result1);
      var result2 = TakeCustomersIncorrect(2).Count();
      Assert.AreEqual(2, result2);
    }

    [Test]
    public void TakeTest()
    {
      var query = Session.Query.All<Customer>()
        .Where(c => c.Address.Country == "Germany")
        .Select(c => c.Key)
        .Take(10);
      var list = query.ToList();
      Assert.Greater(list.Count, 0);
    }

    [Test]
    public void MultipleTakeSkipRandomTest()
    {
      Require.AnyFeatureSupported(ProviderFeatures.RowNumber | ProviderFeatures.NativePaging);
      IQueryable<Customer> query = Session.Query.All<Customer>().OrderBy(customer=>customer.Id);
      int count = query.Count();
      var expected = query.AsEnumerable();
      var randomManager = RandomManager.CreateRandom();
      for (int i = 0; i < 5; i++) {
        int randomInt = randomManager.Next(0, 9);
        switch (randomInt) {
        case 0:
          // Take with negative count
          var takeNegativeCount = randomManager.Next(-count + 1, -1);
          query = query.Take(takeNegativeCount);
          expected = expected.Take(takeNegativeCount);
          break;
        case 1:
        case 2:
        case 3:
        case 4:
          // Take
          var takeCount = randomManager.Next((int) (count * 0.8), count);
          query = query.Take(takeCount);
          expected = expected.Take(takeCount);
          break;
        case 5:
        case 6:
        case 7:
        case 8:
          // Skip
          var skipCount = randomManager.Next((int) (count * 0.1), count);
          query = query.Skip(skipCount);
          expected = expected.Skip(skipCount);
          break;
        case 9:
          // Skip with negative count
          var skipNegativeCount = randomManager.Next((int) (-count*0.1 + 1), -1);
          query = query.Skip(skipNegativeCount);
          expected = expected.Skip(skipNegativeCount);
          break;
        }
      }
      Assert.IsTrue(expected.SequenceEqual(query));
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
    [ExpectedException(typeof (QueryTranslationException))]
    public void ReuseSkipTest()
    {
      var totalCount = Session.Query.All<Customer>().Count();
      var result1 = SkipCustomersIncorrect(1).Count();
      Assert.AreEqual(totalCount - 1, result1);
      var result2 = SkipCustomersIncorrect(2).Count();
      Assert.AreEqual(totalCount - 2, result2);
    }

    [Test]
    public void ReuseSkip2Test()
    {
      Require.AnyFeatureSupported(ProviderFeatures.RowNumber | ProviderFeatures.NativePaging);
      var totalCount = Session.Query.All<Customer>().Count();
      var result1 = SkipCustomersCorrect(1).Count();
      Assert.AreEqual(totalCount - 1, result1);
      var result2 = SkipCustomersCorrect(2).Count();
      Assert.AreEqual(totalCount - 2, result2);
    }

    [Test]
    [ExpectedException(typeof (QueryTranslationException))]
    public void ReuseElementAtTest()
    {
      var customers = Session.Query.All<Customer>().OrderBy(customer => customer.Id).ToList();
      Assert.IsTrue(customers.Count > 0);
      for (int i = 0; i < customers.Count; i++)
        Assert.AreEqual(customers[i], ElementAtIncorrect(i));
    }

    [Test]
    public void ReuseElementAt2Test()
    {
      Require.AnyFeatureSupported(ProviderFeatures.RowNumber | ProviderFeatures.NativePaging);
      var customers = Session.Query.All<Customer>().OrderBy(customer => customer.Id).ToList();
      Assert.IsTrue(customers.Count > 0);
      for (int i = -100; i < customers.Count + 100; i++)
        if (i < 0) {
          int index = i;
          AssertEx.ThrowsInvalidOperationException(() => ElementAtCorrect(index));
        }
        else if (i >= customers.Count) {
          int index = i;
          AssertEx.ThrowsInvalidOperationException(() => ElementAtCorrect(index));
        }
        else
          Assert.AreEqual(customers[i], ElementAtCorrect(i));
    }


    [Test]
    public void ReuseElementAtOrDefaultTest()
    {
      Require.AnyFeatureSupported(ProviderFeatures.RowNumber | ProviderFeatures.NativePaging);
      var customers = Session.Query.All<Customer>().OrderBy(customer => customer.Id).ToList();
      Assert.IsTrue(customers.Count > 0);
      for (int i = -100; i < customers.Count + 100; i++) {
        if (i < 0 || i >= customers.Count)
          Assert.IsNull(ElementAtOrDefaultCorrect(i));
        else
          Assert.AreEqual(customers[i], ElementAtOrDefaultCorrect(i));
      }
    }

    [Test]
    [ExpectedException(typeof (QueryTranslationException))]
    public void ReuseElementAtOrDefault2Test()
    {
      var customers = Session.Query.All<Customer>().OrderBy(customer => customer.Id).ToList();
      Assert.IsTrue(customers.Count > 0);
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
      Require.AnyFeatureSupported(ProviderFeatures.RowNumber | ProviderFeatures.NativePaging);
      var customers = Session.Query.All<Customer>().OrderBy(customer => customer.Id).ToList();
      Assert.IsTrue(customers.Count > 0);
      for (int i = -100; i < customers.Count + 100; i++) {
        if (i < 0 || i >= customers.Count)
          Assert.IsNull(Session.Query.All<Customer>().OrderBy(customer => customer.Id).ElementAtOrDefault(i));
        else
          Assert.AreEqual(customers[i], Session.Query.All<Customer>().OrderBy(customer => customer.Id).ElementAtOrDefault(i));
      }
    }

    [Test]
    public void ElementAtOrDefaultIsRootTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.RowNumber);
      var customers = Session.Query.All<Customer>().ToList();
      Assert.IsTrue(customers.Count > 0);
      for (int i = -100; i < customers.Count + 100; i++) {
        if (i < 0 || i >= customers.Count)
          Assert.IsNull(Session.Query.All<Customer>().ElementAtOrDefault(i));
        else
          Assert.AreEqual(customers[i], Session.Query.All<Customer>().ElementAtOrDefault(i));
      }
    }

    [Test]
    public void ElementAtIsNotRootTest()
    {
      Require.AnyFeatureSupported(ProviderFeatures.RowNumber | ProviderFeatures.NativePaging);
      var customers = Session.Query.All<Customer>().OrderBy(customer => customer.Id).ToList();
      Assert.IsTrue(customers.Count > 0);
      for (int i = -100; i < customers.Count + 100; i++) {
        if (i < 0) {
          int index = i;
          AssertEx.ThrowsArgumentOutOfRangeException(() => Session.Query.All<Customer>().OrderBy(customer => customer.Id).ElementAt(index));
        }
        else if (i >= customers.Count) {
          int index = i;
          AssertEx.ThrowsInvalidOperationException(() => Session.Query.All<Customer>().OrderBy(customer => customer.Id).ElementAt(index));
        }
        else
          Assert.AreEqual(customers[i], Session.Query.All<Customer>().OrderBy(customer => customer.Id).ElementAt(i));
      }
    }

    [Test]
    public void ElementAtIsRootTest()
    {
      Require.AnyFeatureSupported(ProviderFeatures.RowNumber | ProviderFeatures.NativePaging);
      var customers = Session.Query.All<Customer>().ToList();
      Assert.IsTrue(customers.Count > 0);
      for (int i = -100; i < customers.Count + 100; i++) {
        if (i < 0) {
          int index = i;
          AssertEx.ThrowsArgumentOutOfRangeException(() => Session.Query.All<Customer>().ElementAt(index));
        }
        else if (i >= customers.Count) {
          int index = i;
          AssertEx.ThrowsInvalidOperationException(() => Session.Query.All<Customer>().ElementAt(index));
        }
        else
          Assert.AreEqual(customers[i], Session.Query.All<Customer>().ElementAt(i));
      }
    }

    private IEnumerable<Customer> TakeCustomersIncorrect(int amount)
    {
      return Session.Query.Execute(qe => qe.All<Customer>().Take(amount));
    }

    private IEnumerable<Customer> TakeCustomersCorrect(int amount)
    {
      return Session.Query.Execute(qe => qe.All<Customer>().Take(() => amount));
    }

    private IEnumerable<Customer> SkipCustomersIncorrect(int skip)
    {
      return Session.Query.Execute(qe => qe.All<Customer>().Skip(skip));
    }

    private IEnumerable<Customer> SkipCustomersCorrect(int skip)
    {
      return Session.Query.Execute(qe => qe.All<Customer>().Skip(() => skip));
    }

    private Customer ElementAtIncorrect(int index)
    {
      return Session.Query.Execute(qe => qe.All<Customer>().OrderBy(customer => customer.Id).ElementAt(index));
    }

    private Customer ElementAtCorrect(int index)
    {
      return Session.Query.Execute(qe => qe.All<Customer>().OrderBy(customer => customer.Id).ElementAt(() => index));
    }

    private Customer ElementAtOrDefaultIncorrect(int index)
    {
      return Session.Query.Execute(qe => qe.All<Customer>().OrderBy(customer => customer.Id).ElementAtOrDefault(index));
    }

    private Customer ElementAtOrDefaultCorrect(int index)
    {
      return Session.Query.Execute(qe => qe.All<Customer>().OrderBy(customer => customer.Id).ElementAtOrDefault(() => index));
    }
  }
}