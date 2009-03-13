// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2009.03.11

using NUnit.Framework;
using Xtensive.Storage.Model;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;
using System.Linq;
using Xtensive.Core.Helpers;
using Xtensive.Storage.Tests.Storage.Keys;

namespace Xtensive.Storage.Tests.Linq
{
  public class EntitySetTest : NorthwindDOModelTest
  {
    [Test]
    public void OneToManyTest()
    {
      using (Domain.OpenSession())
        using (var t = Transaction.Open()) {
          var categories = Query<Category>.All;
          var resultCount = categories.First().Products.Count();
          var queryResult = categories.First().Products.ToList().Count();
          Assert.AreEqual(queryResult, resultCount);
          t.Complete();
        }
    }

    [Test]
    public void ManyToManyTest()
    {
      using (Domain.OpenSession())
        using (var t = Transaction.Open()) {
          var employees = Query<Employee>.All;
          var territories = Query<Territory>.All;
          var resultCount = employees.First().Territories.Count();
          var queryResult = employees.First().Territories.ToList().Count();
          Assert.AreEqual(queryResult, resultCount);
          resultCount = territories.First().Employees.Count();
          queryResult = territories.First().Employees.ToList().Count();
          Assert.AreEqual(queryResult, resultCount);
          t.Complete();
      }

    }
  }
}