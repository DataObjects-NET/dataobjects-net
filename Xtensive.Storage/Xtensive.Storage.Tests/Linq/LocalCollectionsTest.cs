// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.09.07
using System;
using System.Collections.Generic;
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

    public Poco(T1 Value1, T2 Value2)
    {
      this.Value1 = Value1;
      this.Value2 = Value2;
    }

    public Poco()
    {
      
    }
  }

  public class Poco<T1, T2, T3>
  {
    public T1 Value1 { get; set; }
    public T2 Value2 { get; set; }
    public T3 Value3 { get; set; }

    public Poco(T1 Value1, T2 Value2, T3 Value3)
    {
      this.Value1 = Value1;
      this.Value2 = Value2;
      this.Value3 = Value3;
    }

    public bool Equals(Poco<T1, T2, T3> other)
    {
      if (ReferenceEquals(null, other))
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return Equals(other.Value1, Value1) && Equals(other.Value2, Value2) && Equals(other.Value3, Value3);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType()!=typeof (Poco<T1, T2, T3>))
        return false;
      return Equals((Poco<T1, T2, T3>) obj);
    }

    public override int GetHashCode()
    {
      unchecked {
        int result = Value1.GetHashCode();
        result = (result * 397) ^ Value2.GetHashCode();
        result = (result * 397) ^ Value3.GetHashCode();
        return result;
      }
    }

    public Poco()
    {
      
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
    public void JoinWithLazyLoadFieldTest()
    {
      // Category.Picture is LazyLoad field.
      var categories = Query.All<Category>().Take(10).ToList();
      var result =
        (from c1 in Query.All<Product>().Select(p=>p.Category)
        join c2 in categories on c1 equals c2
        select new {c1, c2})
          .Take(10);
      var list = result.ToList();
    }

    [Test]
    public void ListContainsTest()
    {
      var list = new List<string>(){"FISSA", "PARIS"};
      var query = from c in Query.All<Customer>()   
           where !list.Contains(c.Id)   
           select c.Orders;    
      var expected = from c in Query.All<Customer>().AsEnumerable()   
           where !list.Contains(c.Id)   
           select c.Orders;    
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }


    [Test]
    public void ReuseContainsTest()
    {
      var list = new List<int> {7, 22};
      var orders = GetOrders(list);
      foreach (var order in orders)
        Assert.IsTrue(list.Contains(order.Id));

      list = new List<int> {46};
      orders = GetOrders(list);
      foreach (var order in orders)
        Assert.IsTrue(list.Contains(order.Id));
    }

    private IEnumerable<Order> GetOrders(IEnumerable<int> ids)
    {
      return Query.Execute(() =>
        from order in Query.All<Order>()
        where ids.Contains(order.Id)
        select order);
    }

    [Test]
    public void PairTest()
    {
      var pairs = Query.All<Customer>()
        .Select(customer => new Pair<string, int>(customer.Id, (int)customer.Orders.Count))
        .ToList();
      var query = Query.All<Customer>().Join(pairs, customer => customer.Id, pair => pair.First, (customer, pair) => new {customer, pair.Second});
      var expected = Query.All<Customer>().AsEnumerable().Join(pairs, customer => customer.Id, pair => pair.First, (customer, pair) => new {customer, pair.Second});
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void Pair2Test()
    {
      var pairs = Query.All<Customer>()
        .Select(customer => new Pair<string, int>(customer.Id, (int)customer.Orders.Count))
        .ToList();
      var query = Query.All<Customer>().Join(pairs, customer => customer.Id, pair => pair.First, (customer, pair) => pair.Second);
      var expected = Query.All<Customer>().AsEnumerable().Join(pairs, customer => customer.Id, pair => pair.First, (customer, pair) => pair.Second);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    [ExpectedException(typeof(QueryTranslationException))]
    public void Poco1Test()
    {
      var pocos = Query.All<Customer>()
        .Select(customer => new Poco<string>(){Value = customer.Id})
        .ToList();
      var query = Query.All<Customer>().Join(pocos, customer => customer.Id, poco => poco.Value, (customer, poco) => poco);
      var expected = Query.All<Customer>().AsEnumerable().Join(pocos, customer => customer.Id, poco => poco.Value, (customer, poco) => poco);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void Poco2Test()
    {
      var pocos = Query.All<Customer>()
        .Select(customer => new Poco<string, string>(){Value1 = customer.Id, Value2 = customer.Id})
        .ToList();
      var query = Query.All<Customer>().Join(pocos, customer => customer.Id, poco => poco.Value1, (customer, poco) => poco.Value1);
      var expected = Query.All<Customer>().AsEnumerable().Join(pocos, customer => customer.Id, poco => poco.Value1, (customer, poco) => poco.Value1);
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void Poco3Test()
    {
      var query = Query.All<Customer>()
        .Select(customer => new Poco<string, string>{Value1 = customer.Id, Value2 = customer.Id})
        .Select(poco=>new {poco.Value1, poco.Value2});
      var expected =  Query.All<Customer>()
        .AsEnumerable()
        .Select(customer => new Poco<string, string>{Value1 = customer.Id, Value2 = customer.Id})
        .Select(poco=>new {poco.Value1, poco.Value2});
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void Poco4Test()
    {
      var query = Query.All<Customer>()
        .Select(customer => new Poco<string, string>(customer.Id, customer.Id))
        .Select(poco=>new {poco.Value1, poco.Value2});
      var expected =  Query.All<Customer>()
        .AsEnumerable()
        .Select(customer => new Poco<string, string>{Value1 = customer.Id, Value2 = customer.Id})
        .Select(poco=>new {poco.Value1, poco.Value2});
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void ArrayContainsTest()
    {
      var list = new [] { "FISSA", "PARIS" };
      var query = from c in Query.All<Customer>()
                  where !list.Contains(c.Id)
                  select c.Orders;
      var expected = from c in Query.All<Customer>().AsEnumerable()
                     where !list.Contains(c.Id)
                     select c.Orders;
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void IListContainsTest()
    {
      var list = (IList<string>)new List<string> { "FISSA", "PARIS" };
      var query = from c in Query.All<Customer>()
                  where !list.Contains(c.Id)
                  select c.Orders;
      var expected = from c in Query.All<Customer>().AsEnumerable()
                     where !list.Contains(c.Id)
                     select c.Orders;
      Assert.AreEqual(0, expected.Except(query).Count());
      QueryDumper.Dump(query);
    }

    [Test]
    public void ListNewContainsTest()
    {
      var q = from c in Query.All<Customer>()   
           where !new List<string> {"FISSA", "PARIS"}.Contains(c.Id)   
           select c.Orders;    
      QueryDumper.Dump(q);
    }

    [Test]
    [ExpectedException(typeof(QueryTranslationException))]
    public void TypeLoop1Test()
    {
      var nodes = new Node[10];
      var query = Query.All<Order>().Join(nodes, order => order.Customer.Address.City, node=>node.Name, (order,node)=> new{order, node});
      QueryDumper.Dump(query);
    }

    [Test]
    public void ContainsTest()
    {
      var localOrderFreights = Query.All<Order>().Select(order => order.Freight).Take(5).ToList();
      var query = Query.All<Order>().Where(order => localOrderFreights.Contains(order.Freight));
      QueryDumper.Dump(query);
      var expectedQuery = Query.All<Order>().AsEnumerable().Where(order => localOrderFreights.Contains(order.Freight));
      Assert.AreEqual(0, expectedQuery.Except(query).Count());
    }

    [Test]
    public void AnyTest()
    {
      var localOrderFreights = Query.All<Order>().Select(order => order.Freight).Take(5).ToList();
      var query = Query.All<Order>().Where(order => localOrderFreights.Any(freight=>freight==order.Freight));
      QueryDumper.Dump(query);
      var expectedQuery = Query.All<Order>().AsEnumerable().Where(order => localOrderFreights.Any(freight => freight == order.Freight));
      Assert.AreEqual(0, expectedQuery.Except(query).Count());
    }

    [Test]
    public void AllTest()
    {
      var localOrderFreights = Query.All<Order>().Select(order => order.Freight).Take(5).ToList();
      var query = Query.All<Order>().Where(order => localOrderFreights.All(freight=>freight==order.Freight));
      QueryDumper.Dump(query);
      var expectedQuery = Query.All<Order>().AsEnumerable().Where(order => localOrderFreights.All(freight => freight == order.Freight));
      Assert.AreEqual(0, expectedQuery.Except(query).Count());
    }

    [Test]
    [ExpectedException(typeof(QueryTranslationException))]
    public void KeyTest()
    {
      var keys = Query.All<Order>().Take(10).Select(order => order.Key).ToList();
      var query = Query.All<Order>().Join(keys, order => order.Key, key => key, (order, key) => new {order, key});
      QueryDumper.Dump(query);
      var expectedQuery = Query.All<Order>().AsEnumerable().Join(keys, order => order.Key, key => key, (order, key) => new {order, key});
      Assert.AreEqual(0, expectedQuery.Except(query).Count());
    }

    [Test]
    public void JoinEntityTest()
    {
      var localOrders = Query.All<Order>().Take(5).ToList();
      var query = Query.All<Order>().Join(localOrders, order => order, localOrder => localOrder, (order, localOrder) => new {order, localOrder});
      QueryDumper.Dump(query);
      var expectedQuery = Query.All<Order>().AsEnumerable().Join(localOrders, order => order, localOrder => localOrder, (order, localOrder) => new {order, localOrder});
      Assert.AreEqual(0, expectedQuery.Except(query).Count());
    }

    [Test]
    public void JoinEntityFieldTest()
    {
      var localOrders = Query.All<Order>().Take(5).ToList();
      var query = Query.All<Order>().Join(localOrders, order => order.Freight, localOrder => localOrder.Freight, (order, localOrder) => new {order, localOrder});
      QueryDumper.Dump(query);
      var expectedQuery = Query.All<Order>().AsEnumerable().Join(localOrders, order => order.Freight, localOrder => localOrder.Freight, (order, localOrder) => new {order, localOrder});
      Assert.AreEqual(0, expectedQuery.Except(query).Count());
    }

    [Test]
    public void JoinEntityField2Test()
    {
      var localFreights = Query.All<Order>().Take(5).Select(order => order.Freight).ToList();
      var query = Query.All<Order>().Join(localFreights, order => order.Freight, freight => freight, (order, freight) => new {order, freight});
      QueryDumper.Dump(query);
      var expectedQuery = Query.All<Order>().AsEnumerable().Join(localFreights, order => order.Freight, freight => freight, (order, freight) => new {order, freight});
      Assert.AreEqual(0, expectedQuery.Except(query).Count());
    }

    [Test]
    public void JoinEntityField2MaterializeTest()
    {
      var localFreights = Query.All<Order>().Take(5).Select(order => order.Freight).ToList();
      var query = Query.All<Order>().Join(localFreights, order => order.Freight, freight => freight, (order, freight) => new {order, freight}).Select(x => x.freight);
      QueryDumper.Dump(query);
      var expectedQuery = Query.All<Order>().AsEnumerable().Join(localFreights, order => order.Freight, freight => freight, (order, freight) => new {order, freight}).Select(x => x.freight);
      Assert.AreEqual(0, expectedQuery.Except(query).Count());
    }


    [Test]
    public void SimpleConcatTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var customers = Query.All<Customer>();
      var result = customers.Where(c => c.Orders.Count <= 1).Concat(Query.All<Customer>().ToList().Where(c => c.Orders.Count > 1));
      QueryDumper.Dump(result);
      Assert.AreEqual(customers.Count(), result.Count());
    }

    [Test]
    public void SimpleUnionTest()
    {
      var products = Query.All<Product>();
      var customers = Query.All<Customer>();
      var productFirstChars = products.Select(p => p.ProductName.Substring(0, 1));
      var customerFirstChars = customers.Select(c => c.CompanyName.Substring(0, 1)).ToList();
      var uniqueFirstChars = productFirstChars.Union(customerFirstChars);
      QueryDumper.Dump(uniqueFirstChars);
    }

    [Test]
    public void IntersectTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var products = Query.All<Product>();
      var customers = Query.All<Customer>();
      var productFirstChars = products.Select(p => p.ProductName.Substring(0, 1));
      var customerFirstChars = customers.Select(c => c.CompanyName.Substring(0, 1)).ToList();
      var commonFirstChars = productFirstChars.Intersect(customerFirstChars);
      QueryDumper.Dump(commonFirstChars);
    }

    [Test]
    public void SimpleIntersectTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var query = Query.All<Order>()
        .Select(o => o.Employee.BirthDate)
        .Intersect(Query.All<Order>().ToList().Select(o => o.Employee.BirthDate));

      var expected = Query.All<Order>()
        .AsEnumerable()
        .Select(o => o.Employee.BirthDate)
        .Intersect(Query.All<Order>().ToList().Select(o => o.Employee.BirthDate));

      Assert.AreEqual(0, expected.Except(query).Count());
    }

    [Test]
    public void SimpleIntersectEntityTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var query = Query.All<Order>()
        .Select(o => o.Employee)
        .Intersect(Query.All<Order>().ToList().Select(o => o.Employee));

      var expected = Query.All<Order>()
        .AsEnumerable()
        .Select(o => o.Employee)
        .Intersect(Query.All<Order>().ToList().Select(o => o.Employee));

      Assert.AreEqual(0, expected.Except(query).Count());
    }

    [Test]
    public void SimpleExceptTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var products = Query.All<Product>();
      var customers = Query.All<Customer>();
      var productFirstChars = products.Select(p => p.ProductName.Substring(0, 1));
      var customerFirstChars = customers.Select(c => c.CompanyName.Substring(0, 1)).ToList();
      var productOnlyFirstChars = productFirstChars.Except(customerFirstChars);
      QueryDumper.Dump(productOnlyFirstChars);
    }

    [Test]
    public void ConcatDifferentTest()
    {
      var customers = Query.All<Customer>();
      var employees = Query.All<Employee>();
      var result = customers
        .Select(c => c.Phone)
        .Concat(customers.ToList().Select(c => c.Fax))
        .Concat(employees.ToList().Select(e => e.HomePhone));
      QueryDumper.Dump(result);
    }

    [Test]
    public void ConcatDifferentTest2()
    {
      var customers = Query.All<Customer>();
      var employees = Query.All<Employee>();
      var result = customers
        .Select(c => new {Name = c.CompanyName, c.Phone})
        .Concat(employees.ToList().Select(e => new {Name = e.FirstName + " " + e.LastName, Phone = e.HomePhone}));
      QueryDumper.Dump(result);
    }

    [Test]
    public void UnionDifferentTest()
    {
      var employees = Query.All<Employee>();
      var result = employees
        .Select(c => c.Id)
        .Union(employees.ToList().Select(e => e.Id));
      QueryDumper.Dump(result);
    }

    [Test]
    public void UnionCollationsTest()
    {
      var customers = Query.All<Customer>();
      var employees = Query.All<Employee>();
      var result = customers
        .Select(c => c.Address.Country)
        .Union(employees.ToList().Select(e => e.Address.Country));
      QueryDumper.Dump(result);
    }

    [Test]
    public void IntersectDifferentTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var customers = Query.All<Customer>();
      var employees = Query.All<Employee>();
      var result = customers
        .Select(c => c.Address.Country)
        .Intersect(employees.ToList().Select(e => e.Address.Country));
      QueryDumper.Dump(result);
    }

    [Test]
    public void ExceptDifferentTest()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var customers = Query.All<Customer>();
      var employees = Query.All<Employee>();
      var result = customers
        .Select(c => c.Address.Country)
        .Except(employees.ToList().Select(e => e.Address.Country));
      QueryDumper.Dump(result);
    }

    [Test]
    public void UnionAnonymousTest()
    {
      var customers = Query.All<Order>();
      var result = customers.Select(c => new {c.Freight, c.OrderDate})
        .Union(customers.ToList().Select(c => new {Freight = c.Freight+10, c.OrderDate}));
      var expected = customers.AsEnumerable().Select(c => new {c.Freight, c.OrderDate})
        .Union(customers.ToList().Select(c => new {Freight = c.Freight+10, c.OrderDate}));
      Assert.AreEqual(0, expected.Except(result).Count());
    }

    [Test]
    public void UnionAnonymousCollationsTest()
    {
      var customers = Query.All<Customer>();
      var result = customers.Select(c => new {c.CompanyName, c.ContactName})
        .Take(10)
        .Union(customers.ToList().Select(c => new {c.CompanyName, c.ContactName}));
      QueryDumper.Dump(result);
    }

    [Test]
    public void UnionAnonymous2Test()
    {
      var customers = Query.All<Customer>();
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
      var customers = Query.All<Customer>();
      var shipper = Query.All<Shipper>();
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
      var localItems = GetLocalItems(10);
      var queryable = Query.Store(localItems);
      var result = queryable.GroupBy(keySelector => keySelector.Value3.Substring(0, 1), (key, grouping)=>new {key, Value1 = grouping.Select(p=>p.Value1)}).OrderBy(grouping=>grouping.key);
      var expected = localItems.GroupBy(keySelector => keySelector.Value3.Substring(0, 1), (key, grouping)=>new {key, Value1 = grouping.Select(p=>p.Value1)}).OrderBy(grouping=>grouping.key);
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
      var localItems = GetLocalItems(10);
      var queryable = Query.Store(localItems);
      var result = queryable.GroupBy(keySelector => keySelector.Value3[0], (key, grouping)=>new {key, Value1 = grouping.Select(p=>p.Value1)}).OrderBy(grouping=>grouping.key);
      var expected = localItems.GroupBy(keySelector => keySelector.Value3[0], (key, grouping)=>new {key, Value1 = grouping.Select(p=>p.Value1)}).OrderBy(grouping=>grouping.key);
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
      var localItems = GetLocalItems(10);
      var queryable = Query.Store(localItems);
      var result = queryable.Select(poco=> Query.All<Order>().Where(order=>order.Freight > poco.Value1)).AsEnumerable().Cast<IEnumerable<Order>>();
      var expected = localItems.Select(poco=> Query.All<Order>().AsEnumerable().Where(order=>order.Freight > poco.Value1));
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
      var localItems = GetLocalItems(10);
      var queryable = Query.Store(localItems);
      var result = queryable.Select(poco=> queryable.Where(poco2=>poco2.Value2 > poco.Value2).Select(p=>p.Value3)).AsEnumerable().Cast<IEnumerable<string>>();
      var expected = localItems.Select(poco=> localItems.Where(poco2=>poco2.Value2 > poco.Value2).Select(p=>p.Value3));
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
      var localItems = GetLocalItems(100);
      var queryable = Query.Store(localItems);
      var result = queryable.Average(selector => selector.Value1);
      var expected = localItems.Average(selector => selector.Value1);
      Assert.AreEqual(result, expected);
    }


    [Test]
    public void Aggregate2Test()
    {
      Require.ProviderIsNot(StorageProvider.SqlServerCe);
      var localItems = GetLocalItems(100);
      var queryable = Query.Store(localItems);
      var result = Query.All<Order>().Where(order => order.Freight > queryable.Max(poco=>poco.Value1));
      var expected = Query.All<Order>().AsEnumerable().Where(order => order.Freight > localItems.Max(poco=>poco.Value1));
      Assert.AreEqual(0, expected.Except(result).Count());
      QueryDumper.Dump(result);
    }

    [Test]
    [ExpectedException(typeof(QueryTranslationException))]
    public void ClosureCacheTest()
    {
      var localItems = GetLocalItems(100);
      var queryable = Query.Store(localItems);
      var result = Query.Execute(()=>Query.All<Order>().Where(order => order.Freight > queryable.Max(poco=>poco.Value1)));
      QueryDumper.Dump(result);
    }

    [Test]
    [Ignore("Very long")]
    public void VeryLongTest()
    {
      var localItems = GetLocalItems(1000000);
      var result = Query.All<Order>().Join(localItems, order=>order.Freight, localItem=>localItem.Value1, (order, item) => new {order.Employee, item.Value2});
      QueryDumper.Dump(result);
    }

    private IEnumerable<Poco<int, decimal, string>> GetLocalItems(int count)
    {
      return Enumerable
        .Range(0, count)
        .Select(i => new Poco<int, decimal, string> {
            Value1 = i, 
            Value2 = (decimal)i / 100, 
            Value3 = Guid.NewGuid().ToString()
          }
        )
        .ToList();
    }

  }
}