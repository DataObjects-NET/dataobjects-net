// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.10.06

using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Serialization.Binary;
using Xtensive.Storage.Configuration;
using System.Linq;
using Xtensive.Core.Collections;

#region Model

namespace Xtensive.Storage.Tests.Storage.VersionRootModel
{
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

  public class AdvancedOrder : Order
  {
    [Field]
    public string CustomerName { get; set; }
  }

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

  public class AdvancedOrderItem : OrderItem
  {
    [Field]
    public string SupplierName { get; set; }
  }

  [HierarchyRoot]
  public class Owner1 : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Version, Field]
    public long Version { get; private set; }
  }

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
      return Query<Owner1>.All.ToList().Cast<Entity>().Concat(Query<Owner2>.All.ToList().Cast<Entity>());
    }
  }
}

#endregion

namespace Xtensive.Storage.Tests.Storage
{
  using VersionRootModel;

  [TestFixture]
  public class VersionRootTests
    : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Storage.VersionRootModel");
      return config;
    }

    [Test]
    public void UpdateRootVersionTest()
    {
      Key orderKey;
      VersionInfo orderVersion;
      Key orderItemKey;
      VersionInfo orderItemVersion;
      
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          var order = new Order {Number = 1};
          var orderItem = new OrderItem {Product = "Product", Quantity = 10, Order = order};
          orderKey = order.Key;
          orderVersion = order.GetVersion();
          orderItemKey = orderItem.Key;
          orderItemVersion = orderItem.GetVersion();
          transactionScope.Complete();
        }
      }
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          var order = Query<Order>.Single(orderKey);
          var orderItem = Query<OrderItem>.Single(orderItemKey);
          Assert.IsTrue(orderVersion==order.GetVersion());
          Assert.IsTrue(orderItemVersion==orderItem.GetVersion());
          orderItem.Quantity = 20;
          Assert.IsFalse(orderVersion==order.GetVersion());
          Assert.IsFalse(orderItemVersion==orderItem.GetVersion());
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
      
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          var order = new AdvancedOrder() {Number = 1, CustomerName = "Customer1"};
          var orderItem = new AdvancedOrderItem() {
            Product = "Product", Quantity = 10, Order = order, SupplierName = "Supplier1"
          };
          orderKey = order.Key;
          orderVersion = order.GetVersion();
          orderItemKey = orderItem.Key;
          orderItemVersion = orderItem.GetVersion();
          transactionScope.Complete();
        }
      }
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          var order = Query<Order>.Single(orderKey);
          var orderItem = Query<AdvancedOrderItem>.Single(orderItemKey);
          Assert.IsTrue(orderVersion==order.GetVersion());
          Assert.IsTrue(orderItemVersion==orderItem.GetVersion());
          orderItem.SupplierName = "Supplier2";
          Assert.IsFalse(orderVersion==order.GetVersion());
          Assert.IsFalse(orderItemVersion==orderItem.GetVersion());
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
      
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          var owner1 = new Owner1();
          var owner2 = new Owner2();
          var item = new Item {Value = "Value"};
          owner1Key = owner1.Key;
          owner2Key = owner2.Key;
          owner1Version = owner1.GetVersion();
          owner2Version = owner2.GetVersion();
          itemKey = item.Key;
          itemVersion = item.GetVersion();
          transactionScope.Complete();
        }
      }
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          var owner1 = Query<Owner1>.Single(owner1Key);
          var owner2 = Query<Owner2>.Single(owner2Key);
          var item = Query<Item>.Single(itemKey);
          Assert.IsTrue(owner1Version==owner1.GetVersion());
          Assert.IsTrue(owner2Version==owner2.GetVersion());
          Assert.IsTrue(itemVersion==item.GetVersion());
          item.Value = "New Value";
          Assert.IsFalse(owner1Version==owner1.GetVersion());
          Assert.IsFalse(owner2Version==owner2.GetVersion());
          Assert.IsFalse(itemVersion==item.GetVersion());
          transactionScope.Complete();
        }
      }
    }

    [Test]
    public void SerializeVersionInfoTest()
    {
      VersionInfo itemVersion;
      
      using (var session = Session.Open(Domain)) {
        using (var transactionScope = Transaction.Open()) {
          var owner1 = new Owner1();
          var owner2 = new Owner2();
          var item = new Item {Value = "Value"};
          itemVersion = item.GetVersion();
          transactionScope.Complete();
        }
      }
      Assert.IsFalse(itemVersion.IsVoid);
      var clone = (VersionInfo) LegacyBinarySerializer.Instance.Clone(itemVersion);
      Assert.IsFalse(clone.IsVoid);
      Assert.IsTrue(itemVersion==clone);
    }
  }
}