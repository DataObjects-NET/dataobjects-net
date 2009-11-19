// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.11.11

using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Manual.Upgrade
{
  [TestFixture]
  public class UpgradeTest
  {
    private readonly Assembly myAssembly = typeof (Model_1.Order).Assembly;

    [Test]
    public void Test()
    {
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

      using (Session.Open(domain)) {
        using (var transactionScope = Transaction.Open()) {

          var mMorder = 
            new Model_1.Order { ProductName = "Cheese",  Quantity = 10 };
          new Model_1.Order { ProductName = "Wine", Quantity = 2 };
          new Model_1.Order { ProductName = "Wine", Quantity = 5 };

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

      using (Session.Open(domain)) {
        using (var transactionScope = Transaction.Open()) {

          var cheeseOrder = Query<Model_2.Order>.All.FirstOrDefault(o => o.ProductName == "Cheese");

          Assert.IsNotNull(cheeseOrder);
          Assert.AreEqual(10, cheeseOrder.Quantity);
          Assert.IsNull(cheeseOrder.Customer);
          cheeseOrder.Customer = new Model_2.Customer {Name = "Michael"};
          
          var customer = new Model_2.Customer {Name = "Tony"};
          foreach (var order in Query<Model_2.Order>.All)
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

      using (Session.Open(domain)) {
        using (var transactionScope = Transaction.Open()) {

          var cheeseOrder = Query<Model_3.Order>.All.FirstOrDefault(o => o.ProductName == "Cheese");

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

      using (Session.Open(domain)) {
        using (var transactionScope = Transaction.Open()) {

          int productCount = Query<Model_4.Product>.All.Count();
          Assert.AreEqual(2, productCount);

          var order = Query<Model_4.Order>.All.First();

          AssertEx.Throws<Exception>(() => {
            var name = order.ProductName;
          });

          transactionScope.Complete();
        }
      }
    }
  }
}