// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System;
using System.Linq;
using NUnit.Framework;
using System.Reflection;
using Xtensive.Disposing;
using Xtensive.Core;
using Xtensive.Orm.Tests.Upgrade.Recycled.Model.Version2;
using M1 = Xtensive.Orm.Tests.Upgrade.Recycled.Model.Version1;
using M2 = Xtensive.Orm.Tests.Upgrade.Recycled.Model.Version2;

namespace Xtensive.Orm.Tests.Upgrade.Recycled
{
  [TestFixture, Category("Upgrade")]
  public class RecycledUpgradeTest
  {
    private Domain domain;

    [SetUp]
    public void SetUp()
    {
      Require.ProviderIsNot(StorageProvider.Sqlite);
      BuildDomain("1", DomainUpgradeMode.Recreate);
      FillData();
    }
    
    [Test]
    public void UpgradeToVersion2Test()
    {
      BuildDomain("2", DomainUpgradeMode.Perform);
      using (var session = domain.OpenSession()) {
        using (session.OpenTransaction()) {
          Assert.AreEqual(4, session.Query.All<Person>().Count());
          Assert.AreEqual(2, session.Query.All<Employee>().Count());
          Assert.AreEqual(2, session.Query.All<Customer>().Count());

          Assert.AreEqual("Island Trading", session.Query.All<Employee>()
            .First(employee => employee.Name=="Nancy Davolio").CompanyName);
          Assert.AreEqual("Cowes, UK", session.Query.All<Customer>()
            .First(customer => customer.Name=="Helen Bennett").Address);
          
          Assert.AreEqual(4, session.Query.All<Order>().Count());
          Assert.AreEqual("Maxilaku", session.Query.All<Order>().First(order =>
            order.Employee.Name=="Michael Suyama" && order.Customer.Name=="Helen Bennett")
            .ProductName);
        }
      }
    }

    private void BuildDomain(string version, DomainUpgradeMode upgradeMode)
    {
      if (domain != null)
        domain.DisposeSafely();

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(Assembly.GetExecutingAssembly(),
        "Xtensive.Orm.Tests.Upgrade.Recycled.Model.Version" + version);
      configuration.Types.Register(typeof(Upgrader));
      using (Upgrader.Enable(version)) {
        domain = Domain.Build(configuration);
      }
    }

    #region Data filler

    private void FillData()
    {
      using (var session = domain.OpenSession()) {
        using (var transactionScope = session.OpenTransaction()) {
          // BusinessContacts
          var helen = new Model.Version1.Customer() {
            Address = "Cowes, UK",
            Name = "Helen Bennett"
          };
          var philip = new Model.Version1.Customer {
            Address = "Brandenburg, Germany",
            Name = "Philip Cramer"
          };

          // Employies
          var nancy = new Model.Version1.Employee {
            CompanyName = "Island Trading",
            Name = "Nancy Davolio",
          };
          var michael = new Model.Version1.Employee {
            CompanyName = "Koniglich Essen",
            Name = "Michael Suyama",
          };

          // Orders
          new Model.Version1.Order {
            Customer = helen,
            Employee = michael,
            ProductName = "Maxilaku"
          };
          new Model.Version1.Order {
            Customer = helen,
            Employee = nancy,
            ProductName = "Filo Mix"
          };
          new Model.Version1.Order {
            Customer = philip,
            Employee = michael,
            ProductName = "Tourtiere"
          };
          new Model.Version1.Order {
            Customer = philip,
            Employee = nancy,
            ProductName = "Pate chinois"
          };
          
          // Commiting changes
          transactionScope.Complete();
        }
      }
    }

    #endregion
  }
}