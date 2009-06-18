// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.04

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;
using Xtensive.Core.Testing;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage.Tests.Linq
{
  [Category("Linq")]
  [TestFixture]
  public class GroupByTest : NorthwindDOModelTest
  {
    [Test]
    public void AggregateAfterGroupingTest()
    {
      var query = Query<Product>.All
        .GroupBy(p => p.Category)
        .Select(g => g.Where(p2 => p2.UnitPrice==g.Count()));

      QueryDumper.Dump(query);
    }

    [Test]
    public void AggregateAfterGroupingAnonymousTest()
    {
      var query = Query<Product>.All
        .GroupBy(p => p.Category)
        .Select(g => new {
        CheapestProducts =
          g.Where(p2 => p2.UnitPrice==g.Count())
      });

      QueryDumper.Dump(query);
    }

    [Test]
    public void SimpleTest()
    {
      var result = Query<Product>.All.GroupBy(p => p.UnitPrice);
      QueryDumper.Dump(result);
    }


    [Test]
    public void GroupByWithWhereTest()
    {
      var query =
        Query<Product>.All
        .GroupBy(product => product.Id)
        .Where(grouping => true)
        .Select(products => products.Count());

      QueryDumper.Dump(query);
    }

    [Test]
    public void GroupingAsQueryableTest()
    {
      var result = Query<Product>.All.GroupBy(p => p.UnitPrice);
      foreach (IGrouping<decimal, Product> grouping in result)
        Assert.IsTrue(grouping.GetType().IsOfGenericInterface(typeof (IQueryable<>)));
    }

    [Test]
    public void SimpleEntityGroupTest()
    {
      var result = Query<Product>.All.GroupBy(p => p);
      DumpGrouping(result);
    }

    [Test]
    public void EntityGroupWithOrderTest()
    {
      var result = Query<Product>.All.GroupBy(p => p).OrderBy(g => g.Key.Id);
      var resultList = result.ToList();
      var expectedList = Query<Product>.All.ToList().GroupBy(p => p).OrderBy(g => g.Key.Id).ToList();
      Assert.AreEqual(resultList.Count, expectedList.Count());
      for (int i = 0; i < resultList.Count; i++) {
        Assert.AreEqual(expectedList[i].Key, resultList[i].Key);
        Assert.AreEqual(expectedList[i].Count(), resultList[i].Count());
        Assert.AreEqual(expectedList[i].Count(), resultList[i].AsQueryable().Count());
      }
      DumpGrouping(result);
    }

    [Test]
    public void EntityGroupSimpleTest()
    {
      var groupByResult = Query<Product>.All.GroupBy(p => p.Category);
      DumpGrouping(groupByResult);
    }

    [Test]
    public void EntityGroupTest()
    {
      var groupByResult = Query<Product>.All.GroupBy(p => p.Category);
      IEnumerable<Category> result = groupByResult
        .ToList()
        .Select(g => g.Key);
      IEnumerable<Category> expectedKeys = Query<Product>.All
        .Select(p => p.Category)
        .Distinct()
        .ToList();
      Assert.AreEqual(0, expectedKeys.Except(result).Count());
      foreach (var grouping in groupByResult) {
        var items = Query<Product>.All
          .Where(product => product.Category==grouping.Key)
          .ToList();
        Assert.AreEqual(0, items.Except(grouping).Count());
      }
      DumpGrouping(groupByResult);
    }

    [Test]
    public void EntityKeyGroupTest()
    {
      var groupByResult = Query<Product>.All.GroupBy(p => p.Category.Key);
      var result = groupByResult
        .ToList()
        .Select(g => g.Key);
      var expectedKeys = Query<Product>.All
        .Select(p => p.Category.Key)
        .Distinct()
        .ToList();
      foreach (var grouping in groupByResult) {
        var items = Query<Product>.All
          .Where(product => product.Category.Key==grouping.Key)
          .ToList();
        Assert.AreEqual(0, items.Except(grouping).Count());
      }
      Assert.AreEqual(0, expectedKeys.Except(result).Count());
      DumpGrouping(groupByResult);
    }

    [Test]
    public void EntityFieldGroupTest()
    {
      var groupByResult = Query<Product>.All.GroupBy(p => p.Category.CategoryName);
      var result = groupByResult
        .ToList()
        .Select(g => g.Key);
      var expectedKeys = Query<Product>.All
        .Select(p => p.Category.CategoryName)
        .Distinct()
        .ToList();
      foreach (var grouping in groupByResult) {
        var items = Query<Product>.All
          .Where(product => product.Category.CategoryName==grouping.Key)
          .ToList();
        Assert.AreEqual(0, items.Except(grouping).Count());
      }
      Assert.AreEqual(0, expectedKeys.Except(result).Count());
      DumpGrouping(groupByResult);
    }

    [Test]
    public void StructureGroupTest()
    {
      var groupByResult = Query<Customer>.All.GroupBy(customer => customer.Address);
      var result = groupByResult
        .ToList()
        .Select(g => g.Key);
      var expectedKeys = Query<Customer>.All
        .Select(customer => customer.Address)
        .Distinct()
        .ToList();
      foreach (var grouping in groupByResult) {
        var items = Query<Customer>.All
          .Where(customer => customer.Address==grouping.Key)
          .ToList();
        Assert.AreEqual(0, items.Except(grouping).Count());
      }
      Assert.AreEqual(0, expectedKeys.Except(result).Count());
      DumpGrouping(groupByResult);
    }


    [Test]
    public void StructureGroupWithNullFieldTest()
    {
      var customer = Query<Customer>.All.First();
      customer.Address.Country = null;
      Session.Current.Persist();
      StructureGroupTest();
    }

    [Test]
    public void AnonymousTypeGroupTest()
    {
      var groupByResult = Query<Customer>.All.GroupBy(customer => new {customer.Address.City, customer.Address.Country});
      var result = groupByResult
        .ToList()
        .Select(g => g.Key);
      var expectedKeys = Query<Customer>.All
        .Select(customer => new {customer.Address.City, customer.Address.Country})
        .Distinct()
        .ToList();
      foreach (var grouping in groupByResult) {
        var items = Query<Customer>.All
          .Where(customer => new {customer.Address.City, customer.Address.Country}==grouping.Key)
          .ToList();
        Assert.AreEqual(0, items.Except(grouping).Count());
      }
      Assert.AreEqual(0, expectedKeys.Except(result).Count());
      DumpGrouping(groupByResult);
    }

    [Test]
    public void AnonymousTypeEntityAndStructureGroupTest()
    {
      var groupByResult = Query<Employee>.All.GroupBy(employee => new {employee.Address});
      var result = groupByResult
        .ToList()
        .Select(g => g.Key);
      var expectedKeys = Query<Employee>.All
        .Select(employee => new {employee.Address})
        .Distinct()
        .ToList();
      foreach (var grouping in groupByResult) {
        var items = Query<Employee>.All
          .Where(employee => new {employee.Address}==grouping.Key)
          .ToList();
        Assert.AreEqual(0, items.Except(grouping).Count());
      }
      Assert.AreEqual(0, expectedKeys.Except(result).Count());
      DumpGrouping(groupByResult);
    }

    [Test]
    public void AnonymousStructureGroupTest()
    {
      var groupByResult = Query<Customer>.All.GroupBy(customer => new {customer.Address});
      var result = groupByResult
        .ToList()
        .Select(g => g.Key);
      var expectedKeys = Query<Customer>.All
        .Select(customer => new {customer.Address})
        .Distinct()
        .ToList();
      foreach (var grouping in groupByResult) {
        var items = Query<Customer>.All
          .Where(customer => new {customer.Address}==grouping.Key)
          .ToList();
        Assert.AreEqual(0, items.Except(grouping).Count());
      }
      Assert.AreEqual(0, expectedKeys.Except(result).Count());
      DumpGrouping(groupByResult);
    }

    [Test]
    public void AnonymousTypeEntityAndFieldTest()
    {
      var groupByResult = Query<Product>.All.GroupBy(product => new {
        product.Category,
        product.Category.CategoryName,
      });
      var result = groupByResult
        .ToList()
        .Select(g => g.Key);
      var expectedKeys = Query<Product>.All
        .Select(product => new {
          product.Category,
          product.Category.CategoryName,
        })
        .Distinct()
        .ToList();
      foreach (var grouping in groupByResult) {
        var items = Query<Product>.All
          .Where(product => new {
            product.Category,
            product.Category.CategoryName,
          }==grouping.Key)
          .ToList();
        Assert.AreEqual(0, items.Except(grouping).Count());
      }
      Assert.AreEqual(0, expectedKeys.Except(result).Count());
      DumpGrouping(groupByResult);
    }

    [Test]
    public void AnonymousTypeEntityGroupTest()
    {
      var groupByResult = Query<Product>.All.GroupBy(product => new {product.Category});
      var result = groupByResult
        .ToList()
        .Select(g => g.Key);
      var expectedKeys = Query<Product>.All
        .Select(product => new {product.Category})
        .Distinct()
        .ToList();
      foreach (var grouping in groupByResult) {
        var items = Query<Product>.All
          .Where(product => new {product.Category}==grouping.Key)
          .ToList();
        Assert.AreEqual(0, items.Except(grouping).Count());
      }
      Assert.AreEqual(0, expectedKeys.Except(result).Count());
      DumpGrouping(groupByResult);
    }


    [Test]
    public void DefaultTest()
    {
      var groupByResult = Query<Customer>.All.GroupBy(customer => customer.Address.City);
      var result = groupByResult
        .ToList()
        .Select(g => g.Key);
      var expectedKeys = Query<Customer>.All
        .Select(customer => customer.Address.City)
        .Distinct()
        .ToList();
      foreach (var grouping in groupByResult) {
        var items = Query<Customer>.All
          .Where(customer => customer.Address.City==grouping.Key)
          .ToList();
        Assert.AreEqual(0, items.Except(grouping).Count());
      }
      Assert.AreEqual(0, expectedKeys.Except(result).Count());
      DumpGrouping(groupByResult);
    }


    [Test]
    public void FilterGroupingByKeyTest()
    {
      var groupByResult = Query<Order>.All
        .GroupBy(order => order.ShippingAddress.City)
        .Where(grouping => grouping.Key.StartsWith("L"));
      var result = groupByResult
        .ToList()
        .Select(g => g.Key);
      var expectedKeys = Query<Order>.All
        .Select(order => order.ShippingAddress.City)
        .Where(city => city.StartsWith("L"))
        .Distinct()
        .ToList();
      foreach (var grouping in groupByResult) {
        var items = Query<Order>.All
          .Where(order => order.ShippingAddress.City==grouping.Key)
          .ToList();
        Assert.AreEqual(0, items.Except(grouping).Count());
      }
      Assert.AreEqual(0, expectedKeys.Except(result).Count());
      DumpGrouping(groupByResult);
    }

    [Test]
    public void FilterGroupingByCountAggregateTest()
    {
      var groupByResult = Query<Order>.All
        .GroupBy(o => o.ShippingAddress.City)
        .Where(g => g.Count() > 1);
      var result = groupByResult
        .ToList()
        .Select(g => g.Key);
      var expectedKeys = Query<Order>.All
        .Select(order => order.ShippingAddress.City)
        .Where(city => city.StartsWith("L"))
        .Distinct()
        .ToList();
      foreach (var grouping in groupByResult) {
        var items = Query<Order>.All
          .Where(order => order.ShippingAddress.City==grouping.Key)
          .ToList();
        Assert.AreEqual(0, items.Except(grouping).Count());
      }
      Assert.AreEqual(0, expectedKeys.Except(result).Count());
      DumpGrouping(groupByResult);
    }


    [Test]
    public void FilterGroupingBySumAggregateTest()
    {
      var queryable = Query<Order>.All.GroupBy(order => order.ShippingAddress.City);

      var groupByResult = queryable.Where(city => city.Sum(ord => ord.Freight) > 10);

      var groupByAlternativeResult = queryable.Where(city => city.Sum(ord => ord.Freight) <= 10);

      Assert.AreEqual(queryable.Count(), groupByResult.Count() + groupByAlternativeResult.Count());

      foreach (IGrouping<string, Order> grouping in groupByResult) {
        var sum = grouping.ToList().Sum(ord => ord.Freight);
        Assert.IsTrue(sum > 10);
      }

      foreach (IGrouping<string, Order> grouping in groupByAlternativeResult) {
        var sum = grouping.ToList().Sum(ord => ord.Freight);
        Assert.IsTrue(sum <= 10);
      }

      DumpGrouping(groupByResult);
    }

    [Test]
    public void GroupByWhereTest()
    {
      var queryable = Query<Order>.All.GroupBy(o => o.ShippingAddress.City);
      var result = queryable.Where(g => g.Key.StartsWith("L") && g.Count() > 2);
      var alternativeResult = queryable.Where(g => !g.Key.StartsWith("L") || g.Count() <= 2);

      Assert.AreEqual(queryable.Count(), result.Count() + alternativeResult.Count());

      foreach (IGrouping<string, Order> grouping in result) {
        var startsWithL = grouping.Key.StartsWith("L");
        var countGreater2 = grouping.ToList().Count() > 2;
        Assert.IsTrue(startsWithL && countGreater2);
      }

      foreach (IGrouping<string, Order> grouping in alternativeResult) {
        var startsWithL = grouping.Key.StartsWith("L");
        var countGreater2 = grouping.ToList().Count() > 2;
        Assert.IsTrue(!(startsWithL && countGreater2));
      }

      DumpGrouping(result);
    }

    [Test]
    public void GroupByWhere2Test()
    {
      IQueryable<IEnumerable<Order>> result = Query<Order>.All
        .GroupBy(o => o.ShippingAddress.City)
        .Select(g => g.Where(o => o.Freight > 0));
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupBySelectTest()
    {
      var groupBy = Query<Order>.All.GroupBy(o => o.ShipName);
      var result = groupBy.Select(g => g);
      Assert.AreEqual(groupBy.ToList().Count(), result.ToList().Count());
      DumpGrouping(result);
    }


    [Test]
    public void GroupBySelectWithAnonymousTest()
    {
      var groupBy = Query<Order>.All.GroupBy(o => o.ShipName);
      var result = groupBy.Select(g => new {g});
      Assert.AreEqual(groupBy.ToList().Count(), result.ToList().Count());
      QueryDumper.Dump(result);
    }


    [Test]
    public void GroupBySelectKeyTest()
    {
      var groupBy = Query<Order>.All.GroupBy(o => o.ShipName);
      IQueryable<string> result = groupBy.Select(g => g.Key);
      Assert.AreEqual(groupBy.ToList().Count(), result.ToList().Count());
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupBySelectKeyWithSelectCalculableColumnTest()
    {
      IQueryable<string> result = Query<Order>.All.GroupBy(o => o.ShipName).Select(g => g.Key + "String");
      QueryDumper.Dump(result);
    }


    [Test]
    public void GroupBySelectKeyWithCalculableColumnTest()
    {
      var result = Query<Order>.All.GroupBy(o => o.ShipName + "String");
      DumpGrouping(result);
    }

    [Test]
    public void GroupByWithSelectFirstTest()
    {
      var result = Query<Order>.All
        .GroupBy(o => o.Customer)
        .Select(g => g.First());
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupBySelectGroupingTest()
    {
      var result = Query<Order>.All
        .GroupBy(o => o.Customer)
        .Select(g => g);
      DumpGrouping(result);
    }

    [Test]
    public void GroupBySelectManyTest()
    {
      var result = Query<Customer>.All
        .GroupBy(c => c.Address.City)
        .SelectMany(g => g);
      QueryDumper.Dump(result);
    }

    [Test]
    [ExpectedException(typeof (NotSupportedException))]
    public void GroupBySelectManyKeyTest()
    {
      var result = Query<Customer>.All
        .GroupBy(c => c.Address.City)
        .SelectMany(g => g.Key);
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByEntitySelectManyTest()
    {
      var result = Query<Order>.All
        .GroupBy(o => o.Customer)
        .SelectMany(g => g);
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupBySumTest()
    {
      var result = Query<Order>.All
        .GroupBy(o => o.Customer.Id)
        .Select(g => g.Sum(o => o.Freight));
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByCountTest()
    {
      var result = Query<Order>.All
        .GroupBy(o => o.Customer)
        .Select(g => new {Customer = g.Key, OrdersCount = g.Count()});
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithFilterTest()
    {
      var result = Query<Order>.All
        .GroupBy(o => o.Customer)
        .Select(g => new {Customer = g.Key, Orders = g.Where(order => order.OrderDate < DateTime.Now)});
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupBySumMinMaxAvgTest()
    {
      var result = Query<Order>.All
        .GroupBy(o => o.Customer)
        .Select(g =>
          new {
            Sum = g.Sum(o => o.Freight),
            Min = g.Min(o => o.Freight),
            Max = g.Max(o => o.Freight),
            Avg = g.Average(o => o.Freight)
          });
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithConstantResultSelectorTest()
    {
      var result = Query<Order>.All.GroupBy(o => o.Customer, (c, g) =>
        new {
          ConstString = "ConstantString"
        });
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithConstantSelectorTest()
    {
      var result = Query<Order>.All
        .GroupBy(o => o.Customer)
        .Select(g =>
          new {
            ConstString = "ConstantString"
          });
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithAnonymousSelectTest()
    {
      IQueryable<IGrouping<Customer, Order>> groupings = Query<Order>.All.GroupBy(o => o.Customer);
      var result = groupings.Select(g => new {g});
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithEntityResultSelectorTest()
    {
      var result = Query<Order>.All.GroupBy(o => o.Customer, (c, g) =>c);
      QueryDumper.Dump(result);
    }
    [Test]
    public void GroupByWithEntityResultSelector2Test()
    {
      var result = Query<Order>.All.GroupBy(o => o.Customer, (c, g) =>
        new {
          Customer = c,
        });
      QueryDumper.Dump(result);
    }


    [Test]
    public void GroupByWithEntityResultSelector3Test()
    {
      IQueryable<IEnumerable<Order>> result = Query<Order>.All.GroupBy(o => o.Customer, (c, g) => g);
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByBooleanTest()
    {
      var result = Query<Customer>.All.GroupBy(c => c.CompanyName.StartsWith("A"));
      var expected = Query<Customer>.All.AsEnumerable().GroupBy(c => c.CompanyName.StartsWith("A"));
      Assert.IsTrue(expected.Select(g => g.Key).OrderBy(k => k)
        .SequenceEqual(result.AsEnumerable().Select(g => g.Key).OrderBy(k => k)));
      foreach (var group in expected)
        Assert.IsTrue(expected.Where(g => g.Key==group.Key)
          .SelectMany(g => g).OrderBy(i => i.Id)
          .SequenceEqual(result.AsEnumerable()
            .Where(g => g.Key==group.Key).SelectMany(g => g).OrderBy(i => i.Id)));
    }

    [Test]
    public void GroupByBooleanSubquery1Test()
    {
      var result = Query<Customer>.All.GroupBy(c => c.Orders.Count > 10);
      DumpGrouping(result);
    }

    [Test]
    public void GroupByBooleanSubquery2Test()
    {
      var result = Query<Customer>.All
        .Where(c => c.Orders.Count > 0)
        .GroupBy(c => c.Orders.Average(o => o.Freight) >= 80);
      DumpGrouping(result);
    }

    [Test]
    public void GroupByWithAggregateResultSelectorTest()
    {
      IQueryable<int> result = Query<Order>.All.GroupBy(o => o.Freight, (c, g) => g.Count());
      QueryDumper.Dump(result);
    }


    [Test]
    public void GroupByWithAnonymousResultSelector5Test()
    {
      var result = Query<Order>.All.GroupBy(o => o.Freight, (c, g) => new {Count = g.Count(), Customer = c});
      QueryDumper.Dump(result);
    }


    [Test]
    public void GroupByWithEntityResultSelector5BisTest()
    {
      var result = Query<Order>.All
        .GroupBy(o => o.Freight)
        .Select(g => new {Count = g.Count(), Freight = g.Key});
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithEntityResultSelector5Bis2Test()
    {
      var result = Query<Order>.All
        .GroupBy(o => o.Customer)
        .Select(g => new {Count = g.Count(), Customer = g.Key});
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithEntityResultSelector5Bis20Test()
    {
      var result = Query<Order>.All
        .GroupBy(o => o.Customer)
        .Select(g => g.Key.CompanyName);
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithEntityResultSelector5Bis22Test()
    {
      var result = Query<Order>.All
        .GroupBy(o => o.Customer)
        .Select(g => new {Count = g.Count(), Customer = g.Key.CompanyName});
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithEntityResultSelector5Bis23Test()
    {
      var result = Query<Order>.All
        .GroupBy(o => o.Customer)
        .Select(g => new {Count = g.Count(), Customer = g.Key})
        .Where(g => g.Customer.CompanyName!=null);
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithEntityResultSelector5Bis24Test()
    {
      var result = Query<Order>.All
        .GroupBy(o => o.Customer)
        .Select(g => new {Count = g.Count(), Customer = g.Key})
        .OrderBy(g => g.Customer.CompanyName);
      QueryDumper.Dump(result);
    }


    [Test]
    public void GroupByWithEntityResultSelector5Bis212Test()
    {
      var result = Query<Order>.All
        .GroupBy(o => o.Customer)
        .Select(g => new {Count = new {Count1 = g.Count(), Count2 = g.Count()}});
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithEntityResultSelector5Bis21Test()
    {
      var result = Query<Order>.All
        .GroupBy(o => o.Customer)
        .Select(g => new {Count = new {Count1 = g.Count(), Count2 = g.Count()}, Customer = g.Key});
      QueryDumper.Dump(result);
    }


    [Test]
    public void GroupByWithEntityResultSelector5Bis3Test()
    {
      var result = Query<Order>.All
        .GroupBy(o => new {o.OrderDate, o.Freight})
        .Select(g => new {Count = g.Count(), OrderInfo = g.Key});
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithResultSelectorTest2Test()
    {
      var result = Query<Order>.All.GroupBy(o => o.Customer, (c, g) =>
        new {
          Customer = c,
          Sum = g.Sum(o => o.Freight),
          Min = g.Min(o => o.Freight),
          Max = g.Max(o => o.Freight),
          Avg = g.Average(o => o.Freight)
        });
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithResultSelectorTest3Test()
    {
      var result = Query<Order>.All
        .GroupBy(o => o.Customer)
        .Select(g => new {
          Sum = g.Sum(o => o.Freight),
          Min = g.Min(o => o.Freight),
          Max = g.Max(o => o.Freight),
          Avg = g.Average(o => o.Freight)
        });
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithResultSelectorTest4Test()
    {
      var result = Query<Order>.All.GroupBy(o => o.Customer, (c, g) =>
        new {
          // Customer = c,
          Sum = g.Sum(o => o.Freight),
          Min = g.Min(o => o.Freight),
          Max = g.Max(o => o.Freight),
          Avg = g.Average(o => o.Freight)
        });
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithElementSelectorSumTest()
    {
      var result = Query<Order>.All
        .GroupBy(o => o.Customer, o => o.Freight)
        .Select(g => g.Sum());
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithElementSelectorSumAnonymousTest()
    {
      var result = Query<Order>.All
        .GroupBy(o => o.Customer, o => o.Freight)
        .Select(g => new {A = g.Sum(), B = g.Sum()});
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithElementSelectorTest()
    {
      var result = Query<Order>.All.GroupBy(o => o.Customer, o => o.Freight);
      DumpGrouping(result);
    }

    [Test]
    public void GroupByWithElementSelectorSumMaxTest()
    {
      var result = Query<Order>.All
        .GroupBy(o => o.Customer.Id, o => o.Freight)
        .Select(g => new {Sum = g.Sum(), Max = g.Max()});
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithAnonymousElementTest()
    {
      var result = Query<Order>.All
        .GroupBy(o => o.Customer, o => new {o.Freight})
        .Select(g => g.Sum(x => x.Freight));
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupByWithTwoPartKeyTest()
    {
      var result = Query<Order>.All
        .GroupBy(o => new {o.Customer.Id, o.OrderDate})
        .Select(g => g.Sum(o => o.Freight));
      QueryDumper.Dump(result);
    }

    [Test]
    public void OrderByGroupByTest()
    {
      // NOTE: Order-by is lost when group-by is applied (the sequence of groups is not ordered)
      var result = Query<Order>.All
        .OrderBy(o => o.OrderDate)
        .GroupBy(o => o.Customer.Id)
        .Select(g => g.Sum(o => o.Freight));
      QueryDumper.Dump(result);
    }

    [Test]
    public void OrderByGroupBySelectManyTest()
    {
      // NOTE: Order-by is preserved within grouped sub-collections
      var result = Query<Order>.All
        .OrderBy(o => o.OrderDate)
        .GroupBy(o => o.Customer.Id).SelectMany(g => g);
      QueryDumper.Dump(result);
    }

    [Test]
    public void FilterGroupingTest()
    {
      var result = Query<Order>.All
        .GroupBy(o => o.Customer)
        .Select(g => g.Where(o => o.ShipName.StartsWith("A")));
      QueryDumper.Dump(result);
    }

    [Test]
    public void GroupWithJoinTest()
    {
      var query = Query<Customer>.All
        .GroupBy(c => c.Address.Region)
        .Join(Query<Customer>.All,
          regions => regions.Key,
          c2 => c2.Address.Region,
          (regions, c2) => new {
            region = regions.Key,
            total = c2.Orders.Sum(o => o.Freight)
          });

      QueryDumper.Dump(query);
    }

    private void DumpGrouping<TKey, TValue>(IQueryable<IGrouping<TKey, TValue>> result)
    {
      DumpGrouping(result, false);
    }

    private void DumpGrouping<TKey, TValue>(IQueryable<IGrouping<TKey, TValue>> result, bool logOutput)
    {
      var list = result.ToList();
      Assert.Greater(list.Count, 0);
      Assert.AreEqual(list.Count, result.Count());
      foreach (var grouping in result) {
        Assert.IsNotNull(grouping.Key);
        var count = grouping.ToList().Count();
        Assert.AreEqual(count, grouping.Count());
        Assert.AreEqual(count, grouping.AsQueryable().Count());
      }
      if (logOutput)
        QueryDumper.Dump(result);
    }
  }
}