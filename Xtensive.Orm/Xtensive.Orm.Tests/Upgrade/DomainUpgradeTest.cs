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
using Xtensive.Orm.Tests.Upgrade.Model.Version1;
using Xtensive.Orm.Tests.Upgrade.Model.Version2;
using Xtensive.Testing;
using Address = Xtensive.Orm.Tests.Upgrade.Model.Version1.Address;
using Boy = Xtensive.Orm.Tests.Upgrade.Model.Version2.Boy;
using BusinessContact = Xtensive.Orm.Tests.Upgrade.Model.Version1.BusinessContact;
using Employee = Xtensive.Orm.Tests.Upgrade.Model.Version1.Employee;
using Entity1 = Xtensive.Orm.Tests.Upgrade.Model.Version2.Entity1;
using Entity2 = Xtensive.Orm.Tests.Upgrade.Model.Version2.Entity2;
using Entity3 = Xtensive.Orm.Tests.Upgrade.Model.Version2.Entity3;
using Entity4 = Xtensive.Orm.Tests.Upgrade.Model.Version2.Entity4;
using Girl = Xtensive.Orm.Tests.Upgrade.Model.Version2.Girl;
using M1 = Xtensive.Orm.Tests.Upgrade.Model.Version1;
using M2 = Xtensive.Orm.Tests.Upgrade.Model.Version2;
using MyStructureOwner = Xtensive.Orm.Tests.Upgrade.Model.Version2.MyStructureOwner;
using Order = Xtensive.Orm.Tests.Upgrade.Model.Version1.Order;
using Person = Xtensive.Orm.Tests.Upgrade.Model.Version1.Person;
using Product = Xtensive.Orm.Tests.Upgrade.Model.Version2.Product;
using ReferencedEntity = Xtensive.Orm.Tests.Upgrade.Model.Version2.ReferencedEntity;
using Structure1 = Xtensive.Orm.Tests.Upgrade.Model.Version1.Structure1;
using Structure2 = Xtensive.Orm.Tests.Upgrade.Model.Version1.Structure2;
using Structure3 = Xtensive.Orm.Tests.Upgrade.Model.Version1.Structure3;
using Structure4 = Xtensive.Orm.Tests.Upgrade.Model.Version1.Structure4;
using StructureContainer1 = Xtensive.Orm.Tests.Upgrade.Model.Version2.StructureContainer1;
using StructureContainer2 = Xtensive.Orm.Tests.Upgrade.Model.Version2.StructureContainer2;
using StructureContainer3 = Xtensive.Orm.Tests.Upgrade.Model.Version2.StructureContainer3;
using StructureContainer4 = Xtensive.Orm.Tests.Upgrade.Model.Version2.StructureContainer4;

namespace Xtensive.Orm.Tests.Upgrade
{
  [TestFixture, Category("Upgrade")]
  public class DomainUpgradeTest
  {
    private Domain domain;

