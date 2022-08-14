// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.12.18

using System;
using System.Linq.Expressions;
using System.Threading;
using NUnit.Framework;
using Xtensive.Caching;
using Xtensive.Core;
using Xtensive.Linq;
using System.Linq;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Manual.Caching
{
  #region Model

  [Serializable]
  [HierarchyRoot]
  public class Product : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field(Length = 100)]
    public string Name { get; set; }

    [Field]
    public double Price { get; set; }

    [Field]
    [Association(PairTo = "Product")]
    public EntitySet<Order> Orders { get; private set; }

    public override string ToString()
    {
      return $"Product #{Id}, Name={Name}, Orders.Count={Orders.Count}";
    }

    public Product(Session session)
      : base(session)
    {}
  }

  [Serializable]
  [HierarchyRoot]
  public class Order : Entity
  {
    // Tagging type
    private class TotalPriceServices { }

    private TransactionalValue<double> totalPriceCached;

    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public Product Product { get; set; }

    [Field]
    public double Quantity { get; set; }

    public double TotalPrice {
      get {
        return Quantity * Product.Price;
      }
    }
    
    public double TotalPriceCached {
      get {
        if (totalPriceCached==null) {
          var session = Session;
          totalPriceCached = new TransactionalValue<double>(session, () => TotalPrice);
          if (session.Extensions.Get<TotalPriceServices>()==null) {
            session.Extensions.Set(new TotalPriceServices()); 
            // Setting up the tag to ensure the event handles is attached 
            // just once for each Session
            session.Events.EntityFieldValueSetting += ((sender, e) => {
              var order = e.Entity as Order;
              if (order!=null && (e.Field.Name=="Quantity" || e.Field.Name=="Product")) {
                if (order.totalPriceCached!=null)
                  order.totalPriceCached.Invalidate();
              }
              var product = e.Entity as Product;
              if (product!=null && e.Field.Name=="Price")
                foreach (var pOrder in product.Orders)
                  if (pOrder.totalPriceCached!=null)
                    pOrder.totalPriceCached.Invalidate();
            });
          }
        }
        return totalPriceCached.Value;
      }
    }

    public override string ToString()
    {
      return $"Order #{Id}, Product={Product.Name}, Quantity={Quantity}";
    }

    public Order(Session session)
      : base(session)
    {}
  }

  #endregion

  #region LINQ rewriter (compiler container) for TotalPrice

  [CompilerContainer(typeof (Expression))]
  public static class CustomLinqCompilerContainer
  {
    // Uber-cool custom LINQ translator for non-persistent 
    // Order.TotalPrice property
    [Compiler(typeof (Order), "TotalPrice", TargetKind.PropertyGet)]
    public static Expression TotalPrice(Expression orderExpression)
    {
      Expression<Func<Order, double>> e =
        person => person.Quantity * person.Product.Price;
      return e.BindParameters(orderExpression);
    }
  }

  #endregion
  
  [TestFixture]
  public class CalculatedValueCachingTest
  {
    [Test]
    public void MainTest()
    {
      var config = DomainConfigurationFactory.CreateWithoutSessionConfigurations();
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.Register(typeof(Product).Assembly, typeof(Product).Namespace);
      config.Types.Register(typeof(CustomLinqCompilerContainer));
      var domain = Domain.Build(config);

      using (var session = domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var apple  = new Product (session) {Name = "Apple", Price = 2.0};
          var appleOrder = new Order (session) {Product = apple, Quantity = 1};
          // var banana = new Product {Name = "Banana", Price = 1.0};
          // var bananaOrder = new Order {Product = banana, Quantity = 2};
          
          // First time calculation
          CheckTotalPrice(appleOrder);

          using (session.OpenTransaction(TransactionOpenMode.New)) {
            appleOrder.Quantity = 3;
            apple.Price = 3;
            // Automatic update
            CheckTotalPrice(appleOrder);
            // No transactionScope.Complete(), so here we get a rollback
          }

          // Recovery after rollback
          CheckTotalPrice(appleOrder);

          Console.WriteLine("Sleeping for 1 sec.");
          Thread.Sleep(TimeSpan.FromSeconds(1.1));
        }
      }
    }

    private void CheckTotalPrice(Order order)
    {
      var totalPrice = order.TotalPrice;

      // Checking cached value
      Assert.AreEqual(totalPrice, order.TotalPriceCached);
      // Checking the value in LINQ query
      Assert.AreEqual(totalPrice,
        order.Session.Query.All<Order>()
        .Where(o => o==order)
        .Select(o => o.TotalPrice)
        .Single());
    }
  }
}