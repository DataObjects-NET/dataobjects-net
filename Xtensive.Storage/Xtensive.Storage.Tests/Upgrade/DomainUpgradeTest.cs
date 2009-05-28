// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System;
using System.Linq;
using NUnit.Framework;
using System.Reflection;
using Xtensive.Core.Disposing;
using Xtensive.Core.Testing;
using Xtensive.Storage.Building;
using Xtensive.Storage.Tests.Upgrade.Model.Version1;
using M2 = Xtensive.Storage.Tests.Upgrade.Model.Version2;

namespace Xtensive.Storage.Tests.Upgrade
{
  [TestFixture]
  public class DomainUpgradeTest
  {
    private const string protocol = "mssql2005";

    private Domain domain;

    [SetUp]
    public void SetUp()
    {
      BuildDomain("1", DomainUpgradeMode.Recreate);
      FillData();
    }
    
    [Test]
    public void UpgradeModeTest()
    {
      BuildDomain("1", DomainUpgradeMode.Recreate, null, typeof (Address), typeof (Person));

      BuildDomain("1", DomainUpgradeMode.PerformSafely, null, typeof (Address), typeof (Person), typeof (BusinessContact));
      AssertEx.Throws<SchemaSynchronizationException>(() =>
        BuildDomain("1", DomainUpgradeMode.PerformSafely, null, typeof (Address), typeof (Person)));

      BuildDomain("1", DomainUpgradeMode.Validate, null, typeof (Address), typeof (Person), typeof (BusinessContact));
      AssertEx.Throws<SchemaSynchronizationException>(() =>
        BuildDomain("1", DomainUpgradeMode.Validate, null, typeof (Address), typeof (Person)));
      AssertEx.Throws<SchemaSynchronizationException>(() =>
        BuildDomain("1", DomainUpgradeMode.Validate, null, typeof (Address), typeof (Person), 
        typeof (BusinessContact), typeof (Employee), typeof (Order)));
    }

    [Test]
    public void UpgradeGeneratorsTest()
    {
      BuildDomain("1", DomainUpgradeMode.Recreate, 3, typeof (Address), typeof (Person));
      using (domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          new Person {Address = new Address {City = "City", Country = "Country"}};
          new Person {Address = new Address {City = "City", Country = "Country"}};
          new Person {Address = new Address {City = "City", Country = "Country"}};
          t.Complete();
        }
      }

