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
using M1 = Xtensive.Storage.Tests.Upgrade.Model.Version1;
using M2 = Xtensive.Storage.Tests.Upgrade.Model.Version2;

namespace Xtensive.Storage.Tests.Upgrade
{
  [TestFixture]
  public class DomainUpgradeTest
  {
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
      BuildDomain("1", DomainUpgradeMode.Recreate, null, typeof (M1.Address), typeof (M1.Person));

      BuildDomain("1", DomainUpgradeMode.PerformSafely, null, typeof (M1.Address), typeof (M1.Person), typeof (M1.BusinessContact));
      AssertEx.Throws<SchemaSynchronizationException>(() =>
        BuildDomain("1", DomainUpgradeMode.PerformSafely, null, typeof (M1.Address), typeof (M1.Person)));

      BuildDomain("1", DomainUpgradeMode.Validate, null, typeof (M1.Address), typeof (M1.Person), typeof (M1.BusinessContact));
      AssertEx.Throws<SchemaSynchronizationException>(() =>
        BuildDomain("1", DomainUpgradeMode.Validate, null, typeof (M1.Address), typeof (M1.Person)));
      AssertEx.Throws<SchemaSynchronizationException>(() =>
        BuildDomain("1", DomainUpgradeMode.Validate, null, typeof (M1.Address), typeof (M1.Person), 
        typeof (M1.BusinessContact), typeof (M1.Employee), typeof (M1.Order)));
    }

    [Test]
    public void UpgradeGeneratorsTest()
    {
      BuildDomain("1", DomainUpgradeMode.Recreate, 3, typeof (M1.Address), typeof (M1.Person));
      using (domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          new M1.Person {Address = new M1.Address {City = "City", Country = "Country"}};
          new M1.Person {Address = new M1.Address {City = "City", Country = "Country"}};
          new M1.Person {Address = new M1.Address {City = "City", Country = "Country"}};
          t.Complete();
        }
      }

      BuildDomain("1", DomainUpgradeMode.Perform, 2, typeof (M1.Address), typeof (M1.Person));
      using (domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          Assert.AreEqual(3, Query<M1.Person>.All.Count());
          new M1.Person {Address = new M1.Address {City = "City", Country = "Country"}};
          new M1.Person {Address = new M1.Address {City = "City", Country = "Country"}};
          new M1.Person {Address = new M1.Address {City = "City", Country = "Country"}};
          Assert.AreEqual(6, Query<M1.Person>.All.Count());
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

      BuildDomain("1", DomainUpgradeMode.Recreate, null, typeof (M1.Address), typeof (M1.Person));
      using (domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          personTypeId = Query<Metadata.Type>.All
            .First(type => type.Name=="Xtensive.Storage.Tests.Upgrade.Model.Version1.Person").Id;
          maxTypeId = Query<Metadata.Type>.All.Max(type => type.Id);
        }
      }

