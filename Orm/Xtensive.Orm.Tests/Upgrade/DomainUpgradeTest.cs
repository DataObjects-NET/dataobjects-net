// Copyright (C) 2009-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using M1 = Xtensive.Orm.Tests.Upgrade.Models.Version1;
using M2 = Xtensive.Orm.Tests.Upgrade.Models.Version2;
using Upgrader = Xtensive.Orm.Tests.Upgrade.Models.Upgrader;

namespace Xtensive.Orm.Tests.Upgrade
{
  [TestFixture, Category("Upgrade")]
  public class DomainUpgradeTest
  {
    private readonly Dictionary<int, string> NamespaceForVersion = new Dictionary<int, string>() {
      { 1, "Xtensive.Orm.Tests.Upgrade.Models.Version1" },
      { 2, "Xtensive.Orm.Tests.Upgrade.Models.Version2" }
    };

    private readonly Dictionary<Upgrader.ModelParts, IReadOnlyList<Type>> ModelPartsV1 = new Dictionary<Upgrader.ModelParts, IReadOnlyList<Type>>();
    private readonly Dictionary<Upgrader.ModelParts, IReadOnlyList<Type>> ModelPartsV2 = new Dictionary<Upgrader.ModelParts, IReadOnlyList<Type>>();

    private IReadOnlyList<Type> OrdersModelTypeSet;
    private IReadOnlyList<Type> TwoTypesSet;
    private IReadOnlyList<Type> ThreeTypesSet;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
      var completeModelTypesV1 = new[] {
        typeof(M1.Address),
        typeof(M1.Person),
        typeof(M1.BusinessContact),
        typeof(M1.Employee),
        typeof(M1.Order),

        typeof(M1.Category),
        typeof(M1.Product),

        typeof(M1.Boy),
        typeof(M1.Girl),

        typeof(M1.Entity1),
        typeof(M1.Entity2),
        typeof(M1.Entity3),
        typeof(M1.Entity4),

        typeof(M1.Structure1),
        typeof(M1.Structure2),
        typeof(M1.Structure3),
        typeof(M1.Structure4),

        typeof(M1.StructureContainer1),
        typeof(M1.StructureContainer2),
        typeof(M1.StructureContainer3),
        typeof(M1.StructureContainer4),

        typeof(M1.MyStructure),
        typeof(M1.MyStructureOwner),

        typeof(M1.Sync<>),
      };

      OrdersModelTypeSet = new ArraySegment<Type>(completeModelTypesV1, 0, 5);
      TwoTypesSet = new ArraySegment<Type>(completeModelTypesV1, 0, 2);
      ThreeTypesSet = new ArraySegment<Type>(completeModelTypesV1, 0, 3);

      ModelPartsV1[Upgrader.ModelParts.All] = completeModelTypesV1;
      ModelPartsV1[Upgrader.ModelParts.Order] = new ArraySegment<Type>(completeModelTypesV1, 0, 5);
      ModelPartsV1[Upgrader.ModelParts.Product] = new ArraySegment<Type>(completeModelTypesV1, 5, 2);
      ModelPartsV1[Upgrader.ModelParts.BoyGirl] = new ArraySegment<Type>(completeModelTypesV1, 7, 2);
      ModelPartsV1[Upgrader.ModelParts.CrazyAssociations] = new ArraySegment<Type>(completeModelTypesV1, 9, 12);
      ModelPartsV1[Upgrader.ModelParts.ComplexFieldCopy] = new ArraySegment<Type>(completeModelTypesV1, 21, 2);
      ModelPartsV1[Upgrader.ModelParts.Generics]
        = new ArraySegment<Type>(completeModelTypesV1, 0, 5)
          .Union(new ArraySegment<Type>(completeModelTypesV1, 7, 2))
          .Union(new ArraySegment<Type>(completeModelTypesV1, 23, 1))
          .ToArray();

