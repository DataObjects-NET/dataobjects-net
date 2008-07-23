// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.06.03

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Collections;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Helpers;
using Xtensive.Core.Testing;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Tests.Storage.Vehicles.Debug;
using FieldInfo=Xtensive.Storage.Model.FieldInfo;

namespace Xtensive.Storage.Tests.Storage.Vehicles
{
  [TestFixture]
  public class VehicleTest
  {
    private Random random = RandomManager.CreateRandom();

    private const int CarrierCount = 30;
    private const int CompanyCount = 1;
    private const int DivisionCount = 2;
    private const int FleetCount = 3;
    private const int VehiclesCount = 10;
    private Bitmap companyLogo = ((Bitmap) Image.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Storage\Vehicles\Other\Logo.jpg")));


    [Test]
    [Explicit]
    [Category("Sql")]
    public void QueryIndexCount()
    {
      Domain domain = GetSqlStorage();
      using (var sessionScope = domain.OpenSession()) {
        Session session = sessionScope.Session;
        TypeInfo vehicleType = domain.Model.Types[typeof (Vehicle)];
        IndexInfo primaryIndex = vehicleType.Indexes.PrimaryIndex;
        RecordSet recordSet = session.Select(primaryIndex);
        long count = recordSet.Provider.GetService<ICountable>(true).Count;
        Assert.IsTrue(count >= VehiclesCount);
        LogTemplate<Log>.Info("Received {0} records", count);
      }
    }

    [Test]
    public void SingleTablePlacementPolicyTest()
    {
      // AK, please fix indexbuilder. AG
      DomainConfiguration configuration = GetConfiguration();
      configuration.Builders.Add(typeof (VehicleSingleTableBuilder));
      Domain domain = Domain.Build(configuration);
      domain.Model.Dump();
      Assert.IsNotNull(domain.Model.Types[typeof (Fleet)].Columns["Name"]);
      Assert.IsTrue(domain.Model.Types[typeof (Fleet)].AffectedIndexes[0].Columns.Any(columnInfo => columnInfo.Name=="Name"));
    }

    [Test]
    public void ConcreteTablePlacementPolicyTest()
    {
      // AK, please fix indexbuilder. AG
      DomainConfiguration configuration = GetConfiguration();
      configuration.Builders.Add(typeof (VehicleConcreteTableBuilder));
      Domain domain = Domain.Build(configuration);
      Assert.IsNotNull(domain.Model.Types[typeof (Fleet)].Columns["Name"]);
      Assert.IsTrue(domain.Model.Types[typeof (Fleet)].AffectedIndexes[0].Columns.Any(columnInfo => columnInfo.Name=="Name"));
    }

    [Test]
    public void Test()
    {
      Domain domain = GetStorage();
      TestInternal(domain, 1);
    }

    [Test]
    [Explicit, Category("NotFinishedYet")]
    public void TestSql()
    {
      Domain domain = GetSqlStorage();
      TestInternal(domain, 10);
    } 

    [Test]
    [Explicit, Category("Debug")]
    public void DebugSql()
    {
      Domain domain = GetSqlStorage("Xtensive.Storage.Tests.Storage.Vehicles.Debug");
      Key truckKey;
      Key carKey;
      Key fleetKey;
      const int truckSize = 123;
      const string truckCode = "truck_code";
      const string truckName = "truck_Name";
      const string carCode = "car_code";
      const string carName = "car_Name";
      const string carBodyType = "BodyType";
      const string fleetCode = "fleetCode";
      const string fleetName = "fleetName";
      using (domain.OpenSession()) {
        DebugTruckFleet truckFleet = new DebugTruckFleet();
        truckFleet.Code = truckCode;
        truckFleet.Name = truckName;
        truckFleet.Size = truckSize;
        truckFleet.DivisionId = 1;
        truckKey = truckFleet.Key;
        DebugCarFleet carFleet = new DebugCarFleet();
        carFleet.Code = carCode;
        carFleet.Name = carName;
        carFleet.DivisionId = 2;
        carFleet.BodyType = carBodyType;
        carKey = carFleet.Key;
        DebugFleet fleet = new DebugFleet();
        fleet.Code = fleetCode;
        fleet.Name = fleetName;
        fleet.DivisionId = 3;
        fleetKey = fleet.Key;
      }
      using (domain.OpenSession()) {
        DebugFleet truckFleetBase = truckKey.Resolve<DebugFleet>();
        DebugTruckFleet truckFleet = truckKey.Resolve<DebugTruckFleet>();
        Assert.AreEqual(truckFleet, truckFleetBase);
        Assert.IsTrue(ReferenceEquals(truckFleet, truckFleetBase));
        Assert.AreEqual(truckFleet.Code, truckCode);
        Assert.AreEqual(truckFleet.Name, truckName);
        Assert.AreEqual(truckFleet.Size, truckSize);
        Assert.AreEqual(truckFleet.DivisionId, 1);
        DebugCarFleet carFleet = carKey.Resolve<DebugCarFleet>();
        Assert.AreEqual(carFleet.Code, carCode);
        Assert.AreEqual(carFleet.Name, carName);
        Assert.AreEqual(carFleet.BodyType, carBodyType);
        Assert.AreEqual(carFleet.DivisionId, 2);
        DebugFleet fleet = fleetKey.Resolve<DebugFleet>();
        Assert.AreEqual(fleet.Code, fleetCode);
        Assert.AreEqual(fleet.Name, fleetName);
        Assert.AreEqual(fleet.DivisionId, 3);
      }
      using (domain.OpenSession()) {
        List<DebugCarFleet> carFleets = Session.Current.All<DebugCarFleet>().ToList();
        Assert.AreEqual(carFleets.Count, 1);
        Assert.AreEqual(carFleets[0].Key, carKey);
        Assert.IsTrue(ReferenceEquals(carFleets[0].Key, carKey));
        List<DebugTruckFleet> truckFleets = Session.Current.All<DebugTruckFleet>().ToList();
        Assert.AreEqual(truckFleets.Count, 1);
        Assert.AreEqual(truckFleets[0].Key, truckKey);
        Assert.IsTrue(ReferenceEquals(truckFleets[0].Key, truckKey));
        List<DebugFleet> fleets = Session.Current.All<DebugFleet>().ToList();
        Assert.AreEqual(fleets.Count, 3);
        Assert.AreEqual(fleets.FindAll(fleet => typeof (DebugFleet)==fleet.GetType()).Count, 1);
        Assert.AreEqual(fleets.FindAll(fleet => typeof (DebugTruckFleet)==fleet.GetType()).Count, 1);
        Assert.AreEqual(fleets.FindAll(fleet => typeof (DebugCarFleet)==fleet.GetType()).Count, 1);
      }
      using (domain.OpenSession()) {
        List<DebugFleet> fleets = Session.Current.All<DebugFleet>().ToList();
        while (fleets.Count > 0) {
          fleets[0].Remove();
          fleets = Session.Current.All<DebugFleet>().ToList();
        }
      }
    }

    private void TestInternal(Domain domain, int factor)
    {
      ValidateModel(domain.Model);
      int itemsCount = factor * CarrierCount
        + factor * CompanyCount
          + (factor * CompanyCount) * (factor * DivisionCount)
            + (factor * CompanyCount) * (factor * DivisionCount) * (factor * FleetCount)
              + (factor * CompanyCount) * (factor * DivisionCount) * (factor * FleetCount) * (factor * VehiclesCount);
      using (new Measurement("Item fill", itemsCount)) {
        FillStorage(domain, factor);
      }
      ValidateData(domain, factor);
      UpdateExistingData(domain);
      // DeleteExistingData(storage);
      // ValidateBehaviour(storage);
    }

    private void DeleteExistingData(Domain domain)
    {
      using (SessionScope sessionScope = domain.OpenSession()) {
        Session session = sessionScope.Session;
        foreach (Carrier carrier in session.All<Carrier>()) {
          carrier.Remove();
        }
        foreach (Company company in session.All<Company>()) {
          company.Remove();
        }
        session.Persist();
        Assert.AreEqual(0, session.All<Carrier>().Count());
        Assert.AreEqual(0, session.All<Company>().Count());
      }
      using (SessionScope sessionScope = domain.OpenSession()) {
        Session session = sessionScope.Session;
        Assert.AreEqual(0, session.All<Carrier>().Count());
        Assert.AreEqual(0, session.All<Company>().Count());
        session.Persist();
      }
    }

    [Test]
    [Explicit, Category("Debug")]
    public void Debug()
    {
      Domain domain = GetStorage();
      ValidateModel(domain.Model);
      const int factor = 1;
      FillStorage(domain, factor);
      ValidateData(domain, factor);
      ValidateBehaviour(domain);
    }

    [Test]
    [Explicit, Category("Debug")]
    public void BuildSqlStorage()
    {
      GetSqlStorage();
    }

    private void UpdateExistingData(Domain domain)
    {
      using (SessionScope sessionScope = domain.OpenSession()) {
        Session session = sessionScope.Session;
        foreach (Company company in session.All<Company>()) {
          FillCompany(company);
        }
        foreach (Carrier carrier in session.All<Carrier>()) {
          FillCarrier(carrier);
        }
        session.Persist();
      }
    }

    private void ValidateBehaviour(Domain domain)
    {
      using (SessionScope sessionScope = domain.OpenSession()) {
        Session session = sessionScope.Session;
        // Structures
        CompanyAddress ca1 = new CompanyAddress();
        CompanyAddress caEmpty = new CompanyAddress();
        Assert.AreEqual(ca1, caEmpty);
        Assert.IsNull(ca1.Fax);
        AssertEx.Throws<ArgumentException>(() => ca1.Fax = null);
        ca1.Fax = "fax";
        Assert.AreNotEqual(ca1, caEmpty);
        Assert.IsNotNull(ca1.Fax);
//        ca1.Clear();
//        Assert.AreEqual(ca1, caEmpty);
//        Assert.IsNull(ca1.Fax);
//        Assert.AreEqual(ca1.GetHashCode(), caEmpty.GetHashCode());

        Company company = new Company();
        Assert.AreEqual(company.Address, caEmpty);
        company.Address = caEmpty;
        Assert.AreEqual(company.Address, caEmpty);
        company.Address.Fax = "fax";
        Assert.AreNotEqual(company.Address, caEmpty);
//        Assert.IsFalse(company.Address.IsEmpty);
//        Assert.IsTrue(caEmpty.IsEmpty);
        Country country = new Country("TestCountry");

        //Length excess
        AssertEx.Throws<ArgumentException>(() => country.Flag = companyLogo);

        // Empty EntytyRef
        // Country currentCountry = new Carrier().Address.Country;
      }
    }

    private void ValidateData(Domain domain, int factor)
    {
      using (SessionScope sessionScope = domain.OpenSession()) {
        Session session = sessionScope.Session;
        foreach (Company company in session.All<Company>()) {
          // Get/set properties
          Assert.AreEqual(company.GetProperty<string>("Name"), company.Name);
          Assert.AreEqual(company.GetProperty<string>("Name"), company["Name"]);
          company.SetProperty("Name", company.Name + "_test");
          Assert.AreEqual(company.GetProperty<string>("Name"), company.Name);
          Assert.AreEqual(company.GetProperty<string>("WebSite"), company.WebSiteProperty);
          company["WebSite"] = "http://" + company["WebSite"];
          Assert.AreEqual(company.GetProperty<string>("WebSite"), company.WebSiteProperty);
          Logo logo = company.Logo;
//          Assert.AreEqual(logo.Image.Height, logo.Height);
//          Assert.AreEqual(logo.Image.Width, logo.Width);

          // Structures
          AssertEx.Throws<ArgumentNullException>(() => company.Address = null);
          CompanyAddress emptyAddress = new CompanyAddress();
          Assert.AreNotEqual(emptyAddress, company.Address);
//          Assert.IsTrue(emptyAddress.IsEmpty);
//          Assert.IsFalse(company.Address.IsEmpty);
//          company.Address.Clear();
//          Assert.AreEqual(emptyAddress, company.Address);
//          Assert.IsTrue(company.Address.IsEmpty);
        }
      }
    }

    private void FillStorage(Domain domain, int factor)
    {
      int carrierCount = factor * CarrierCount;
      int companyCount = factor * CompanyCount;
      int divisionCount = factor * DivisionCount;
      int fleetCount = factor * FleetCount;
      int vehiclesCount = factor * VehiclesCount;
      using (domain.OpenSession()) {
        // Create countries
        GetCountry("Russia");
        GetCountry("USA");

        for (int carrierIdx = 0; carrierIdx < carrierCount; carrierIdx++) {
          CreateCarrier();
        }
        for (int companyIdx = 0; companyIdx < companyCount; companyIdx++) {
          Company company = CreateCompany();
          for (int divisionIdx = 0; divisionIdx < divisionCount; divisionIdx++) {
            Division division = CreateDivision(company);
            for (int fleetIdx = 0; fleetIdx < fleetCount; fleetIdx++) {
              Fleet fleet = CreateFleet(division);
              for (int vehicleIdx = 0; vehicleIdx < vehiclesCount; vehicleIdx++) {
                CreateVehicle(fleet);
              }
            }
          }
        }
      }
    }

    private void CreateVehicle(Fleet fleet)
    {
      bool isTruck = fleet is TruckFleet;
      Vehicle vehicle = isTruck ? (Vehicle) CreateTruck() : CreateCar();
      vehicle.Fleet = fleet;
    }

    private Car CreateCar()
    {
      Car result;
      if (random.NextDouble() < 0.5) {
        result = new Bmw();
        result.Model = "Bmw_" + random.Next();
      }
      else {
        result = new Opel();
        result.Model = "Opel_" + random.Next();
      }
      return result;
    }

    private Truck CreateTruck()
    {
      Truck result;
      if (random.NextDouble() < 0.5) {
        result = new Volvo();
        result.Model = "Volvo_" + random.Next();
      }
      else {
        result = new Kamaz();
        result.Model = "Kamaz_" + random.Next();
      }
      return result;
    }

    private Fleet
      CreateFleet(Division division)
    {
      // Create common fleet part
      bool isTruckFleet = random.NextDouble() < 0.5;
      Fleet fleet = isTruckFleet ? (Fleet) new TruckFleet() : new CarFleet();
      fleet.Division = division;
      if (isTruckFleet) {
        fleet.Name = "TruckFleet_" + division.Name + "_" + random.Next();
        ((TruckFleet) fleet).Type = random.NextDouble() < 0.5 ? TruckFleetType.Foreign : TruckFleetType.Local;
      }
      else {
        fleet.Name = "CarFleet_" + division.Name + "_" + random.Next();
      }
      return fleet;
    }

    private Division CreateDivision(Company company)
    {
      Division division = new Division
        {
          Company = company,
          Name = "Division_" + company.Name + "_" + random.Next(),
          Number = random.Next(),
        };
      return division;
    }

    private Company CreateCompany()
    {
      Company company = new Company();
      FillCompany(company);
      return company;
    }

    private void FillCompany(Company company)
    {
      company.Name = ("Company_" + random.Next());
      company.Logo = new Logo(companyLogo);
      company.RegNumber = random.Next();
      company.WebSiteProperty = "www." + company.Name + ".com";
      CompanyAddress address = new CompanyAddress
        {
          City = (company.Name + "_Address_City"),
          Country = GetCountry("France"),
          Street = (company.Name + "_Address_Street"),
          Zip = (company.Name + "_Address_Zip"),
          Email = (company.Name + "_Address_Email"),
          Fax = (company.Name + "_Address_Fax"),
          MailAddress = (company.Name + "_Address_MailAddress"),
          Phone = (company.Name + "_Address_Phone")
        };
      company.Address = address;
    }

    private Carrier CreateCarrier()
    {
      Carrier carrier = new Carrier();
      FillCarrier(carrier);
      return carrier;
    }

    private void FillCarrier(Carrier carrier)
    {
      Country country = GetCountry("Russia");
      carrier.Name = "Carrier_" + random.Next();
      carrier.Address.Phone = carrier.Name + "_Address_Phone";
      carrier.Address.Fax = carrier.Name + "_Address_Fax";
      carrier.Address.Zip = carrier.Name + "_Address_Zip";
      carrier.Address.Street = carrier.Name + "_Address_Street";
      carrier.Address.MailAddress = carrier.Name + "_Address_MailAddress";
      carrier.Address.City = carrier.Name + "_Address_City";
      carrier.Address.Country = country;
      carrier.Address.HeadquaterAddress = carrier.Address;
      carrier.Address.HeadquaterAddress.City += "_HeadQuater";
      carrier.Address.HeadquaterAddress.Country = GetCountry("USA");
      carrier.Address.HeadquaterAddress.Street += "_HeadQuater";
      carrier.Address.HeadquaterAddress.Zip += "_HeadQuater";
    }

    private Country GetCountry(string countryName)
    {
      Session session = SessionScope.Current.Session;
      Key key = Key.Get(typeof (Country), Tuple.Create<string>(countryName));
      Country result = key.Resolve<Country>();
      if (result == null) {
        result = new Country(countryName);
        result.Code = countryName.Substring(0, 3).ToUpper();
        result.OptionalData = new byte[10];
      }
      return result;
    }

    private void ValidateModel(DomainInfo model)
    {
      // Check for missing nested structures.
      foreach (TypeInfo type in model.Types) {
        foreach (FieldInfo field in type.Fields) {
          Assert.IsFalse(field.IsStructure && field.IsNested);
        }
      }

      // Company
      TypeInfo company = model.Types[typeof (Company)];
      Assert.IsNotNull(company);
//      Assert.AreEqual(7 + 2 + model.Types[typeof (CompanyAddress)].Fields.Count + model.Types[typeof (Logo)].Fields.Count, company.Fields.Count);
      // 6 fields + TypeInfo + Id
//      AssertFieldsPresent(company, "WebSite", "Name", "RegNumber", "Divisions", "Carriers", "Address", "Logo");
      foreach (FieldInfo info in model.Types[typeof (CompanyAddress)].Fields) {
        Assert.IsTrue(company.Fields["Address." + info.Name]!=null);
      }
      Assert.IsTrue(company.Fields["WebSite"].ValueType==typeof (string));      

      // TruckFleet
      TypeInfo truckFleet = model.Types[typeof (TruckFleet)];
      Assert.IsNotNull(truckFleet);
      AssertFieldsPresent(truckFleet, "Type", "Division", "Division.Id", "Name", "Code", "Vehicles");
      Assert.AreEqual(6 + 2, truckFleet.Fields.Count); // 6 fields + TypeInfo + Id

      // Volvo
      TypeInfo volvo = model.Types[typeof (Volvo)];
      Assert.IsNotNull(volvo);
      AssertFieldsPresent(volvo, "Weight", "Power", "Length", "Fleet", "Fleet.Id", "Model");
      Assert.AreEqual(6 + 4, volvo.Fields.Count); // 6 fields + TypeInfo + (PK: Id + Guid + Long)
    }

    private void AssertFieldsPresent(TypeInfo persistent, params string[] names)
    {
      foreach (string name in names) {
        Assert.IsTrue(persistent.Fields[name]!=null);
      }
    }

    private Domain GetStorage()
    {
      return Domain.Build(GetConfiguration());
    }

    private DomainConfiguration GetConfiguration()
    {
      DomainConfiguration configuration = new DomainConfiguration("memory://localhost/Vehicles");
      configuration.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Storage.Vehicles");
      configuration.Builders.Add(typeof (VehiclesBuilder));
      return configuration;
    }

    private Domain GetSqlStorage(string nameSpace)
    {
      DomainConfiguration configuration = new DomainConfiguration("mssql2005://localhost/Vehicles"); // TODO: real address.
      configuration.Types.Register(Assembly.GetExecutingAssembly(), nameSpace);
      configuration.Builders.Add(typeof (VehiclesBuilder));
      return Domain.Build(configuration);
    }

    private Domain GetSqlStorage()
    {
      return GetSqlStorage("Xtensive.Storage.Tests.Storage.Vehicles");
    }
  }
}