// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.10.06

using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using System.Linq;
using Xtensive.Collections;

#region Model

namespace Xtensive.Orm.Tests.Storage.VersionRootModel
{
  [Serializable]
  [HierarchyRoot]
  public class Order : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Version, Field]
    public byte Version { get; set; }

    [Field]
    public int Number { get; set; }
  }

  [Serializable]
  public class AdvancedOrder : Order
  {
    [Field]
    public string CustomerName { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class OrderItem : Entity,
    IHasVersionRoots
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public Order Order { get; set; }

    [Field]
    public string Product { get; set; }

    [Field]
    public int Quantity { get; set; }

    public IEnumerable<Entity> GetVersionRoots()
    {
      if (Order==null)
        return Enumerable.Empty<Entity>();
      else 
        return EnumerableUtils.One((Entity) Order);
    }
  }

  [Serializable]
  public class AdvancedOrderItem : OrderItem
  {
    [Field]
    public string SupplierName { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Owner1 : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Version, Field]
    public long Version { get; private set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Owner2 : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Version, Field]
    internal byte Version1 { get; set; }

    [Version, Field]
    internal ushort Version2 { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Item : Entity,
    IHasVersionRoots
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string Value { get; set; }

    public IEnumerable<Entity> GetVersionRoots()
    {
      return Session.Query.All<Owner1>().ToList().Cast<Entity>().Concat(Session.Query.All<Owner2>().ToList().Cast<Entity>());
    }
  }
}

#endregion

namespace Xtensive.Orm.Tests.Storage
{
  using VersionRootModel;

  [TestFixture]
  public class VersionRootTests
    : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Orm.Tests.Storage.VersionRootModel");
      return config;
    }

    [Test]
    public void UpdateRootVersionTest()
    {
      Key orderKey;
      VersionInfo orderVersion;
      Key orderItemKey;
      VersionInfo orderItemVersion;
      
      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var order = new Order {Number = 1};
          var orderItem = new OrderItem {Product = "Product", Quantity = 10, Order = order};
          orderKey = order.Key;
          orderVersion = order.VersionInfo;
          orderItemKey = orderItem.Key;
          orderItemVersion = orderItem.VersionInfo;
          transactionScope.Complete();
        }
      }
      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var order = session.Query.Single<Order>(orderKey);
          var orderItem = session.Query.Single<OrderItem>(orderItemKey);
          Assert.IsTrue(orderVersion==order.VersionInfo);
          Assert.IsTrue(orderItemVersion==orderItem.VersionInfo);
          orderItem.Quantity = 20;
          Assert.IsFalse(orderVersion==order.VersionInfo);
          Assert.IsFalse(orderItemVersion==orderItem.VersionInfo);
          transactionScope.Complete();
        }
      }
    }

    [Test]
    public void UpdateRootVersionWithDescendantTest()
    {
      Key orderKey;
      VersionInfo orderVersion;
      Key orderItemKey;
      VersionInfo orderItemVersion;
      
      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var order = new AdvancedOrder() {Number = 1, CustomerName = "Customer1"};
          var orderItem = new AdvancedOrderItem() {
            Product = "Product", Quantity = 10, Order = order, SupplierName = "Supplier1"
          };
          orderKey = order.Key;
          orderVersion = order.VersionInfo;
          orderItemKey = orderItem.Key;
          orderItemVersion = orderItem.VersionInfo;
          transactionScope.Complete();
        }
      }
      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var order = session.Query.Single<Order>(orderKey);
          var orderItem = session.Query.Single<AdvancedOrderItem>(orderItemKey);
          Assert.IsTrue(orderVersion==order.VersionInfo);
          Assert.IsTrue(orderItemVersion==orderItem.VersionInfo);
          orderItem.SupplierName = "Supplier2";
          Assert.IsFalse(orderVersion==order.VersionInfo);
          Assert.IsFalse(orderItemVersion==orderItem.VersionInfo);
          transactionScope.Complete();
        }
      }
    }

    [Test]
    public void UpdateVersionWithManyRootsTest()
    {
      Key owner1Key;
      VersionInfo owner1Version;
      Key owner2Key;
      VersionInfo owner2Version;
      Key itemKey;
      VersionInfo itemVersion;
      
      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var owner1 = new Owner1();
          var owner2 = new Owner2();
          var item = new Item {Value = "Value"};
          owner1Key = owner1.Key;
          owner2Key = owner2.Key;
          owner1Version = owner1.VersionInfo;
          owner2Version = owner2.VersionInfo;
          itemKey = item.Key;
          itemVersion = item.VersionInfo;
          transactionScope.Complete();
        }
      }
      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var owner1 = session.Query.Single<Owner1>(owner1Key);
          var owner2 = session.Query.Single<Owner2>(owner2Key);
          var item = session.Query.Single<Item>(itemKey);
          Assert.IsTrue(owner1Version==owner1.VersionInfo);
          Assert.IsTrue(owner2Version==owner2.VersionInfo);
          Assert.IsTrue(itemVersion==item.VersionInfo);
          item.Value = "New Value";
          Assert.IsFalse(owner1Version==owner1.VersionInfo);
          Assert.IsFalse(owner2Version==owner2.VersionInfo);
          Assert.IsFalse(itemVersion==item.VersionInfo);
          transactionScope.Complete();
        }
      }
    }

    [Test]
    public void SerializeVersionInfoTest()
    {
      VersionInfo itemVersion;
      
      using (var session = Domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          var owner1 = new Owner1();
          var owner2 = new Owner2();
          var item = new Item {Value = "Value"};
          itemVersion = item.VersionInfo;
          transactionScope.Complete();
        }
      }
      Assert.IsFalse(itemVersion.IsVoid);
      var clone = Cloner.Clone(itemVersion);
      Assert.IsFalse(clone.IsVoid);
      Assert.IsTrue(itemVersion==clone);
    }
  }
}