      var completeModelTypesV2 = new[] {
        typeof(M2.Address),
        typeof(M2.Person),
        typeof(M2.BusinessContact),
        typeof(M2.Employee),
        typeof(M2.Order),

        typeof(M2.ProductGroup),
        typeof(M2.Product),

        typeof(M2.Boy),
        typeof(M2.Girl),

        typeof(M2.Entity1),
        typeof(M2.Entity2),
        typeof(M2.Entity3),
        typeof(M2.Entity4),

        typeof(M2.Structure1),
        typeof(M2.Structure2),
        typeof(M2.Structure3),
        typeof(M2.Structure4),

        typeof(M2.StructureContainer1),
        typeof(M2.StructureContainer2),
        typeof(M2.StructureContainer3),
        typeof(M2.StructureContainer4),

        typeof(M2.ReferencedEntity),
        typeof(M2.MyStructureOwner),

        typeof(M2.NewSync<>)
      };

      ModelPartsV2[Upgrader.ModelParts.All] = completeModelTypesV2;
      ModelPartsV2[Upgrader.ModelParts.Order] = new ArraySegment<Type>(completeModelTypesV2, 0, 5);
      ModelPartsV2[Upgrader.ModelParts.Product] = new ArraySegment<Type>(completeModelTypesV2, 5, 2);
      ModelPartsV2[Upgrader.ModelParts.BoyGirl] = new ArraySegment<Type>(completeModelTypesV2, 7, 2);
      ModelPartsV2[Upgrader.ModelParts.CrazyAssociations] = new ArraySegment<Type>(completeModelTypesV2, 9, 12);
      ModelPartsV2[Upgrader.ModelParts.ComplexFieldCopy] = new ArraySegment<Type>(completeModelTypesV2, 21, 2);
      ModelPartsV2[Upgrader.ModelParts.Generics]
        = new ArraySegment<Type>(completeModelTypesV2, 0, 5)
          .Union(new ArraySegment<Type>(completeModelTypesV2, 7, 2))
          .Union(new ArraySegment<Type>(completeModelTypesV2, 23, 1))
          .ToArray();
    }

    [Test]
    public void UpgradeModeTest()
    {
      var syncedTypeSet = ThreeTypesSet;
      var missingTypeSet = TwoTypesSet;

      BuildDomain(1, DomainUpgradeMode.Recreate, missingTypeSet).DisposeSafely();

      BuildDomain(1, DomainUpgradeMode.PerformSafely, syncedTypeSet).DisposeSafely();
      AssertEx.Throws<SchemaSynchronizationException>(() =>
        BuildDomain(1, DomainUpgradeMode.PerformSafely, missingTypeSet));

      BuildDomain(1, DomainUpgradeMode.Validate, syncedTypeSet).DisposeSafely();
      AssertEx.Throws<SchemaSynchronizationException>(() =>
        BuildDomain(1, DomainUpgradeMode.Validate, missingTypeSet));

      AssertEx.Throws<SchemaSynchronizationException>(() =>
        BuildDomain(1, DomainUpgradeMode.Validate, OrdersModelTypeSet));
    }

