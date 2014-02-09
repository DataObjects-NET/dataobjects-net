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
    
    private void VerifyOrder(Order order, long expectedCount)
    {
      Assert.That(order.Items.Count, Is.EqualTo(expectedCount));
      foreach (var item in order.Items)
        Assert.That(item, Is.Not.Null);
    }
  }
}