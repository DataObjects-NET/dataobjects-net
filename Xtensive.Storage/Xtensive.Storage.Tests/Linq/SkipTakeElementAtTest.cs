// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.10.20

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Testing;
using Xtensive.Storage.Linq;
using Xtensive.Storage.Providers;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class SkipTakeElementAtTest : NorthwindDOModelTest
  {
    [Test]
    [ExpectedException(typeof (TranslationException))]
    public void ReuseTake1Test()
    {
      var result1 = TakeCustomersIncorrect(1).Count();
      Assert.AreEqual(1, result1);
      var result2 = TakeCustomersIncorrect(2).Count();
      Assert.AreEqual(2, result2);
    }

    [Test]
    public void MultipleTakeSkipRandomTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.RowNumber);
      IQueryable<Customer> query = Query.All<Customer>().OrderBy(customer=>customer.Id);
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
    [ExpectedException(typeof (TranslationException))]
    public void ReuseSkipTest()
    {
      var totalCount = Query.All<Customer>().Count();
      var result1 = SkipCustomersIncorrect(1).Count();
      Assert.AreEqual(totalCount - 1, result1);
      var result2 = SkipCustomersIncorrect(2).Count();
      Assert.AreEqual(totalCount - 2, result2);
    }

    [Test]
    public void ReuseSkip2Test()
    {
      Require.AllFeaturesSupported(ProviderFeatures.RowNumber);
      var totalCount = Query.All<Customer>().Count();
      var result1 = SkipCustomersCorrect(1).Count();
      Assert.AreEqual(totalCount - 1, result1);
      var result2 = SkipCustomersCorrect(2).Count();
      Assert.AreEqual(totalCount - 2, result2);
    }

    [Test]
    [ExpectedException(typeof (TranslationException))]
    public void ReuseElementAtTest()
    {
      var customers = Query.All<Customer>().OrderBy(customer => customer.Id).ToList();
      Assert.IsTrue(customers.Count > 0);
      for (int i = 0; i < customers.Count; i++)
        Assert.AreEqual(customers[i], ElementAtIncorrect(i));
    }

    [Test]
    public void ReuseElementAt2Test()
    {
      Require.AllFeaturesSupported(ProviderFeatures.RowNumber);
      var customers = Query.All<Customer>().OrderBy(customer => customer.Id).ToList();
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
      Require.AllFeaturesSupported(ProviderFeatures.RowNumber);
      var customers = Query.All<Customer>().OrderBy(customer => customer.Id).ToList();
      Assert.IsTrue(customers.Count > 0);
      for (int i = -100; i < customers.Count + 100; i++) {
        if (i < 0 || i >= customers.Count)
          Assert.IsNull(ElementAtOrDefaultCorrect(i));
        else
          Assert.AreEqual(customers[i], ElementAtOrDefaultCorrect(i));
      }
    }

    [Test]
    [ExpectedException(typeof (TranslationException))]
    public void ReuseElementAtOrDefault2Test()
    {
      var customers = Query.All<Customer>().OrderBy(customer => customer.Id).ToList();
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
      Require.AllFeaturesSupported(ProviderFeatures.RowNumber);
      var customers = Query.All<Customer>().OrderBy(customer => customer.Id).ToList();
      Assert.IsTrue(customers.Count > 0);
      for (int i = -100; i < customers.Count + 100; i++) {
        if (i < 0 || i >= customers.Count)
          Assert.IsNull(Query.All<Customer>().OrderBy(customer => customer.Id).ElementAtOrDefault(i));
        else
          Assert.AreEqual(customers[i], Query.All<Customer>().OrderBy(customer => customer.Id).ElementAtOrDefault(i));
      }
    }

    [Test]
    public void ElementAtOrDefaultIsRootTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.RowNumber);
      var customers = Query.All<Customer>().ToList();
      Assert.IsTrue(customers.Count > 0);
      for (int i = -100; i < customers.Count + 100; i++) {
        if (i < 0 || i >= customers.Count)
          Assert.IsNull(Query.All<Customer>().ElementAtOrDefault(i));
        else
          Assert.AreEqual(customers[i], Query.All<Customer>().ElementAtOrDefault(i));
      }
    }

    [Test]
    public void ElementAtIsNotRootTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.RowNumber);
      var customers = Query.All<Customer>().OrderBy(customer => customer.Id).ToList();
      Assert.IsTrue(customers.Count > 0);
      for (int i = -100; i < customers.Count + 100; i++) {
        if (i < 0) {
          int index = i;
          AssertEx.ThrowsArgumentOutOfRangeException(() => Query.All<Customer>().OrderBy(customer => customer.Id).ElementAt(index));
        }
        else if (i >= customers.Count) {
          int index = i;
          AssertEx.ThrowsInvalidOperationException(() => Query.All<Customer>().OrderBy(customer => customer.Id).ElementAt(index));
        }
        else
          Assert.AreEqual(customers[i], Query.All<Customer>().OrderBy(customer => customer.Id).ElementAt(i));
      }
    }

    [Test]
    public void ElementAtIsRootTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.RowNumber);
      var customers = Query.All<Customer>().ToList();
      Assert.IsTrue(customers.Count > 0);
      for (int i = -100; i < customers.Count + 100; i++) {
        if (i < 0) {
          int index = i;
          AssertEx.ThrowsArgumentOutOfRangeException(() => Query.All<Customer>().ElementAt(index));
        }
        else if (i >= customers.Count) {
          int index = i;
          AssertEx.ThrowsInvalidOperationException(() => Query.All<Customer>().ElementAt(index));
        }
        else
          Assert.AreEqual(customers[i], Query.All<Customer>().ElementAt(i));
      }
    }

    private IEnumerable<Customer> TakeCustomersIncorrect(int amount)
    {
      return Query.Execute(() => Query.All<Customer>().Take(amount));
    }

    private IEnumerable<Customer> TakeCustomersCorrect(int amount)
    {
      return Query.Execute(() => Query.All<Customer>().Take(() => amount));
    }

    private IEnumerable<Customer> SkipCustomersIncorrect(int skip)
    {
      return Query.Execute(() => Query.All<Customer>().Skip(skip));
    }

    private IEnumerable<Customer> SkipCustomersCorrect(int skip)
    {
      return Query.Execute(() => Query.All<Customer>().Skip(() => skip));
    }

    private Customer ElementAtIncorrect(int index)
    {
      return Query.Execute(() => Query.All<Customer>().OrderBy(customer => customer.Id).ElementAt(index));
    }

    private Customer ElementAtCorrect(int index)
    {
      return Query.Execute(() => Query.All<Customer>().OrderBy(customer => customer.Id).ElementAt(() => index));
    }

    private Customer ElementAtOrDefaultIncorrect(int index)
    {
      return Query.Execute(() => Query.All<Customer>().OrderBy(customer => customer.Id).ElementAtOrDefault(index));
    }

    private Customer ElementAtOrDefaultCorrect(int index)
    {
      return Query.Execute(() => Query.All<Customer>().OrderBy(customer => customer.Id).ElementAtOrDefault(() => index));
    }
  }
}