    [Test]
    public void UpgradeGeneratorsTest()
    {
      Require.AnyFeatureSupported(ProviderFeatures.Sequences | ProviderFeatures.ArbitraryIdentityIncrement);

      var isFirebird = StorageProviderInfo.Instance.CheckProviderIs(StorageProvider.Firebird);

      var generatorCacheSize = 3;
      using (var initDomain = BuildDomain(1, DomainUpgradeMode.Recreate, TwoTypesSet, generatorCacheSize))
      using (var session = initDomain.OpenSession())
      using (var t = session.OpenTransaction()) {
        for (var i = 0; i < generatorCacheSize; i++) {
          _ = new M1.Person {
            Address = new M1.Address { City = "City", Country = "Country" }
          };
        }

        Assert.LessOrEqual(session.Query.All<M1.Person>().Max(p => p.Id), 4);
        Assert.GreaterOrEqual(session.Query.All<M1.Person>().Max(p => p.Id), 3);
        t.Complete();
      }

      using (var upgradedDomain = BuildDomain(1, DomainUpgradeMode.Perform, TwoTypesSet, generatorCacheSize))
      using (var session = upgradedDomain.OpenSession())
      using (var t = session.OpenTransaction()) {
        for (var i = 0; i < generatorCacheSize; i++) {
          _ = new M1.Person {
            Address = new M1.Address { City = "City", Country = "Country" }
          };
        }

        Assert.LessOrEqual(session.Query.All<M1.Person>().Max(p => p.Id), 8);
        Assert.GreaterOrEqual(session.Query.All<M1.Person>().Max(p => p.Id), 6);
        t.Complete();
      }

      generatorCacheSize = 2;
      using (var upgradedDomain = BuildDomain(1, DomainUpgradeMode.Perform, TwoTypesSet, generatorCacheSize))
      using (var session = upgradedDomain.OpenSession())
      using (var t = session.OpenTransaction()) {
        _ = new M1.Person { Address = new M1.Address { City = "City", Country = "Country" } };
        _ = new M1.Person { Address = new M1.Address { City = "City", Country = "Country" } };
        _ = new M1.Person { Address = new M1.Address { City = "City", Country = "Country" } };

        if (isFirebird) {
          Assert.LessOrEqual(session.Query.All<M1.Person>().Max(p => p.Id), 10);
          Assert.GreaterOrEqual(session.Query.All<M1.Person>().Max(p => p.Id), 9);
        }
        else {
          Assert.LessOrEqual(session.Query.All<M1.Person>().Max(p => p.Id), 13);
          Assert.GreaterOrEqual(session.Query.All<M1.Person>().Max(p => p.Id), 12);
        }
        t.Complete();
      }
    }

    [Test]
    public void UpdateTypeIdTest()
    {
      int personTypeId;
      int maxTypeId;

      using (var initDomain = BuildDomain(1, DomainUpgradeMode.Recreate, TwoTypesSet))
      using (var session = initDomain.OpenSession())
      using (var t = session.OpenTransaction()) {
        personTypeId = session.Query.All<Metadata.Type>()
          .First(type => type.Name == $"{NamespaceForVersion[1]}.Person").Id;
        maxTypeId = session.Query.All<Metadata.Type>().Max(type => type.Id);
      }

      using (var upgradedDomain = BuildDomain(1, DomainUpgradeMode.PerformSafely, ThreeTypesSet))
      using (var session = upgradedDomain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var businessContactTypeId = session.Query.All<Metadata.Type>()
          .First(type => type.Name == $"{NamespaceForVersion[1]}.BusinessContact").Id;
        var newPersonTypeId = session.Query.All<Metadata.Type>()
          .First(type => type.Name == $"{NamespaceForVersion[1]}.Person").Id;

        Assert.AreEqual(personTypeId, newPersonTypeId);
        Assert.AreEqual(maxTypeId + 1, businessContactTypeId);
      }
    }

    [Test]
    [IgnoreIfGithubActions(StorageProvider.Sqlite | StorageProvider.MySql)]
    public void UpdateTypeIdWithMutualRenameTest()
    {
      Require.ProviderIsNot(StorageProvider.Firebird);

      int personTypeId;
      int businessContactTypeId;

      using (var initDomain = BuildDomain(1, DomainUpgradeMode.Recreate))
      using (var session = initDomain.OpenSession())
      using (var t = session.OpenTransaction()) {
        personTypeId = session.Query.All<Metadata.Type>()
          .First(type => type.Name == $"{NamespaceForVersion[1]}.Person").Id;
        businessContactTypeId = session.Query.All<Metadata.Type>()
          .First(type => type.Name == $"{NamespaceForVersion[1]}.BusinessContact").Id;
      }

      using (var upgradedDomain = BuildDomain(2, DomainUpgradeMode.Perform))
      using (var session = upgradedDomain.OpenSession())
      using (var t = session.OpenTransaction()) {
        var newBusinessContactTypeId = session.Query.All<Metadata.Type>()
          .First(type => type.Name == $"{NamespaceForVersion[2]}.BusinessContact").Id;
        var newPersonTypeId = session.Query.All<Metadata.Type>()
          .First(type => type.Name == $"{NamespaceForVersion[2]}.Person").Id;

        Assert.AreEqual(personTypeId, newBusinessContactTypeId);
        Assert.AreEqual(businessContactTypeId, newPersonTypeId);
      }
    }

