// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.01.15

using System;
using System.Diagnostics;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Disposing;
using SimpleVersion1 = Xtensive.Storage.Tests.Upgrade.Model.SimpleVersion1;

namespace Xtensive.Storage.Tests.Upgrade
{
  [TestFixture, Category("Upgrade")]
  public class DomainUpgradeSimpleTest
  {
    private Domain domain;

    [SetUp]
    public void SetUp()
    {
      BuildDomain("SimpleVersion1", DomainUpgradeMode.Recreate);
      FillData();
    }

    [Test]
    public void UpgradeTest()
    {
      BuildDomain("SimpleVersion2", DomainUpgradeMode.Perform);
    }

    private void BuildDomain(string version, DomainUpgradeMode upgradeMode)
    {
      if (domain!=null)
        domain.DisposeSafely();

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(Assembly.GetExecutingAssembly(),
        "Xtensive.Storage.Tests.Upgrade.Model." + version);
      configuration.Types.Register(typeof(SimpleUpgrader));
      using (SimpleUpgrader.Enable(version)) {
        domain = Domain.Build(configuration);
      }
    }

    private void FillData()
    {
      using (Session.Open(domain))
      {
        using (var transactionScope = Transaction.Open())
        {
          // BusinessContacts
          var helen = new SimpleVersion1.BusinessContact
          {
            Address = new SimpleVersion1.Address
            {
              City = "Cowes",
              Country = "UK"
            },
            CompanyName = "Island Trading",
            ContactName = "Helen Bennett",
            PassportNumber = "123"
          };
          var philip = new SimpleVersion1.BusinessContact
          {
            Address = new SimpleVersion1.Address
            {
              City = "Brandenburg",
              Country = "Germany"
            },
            CompanyName = "Koniglich Essen",
            ContactName = "Philip Cramer",
            PassportNumber = "321"
          };

          // Employies
          var director = new SimpleVersion1.Employee
          {
            Address = new SimpleVersion1.Address
            {
              City = "Tacoma",
              Country = "USA"
            },
            FirstName = "Andrew",
            LastName = "Fuller",
            HireDate = new DateTime(1992, 8, 13)
          };
          var nancy = new SimpleVersion1.Employee
          {
            Address = new SimpleVersion1.Address
            {
              City = "Seattle",
              Country = "USA"
            },
            FirstName = "Nancy",
            LastName = "Davolio",
            HireDate = new DateTime(1992, 5, 1),
            ReportsTo = director
          };
          var michael = new SimpleVersion1.Employee
          {
            Address = new SimpleVersion1.Address
            {
              City = "London",
              Country = "UK"
            },
            FirstName = "Michael",
            LastName = "Suyama",
            HireDate = new DateTime(1993, 10, 17),
            ReportsTo = director
          };

          // Orders
          new SimpleVersion1.Order
          {
            OrderNumber = "1",
            Customer = helen,
            Employee = michael,
            Freight = 12,
            OrderDate = new DateTime(1996, 7, 4),
            ProductName = "Maxilaku"
          };
          new SimpleVersion1.Order
          {
            OrderNumber = "2",
            Customer = helen,
            Employee = nancy,
            Freight = 12,
            OrderDate = new DateTime(1996, 7, 4),
            ProductName = "Filo Mix"
          };
          new SimpleVersion1.Order
          {
            OrderNumber = "3",
            Customer = philip,
            Employee = michael,
            Freight = 12,
            OrderDate = new DateTime(1996, 7, 4),
            ProductName = "Tourtiere"
          };
          new SimpleVersion1.Order
          {
            OrderNumber = "4",
            Customer = philip,
            Employee = nancy,
            Freight = 12,
            OrderDate = new DateTime(1996, 7, 4),
            ProductName = "Pate chinois"
          };

          // Commiting changes
          transactionScope.Complete();
        }
      }
    }

  }
}