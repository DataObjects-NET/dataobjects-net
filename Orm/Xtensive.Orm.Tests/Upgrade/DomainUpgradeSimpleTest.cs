// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.01.15

using System;
using System.Diagnostics;
using System.Reflection;
using NUnit.Framework;

using Xtensive.Core;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Upgrade.Model.SimpleVersion1;
using SimpleVersion1 = Xtensive.Orm.Tests.Upgrade.Model.SimpleVersion1;

namespace Xtensive.Orm.Tests.Upgrade
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
      BuildDomain("SimpleVersion2", DomainUpgradeMode.PerformSafely);
    }

    private void BuildDomain(string version, DomainUpgradeMode upgradeMode)
    {
      if (domain!=null)
        domain.DisposeSafely();

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(Assembly.GetExecutingAssembly(),
        "Xtensive.Orm.Tests.Upgrade.Model." + version);
      configuration.Types.Register(typeof(SimpleUpgrader));
      using (SimpleUpgrader.Enable(version)) {
        domain = Domain.Build(configuration);
      }
    }

    private void FillData()
    {
      using (var session = domain.OpenSession())
      {
        using (var transactionScope = session.OpenTransaction())
        {
          // BusinessContacts
          var helen = new BusinessContact
          {
            Address = new Address
            {
              City = "Cowes",
              Country = "UK"
            },
            CompanyName = "Island Trading",
            ContactName = "Helen Bennett",
            PassportNumber = "123"
          };
          var philip = new BusinessContact
          {
            Address = new Address
            {
              City = "Brandenburg",
              Country = "Germany"
            },
            CompanyName = "Koniglich Essen",
            ContactName = "Philip Cramer",
            PassportNumber = "321"
          };

          // Employies
          var director = new Employee
          {
            Address = new Address
            {
              City = "Tacoma",
              Country = "USA"
            },
            FirstName = "Andrew",
            LastName = "Fuller",
            HireDate = new DateTime(1992, 8, 13)
          };
          var nancy = new Employee
          {
            Address = new Address
            {
              City = "Seattle",
              Country = "USA"
            },
            FirstName = "Nancy",
            LastName = "Davolio",
            HireDate = new DateTime(1992, 5, 1),
            ReportsTo = director
          };
          var michael = new Employee
          {
            Address = new Address
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
          new Order
          {
            OrderNumber = "1",
            Customer = helen,
            Employee = michael,
            Freight = 12,
            OrderDate = new DateTime(1996, 7, 4),
            ProductName = "Maxilaku"
          };
          new Order
          {
            OrderNumber = "2",
            Customer = helen,
            Employee = nancy,
            Freight = 12,
            OrderDate = new DateTime(1996, 7, 4),
            ProductName = "Filo Mix"
          };
          new Order
          {
            OrderNumber = "3",
            Customer = philip,
            Employee = michael,
            Freight = 12,
            OrderDate = new DateTime(1996, 7, 4),
            ProductName = "Tourtiere"
          };
          new Order
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