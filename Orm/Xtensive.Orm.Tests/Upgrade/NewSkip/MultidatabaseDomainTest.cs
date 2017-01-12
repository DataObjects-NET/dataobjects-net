// Copyright (C) 2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.02.24

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Providers;
using Xtensive.Orm.Tests.Upgrade.NewSkip.Model.Laptops;
using Xtensive.Orm.Tests.Upgrade.NewSkip.Model.Users;
using ProviderInfo = Xtensive.Orm.Tests.Upgrade.NewSkip.Model.Users.ProviderInfo;
using XType = Xtensive.Orm.Tests.Upgrade.NewSkip.Model.Users.X;

namespace Xtensive.Orm.Tests.Upgrade.NewSkip
{
  /// <summary>
  /// Test for domain which inclules several databases
  /// </summary>
  public abstract class MultidatabaseDomainTest : SkipBuildingTestBase
  {
    private readonly string[] countries = new[] { "Russian Federation", "The United States of America", "Germany", "The United Kingdom" };
    private readonly string[] positions = new[] { "Position1", "Position2", "Position3", "Position4" };
    private readonly string[] emails = new[] { "aaaa@bbbb.ru", "bbbb@cccc.ru", "cccc@dddd.ru" };
    private readonly string[] names = new[] { "James Bond", "Jeremy Clarkson", "Lebron James" };

    [Test]
    public void MainTest()
    {
      using (var session = Domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        TestFirstModel(session);
        TestSecondModel(session);
      }
    }

    protected abstract ForeignKeyMode GetForeignKeyMode();