    [Test]
    [IgnoreIfGithubActions(StorageProvider.Sqlite | StorageProvider.MySql)]
    public void UpgradeToVersion2Test()
    {
      Require.ProviderIsNot(StorageProvider.Firebird);

      var domain = BuildDomain(1, DomainUpgradeMode.Recreate);
      FillData(domain);

      domain = BuildDomain(2, DomainUpgradeMode.Perform);
      TestComplexModelUpgradedData(domain);
    }

    private void TestComplexModelUpgradedData(Domain domain)
    {
      using (var session = domain.OpenSession())
      using (session.OpenTransaction()) {
        TestOrdersPart(session);
        TestProductPart(session);
        TestBoyGirlPart(session);
        TestCrazyAssociationsPart(session);
        TestComplexFieldCopy(session);
        TestGenericsPart(session);
      }
    }

    #region Partial model upgrades

    [Test, Explicit]
    public void OrderPartTest()
    {
      Require.ProviderIsNot(StorageProvider.Firebird);

      using (var domain = BuildDomain(1, DomainUpgradeMode.Recreate, modelParts: Upgrader.ModelParts.Order)) {
        FillData(domain);
      }

      using (var domain = BuildDomain(2, DomainUpgradeMode.Perform, modelParts: Upgrader.ModelParts.Order)) {
        using (var session = domain.OpenSession())
        using (var tx = session.OpenTransaction()) {
          TestOrdersPart(session);
        }
      }
    }

    [Test, Explicit]
    public void ProductPartTest()
    {
      Require.ProviderIsNot(StorageProvider.Firebird);

      using (var domain = BuildDomain(1, DomainUpgradeMode.Recreate, modelParts: Upgrader.ModelParts.Product)) {
        FillData(domain);
      }

      using (var domain = BuildDomain(2, DomainUpgradeMode.Perform, modelParts: Upgrader.ModelParts.Product)) {
        using (var session = domain.OpenSession())
        using (var tx = session.OpenTransaction()) {
          TestProductPart(session);
        }
      }
    }

    [Test, Explicit]
    public void BoyGirlPartTest()
    {
      Require.ProviderIsNot(StorageProvider.Firebird);

      using (var domain = BuildDomain(1, DomainUpgradeMode.Recreate, modelParts: Upgrader.ModelParts.BoyGirl)) {
        FillData(domain);
      }

      using (var domain = BuildDomain(2, DomainUpgradeMode.Perform, modelParts: Upgrader.ModelParts.BoyGirl)) {
        using (var session = domain.OpenSession())
        using (var tx = session.OpenTransaction()) {
          TestBoyGirlPart(session);
        }
      }
    }

    [Test, Explicit]
    public void CrazyAssociationsTest()
    {
      Require.ProviderIsNot(StorageProvider.Firebird);

      using (var domain = BuildDomain(1, DomainUpgradeMode.Recreate, modelParts: Upgrader.ModelParts.CrazyAssociations)) {
        FillData(domain);
      }

      using (var domain = BuildDomain(2, DomainUpgradeMode.Perform, modelParts: Upgrader.ModelParts.CrazyAssociations)) {
        using (var session = domain.OpenSession())
        using (var tx = session.OpenTransaction()) {
          TestCrazyAssociationsPart(session);
        }
      }
    }

    [Test, Explicit]
    public void ComplexFieldCopyTest()
    {
      Require.ProviderIsNot(StorageProvider.Firebird);

      using (var domain = BuildDomain(1, DomainUpgradeMode.Recreate, modelParts: Upgrader.ModelParts.ComplexFieldCopy)) {
        FillData(domain);
      }

      using (var domain = BuildDomain(2, DomainUpgradeMode.Perform, modelParts: Upgrader.ModelParts.ComplexFieldCopy)) {
        using (var session = domain.OpenSession())
        using (var tx = session.OpenTransaction()) {
          TestComplexFieldCopy(session);
        }
      }
    }

