// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2014.02.09

using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.Issues.IssueJira0519_StaleKeyInEntitySetModel;

namespace Xtensive.Orm.Tests.Issues
{
  namespace IssueJira0519_StaleKeyInEntitySetModel
  {
    [HierarchyRoot]
    public class Order : Entity
    {
      [Key, Field]
      public int Id { get; private set; }

      [Field]
      [Association(PairTo = "Order",
        OnOwnerRemove = OnRemoveAction.Cascade,
        OnTargetRemove = OnRemoveAction.Clear)]
      public EntitySet<OrderItem> Items { get; private set; }
    }

    [HierarchyRoot]
    public class OrderItem : Entity
    {
      [Key, Field]
      public int Id { get; private set; }

      [Field]
      public Order Order { get; set; }
    }
  }

  [TestFixture]
  public class IssueJira0519_StaleKeyInEntitySet : AutoBuildTest
  {
    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (Order).Assembly, typeof (Order).Namespace);
      var defaultConfiguration = configuration.Sessions.Default;
      defaultConfiguration.Options = SessionOptions.ClientProfile | SessionOptions.AutoActivation;
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession()) {
        var item1 = new OrderItem();
        var item2 = new OrderItem();
        var item3 = new OrderItem();
        var order = new Order();
        order.Items.Add(item1);
        order.Items.Add(item2);
        order.Items.Add(item3);
        session.SaveChanges();
        VerifyOrder(order, 3);

        var items = order.Items.ToList();
        item1.Remove();
        session.SaveChanges();
        VerifyOrder(order, 2);
      }
    }
    
    [Test]
    public void ChangesInDifferentSessionsTest()
    {
      int id;
      using (var session = Domain.OpenSession()) {
        var item1 = new OrderItem();
        var item2 = new OrderItem();
        var item3 = new OrderItem();
        var order = new Order();
        order.Items.Add(item1);
        order.Items.Add(item2);
        order.Items.Add(item3);
        session.SaveChanges();
        id = item1.Id;
        VerifyOrder(order, 3);
      }
      using (var session = Domain.OpenSession()) {
        var item1 = session.Query.All<OrderItem>().First(el => el.Id==id);
        var order = item1.Order;
        item1.Remove();
        session.SaveChanges();
        VerifyOrder(order, 2);
      }
    }

    [Test]
    public void AddDoubleRemoveAndAddNewItemsInDifferentSavesTest()
    {
      using (var session = Domain.OpenSession()) {
        var item1 = new OrderItem();
        var item2 = new OrderItem();
        var item3 = new OrderItem();
        var order = new Order();
        order.Items.Add(item1);
        order.Items.Add(item2);
        order.Items.Add(item3);
        session.SaveChanges();
        VerifyOrder(order, 3);

        var items = order.Items.ToList();
        item1.Remove();
        item2.Remove();
        session.SaveChanges();
        VerifyOrder(order, 1);
        items = order.Items.ToList();

        var item4 = new OrderItem();
        var item5 = new OrderItem();
        order.Items.Add(item4);
        order.Items.Add(item5);
        session.SaveChanges();
        VerifyOrder(order, 3);
      }
    }

    [Test]
    public void AddDoubleRemoveAndAddNewItemsInSingleSaveTest()
    {
      using (var session = Domain.OpenSession()) {
        var item1 = new OrderItem();
        var item2 = new OrderItem();
        var item3 = new OrderItem();
        var order = new Order();
        order.Items.Add(item1);
        order.Items.Add(item2);
        order.Items.Add(item3);
        session.SaveChanges();
        VerifyOrder(order, 3);

        var items = order.Items.ToList();
        item1.Remove();
        item2.Remove();
        var item4 = new OrderItem();
        var item5 = new OrderItem();
        order.Items.Add(item4);
        order.Items.Add(item5);
        session.SaveChanges();
        VerifyOrder(order, 3);
      }
    }

    [Test]
    public void AddRemoveAndAddNewItemsTest()
    {
      using (var session = Domain.OpenSession()) {
        var item1 = new OrderItem();
        var item2 = new OrderItem();
        var item3 = new OrderItem();
        var order = new Order();
        order.Items.Add(item1);
        order.Items.Add(item2);
        order.Items.Add(item3);
        session.SaveChanges();
        VerifyOrder(order, 3);

        var items = order.Items.ToList();
        item1.Remove();
        session.SaveChanges();
        VerifyOrder(order, 2);
        items = order.Items.ToList();

        var item4 = new OrderItem();
        var item5 = new OrderItem();
        order.Items.Add(item4);
        order.Items.Add(item5);
        session.SaveChanges();
        VerifyOrder(order, 4);
      }
    }

    [Test]
    public void AddMoveToAnotherOrderAndAddNewItems()
    {
      using (var session = Domain.OpenSession()) {
        var item1 = new OrderItem();
        var item2 = new OrderItem();
        var item3 = new OrderItem();
        var order = new Order();
        order.Items.Add(item1);
        order.Items.Add(item2);
        order.Items.Add(item3);
        session.SaveChanges();
        VerifyOrder(order, 3);

        var items = order.Items.ToList();
        var order1 = new Order();
        order1.Items.Add(item1);
        var item4 = new OrderItem();
        var item5 = new OrderItem();
        order.Items.Add(item4);
        order.Items.Add(item5);
        session.SaveChanges();
        VerifyOrder(order, 4);
        VerifyOrder(order1, 1);
      }
    }

    private void VerifyOrder(Order order, long expectedCount)
    {
      Assert.That(order.Items.Count, Is.EqualTo(expectedCount));
      foreach (var item in order.Items)
        Assert.That(item, Is.Not.Null);
    }
  }
}