    protected override void CheckRequirements()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Multidatabase);
    }

    protected override void PopulateData(Domain domain)
    {
      using (var session = domain.OpenSession())
      using (var transaction = session.OpenTransaction()) {
        PopulateDataForModel1();
        PopulateDataForModel2();
        transaction.Complete();
      }
    }

    protected override DomainConfiguration BuildConfiguration()
    {
      var configuration = base.BuildConfiguration();
      configuration.Types.Register(typeof (User).Assembly, typeof (User).Namespace);
      configuration.Types.Register(typeof (Laptop).Assembly, typeof (Laptop).Namespace);
      configuration.DefaultDatabase = "DO-Tests-1";
      configuration.DefaultSchema = "Model1";
      configuration.ForeignKeyMode = GetForeignKeyMode();
      configuration.MappingRules.Map(typeof (User).Assembly, typeof (User).Namespace).ToDatabase("DO-Tests-1");
      configuration.MappingRules.Map(typeof (Laptop).Assembly, typeof (Laptop).Namespace).ToDatabase("DO-Tests-2");
      return configuration;
    }

    private void PopulateDataForModel1()
    {
      foreach (var position in positions)
        new Position {Value = position};

      foreach (var country in countries)
        new Country {Value = country};

      new SimpleFilterWithProperty {TestField = "hello world!"};
      new SimpleFilterWithProperty {TestField = "hello world!!"};

      var md5hash = new MD5Hash();
      var buildInProvider = new BuildInProviderInfo(md5hash);
      var googleProvider = new GoogleOAuthProvider(md5hash) {Url = "gooogle.com"};
      var aolProvider = new AolOpenIdProviderInfo(md5hash) {Url = "aol.com"};
      var providers = new ProviderInfo[] {buildInProvider, googleProvider, aolProvider};

      for (var index = 0; index < names.Length; index++) {
        var user = new User {Email = emails[index]};
        user.Person = new Person {FirstName = names[index].Split(' ')[0], LastName = names[index].Split(' ')[1]};
        user.AuthorizationInfos.Add(new AuthorizationInfo {Provider = providers[index]});
      }

      new XType {
        FBool = true,
        FByte = 1,
        FSByte = 1,
        FShort = 1,
        FUShort = 1,
        FInt = 1,
        FUInt = 1,
        FLong = 1,
        FULong = 1,
        FGuid = Guid.NewGuid(),
        FFloat = 1.0f,
        FDouble = 1.0d,
        FDecimal = 1,
        FDateTime = new DateTime(2001, 1, 1, 1, 1, 1),
        FTimeSpan = new TimeSpan(1, 1, 1, 1),
        FString = "1",
        FLongString = "1",
        FEByte = EByte.Min,
        FESByte = ESByte.Min,
        FEShort = EShort.Min,
        FEUShort = EUShort.Min,
        FEInt = EInt.Min,
        FEUInt = EUInt.Min,
        FELong = ELong.Min,
        FEULong = EULong.Min,

        FNBool = true,
        FNByte = 1,
        FNSByte = 1,
        FNShort = 1,
        FNUShort = 1,
        FNInt = 1,
        FNUInt = 1,
        FNLong = 1,
        FNULong = 1,
        FNGuid = Guid.NewGuid(),
        FNFloat = 1.0f,
        FNDouble = 1.0d,
        FNDecimal = 1,
        FNDateTime = new DateTime(2001, 1, 1, 1, 1, 1),
        FNTimeSpan = new TimeSpan(1, 1, 1, 1),
        FNEByte = EByte.Min,
        FNESByte = ESByte.Min,
        FNEShort = EShort.Min,
        FNEUShort = EUShort.Min,
        FNEInt = EInt.Min,
        FNEUInt = EUInt.Min,
        FNELong = ELong.Min,
        FNEULong = EULong.Min,
        Ref = new XRef(Guid.NewGuid())
      };
    }

    private void PopulateDataForModel2()
    {
      var amd = new Manufacturer {Name = "AMD"};
      var acer = new Manufacturer {Name = "Acer"};
      var intel = new Manufacturer {Name = "Intel"};
      var samsung = new Manufacturer {Name = "Samsung"};
      var seagate = new Manufacturer {Name = "Seagate"};
      var toshiba = new Manufacturer {Name = "Toshiba"};

      var firstLaptop = new Laptop {
        Manufacturer = samsung,
        SerialNumber = Guid.NewGuid().ToString(),
        DisplayInfo = new DisplayInfo {
          Dpi = 300,
          Format = Formats.Wide,
          Manufacturer = new Manufacturer {Name = "FirstDisplayProducer"},
          Resolution = new Resolution {Wide = 1920, Hight = 1080},
        },
        CpuInfo = new CPUInfo {
          ArchitectureName = "SuperDuperFastArchitecture",
          BaseClockSpeed = 200,
          Multiplier = 22,
          Manufacturer = intel,
          Name = "Core i3"
        },
        StorageInfo = new StorageInfo {
          Manufacturer = seagate,
          Capacity = new Capacity {
            Value = 1000,
            Measure = StorageCapacityMeasure.Gigabytes
          },
          IsSsd = false,
          MaxReadSpeed = 250,
          MaxWriteSpeed = 150,
        },
        GraphicsCardInfo = new GraphicsCardInfo {
          ChipProducer = ChipProducers.Amd,
          Name = "R9 370",
          Vendor = new Manufacturer {Name = "Gigabyte"},
        },
        IOPortsInfo = new IOPortsInfo {
          HasEthernetPort = true,
          HasHDMIPort = true,
          HasHeadphonesOutput = true,
          HasMicroInput = true,
          HasWiFiModule = true,
          Usb30PortsCount = 2,
          UsbPortsCount = 3
        },
        KeyboardInfo = new KeyboardInfo {
          KeysCount = 100,
          Type = KeyboardType.First
        }
      };

      var secondLaptop = new Laptop {
        Manufacturer = acer,
        SerialNumber = Guid.NewGuid().ToString(),
        DisplayInfo = new DisplayInfo {
          Dpi = 300,
          Format = Formats.Wide,
          Manufacturer = new Manufacturer {Name = "SecondDisplayProducer"},
          Resolution = new Resolution {Wide = 1920, Hight = 1080},
        },
        CpuInfo = new CPUInfo {
          ArchitectureName = "BadassArchitecture",
          BaseClockSpeed = 200,
          Multiplier = 22,
          Manufacturer = amd,
          Name = "Core i5",
        },
        StorageInfo = new StorageInfo {
          Manufacturer = toshiba,
          Capacity = new Capacity {
            Value = 5000,
            Measure = StorageCapacityMeasure.Gigabytes
          },
          IsSsd = false,
          MaxReadSpeed = 250,
          MaxWriteSpeed = 150,
        },
        GraphicsCardInfo = new GraphicsCardInfo {
          ChipProducer = ChipProducers.Amd,
          Name = "GTX 710",
          Vendor = new Manufacturer {Name = "MSI"},
        },
        IOPortsInfo = new IOPortsInfo {
          HasEthernetPort = true,
          HasHDMIPort = true,
          HasHeadphonesOutput = true,
          HasMicroInput = true,
          HasWiFiModule = true,
          Usb30PortsCount = 2,
          UsbPortsCount = 3
        },
        KeyboardInfo = new KeyboardInfo {
          KeysCount = 100,
          Type = KeyboardType.First
        }
      };
    }

    private void TestFirstModel(Session session)
    {
      var countriesCount = session.Query.All<Country>().Count();
      Assert.That(countriesCount, Is.EqualTo(countries.Length));
      foreach (var country in countries) {
        var c = session.Query.All<Country>().FirstOrDefault(el => el.Value==country);
        Assert.That(c, Is.Not.Null);
        Assert.That(c.Value, Is.EqualTo(country));
      }

      var positionsCount = session.Query.All<Position>().Count();
      Assert.That(positionsCount, Is.EqualTo(positions.Length));
      foreach (var position in positions) {
        var p = session.Query.All<Position>().FirstOrDefault(el => el.Value==position);
        Assert.That(p, Is.Not.Null);
        Assert.That(p.Value, Is.EqualTo(position));
      }

      var propertiesCount = session.Query.All<SimpleFilterWithProperty>().Count();
      Assert.That(propertiesCount, Is.EqualTo(2));

      var hashersCount = session.Query.All<HashAlgorithm>().Count();
      Assert.That(hashersCount, Is.EqualTo(1));

      var md5Count = session.Query.All<MD5Hash>().Count();
      Assert.That(md5Count, Is.EqualTo(1));

      var providersCount = session.Query.All<ProviderInfo>().Count();
      Assert.That(providersCount, Is.EqualTo(3));

      var buildInProvidersCount = session.Query.All<BuildInProviderInfo>().Count();
      Assert.That(buildInProvidersCount, Is.EqualTo(1));
      var buildInProvider = session.Query.All<BuildInProviderInfo>().First();
      Assert.That(buildInProvider.Name, Is.EqualTo("SimpleBuildInProvider"));

      var oauthProvidersCount = session.Query.All<OAuthProviderInfo>().Count();
      Assert.That(oauthProvidersCount, Is.EqualTo(1));

      var googleProvidersCount = session.Query.All<GoogleOAuthProvider>().Count();
      Assert.That(googleProvidersCount, Is.EqualTo(1));

      var openIdProvidersCount = session.Query.All<OpenIdProviderInfo>().Count();
      Assert.That(openIdProvidersCount, Is.EqualTo(1));

      var aolProvidersCount = session.Query.All<AolOpenIdProviderInfo>().Count();
      Assert.That(aolProvidersCount, Is.EqualTo(1));

      var personsCount = session.Query.All<Person>().Count();
      var usersCount = session.Query.All<User>().Count();
      Assert.That(personsCount, Is.EqualTo(names.Length));
      Assert.That(usersCount, Is.EqualTo(emails.Length));
      Assert.That(usersCount, Is.EqualTo(personsCount));
      foreach (var name in names) {
        var firstName = name.Split(' ')[0];
        var lastName = name.Split(' ')[1];
        var person = session.Query.All<Person>().FirstOrDefault(el => el.FirstName==firstName && el.LastName==lastName);
        Assert.That(person, Is.Not.Null);
        Assert.That(person.User, Is.Not.Null);
        Assert.That(emails.Contains(person.User.Email), Is.True);
        Assert.That(person.User.AuthorizationInfos.Count(), Is.EqualTo(1));
      }

      var authorizationInfosCount = session.Query.All<AuthorizationInfo>().Count();
      Assert.That(authorizationInfosCount, Is.EqualTo(3));

      var xCount = session.Query.All<XType>().Count();
      Assert.That(xCount, Is.EqualTo(1));
      var x = session.Query.All<XType>().First();
      Assert.That(x.FBool, Is.True);
      Assert.That(x.FByte, Is.EqualTo(1));
      Assert.That(x.FSByte, Is.EqualTo(1));
      Assert.That(x.FShort, Is.EqualTo(1));
      Assert.That(x.FUShort, Is.EqualTo(1));
      Assert.That(x.FInt, Is.EqualTo(1));
      Assert.That(x.FUInt, Is.EqualTo(1));
      Assert.That(x.FLong, Is.EqualTo(1));
      Assert.That(x.FULong, Is.EqualTo(1));
      Assert.That(x.FFloat, Is.EqualTo(1.0f));
      Assert.That(x.FDouble, Is.EqualTo(1.0d));
      Assert.That(x.FDecimal, Is.EqualTo(1));
      Assert.That(x.FDateTime, Is.EqualTo(new DateTime(2001, 1, 1, 1, 1, 1)));
      Assert.That(x.FTimeSpan, Is.EqualTo(new TimeSpan(1, 1, 1, 1)));
      Assert.That(x.FString, Is.EqualTo(1.ToString()));
      Assert.That(x.FLongString, Is.EqualTo(1.ToString()));
      Assert.That(x.FEByte, Is.EqualTo(EByte.Min));
      Assert.That(x.FESByte, Is.EqualTo(ESByte.Min));
      Assert.That(x.FEShort, Is.EqualTo(EShort.Min));
      Assert.That(x.FEUShort, Is.EqualTo(EUShort.Min));
      Assert.That(x.FEInt, Is.EqualTo(EInt.Min));
      Assert.That(x.FEUInt, Is.EqualTo(EUInt.Min));
      Assert.That(x.FELong, Is.EqualTo(ELong.Min));
      Assert.That(x.FEULong, Is.EqualTo(EULong.Min));

      Assert.That(x.FNBool, Is.EqualTo(true));
      Assert.That(x.FNByte, Is.EqualTo(1));
      Assert.That(x.FNSByte, Is.EqualTo(1));
      Assert.That(x.FNShort, Is.EqualTo(1));
      Assert.That(x.FNUShort, Is.EqualTo(1));
      Assert.That(x.FNInt, Is.EqualTo(1));
      Assert.That(x.FNUInt, Is.EqualTo(1));
      Assert.That(x.FNLong, Is.EqualTo(1));
      Assert.That(x.FNULong, Is.EqualTo(1));
      Assert.That(x.FNFloat, Is.EqualTo(1.0f));
      Assert.That(x.FNDouble, Is.EqualTo(1.0d));
      Assert.That(x.FNDecimal, Is.EqualTo(1));
      Assert.That(x.FNDateTime, Is.EqualTo(new DateTime(2001, 1, 1, 1, 1, 1)));
      Assert.That(x.FNTimeSpan, Is.EqualTo(new TimeSpan(1, 1, 1, 1)));
      Assert.That(x.FNEByte, Is.EqualTo(EByte.Min));
      Assert.That(x.FNESByte, Is.EqualTo(ESByte.Min));
      Assert.That(x.FNEShort, Is.EqualTo(EShort.Min));
      Assert.That(x.FNEUShort, Is.EqualTo(EUShort.Min));
      Assert.That(x.FNEInt, Is.EqualTo(EInt.Min));
      Assert.That(x.FNEUInt, Is.EqualTo(EUInt.Min));
      Assert.That(x.FNELong, Is.EqualTo(ELong.Min));
      Assert.That(x.FNEULong, Is.EqualTo(EULong.Min));
      Assert.That(x.Ref, Is.Not.Null);

      for (int i = 0; i < 200; i++)
        new Country {Value = string.Format("Country{0}", i)};
    }

    private void TestSecondModel(Session session)
    {
      var manufacturersCount = session.Query.All<Manufacturer>().Count();
      Assert.That(manufacturersCount, Is.EqualTo(10));
      var amd = session.Query.All<Manufacturer>().First(el => el.Name=="AMD");
      var acer = session.Query.All<Manufacturer>().First(el => el.Name=="Acer");
      var intel = session.Query.All<Manufacturer>().First(el => el.Name=="Intel");
      var samsung = session.Query.All<Manufacturer>().First(el => el.Name=="Samsung");
      var seagate = session.Query.All<Manufacturer>().First(el => el.Name=="Seagate");
      var msi = session.Query.All<Manufacturer>().First(el => el.Name=="MSI");
      var gigabyte = session.Query.All<Manufacturer>().First(el => el.Name=="Gigabyte");
      var firstdisplayproducer = session.Query.All<Manufacturer>().First(el => el.Name=="FirstDisplayProducer");
      var seconddisplayproducer = session.Query.All<Manufacturer>().First(el => el.Name=="SecondDisplayProducer");

      var laptopCount = session.Query.All<Laptop>().Count();
      Assert.That(laptopCount, Is.EqualTo(2));
      var displaysCount = session.Query.All<DisplayInfo>().Count();
      Assert.That(displaysCount, Is.EqualTo(2));

      var cpusCount = session.Query.All<CPUInfo>().Count();
      Assert.That(cpusCount, Is.EqualTo(2));

      var storagesCount = session.Query.All<StorageInfo>().Count();
      Assert.That(storagesCount, Is.EqualTo(2));

      var graphicsCardsCount = session.Query.All<GraphicsCardInfo>().Count();
      Assert.That(graphicsCardsCount, Is.EqualTo(2));

      var keyboardsCount = session.Query.All<KeyboardInfo>().Count();
      Assert.That(keyboardsCount, Is.EqualTo(2));

      var ioPostsInfosCount = session.Query.All<IOPortsInfo>().Count();
      Assert.That(ioPostsInfosCount, Is.EqualTo(2));

      for (int i = 0; i < 200; i++)
        new Manufacturer {Name = string.Format("Manufacturer{0}", i)};
    }
  }

  public sealed class MultidatabaseDomainWithoutForeignKeysTest : MultidatabaseDomainTest
  {
    protected override ForeignKeyMode GetForeignKeyMode()
    {
      return ForeignKeyMode.None;
    }
  }

  public sealed class MultidatabaseDomainHierarchyForeignKeysTest : MultidatabaseDomainTest
  {
    protected override ForeignKeyMode GetForeignKeyMode()
    {
      return ForeignKeyMode.Hierarchy;
    }
  }

  public sealed class MultidatabaseDomainReferenceForeignKeysTest : MultidatabaseDomainTest
  {
    protected override ForeignKeyMode GetForeignKeyMode()
    {
      return ForeignKeyMode.Reference;
    }
  }

  public sealed class MultidatabaseDomainAllForeignKeysTest : MultidatabaseDomainTest
  {
    protected override ForeignKeyMode GetForeignKeyMode()
    {
      return ForeignKeyMode.All;
    }
  }
}