    [Test, Explicit]
    public void GenericsTest()
    {
      Require.ProviderIsNot(StorageProvider.Firebird);

      using (var domain = BuildDomain(1, DomainUpgradeMode.Recreate, modelParts: Upgrader.ModelParts.Generics)) {
        FillData(domain);
      }

      using (var domain = BuildDomain(2, DomainUpgradeMode.Perform, modelParts: Upgrader.ModelParts.Generics)) {
        using (var session = domain.OpenSession())
        using (var tx = session.OpenTransaction()) {
          TestGenericsPart(session);
        }
      }
    }

    #endregion


    private static void TestOrdersPart(Session session)
    {
      Assert.AreEqual(2, session.Query.All<M2.Person>().Count());
      Assert.AreEqual("Island Trading", session.Query.All<M2.Person>()
        .First(person => person.ContactName == "Helen Bennett").CompanyName);
      Assert.AreEqual(5, session.Query.All<M2.BusinessContact>().Count());
      Assert.AreEqual("Suyama", session.Query.All<M2.BusinessContact>()
        .First(contact => contact.FirstName == "Michael").LastName);
      Assert.AreEqual("Fuller", session.Query.All<M2.Employee>()
        .First(employee => employee.FirstName == "Nancy").ReportsTo.LastName);
      Assert.AreEqual(123, session.Query.All<M2.Person>()
        .First(person => person.ContactName == "Helen Bennett").PassportNumber);
      Assert.AreEqual(1, session.Query.All<M2.Order>()
        .First(order => order.ProductName == "Maxilaku").Number);
    }

    private static void TestProductPart(Session session)
    {
      _ = session.Query.All<M2.Product>().Single(product => product.Title == "DataObjects.NET");
      _ = session.Query.All<M2.Product>().Single(product => product.Title == "HelpServer");
      Assert.AreEqual(2, session.Query.All<M2.Product>().Count());
      var webApps = session.Query.All<M2.ProductGroup>().Single(group => group.Name == "Web applications");
      var frameworks = session.Query.All<M2.ProductGroup>().Single(group => group.Name == "Frameworks");
      Assert.AreEqual(1, webApps.Products.Count);
      Assert.AreEqual(1, frameworks.Products.Count);
    }

    private static void TestBoyGirlPart(Session session)
    {
      var allBoys = session.Query.All<M2.Boy>().ToArray();
      var allGirls = session.Query.All<M2.Girl>().ToArray();
      Assert.AreEqual(2, allBoys.Length);
      Assert.AreEqual(2, allGirls.Length);
      var alex = allBoys.Single(boy => boy.Name == "Alex");
      foreach (var girl in allGirls) {
        Assert.IsTrue(alex.MeetWith.Contains(girl));
      }
    }

    private static void TestCrazyAssociationsPart(Session session)
    {
      var e1 = session.Query.All<M2.Entity1>().Single();
      var e2 = session.Query.All<M2.Entity2>().Single();
      var e3 = session.Query.All<M2.Entity3>().Single();
      var e4 = session.Query.All<M2.Entity4>().Single();
      var se1 = session.Query.All<M2.StructureContainer1>().Single();
      var se2 = session.Query.All<M2.StructureContainer2>().Single();
      var se3 = session.Query.All<M2.StructureContainer3>().Single();
      var se4 = session.Query.All<M2.StructureContainer4>().Single();

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
    }

    private static void TestComplexFieldCopy(Session session)
    {
      var so1 = session.Query.All<M2.MyStructureOwner>().Single(e => e.Id == 0);
      var so2 = session.Query.All<M2.MyStructureOwner>().Single(e => e.Id == 1);
      var re1 = session.Query.All<M2.ReferencedEntity>().Single(e => e.A == 1 && e.B == 2);
      var re2 = session.Query.All<M2.ReferencedEntity>().Single(e => e.A == 2 && e.B == 3);
      if (!IncludeTypeIdModifier.IsEnabled) {
        Assert.AreEqual(so1.Reference, re1);
        Assert.AreEqual(so2.Reference, re2);
      }
    }