    [TestFixtureSetUp]
    public void TestSetUp()
    {
      Require.ProviderIsNot(StorageProvider.Memory);
    }

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
      var generatorCacheSize = 3;
      BuildDomain("1", DomainUpgradeMode.Recreate, generatorCacheSize, typeof (Address), typeof (Person));
      using (var session = domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          for (int i = 0; i < generatorCacheSize; i++)
            new Person {
              Address = new Address {City = "City", Country = "Country"}
            };
          Assert.AreEqual(3, session.Query.All<Person>().Max(p => p.Id));
          t.Complete();
        }
      }
      BuildDomain("1", DomainUpgradeMode.Perform, generatorCacheSize, typeof (Address), typeof (Person));
      using (var session = domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          for (int i = 0; i < generatorCacheSize; i++)
            new Person {
              Address = new Address {City = "City", Country = "Country"}
            };
          Assert.AreEqual(6, session.Query.All<Person>().Max(p => p.Id));
          t.Complete();
        }
      }
      
      generatorCacheSize = 2;
      BuildDomain("1", DomainUpgradeMode.Perform, generatorCacheSize, typeof (Address), typeof (Person));
      using (var session = domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          new Person {Address = new Address {City = "City", Country = "Country"}};
          new Person {Address = new Address {City = "City", Country = "Country"}};
          new Person {Address = new Address {City = "City", Country = "Country"}};
          Assert.AreEqual(12, session.Query.All<Person>().Max(p => p.Id));
          t.Complete();
        }
      }
    }

    [Test]
    public void UpdateTypeIdTest()
    {
      int personTypeId;
      int maxTypeId;

      BuildDomain("1", DomainUpgradeMode.Recreate, null, typeof (Address), typeof (Person));
      using (var session = domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          personTypeId = session.Query.All<Metadata.Type>()
            .First(type => type.Name=="Xtensive.Orm.Tests.Upgrade.Model.Version1.Person").Id;
          maxTypeId = session.Query.All<Metadata.Type>().Max(type => type.Id);
        }
      }

      BuildDomain("1", DomainUpgradeMode.PerformSafely, null, typeof (Address), typeof (Person), typeof (BusinessContact));
      using (var session = domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var businessContactTypeId = session.Query.All<Metadata.Type>()
            .First(type => type.Name=="Xtensive.Orm.Tests.Upgrade.Model.Version1.BusinessContact").Id;
          var newPersonTypeId = session.Query.All<Metadata.Type>()
            .First(type => type.Name=="Xtensive.Orm.Tests.Upgrade.Model.Version1.Person").Id;

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
      
      using (var session = domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          personTypeId = session.Query.All<Metadata.Type>()
            .First(type => type.Name=="Xtensive.Orm.Tests.Upgrade.Model.Version1.Person").Id;
          businessContactTypeId = session.Query.All<Metadata.Type>()
            .First(type => type.Name=="Xtensive.Orm.Tests.Upgrade.Model.Version1.BusinessContact").Id;
        }
      }

      BuildDomain("2", DomainUpgradeMode.Perform);
      using (var session = domain.OpenSession()) {
        using (var t = session.OpenTransaction()) {
          var newBusinessContactTypeId = session.Query.All<Metadata.Type>()
            .First(type => type.Name=="Xtensive.Orm.Tests.Upgrade.Model.Version2.BusinessContact").Id;
          var newPersonTypeId = session.Query.All<Metadata.Type>()
            .First(type => type.Name=="Xtensive.Orm.Tests.Upgrade.Model.Version2.Person").Id;

          Assert.AreEqual(personTypeId, newBusinessContactTypeId);
          Assert.AreEqual(businessContactTypeId, newPersonTypeId);
        }
      }
    }

    [Test]
    public void UpgradeToVersion2Test()
    {
      BuildDomain("2", DomainUpgradeMode.Perform);
      using (var session = domain.OpenSession()) {
        using (session.OpenTransaction()) {
          Assert.AreEqual(2, session.Query.All<Model.Version2.Person>().Count());
          Assert.AreEqual("Island Trading", session.Query.All<Model.Version2.Person>()
            .First(person => person.ContactName=="Helen Bennett").CompanyName);
          Assert.AreEqual(5, session.Query.All<Model.Version2.BusinessContact>().Count());
          Assert.AreEqual("Suyama", session.Query.All<Model.Version2.BusinessContact>()
            .First(contact => contact.FirstName=="Michael").LastName);
          Assert.AreEqual("Fuller", session.Query.All<Model.Version2.Employee>()
            .First(employee => employee.FirstName=="Nancy").ReportsTo.LastName);
          Assert.AreEqual(123, session.Query.All<Model.Version2.Person>()
            .First(person => person.ContactName=="Helen Bennett").PassportNumber);
          Assert.AreEqual(1, session.Query.All<Model.Version2.Order>()
            .First(order => order.ProductName=="Maxilaku").Number);

          session.Query.All<Product>().Single(product => product.Title=="DataObjects.NET");
          session.Query.All<Product>().Single(product => product.Title=="HelpServer");
          Assert.AreEqual(2, session.Query.All<Product>().Count());
          var webApps = session.Query.All<ProductGroup>().Single(group => group.Name=="Web applications");
          var frameworks = session.Query.All<ProductGroup>().Single(group => group.Name=="Frameworks");
          Assert.AreEqual(1, webApps.Products.Count);
          Assert.AreEqual(1, frameworks.Products.Count);

          var allBoys = session.Query.All<Boy>().ToArray();
          var allGirls = session.Query.All<Girl>().ToArray();
          Assert.AreEqual(2, allBoys.Length);
          Assert.AreEqual(2, allGirls.Length);
          var alex = allBoys.Single(boy => boy.Name=="Alex");
          foreach (var girl in allGirls)
            Assert.IsTrue(alex.MeetWith.Contains(girl));

          var e1 = session.Query.All<Entity1>().Single();
          var e2 = session.Query.All<Entity2>().Single();
          var e3 = session.Query.All<Entity3>().Single();
          var e4 = session.Query.All<Entity4>().Single();
          var se1 = session.Query.All<StructureContainer1>().Single();
          var se2 = session.Query.All<StructureContainer2>().Single();
          var se3 = session.Query.All<StructureContainer3>().Single();
          var se4 = session.Query.All<StructureContainer4>().Single();

          Assert.AreEqual(1, e1.Code);
          Assert.AreEqual(2, e2.Code);
          Assert.AreEqual(3, e3.Code);
          Assert.AreEqual(4, e4.Code);

          Assert.AreEqual(e1, e2.E1);
          Assert.AreEqual(e2, e3.E2);
          Assert.AreEqual(e3, e4.E3);

          Assert.AreEqual(se1.S1.MyE1, e1);

          Assert.AreEqual(se2.S2.MyE2, e2);
          Assert.AreEqual(se2.S2.S1.MyE1, e1);

          Assert.AreEqual(se3.S3.MyE3, e3);
          Assert.AreEqual(se3.S3.S2.MyE2, e2);
          Assert.AreEqual(se3.S3.S2.S1.MyE1, e1);

          Assert.AreEqual(se4.S4.MyE4, e4);
          Assert.AreEqual(se4.S4.S3.MyE3, e3);
          Assert.AreEqual(se4.S4.S3.S2.MyE2, e2);
          Assert.AreEqual(se4.S4.S3.S2.S1.MyE1, e1);

          var so1 = session.Query.All<MyStructureOwner>().Single(e => e.Id==0);
          var so2 = session.Query.All<MyStructureOwner>().Single(e => e.Id==1);
          var re1 = session.Query.All<ReferencedEntity>().Single(e => e.A==1 && e.B==2);
          var re2 = session.Query.All<ReferencedEntity>().Single(e => e.A==2 && e.B==3);
          if (!IncludeTypeIdModifier.IsEnabled) {
            Assert.AreEqual(so1.Reference, re1);
            Assert.AreEqual(so2.Reference, re2);
          }

          Assert.AreEqual(2, session.Query.All<NewSync<M2.BusinessContact>>().Count());
          Assert.AreEqual("Alex", session.Query.All<NewSync<M2.Boy>>().First().NewRoot.Name);
        }
      }
    }

    private void BuildDomain(string version, DomainUpgradeMode upgradeMode)
    {
      if (domain!=null)
        domain.DisposeSafely();

      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(Assembly.GetExecutingAssembly(),
        "Xtensive.Orm.Tests.Upgrade.Model.Version" + version);
      configuration.Types.Register(typeof (Upgrader));
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
        configuration.KeyGeneratorCacheSize = keyCacheSize.Value;
      configuration.Types.Register(typeof (Upgrader));
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
          var helen = new BusinessContact {
            Address = new Address {
              City = "Cowes",
              Country = "UK"
            },
            CompanyName = "Island Trading",
            ContactName = "Helen Bennett",
            PassportNumber = "123"
          };
          var philip = new BusinessContact {
            Address = new Address {
              City = "Brandenburg",
              Country = "Germany"
            },
            CompanyName = "Koniglich Essen",
            ContactName = "Philip Cramer",
            PassportNumber = "321"
          };
          
          // Employies
          var director = new Employee {
            Address = new Address {
              City = "Tacoma",
              Country = "USA"
            },
            FirstName = "Andrew",
            LastName = "Fuller",
            HireDate = new DateTime(1992, 8, 13)
          };
          var nancy = new Employee {
            Address = new Address {
              City = "Seattle",
              Country = "USA"
            },
            FirstName = "Nancy",
            LastName = "Davolio",
            HireDate = new DateTime(1992, 5, 1),
            ReportsTo = director
          };
          var michael = new Employee {
            Address = new Address {
              City = "London",
              Country = "UK"
            },
            FirstName = "Michael",
            LastName = "Suyama",
            HireDate = new DateTime(1993, 10, 17),
            ReportsTo = director
          };

          // Orders
          new Order {
            OrderNumber = "1",
            Customer = helen,
            Employee = michael,
            Freight = 12,
            OrderDate = new DateTime(1996, 7, 4),
            ProductName = "Maxilaku"
          };
          new Order {
            OrderNumber = "2",
            Customer = helen,
            Employee = nancy,
            Freight = 12,
            OrderDate = new DateTime(1996, 7, 4),
            ProductName = "Filo Mix"
          };
          new Order {
            OrderNumber = "3",
            Customer = philip,
            Employee = michael,
            Freight = 12,
            OrderDate = new DateTime(1996, 7, 4),
            ProductName = "Tourtiere"
          };
          new Order {
            OrderNumber = "4",
            Customer = philip,
            Employee = nancy,
            Freight = 12,
            OrderDate = new DateTime(1996, 7, 4),
            ProductName = "Pate chinois"
          };

          // Products & catgories
          new Category {
            Name = "Web applications",
            Products = {new Model.Version1.Product {Name = "HelpServer", IsActive = true}}
          };

          new Category {
            Name = "Frameworks",
            Products = {new Model.Version1.Product {Name = "DataObjects.NET", IsActive = true}}
          };

          // Boys & girls
          var alex = new Model.Version1.Boy("Alex");
          var dmitry = new Model.Version1.Boy("Dmitry");
          var elena = new Model.Version1.Girl("Elena");
          var tanya = new Model.Version1.Girl("Tanya");
          alex.FriendlyGirls.Add(elena);
          alex.FriendlyGirls.Add(tanya);
          elena.FriendlyBoys.Add(dmitry);

          // EntityX
          var e1 = new Model.Version1.Entity1(1);
          var e2 = new Model.Version1.Entity2(2, e1);
          var e3 = new Model.Version1.Entity3(3, e2);
          var e4 = new Model.Version1.Entity4(4, e3);

          // StructureContainerX
          var se1 = new Model.Version1.StructureContainer1 {S1 = new Structure1 {E1 = e1}};
          var se2 = new Model.Version1.StructureContainer2 {S2 = new Structure2 {E2 = e2, S1 = se1.S1}};
          var se3 = new Model.Version1.StructureContainer3 {S3 = new Structure3 {E3 = e3, S2 = se2.S2}};
          var se4 = new Model.Version1.StructureContainer4 {S4 = new Structure4 {E4 = e4, S3 = se3.S3}};

          // MyStructureOwner, ReferencedEntity
          new Model.Version1.MyStructureOwner(0) {Structure = new MyStructure {A = 1, B = 2}};
          new Model.Version1.MyStructureOwner(1) {Structure = new MyStructure {A = 2, B = 3}};
          new Model.Version1.ReferencedEntity(1, 2);
          new Model.Version1.ReferencedEntity(2, 3);

          // Generic types
          new Sync<M1.Person> {Root = helen};
          new Sync<M1.Person> {Root = director};
          new Sync<M1.Boy> {Root = alex};
          
          // Commiting changes
          transactionScope.Complete();
        }
      }
    }

    #endregion
  }
}