      BuildDomain("1", DomainUpgradeMode.Recreate, null, typeof (M1.Address), typeof (M1.Person), typeof (M1.BusinessContact));
      using (domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var businessContactTypeId = Query<Metadata.Type>.All
            .First(type => type.Name=="Xtensive.Storage.Tests.Upgrade.Model.Version1.BusinessContact").Id;
          var newPersonTypeId = Query<Metadata.Type>.All
            .First(type => type.Name=="Xtensive.Storage.Tests.Upgrade.Model.Version1.Person").Id;

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
            .First(type => type.Name=="Xtensive.Storage.Tests.Upgrade.Model.Version1.Person").Id;
          businessContactTypeId = Query<Metadata.Type>.All
            .First(type => type.Name=="Xtensive.Storage.Tests.Upgrade.Model.Version1.BusinessContact").Id;
        }
      }

      BuildDomain("2", DomainUpgradeMode.Perform);
      using (domain.OpenSession()) {
        using (var t = Transaction.Open()) {
          var newBusinessContactTypeId = Query<Metadata.Type>.All
            .First(type => type.Name=="Xtensive.Storage.Tests.Upgrade.Model.Version2.BusinessContact").Id;
          var newPersonTypeId = Query<Metadata.Type>.All
            .First(type => type.Name=="Xtensive.Storage.Tests.Upgrade.Model.Version2.Person").Id;

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

          Query<M2.Product>.All.Single(product => product.Title=="DataObjects.NET");
          Query<M2.Product>.All.Single(product => product.Title=="HelpServer");
          Assert.AreEqual(2, Query<M2.Product>.All.Count());
          var webApps = Query<M2.ProductGroup>.All.Single(group => group.Name=="Web applications");
          var frameworks = Query<M2.ProductGroup>.All.Single(group => group.Name=="Frameworks");
          Assert.AreEqual(1, webApps.Products.Count);
          Assert.AreEqual(1, frameworks.Products.Count);

          var allBoys = Query<M2.Boy>.All.ToArray();
          var allGirls = Query<M2.Girl>.All.ToArray();
          Assert.AreEqual(2, allBoys.Length);
          Assert.AreEqual(2, allGirls.Length);
          var alex = allBoys.Single(boy => boy.Name=="Alex");
          foreach (var girl in allGirls)
            Assert.IsTrue(alex.MeetWith.Contains(girl));
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
        "Xtensive.Storage.Tests.Upgrade.Model.Version" + version);
      using (Upgrader.Enable(version)) {
        domain = Domain.Build(configuration);
      }
    }

    private void BuildDomain(string version, DomainUpgradeMode upgradeMode, int? keyCacheSize, params Type[] types)
    {
      if (domain != null)
        domain.DisposeSafely();

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      foreach (var type in types)
        configuration.Types.Register(type);
      if (keyCacheSize.HasValue)
        configuration.KeyCacheSize = keyCacheSize.Value;
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
          var helen = new M1.BusinessContact {
            Address = new M1.Address {
              City = "Cowes",
              Country = "UK"
            },
            CompanyName = "Island Trading",
            ContactName = "Helen Bennett"
          };
          var philip = new M1.BusinessContact {
            Address = new M1.Address {
              City = "Brandenburg",
              Country = "Germany"
            },
            CompanyName = "Koniglich Essen",
            ContactName = "Philip Cramer"
          };

          // Employies
          var director = new M1.Employee {
            Address = new M1.Address {
              City = "Tacoma",
              Country = "USA"
            },
            FirstName = "Andrew",
            LastName = "Fuller",
            HireDate = DateTime.Parse("14.08.1992 0:00:00")
          };
          var nancy = new M1.Employee {
            Address = new M1.Address {
              City = "Seattle",
              Country = "USA"
            },
            FirstName = "Nancy",
            LastName = "Davolio",
            HireDate = DateTime.Parse("01.05.1992 0:00:00"),
            ReportsTo = director
          };
          var michael = new M1.Employee {
            Address = new M1.Address {
              City = "London",
              Country = "UK"
            },
            FirstName = "Michael",
            LastName = "Suyama",
            HireDate = DateTime.Parse("17.10.1993 0:00:00"),
            ReportsTo = director
          };

          // Orders
          new M1.Order {
            Customer = helen,
            Employee = michael,
            Freight = 12,
            OrderDate = DateTime.Parse("04.07.1996 0:00:00"),
            ProductName = "Maxilaku"
          };
          new M1.Order {
            Customer = helen,
            Employee = nancy,
            Freight = 12,
            OrderDate = DateTime.Parse("04.07.1996 0:00:00"),
            ProductName = "Filo Mix"
          };
          new M1.Order {
            Customer = philip,
            Employee = michael,
            Freight = 12,
            OrderDate = DateTime.Parse("04.07.1996 0:00:00"),
            ProductName = "Tourtiere"
          };
          new M1.Order {
            Customer = philip,
            Employee = nancy,
            Freight = 12,
            OrderDate = DateTime.Parse("04.07.1996 0:00:00"),
            ProductName = "Pate chinois"
          };

          // Products & catgories
          new M1.Category
            {
              Name = "Web applications",
              Products = {new M1.Product {Name = "HelpServer", IsActive = true}}
            };

          new M1.Category
            {
              Name = "Frameworks",
              Products = {new M1.Product {Name = "DataObjects.NET", IsActive = true}}
            };

          // Boys & girls
          var alex = new M1.Boy("Alex");
          var dmitry = new M1.Boy("Dmitry");
          var elena = new M1.Girl("Elena");
          var tanya = new M1.Girl("Tanya");
          alex.FriendlyGirls.Add(elena);
          alex.FriendlyGirls.Add(tanya);
          elena.FriendlyBoys.Add(dmitry);

          // Commiting changes
          transactionScope.Complete();
        }
      }
    }

    #endregion
  }
}