    private static void TestGenericsPart(Session session)
    {
      Assert.AreEqual(2, session.Query.All<M2.NewSync<M2.BusinessContact>>().Count());
      Assert.AreEqual("Alex", session.Query.All<M2.NewSync<M2.Boy>>().First().NewRoot.Name);
    }

    private Domain BuildDomain(int version, DomainUpgradeMode upgradeMode)
    {
      using (Upgrader.EnableForVersion(version)) {
        return BuildDomainFromConfig(CreateConfiguration(version, upgradeMode));
      }
    }

    private Domain BuildDomain(int version, DomainUpgradeMode upgradeMode, Upgrader.ModelParts modelParts = Upgrader.ModelParts.All, int? keyCacheSize = null)
    {
      var types = (version == 1)
        ? ModelPartsV1[modelParts]
        : ModelPartsV2[modelParts];
      var configuration = CreateConfiguration(upgradeMode, types, keyCacheSize);

      using (Upgrader.EnableForVersion(version, modelParts)) {
        return BuildDomainFromConfig(configuration);
      }
    }

    private Domain BuildDomain(int version, DomainUpgradeMode upgradeMode, IReadOnlyList<Type> types, int? keyCacheSize = null)
    {
      var configuration = CreateConfiguration(upgradeMode, types, keyCacheSize);

      using (Upgrader.EnableForVersion(version)) {
        return BuildDomainFromConfig(configuration);
      }
    }

    private DomainConfiguration CreateConfiguration(int version, DomainUpgradeMode upgradeMode)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      configuration.Types.Register(Assembly.GetExecutingAssembly(), NamespaceForVersion[version]);
      configuration.Types.Register(typeof(Upgrader));