      BuildDomain("1", DomainUpgradeMode.Perform, 2, typeof (Address), typeof (Person));
      using (domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          Assert.AreEqual(3, Query<Person>.All.Count());
          new Person {Address = new Address {City = "City", Country = "Country"}};
          new Person {Address = new Address {City = "City", Country = "Country"}};
          new Person {Address = new Address {City = "City", Country = "Country"}};
          Assert.AreEqual(6, Query<Person>.All.Count());
          t.Complete();
        }
      }
    }

    [Test]
    public void ColumnTypesTest()
    {
      var type = typeof (Storage.DbTypeSupportModel.X);
      BuildDomain("1", DomainUpgradeMode.Recreate, null, type);
      BuildDomain("1", DomainUpgradeMode.Validate, null, type);
    }

    [Test]
    public void UpdateTypeIdTest()
    {
      int personTypeId;
      int maxTypeId;

      BuildDomain("1", DomainUpgradeMode.Recreate, null, typeof (Address), typeof (Person));
      using (domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          personTypeId = Query<Metadata.Type>.All
            .First(type => type.Name=="Xtensive.Storage.Tests.Upgrade.ModelPerson").Id;
          maxTypeId = Query<Metadata.Type>.All.Max(type => type.Id);
        }
      }

      BuildDomain("1", DomainUpgradeMode.Recreate, null, typeof (Address), typeof (Person), typeof (BusinessContact));
      using (domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var businessContactTypeId = Query<Metadata.Type>.All
            .First(type => type.Name=="Xtensive.Storage.Tests.Upgrade.ModelBusinessContact").Id;
          var newPersonTypeId = Query<Metadata.Type>.All
            .First(type => type.Name=="Xtensive.Storage.Tests.Upgrade.ModelPerson").Id;

          Assert.AreEqual(personTypeId, newPersonTypeId);
          Assert.AreEqual(maxTypeId + 1, businessContactTypeId);
        }
      }
    }

    [Test]
    public void UpdateTypeIdWithMutualRenameTest()
    {
      int personTypeId;
      int businessContactTypeId;
      
      using (domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          personTypeId = Query<Metadata.Type>.All
            .First(type => type.Name=="Xtensive.Storage.Tests.Upgrade.ModelPerson").Id;
          businessContactTypeId = Query<Metadata.Type>.All
            .First(type => type.Name=="Xtensive.Storage.Tests.Upgrade.ModelBusinessContact").Id;
        }
      }

      BuildDomain("2", DomainUpgradeMode.Perform);
      using (domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var newBusinessContactTypeId = Query<Metadata.Type>.All
            .First(type => type.Name=="Xtensive.Storage.Tests.Upgrade.ModelBusinessContact").Id;
          var newPersonTypeId = Query<Metadata.Type>.All
            .First(type => type.Name=="Xtensive.Storage.Tests.Upgrade.ModelPerson").Id;

          Assert.AreEqual(personTypeId, newBusinessContactTypeId);
          Assert.AreEqual(businessContactTypeId, newPersonTypeId);
        }
      }
    }

    [Test]
    public void UpgradeToVersion2Test()
    {
      BuildDomain("2", DomainUpgradeMode.Perform);
      using (domain.OpenSession()) {
        using (Transaction.Open()) {
          Assert.AreEqual(2, Query<M2.Person>.All.Count());
          Assert.AreEqual("Island Trading", Query<M2.Person>.All
            .First(person => person.ContactName=="Helen Bennett").CompanyName);
          Assert.AreEqual(5, Query<M2.BusinessContact>.All.Count());
          Assert.AreEqual("Suyama", Query<M2.BusinessContact>.All
            .First(contact => contact.FirstName=="Michael").LastName);
          Assert.AreEqual("Fuller", Query<M2.Employee>.All
            .First(employee => employee.FirstName=="Nancy").ReportsTo.LastName);
        }
      }
    }

    private void BuildDomain(string version, DomainUpgradeMode upgradeMode)
    {
      if (domain != null)
        domain.DisposeSafely();

      var configuration = DomainConfigurationFactory.Create(protocol);
      configuration.UpgradeMode = upgradeMode;
      configuration.ForeignKeyMode = ForeignKeyMode.All;
      configuration.Types.Register(Assembly.GetExecutingAssembly(),
        "Xtensive.Storage.Tests.Upgrade.Model.Version" + version);
      using (Upgrader.Enable(version)) {
        domain = Domain.Build(configuration);
      }
    }

    private void BuildDomain(string version, DomainUpgradeMode upgradeMode, int? keyCacheSize, params Type[] types)
    {
      if (domain != null)
        domain.DisposeSafely();

      var configuration = DomainConfigurationFactory.Create(protocol);
      configuration.UpgradeMode = upgradeMode;
      foreach (var type in types)
        configuration.Types.Register(type);
      if (keyCacheSize.HasValue)
        configuration.KeyCacheSize = keyCacheSize.Value;
      using (Upgrader.Enable(version)) {
        domain = Domain.Build(configuration);
      }
    }
    
    private void FillData()
    {
      using (domain.OpenSession()) {
        using (var transactionScope = Transaction.Open()) {
          // BusinessContacts
          var helen = new BusinessContact {
            Address = new Address {
              City = "Cowes",
              Country = "UK"
            },
            CompanyName = "Island Trading",
            ContactName = "Helen Bennett"
          };
          var philip = new BusinessContact {
            Address = new Address {
              City = "Brandenburg",
              Country = "Germany"
            },
            CompanyName = "Koniglich Essen",
            ContactName = "Philip Cramer"
          };
          // Employies
          var director = new Employee {
            Address = new Address {
              City = "Tacoma",
              Country = "USA"
            },
            FirstName = "Andrew",
            LastName = "Fuller",
            HireDate = DateTime.Parse("14.08.1992 0:00:00")
          };
          var nancy = new Employee {
            Address = new Address {
              City = "Seattle",
              Country = "USA"
            },
            FirstName = "Nancy",
            LastName = "Davolio",
            HireDate = DateTime.Parse("01.05.1992 0:00:00"),
            ReportsTo = director
          };
          var michael = new Employee {
            Address = new Address {
              City = "London",
              Country = "UK"
            },
            FirstName = "Michael",
            LastName = "Suyama",
            HireDate = DateTime.Parse("17.10.1993 0:00:00"),
            ReportsTo = director
          };
          // Orders
          new Order {
            Customer = helen,
            Employee = michael,
            Freight = 12,
            OrderDate = DateTime.Parse("04.07.1996 0:00:00"),
            ProductName = "Maxilaku"
          };
          new Order {
            Customer = helen,
            Employee = nancy,
            Freight = 12,
            OrderDate = DateTime.Parse("04.07.1996 0:00:00"),
            ProductName = "Filo Mix"
          };
          new Order {
            Customer = philip,
            Employee = michael,
            Freight = 12,
            OrderDate = DateTime.Parse("04.07.1996 0:00:00"),
            ProductName = "Tourtiere"
          };
          new Order {
            Customer = philip,
            Employee = nancy,
            Freight = 12,
            OrderDate = DateTime.Parse("04.07.1996 0:00:00"),
            ProductName = "Pate chinois"
          };
          transactionScope.Complete();
        }
      }
    }
  }
}