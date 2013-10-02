// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.11.11

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Orm.Tests;
using Xtensive.Orm.Configuration;

namespace Xtensive.Orm.Manual.Upgrade
{
  [TestFixture]
  public class UpgradeTest
  {
    private readonly Assembly myAssembly = typeof (Model_1.Order).Assembly;

    [Test]
    public void Test()
    {
      UpgradeHandlerEnabler.EnabledUpgradeHandler = null;
      try {
        BuildFirstDomain();

        UpgradeHandlerEnabler.EnabledUpgradeHandler = typeof (Model_2.Upgrader);
        BuildSecondDomain();

        UpgradeHandlerEnabler.EnabledUpgradeHandler = typeof (Model_3.Upgrader);
        BuildThirdDomain();

        UpgradeHandlerEnabler.EnabledUpgradeHandler = typeof (Model_4.Upgrader);
        BuildFourthDomain();
      }
      finally {
        UpgradeHandlerEnabler.EnabledUpgradeHandler = null;
      }
    }

    private void BuildFirstDomain()
    {
      var domainConfig = new DomainConfiguration("sqlserver://localhost/DO40-Tests");
      domainConfig.Types.Register(myAssembly, typeof (Model_1.Order).Namespace);
      domainConfig.UpgradeMode = DomainUpgradeMode.Recreate;
      var domain = Domain.Build(domainConfig);

      using (var session = domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {

          var mMorder = 
            new Model_1.Order(session) { ProductName = "Cheese",  Quantity = 10 };
          new Model_1.Order(session) { ProductName = "Wine", Quantity = 2 };
          new Model_1.Order(session) { ProductName = "Wine", Quantity = 5 };

          transactionScope.Complete();
        }
      }
    }

    private void BuildSecondDomain()
    {
      var domainConfig = new DomainConfiguration("sqlserver://localhost/DO40-Tests");
      domainConfig.Types.Register(myAssembly, typeof (Model_2.Order).Namespace);
      domainConfig.UpgradeMode = DomainUpgradeMode.PerformSafely;
      var domain = Domain.Build(domainConfig);

      using (var session = domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {

          var cheeseOrder = session.Query.All<Model_2.Order>().FirstOrDefault(o => o.ProductName == "Cheese");

          Assert.IsNotNull(cheeseOrder);
          Assert.AreEqual(10, cheeseOrder.Quantity);
          Assert.IsNull(cheeseOrder.Customer);
          cheeseOrder.Customer = new Model_2.Customer(session) {Name = "Michael"};
          
          var customer = new Model_2.Customer(session) {Name = "Tony"};
          foreach (var order in session.Query.All<Model_2.Order>())
            if (order.Customer==null)
              order.Customer = customer;

          transactionScope.Complete();
        }
      }
    }

    public void BuildThirdDomain()
    {
      var domainConfig = new DomainConfiguration("sqlserver://localhost/DO40-Tests");
      domainConfig.Types.Register(myAssembly, typeof (Model_3.Order).Namespace);
      domainConfig.UpgradeMode = DomainUpgradeMode.PerformSafely;
      var domain = Domain.Build(domainConfig);

      using (var session = domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {

          var cheeseOrder = session.Query.All<Model_3.Order>().FirstOrDefault(o => o.ProductName == "Cheese");

          Assert.IsNotNull(cheeseOrder);
          Assert.AreEqual(10, cheeseOrder.Quantity);
          Assert.AreEqual("Michael", cheeseOrder.Customer.FullName);

          transactionScope.Complete();
        }
      }
    }

    public void BuildFourthDomain()
    {
      var domainConfig = new DomainConfiguration("sqlserver://localhost/DO40-Tests");
      domainConfig.Types.Register(myAssembly, typeof (Model_4.Order).Namespace);
      domainConfig.UpgradeMode = DomainUpgradeMode.PerformSafely;
      var domain = Domain.Build(domainConfig);

      using (var session = domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {

          int productCount = session.Query.All<Model_4.Product>().Count();
          Assert.AreEqual(2, productCount);

          var order = session.Query.All<Model_4.Order>().First();

          AssertEx.Throws<Exception>(() => {
#pragma warning disable 612,618
            var name = order.ProductName;
#pragma warning restore 612,618
          });

          transactionScope.Complete();
        }
      }
    }
  }
}