      return configuration;
    }
    private DomainConfiguration CreateConfiguration(DomainUpgradeMode upgradeMode, IReadOnlyList<Type> types, int? keyCacheSize)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = upgradeMode;
      foreach (var type in types) {
        configuration.Types.Register(type);
      }
      configuration.Types.Register(typeof(Upgrader));

      if (keyCacheSize.HasValue) {
        configuration.KeyGeneratorCacheSize = keyCacheSize.Value;
      }
      return configuration;
    }

    private Domain BuildDomainFromConfig(DomainConfiguration configuration)
    {
      return Domain.Build(configuration);
    }

    #region Data filler

    private void FillData(Domain domain)
    {
      using (var session = domain.OpenSession())
      using (var transactionScope = session.OpenTransaction()) {
        Key helenKey = null;
        Key directorKey = null;
        Key alexKey = null;
        // orders part
        if (CheckTypeExistsInDomain(domain, typeof(M1.Order))) {
          // BusinessContacts
          var helen = new M1.BusinessContact {
            Address = new M1.Address {
              City = "Cowes",
              Country = "UK"
            },
            CompanyName = "Island Trading",
            ContactName = "Helen Bennett",
            PassportNumber = "123"
          };
          helenKey = helen.Key;
          var philip = new M1.BusinessContact {
            Address = new M1.Address {
              City = "Brandenburg",
              Country = "Germany"
            },
            CompanyName = "Koniglich Essen",
            ContactName = "Philip Cramer",
            PassportNumber = "321"
          };

          // Employees
          var director = new M1.Employee {
            Address = new M1.Address {
              City = "Tacoma",
              Country = "USA"
            },
            FirstName = "Andrew",
            LastName = "Fuller",
            HireDate = new DateTime(1992, 8, 13)
          };
          directorKey = director.Key;
          var nancy = new M1.Employee {
            Address = new M1.Address {
              City = "Seattle",
              Country = "USA"
            },
            FirstName = "Nancy",
            LastName = "Davolio",
            HireDate = new DateTime(1992, 5, 1),
            ReportsTo = director
          };
          var michael = new M1.Employee {
            Address = new M1.Address {
              City = "London",
              Country = "UK"
            },
            FirstName = "Michael",
            LastName = "Suyama",
            HireDate = new DateTime(1993, 10, 17),
            ReportsTo = director
          };

          // Orders
          _ = new M1.Order {
            OrderNumber = "1",
            Customer = helen,
            Employee = michael,
            Freight = 12,
            OrderDate = new DateTime(1996, 7, 4),
            ProductName = "Maxilaku"
          };
          _ = new M1.Order {
            OrderNumber = "2",
            Customer = helen,
            Employee = nancy,
            Freight = 12,
            OrderDate = new DateTime(1996, 7, 4),
            ProductName = "Filo Mix"
          };
          _ = new M1.Order {
            OrderNumber = "3",
            Customer = philip,
            Employee = michael,
            Freight = 12,
            OrderDate = new DateTime(1996, 7, 4),
            ProductName = "Tourtiere"
          };
          _ = new M1.Order {
            OrderNumber = "4",
            Customer = philip,
            Employee = nancy,
            Freight = 12,
            OrderDate = new DateTime(1996, 7, 4),
            ProductName = "Pate chinois"
          };
        }

        // Products & catgories
        if (CheckTypeExistsInDomain(domain, typeof(M1.Category))) {
          _ = new M1.Category {
            Name = "Web applications",
            Products = { new M1.Product { Name = "HelpServer", IsActive = true } }
          };

          _ = new M1.Category {
            Name = "Frameworks",
            Products = { new M1.Product { Name = "DataObjects.NET", IsActive = true } }
          };
        }

        // Boys & girls
        if (CheckTypeExistsInDomain(domain, typeof(M1.Boy))) {
          var alex = new M1.Boy("Alex");
          alexKey = alex.Key;
          var dmitry = new M1.Boy("Dmitry");
          var elena = new M1.Girl("Elena");
          var tanya = new M1.Girl("Tanya");
          _ = alex.FriendlyGirls.Add(elena);
          _ = alex.FriendlyGirls.Add(tanya);
          _ = elena.FriendlyBoys.Add(dmitry);
        }

        if (CheckTypeExistsInDomain(domain, typeof(M1.Entity4))) {
          // EntityX
          var e1 = new M1.Entity1(1);
          var e2 = new M1.Entity2(2, e1);
          var e3 = new M1.Entity3(3, e2);
          var e4 = new M1.Entity4(4, e3);

          // StructureContainerX
          var se1 = new M1.StructureContainer1 { S1 = new M1.Structure1 { E1 = e1 } };
          var se2 = new M1.StructureContainer2 { S2 = new M1.Structure2 { E2 = e2, S1 = se1.S1 } };
          var se3 = new M1.StructureContainer3 { S3 = new M1.Structure3 { E3 = e3, S2 = se2.S2 } };
          var se4 = new M1.StructureContainer4 { S4 = new M1.Structure4 { E4 = e4, S3 = se3.S3 } };
        }

        if (CheckTypeExistsInDomain(domain, typeof(M1.MyStructureOwner))) {
          // MyStructureOwner, ReferencedEntity
          _ = new M1.MyStructureOwner(0) { Structure = new M1.MyStructure { A = 1, B = 2 } };
          _ = new M1.MyStructureOwner(1) { Structure = new M1.MyStructure { A = 2, B = 3 } };
          _ = new M1.ReferencedEntity(1, 2);
          _ = new M1.ReferencedEntity(2, 3);
        }

        // Generic types
        if (CheckTypeExistsInDomain(domain, typeof(M1.Sync<M1.Person>))
          && CheckTypeExistsInDomain(domain, typeof(M1.Sync<M1.Boy>))) {
          var helen = session.Query.Single<M1.BusinessContact>(helenKey);
          var director = session.Query.Single<M1.Employee>(directorKey);
          var alex = session.Query.Single<M1.Boy>(alexKey);
          _ = new M1.Sync<M1.Person> { Root = helen };
          _ = new M1.Sync<M1.Person> { Root = director };
          _ = new M1.Sync<M1.Boy> { Root = alex };
        }

        // Commiting changes
        transactionScope.Complete();
      }

      static bool CheckTypeExistsInDomain(Domain domain, Type type)
      {
        return domain.Model.Types.Contains(type);
      }
    }

    #endregion
  }
}