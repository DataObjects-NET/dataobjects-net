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
using Xtensive.Core;

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

  public class Poco<T>
  {
    public T Value { get; set; }

    public bool Equals(Poco<T> other)
    {
      if (ReferenceEquals(null, other))
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return Equals(other.Value, Value);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType()!=typeof (Poco<T>))
        return false;
      return Equals((Poco<T>) obj);
    }

    public override int GetHashCode()
    {
      return Value.GetHashCode();
    }
  }
  
  public class Poco<T1, T2>
  {
    public T1 Value1 { get; set; }
    public T2 Value2 { get; set; }

    public bool Equals(Poco<T1, T2> other)
    {
      if (ReferenceEquals(null, other))
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return Equals(other.Value1, Value1) && Equals(other.Value2, Value2);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType()!=typeof (Poco<T1, T2>))
        return false;
      return Equals((Poco<T1, T2>) obj);
    }

    public override int GetHashCode()
    {
      unchecked {
        return (Value1.GetHashCode() * 397) ^ Value2.GetHashCode();
      }
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
    public void PairTest()
    {
      var pairs = Query<Customer>.All
        .Select(customer => new Pair<string, int>(customer.Id, (int)customer.Orders.Count))
        .ToList();
      var query = Query<Customer>.All.Join(pairs, customer => customer.Id, pair => pair.First, (customer, pair) => new {customer, pair.Second});
      var expected = Query<Customer>.All.AsEnumerable().Join(pairs, customer => customer.Id, pair => pair.First, (customer, pair) => new {customer, pair.Second});
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void Pair2Test()
    {
      var pairs = Query<Customer>.All
        .Select(customer => new Pair<string, int>(customer.Id, (int)customer.Orders.Count))
        .ToList();
      var query = Query<Customer>.All.Join(pairs, customer => customer.Id, pair => pair.First, (customer, pair) => pair.Second);
      var expected = Query<Customer>.All.AsEnumerable().Join(pairs, customer => customer.Id, pair => pair.First, (customer, pair) => pair.Second);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    [ExpectedException(typeof(NotSupportedException))]
    public void Poco1Test()
    {
      var pocos = Query<Customer>.All
        .Select(customer => new Poco<string>(){Value = customer.Id})
        .ToList();
      var query = Query<Customer>.All.Join(pocos, customer => customer.Id, poco => poco.Value, (customer, poco) => poco);
      var expected = Query<Customer>.All.AsEnumerable().Join(pocos, customer => customer.Id, poco => poco.Value, (customer, poco) => poco);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void Poco2Test()
    {
      var pocos = Query<Customer>.All
        .Select(customer => new Poco<string, string>(){Value1 = customer.Id, Value2 = customer.Id})
        .ToList();
      var query = Query<Customer>.All.Join(pocos, customer => customer.Id, poco => poco.Value1, (customer, poco) => poco.Value1);
      var expected = Query<Customer>.All.AsEnumerable().Join(pocos, customer => customer.Id, poco => poco.Value1, (customer, poco) => poco.Value1);
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
           where !new List<string> {"FISSA", "PARIS"}.Contains(c.Id)   
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
      var localOrderFreights = Query<Order>.All.Select(order => order.Freight).Take(5).ToList();
      var query = Query<Order>.All.Where(order => localOrderFreights.Any(freight=>freight==order.Freight));
      QueryDumper.Dump(query);
      var expectedQuery = Query<Order>.All.AsEnumerable().Where(order => localOrderFreights.Any(freight => freight == order.Freight));
      Assert.AreEqual(0, expectedQuery.Except(query).Count());
    }

    [Test]
    public void AllTest()
    {
      var localOrderFreights = Query<Order>.All.Select(order => order.Freight).Take(5).ToList();
      var query = Query<Order>.All.Where(order => localOrderFreights.All(freight=>freight==order.Freight));
      QueryDumper.Dump(query);
      var expectedQuery = Query<Order>.All.AsEnumerable().Where(order => localOrderFreights.All(freight => freight == order.Freight));
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
        .Select(o => o.Employee.BirthDate)
        .Intersect(Query<Order>.All.ToList().Select(o => o.Employee.BirthDate));

      var expected = Query<Order>.All
        .AsEnumerable()
        .Select(o => o.Employee.BirthDate)
        .Intersect(Query<Order>.All.ToList().Select(o => o.Employee.BirthDate));

      Assert.AreEqual(0, expected.Except(query).Count());
    }

    [Test]
    public void SimpleIntersectEntityTest()
    {
      var query = Query<Order>.All
        .Select(o => o.Employee)
        .Intersect(Query<Order>.All.ToList().Select(o => o.Employee));

      var expected = Query<Order>.All
        .AsEnumerable()
        .Select(o => o.Employee)
        .Intersect(Query<Order>.All.ToList().Select(o => o.Employee));

      Assert.AreEqual(0, expected.Except(query).Count());
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
    public void Grouping1Test()
    {
      var localItems = GetLocalItems(1000);
      var queryable = Query.Store(localItems);
      var result = queryable.GroupBy(keySelector => keySelector.Value2.Substring(0, 1), (key, grouping)=>new {key, Value1 = grouping.Select(p=>p.Value1)}).OrderBy(grouping=>grouping.key);
      var expected = localItems.GroupBy(keySelector => keySelector.Value2.Substring(0, 1), (key, grouping)=>new {key, Value1 = grouping.Select(p=>p.Value1)}).OrderBy(grouping=>grouping.key);
      var expectedList = expected.ToList();
      var resultList = result.ToList();
      Assert.AreEqual(resultList.Count, expectedList.Count);
      for (int i = 0; i < resultList.Count; i++) {
        Assert.AreEqual(resultList[i].key, expectedList[i].key); 
        Assert.AreEqual(0, expectedList[i].Value1.Except(resultList[i].Value1).Count()); 
      }
      QueryDumper.Dump(result);
    }

    [Test]
    public void Grouping2Test()
    {
      var localItems = GetLocalItems(1000);
      var queryable = Query.Store(localItems);
      var result = queryable.GroupBy(keySelector => keySelector.Value2[0], (key, grouping)=>new {key, Value1 = grouping.Select(p=>p.Value1)}).OrderBy(grouping=>grouping.key);
      var expected = localItems.GroupBy(keySelector => keySelector.Value2[0], (key, grouping)=>new {key, Value1 = grouping.Select(p=>p.Value1)}).OrderBy(grouping=>grouping.key);
      var expectedList = expected.ToList();
      var resultList = result.ToList();
      Assert.AreEqual(resultList.Count, expectedList.Count);
      for (int i = 0; i < resultList.Count; i++) {
        Assert.AreEqual(resultList[i].key, expectedList[i].key); 
        Assert.AreEqual(0, expectedList[i].Value1.Except(resultList[i].Value1).Count()); 
      }
      QueryDumper.Dump(result);
    }

    [Test]
    public void Subquery1Test()
    {
      var localItems = GetLocalItems(1000);
      var queryable = Query.Store(localItems);
      var result = queryable.Select(poco=> Query<Order>.All.Where(order=>order.Freight > poco.Value1)).AsEnumerable().Cast<IEnumerable<Order>>();
      var expected = localItems.Select(poco=> Query<Order>.All.AsEnumerable().Where(order=>order.Freight > poco.Value1));
      var expectedList = expected.ToList();
      var resultList = result.ToList();
      Assert.AreEqual(resultList.Count, expectedList.Count);
      for (int i = 0; i < resultList.Count; i++) {
        Assert.AreEqual(0, expectedList[i].Except(resultList[i]).Count()); 
      }
      QueryDumper.Dump(result);
    }



    [Test]
    public void Subquery2Test()
    {
      var localItems = GetLocalItems(100);
      var queryable = Query.Store(localItems);
      var result = queryable.Select(poco=> queryable.Where(poco2=>poco2.Value1 > poco.Value1).Select(p=>p.Value2)).AsEnumerable().Cast<IEnumerable<string>>();
      var expected = localItems.Select(poco=> localItems.Where(poco2=>poco2.Value1 > poco.Value1).Select(p=>p.Value2));
      var expectedList = expected.ToList();
      var resultList = result.ToList();
      Assert.AreEqual(resultList.Count, expectedList.Count);
      for (int i = 0; i < resultList.Count; i++) {
        Assert.AreEqual(0, expectedList[i].Except(resultList[i]).Count()); 
      }
      QueryDumper.Dump(result);
    }

    [Test]
    public void Aggregate1Test()
    {
      var localItems = GetLocalItems(1000);
      var queryable = Query.Store(localItems);
      var result = queryable.Average(selector => selector.Value1);
      var expected = localItems.Average(selector => selector.Value1);
      Assert.AreEqual(result, expected);
    }


    [Test]
    public void Aggregate2Test()
    {
      var localItems = GetLocalItems(1000);
      var queryable = Query.Store(localItems);
      var result = Query<Order>.All.Where(order => order.Freight > queryable.Max(poco=>poco.Value1));
      var expected = Query<Order>.All.AsEnumerable().Where(order => order.Freight > localItems.Max(poco=>poco.Value1));
      Assert.AreEqual(0, expected.Except(result).Count());
      QueryDumper.Dump(result);
    }

    [Test]
    [Ignore("Very long")]
    public void VeryLongTest()
    {
      var localItems = GetLocalItems(1000000);
      var result = Query<Order>.All.Join(localItems, order=>order.Freight, localItem=>localItem.Value1, (order, item) => new {order.Employee, item.Value2});
      QueryDumper.Dump(result);
    }

    private IEnumerable<Poco<decimal, string>> GetLocalItems(int count)
    {
      return Enumerable
        .Range(0, count)
        .Select(i => new Poco<decimal, string> {
            Value1 = (decimal)i / 100, 
            Value2 = Guid.NewGuid().ToString()
          }
        )
        .ToList();
    }

  }
}