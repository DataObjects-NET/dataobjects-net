// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.09.07
using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using System.Linq;
using Xtensive.Storage.Tests.Linq.LocalCollectionsTest_Model;
using Xtensive.Storage.Tests.ObjectModel;
using Xtensive.Storage.Tests.ObjectModel.NorthwindDO;

namespace Xtensive.Storage.Tests.Linq.LocalCollectionsTest_Model
{
  public class Node
  {
    public string Name { get; set; }

    public Node Parent { get; set; }

    public Node(string name)
    {
      Name = name;
    }
  }
}

namespace Xtensive.Storage.Tests.Linq
{
  public class LocalCollectionsTest : NorthwindDOModelTest
  {
    protected override Xtensive.Storage.Configuration.DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof (Node).Assembly, typeof (Node).Namespace);
      return config;
    }

    [Test]
    public void ListContainsTest()
    {
      var list = new List<string>(){"FISSA", "PARIS"};
      var query = from c in Query<Customer>.All   
           where !list.Contains(c.Id)   
           select c.Orders;    
      var expected = from c in Query<Customer>.All.AsEnumerable()   
           where !list.Contains(c.Id)   
           select c.Orders;    
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void ArrayContainsTest()
    {
      var list = new [] { "FISSA", "PARIS" };
      var query = from c in Query<Customer>.All
                  where !list.Contains(c.Id)
                  select c.Orders;
      var expected = from c in Query<Customer>.All.AsEnumerable()
                     where !list.Contains(c.Id)
                     select c.Orders;
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void IListContainsTest()
    {
      var list = (IList<string>)new List<string> { "FISSA", "PARIS" };
      var query = from c in Query<Customer>.All
                  where !list.Contains(c.Id)
                  select c.Orders;
      var expected = from c in Query<Customer>.All.AsEnumerable()
                     where !list.Contains(c.Id)
                     select c.Orders;
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void ListNewContainsTest()
    {
      var q = from c in Query<Customer>.All   
           where !new List<string>(){"FISSA", "PARIS"}.Contains(c.Id)   
           select c.Orders;    
      QueryDumper.Dump(q);
    }

    [Test]
    [ExpectedException(typeof(TargetInvocationException))]
    public void TypeLoop1Test()
    {
      var nodes = new Node[10];
      var query = Query<Order>.All.Join(nodes, order => order.Customer.Address.City, node=>node.Name, (order,node)=> new{order, node});
      QueryDumper.Dump(query);
    }

    [Test]
    public void ContainsTest()
    {
      var localOrderFreights = Query<Order>.All.Select(order => order.Freight).Take(5).ToList();
      var query = Query<Order>.All.Where(order => localOrderFreights.Contains(order.Freight));
      QueryDumper.Dump(query);
      var expectedQuery = Query<Order>.All.AsEnumerable().Where(order => localOrderFreights.Contains(order.Freight));
      Assert.AreEqual(0, expectedQuery.Except(query).Count());
    }

    [Test]
    public void AnyTest()
    {
      var orders = Query<Order>.All.Select(order => order).Take(5).ToList();
      var query = Query<Order>.All.Where(order => orders.Any(localOrder => localOrder.Freight==order.Freight));
      QueryDumper.Dump(query);
      var expectedQuery = Query<Order>.All.AsEnumerable().Where(order => orders.Any(localOrder => localOrder.Freight==order.Freight));
      Assert.AreEqual(0, expectedQuery.Except(query).Count());
    }

    [Test]
    public void AllTest()
    {
      var orders = Query<Order>.All.Select(order => order).Take(5).ToList();
      var query = Query<Order>.All.Where(order => orders.All(localOrder => localOrder.Freight==order.Freight));
      QueryDumper.Dump(query);
      var expectedQuery = Query<Order>.All.AsEnumerable().Where(order => orders.All(localOrder => localOrder.Freight==order.Freight));
      Assert.AreEqual(0, expectedQuery.Except(query).Count());
    }

    [Test]
    public void IndexOfTest()
    {
      var localOrderFreights = Query<Order>.All.Select(order => order.Freight).Take(5).ToList();
      var query = Query<Order>.All.Select(order => localOrderFreights.IndexOf(order.Freight));
      QueryDumper.Dump(query);
      var expectedQuery = Query<Order>.All.AsEnumerable().Select(order => localOrderFreights.IndexOf(order.Freight));
      Assert.AreEqual(0, expectedQuery.Except(query).Count());
    }

    [Test]
    public void JoinEntityTest()
    {
      var localOrders = Query<Order>.All.Take(5).ToList();
      var query = Query<Order>.All.Join(localOrders, order => order, localOrder => localOrder, (order, localOrder) => new {order, localOrder});
      QueryDumper.Dump(query);
      var expectedQuery = Query<Order>.All.AsEnumerable().Join(localOrders, order => order, localOrder => localOrder, (order, localOrder) => new {order, localOrder});
      Assert.AreEqual(0, expectedQuery.Except(query).Count());
    }

    [Test]
    public void JoinEntityFieldTest()
    {
      var localOrders = Query<Order>.All.Take(5).ToList();
      var query = Query<Order>.All.Join(localOrders, order => order.Freight, localOrder => localOrder.Freight, (order, localOrder) => new {order, localOrder});
      QueryDumper.Dump(query);
      var expectedQuery = Query<Order>.All.AsEnumerable().Join(localOrders, order => order.Freight, localOrder => localOrder.Freight, (order, localOrder) => new {order, localOrder});
      Assert.AreEqual(0, expectedQuery.Except(query).Count());
    }

    [Test]
    public void JoinEntityField2Test()
    {
      var localFreights = Query<Order>.All.Take(5).Select(order => order.Freight).ToList();
      var query = Query<Order>.All.Join(localFreights, order => order.Freight, freight => freight, (order, freight) => new {order, freight});
      QueryDumper.Dump(query);
      var expectedQuery = Query<Order>.All.AsEnumerable().Join(localFreights, order => order.Freight, freight => freight, (order, freight) => new {order, freight});
      Assert.AreEqual(0, expectedQuery.Except(query).Count());
    }

    [Test]
    public void JoinEntityField2MaterializeTest()
    {
      var localFreights = Query<Order>.All.Take(5).Select(order => order.Freight).ToList();
      var query = Query<Order>.All.Join(localFreights, order => order.Freight, freight => freight, (order, freight) => new {order, freight}).Select(x => x.freight);
      QueryDumper.Dump(query);
      var expectedQuery = Query<Order>.All.AsEnumerable().Join(localFreights, order => order.Freight, freight => freight, (order, freight) => new {order, freight}).Select(x => x.freight);
      Assert.AreEqual(0, expectedQuery.Except(query).Count());
    }


    [Test]
    public void SimpleConcatTest()
    {
      var customers = Query<Customer>.All;
      var result = customers.Where(c => c.Orders.Count <= 1).Concat(Query<Customer>.All.ToList().Where(c => c.Orders.Count > 1));
      QueryDumper.Dump(result);
      Assert.AreEqual(customers.Count(), result.Count());
    }

    [Test]
    public void SimpleUnionTest()
    {
      var products = Query<Product>.All;
      var customers = Query<Customer>.All;
      var productFirstChars = products.Select(p => p.ProductName.Substring(0, 1));
      var customerFirstChars = customers.Select(c => c.CompanyName.Substring(0, 1)).ToList();
      var uniqueFirstChars = productFirstChars.Union(customerFirstChars);
      QueryDumper.Dump(uniqueFirstChars);
    }

    [Test]
    public void IntersectTest()
    {
      var products = Query<Product>.All;
      var customers = Query<Customer>.All;
      var productFirstChars = products.Select(p => p.ProductName.Substring(0, 1));
      var customerFirstChars = customers.Select(c => c.CompanyName.Substring(0, 1)).ToList();
      var commonFirstChars = productFirstChars.Intersect(customerFirstChars);
      QueryDumper.Dump(commonFirstChars);
    }

    [Test]
    public void SimpleIntersectTest()
    {
      var query = Query<Order>.All
        .Select(o => o.Employee)
        .Intersect(Query<Order>.All.ToList().Select(o => o.Employee));

      QueryDumper.Dump(query);
    }

    [Test]
    public void SimpleExceptTest()
    {
      var products = Query<Product>.All;
      var customers = Query<Customer>.All;
      var productFirstChars = products.Select(p => p.ProductName.Substring(0, 1));
      var customerFirstChars = customers.Select(c => c.CompanyName.Substring(0, 1)).ToList();
      var productOnlyFirstChars = productFirstChars.Except(customerFirstChars);
      QueryDumper.Dump(productOnlyFirstChars);
    }

    [Test]
    public void ConcatDifferentTest()
    {
      var customers = Query<Customer>.All;
      var employees = Query<Employee>.All;
      var result = customers
        .Select(c => c.Phone)
        .Concat(customers.ToList().Select(c => c.Fax))
        .Concat(employees.ToList().Select(e => e.HomePhone));
      QueryDumper.Dump(result);
    }

    [Test]
    public void ConcatDifferentTest2()
    {
      var customers = Query<Customer>.All;
      var employees = Query<Employee>.All;
      var result = customers
        .Select(c => new {Name = c.CompanyName, c.Phone})
        .Concat(employees.ToList().Select(e => new {Name = e.FirstName + " " + e.LastName, Phone = e.HomePhone}));
      QueryDumper.Dump(result);
    }

    [Test]
    public void UnionDifferentTest()
    {
      var employees = Query<Employee>.All;
      var result = employees
        .Select(c => c.Id)
        .Union(employees.ToList().Select(e => e.Id));
      QueryDumper.Dump(result);
    }

    [Test]
    public void UnionCollationsTest()
    {
      var customers = Query<Customer>.All;
      var employees = Query<Employee>.All;
      var result = customers
        .Select(c => c.Address.Country)
        .Union(employees.ToList().Select(e => e.Address.Country));
      QueryDumper.Dump(result);
    }

    [Test]
    public void IntersectDifferentTest()
    {
      var customers = Query<Customer>.All;
      var employees = Query<Employee>.All;
      var result = customers
        .Select(c => c.Address.Country)
        .Intersect(employees.ToList().Select(e => e.Address.Country));
      QueryDumper.Dump(result);
    }

    [Test]
    public void ExceptDifferentTest()
    {
      var customers = Query<Customer>.All;
      var employees = Query<Employee>.All;
      var result = customers
        .Select(c => c.Address.Country)
        .Except(employees.ToList().Select(e => e.Address.Country));
      QueryDumper.Dump(result);
    }

    [Test]
    public void UnionAnonymousTest()
    {
      var customers = Query<Order>.All;
      var result = customers.Select(c => new {c.Freight, c.OrderDate})
        .Union(customers.ToList().Select(c => new {Freight = c.Freight+10, c.OrderDate}));
      var expected = customers.AsEnumerable().Select(c => new {c.Freight, c.OrderDate})
        .Union(customers.ToList().Select(c => new {Freight = c.Freight+10, c.OrderDate}));
      Assert.AreEqual(0, expected.Except(result).Count());
    }

    [Test]
    public void UnionAnonymousCollationsTest()
    {
      var customers = Query<Customer>.All;
      var result = customers.Select(c => new {c.CompanyName, c.ContactName})
        .Take(10)
        .Union(customers.ToList().Select(c => new {c.CompanyName, c.ContactName}));
      QueryDumper.Dump(result);
    }

    [Test]
    public void UnionAnonymous2Test()
    {
      var customers = Query<Customer>.All;
      var result = customers.Select(c => new {c.CompanyName, c.ContactName, c.Address})
        .Where(c => c.Address.StreetAddress.Length < 10)
        .Select(c => new {c.CompanyName, c.Address.City})
        .Take(10)
        .Union(customers.ToList().Select(c => new {c.CompanyName, c.Address.City})).Where(c => c.CompanyName.Length < 10);
      QueryDumper.Dump(result);
    }

    [Test]
    public void UnionAnonymous3Test()
    {
      var customers = Query<Customer>.All;
      var shipper = Query<Shipper>.All;
      var result = customers.Select(c => new {c.CompanyName, c.ContactName, c.Address})
        .Where(c => c.Address.StreetAddress.Length < 15)
        .Select(c => new {Name = c.CompanyName, Address = c.Address.City})
        .Take(10)
        .Union(shipper.ToList().Select(s => new {Name = s.CompanyName, Address = s.Phone}))
        .Where(c => c.Address.Length < 7);
      QueryDumper.Dump(result);
    }

    [Test]
    public void UnionStructureTest()
    {
      var customers = Query<Customer>.All;
      var result = customers.Select(c => c.Address)
        .Where(c => c.StreetAddress.Length > 0)
        .Union(customers.ToList().Select(c => c.Address))
        .Where(c => c.Region=="BC");
      QueryDumper.Dump(result);
    }

    [Test]
    public void IntersectWithoutOneOfSelect()
    {
      EnsureProtocolIs(StorageProtocol.Index | StorageProtocol.SqlServer);
      var actual = from c in Query<Customer>.All
      from r in (c.Orders)
        .Intersect(c.Orders.ToList())
        .Select(o => o.ShippedDate)
      orderby r
      select r;
      var expected = from c in Query<Customer>.All.ToList()
      from r in (c.Orders)
        .Intersect(c.Orders).Select(o => o.ShippedDate)
      orderby r
      select r;
      QueryDumper.Dump(actual);
    }
  }
}