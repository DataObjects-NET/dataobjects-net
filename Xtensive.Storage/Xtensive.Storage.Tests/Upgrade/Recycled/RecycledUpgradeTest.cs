// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System.Linq;
using NUnit.Framework;
using System.Reflection;
using Xtensive.Core.Disposing;
using M1 = Xtensive.Storage.Tests.Upgrade.Recycled.Model.Version1;
using M2 = Xtensive.Storage.Tests.Upgrade.Recycled.Model.Version2;

namespace Xtensive.Storage.Tests.Upgrade.Recycled
{
  [TestFixture, Category("Upgrade")]
  public class RecycledUpgradeTest
  {
    private Domain domain;

    [SetUp]
    public void SetUp()
    {
      BuildDomain("1", DomainUpgradeMode.Recreate);
      FillData();
    }
    
    [Test]
    public void UpgradeToVersion2Test()
    {
      BuildDomain("2", DomainUpgradeMode.Perform);
      using (domain.OpenSession()) {
        using (Transaction.Open()) {
          Assert.AreEqual(4, Query<M2.Person>.All.Count());
          Assert.AreEqual(2, Query<M2.Employee>.All.Count());
          Assert.AreEqual(2, Query<M2.Customer>.All.Count());

          Assert.AreEqual("Island Trading", Query<M2.Employee>.All
            .First(employee => employee.Name=="Nancy Davolio").CompanyName);
          Assert.AreEqual("Cowes, UK", Query<M2.Customer>.All
            .First(customer => customer.Name=="Helen Bennett").Address);
          
          Assert.AreEqual(4, Query<M2.Order>.All.Count());
          Assert.AreEqual("Maxilaku", Query<M2.Order>.All.First(order =>
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
        "Xtensive.Storage.Tests.Upgrade.Recycled.Model.Version" + version);
      using (Upgrader.Enable(version)) {
        domain = Domain.Build(configuration);
      }
    }

    #region Data filler

    private void FillData()
    {
      using (domain.OpenSession()) {
        using (var transactionScope = Transaction.Open()) {
          // BusinessContacts
          var helen = new M1.Customer() {
            Address = "Cowes, UK",
            Name = "Helen Bennett"
          };
          var philip = new M1.Customer {
            Address = "Brandenburg, Germany",
            Name = "Philip Cramer"
          };

          // Employies
          var nancy = new M1.Employee {
            CompanyName = "Island Trading",
            Name = "Nancy Davolio",
          };
          var michael = new M1.Employee {
            CompanyName = "Koniglich Essen",
            Name = "Michael Suyama",
          };

          // Orders
          new M1.Order {
            Customer = helen,
            Employee = michael,
            ProductName = "Maxilaku"
          };
          new M1.Order {
            Customer = helen,
            Employee = nancy,
            ProductName = "Filo Mix"
          };
          new M1.Order {
            Customer = philip,
            Employee = michael,
            ProductName = "Tourtiere"
          };
          new M1.Order {
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