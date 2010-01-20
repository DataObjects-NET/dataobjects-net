// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.16

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Storage.Disconnected;
using Xtensive.Storage.ObjectMapping;
using Xtensive.Storage.Tests.Storage.ObjectMapping.Model;

namespace Xtensive.Storage.Tests.Storage.ObjectMapping
{
  [TestFixture]
  public sealed class MapperTest : AutoBuildTest
  {
    protected override Xtensive.Storage.Configuration.DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.Register(typeof(Person).Assembly, typeof(Supplier).Namespace);
      return config;
    }

    [Test]
    public void SimpleEntitiesMappingTest()
    {
      Mapper mapper;
      var productDto = ServerCreateDtoGraphForSimpleEntitiesMappingTest(out mapper);

      var modifiedProductDto = (PersonalProductDto) productDto.Clone();
      var productNewName = modifiedProductDto.Name + "!!!";
      modifiedProductDto.Name = productNewName;
      const string newEmployeeName = "NewEmployee";
      const int newEmployeeAge = 26;
      const string newEmployeePosition = "F";
      modifiedProductDto.Employee = new EmployeeDto {
        Age = newEmployeeAge, Name = newEmployeeName,
        Position = newEmployeePosition, Key = Guid.NewGuid().ToString()
      };

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var modificationSet = mapper.Compare(productDto, modifiedProductDto).Operations;
        modificationSet.Apply();
        tx.Complete();
      }

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var product = Query.All<PersonalProduct>().Single();
        Assert.AreEqual(1, Query.All<Employee>().Count());
        Assert.AreEqual(productNewName, product.Name);
        Assert.AreNotEqual(productDto.Employee.Key, product.Employee.Key.Format());
        Assert.AreEqual(newEmployeeAge, product.Employee.Age);
        Assert.AreEqual(newEmployeeName, product.Employee.Name);
        Assert.AreEqual(newEmployeePosition, product.Employee.Position);
      }
    }

    [Test]
    public void CollectionMappingTest()
    {
      Mapper mapper;
      var publisherDto = ServerCreateDtoGraphForCollectionMappingTest(out mapper);

      var removedBookShop = publisherDto.Distributors.First();
      var modifiedPublisherDto = Clone(publisherDto);
      var newBookShop0 = new BookShopDto {Key = Guid.NewGuid().ToString(), Name = "NB0"};
      var newBookShop1 = new BookShopDto {
        Key = Guid.NewGuid().ToString(), Name = "NB1", Suppliers = new[] {modifiedPublisherDto}
      };
      modifiedPublisherDto.Distributors.Add(newBookShop0);
      modifiedPublisherDto.Distributors.Add(newBookShop1);
      Assert.AreEqual(1, modifiedPublisherDto.Distributors.RemoveWhere(b => b.Key==removedBookShop.Key));
      Assert.IsNotNull(modifiedPublisherDto);

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var modifications = mapper.Compare(publisherDto, modifiedPublisherDto).Operations;
        modifications.Apply();
        tx.Complete();
      }

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var publisher = Query.All<Publisher>().Single();
        Assert.AreEqual(4, Query.All<BookShop>().Count());
        Assert.AreEqual(modifiedPublisherDto.Key, publisher.Key.Format());
        Assert.AreEqual(modifiedPublisherDto.Country, publisher.Country);
        var expectedBookShops = modifiedPublisherDto.Distributors.ToDictionary(d => d.Name, d => d);
        Assert.AreEqual(expectedBookShops.Count, publisher.Distributors.Count);
        foreach (var bookShop in publisher.Distributors.AsEnumerable().Cast<BookShop>()) {
          var expectedBookShop = expectedBookShops[bookShop.Name];
          if (expectedBookShop.Key != newBookShop0.Key && expectedBookShop.Key != newBookShop1.Key)
            Assert.AreEqual(expectedBookShop.Key, bookShop.Key.Format());
          if (expectedBookShop.Key != newBookShop0.Key) {
            Assert.AreEqual(1, expectedBookShop.Suppliers.Length);
            Assert.AreEqual(expectedBookShop.Suppliers[0].Key, bookShop.Suppliers.Single().Key.Format());
          }
          Assert.AreEqual(expectedBookShop.Url, bookShop.Url);
        }
      }
    }

    [Test]
    public void CustomEntitySetMappingTest()
    {
      Mapper mapper;
      var bookShopDto = ServerCreateDtoGraphForCustomEntitySetMappingTest(out mapper);

      Assert.IsNotNull(bookShopDto);
      Assert.IsTrue(bookShopDto.Suppliers.Any(s => s.Trademark=="A"));
      Assert.IsTrue(bookShopDto.Suppliers.Any(s => s.Trademark=="B"));
      Assert.IsTrue(bookShopDto.Suppliers.Any(s => s.Trademark=="C"));
      var modifiedBookShopDto = Clone(bookShopDto);
      var newPublisherDto = new PublisherDto {Key = Guid.NewGuid().ToString(), Trademark = "D"};
      var newSuppliers = new PublisherDto[4];
      Array.Copy(modifiedBookShopDto.Suppliers, newSuppliers, modifiedBookShopDto.Suppliers.Length);
      modifiedBookShopDto.Suppliers = newSuppliers;
      modifiedBookShopDto.Suppliers[3] = newPublisherDto;

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var operations = mapper.Compare(bookShopDto, modifiedBookShopDto).Operations;
        operations.Apply();
        var newPublisher = Query.All<Publisher>().Where(p => p.Trademark==newPublisherDto.Trademark).Single();
        Assert.AreEqual("D", newPublisher.Trademark);
        var bookShop = Query.All<AnotherBookShop>().Single();
        Assert.AreEqual(4, bookShop.Suppliers.Count());
        tx.Complete();
      }
    }

    [Test]
    public void StructureMappingTest()
    {
      ApartmentDto originalApartmentDto0;
      ApartmentDto originalApartmentDto1;
      Key apartment0Key;
      Key apartment1Key;
      Mapper mapper;
      ServerCreateDtoGraphForStructureMappingTest(out originalApartmentDto0, out originalApartmentDto1,
        out apartment0Key, out apartment1Key, out mapper);

      var modifiedApartmentDto0 = Clone(originalApartmentDto0);
      var newDescription0 = new ApartmentDescriptionDto {
        Area = modifiedApartmentDto0.Description.Area,
        RentalFee = modifiedApartmentDto0.Description.RentalFee + 10,
        Manager = modifiedApartmentDto0.Description.Manager
      };
      modifiedApartmentDto0.Description = newDescription0;
      newDescription0.Manager.Name += "Modified0";
      var modifiedApartmentDto1 = Clone(originalApartmentDto1);
      var currentAddress = originalApartmentDto1.Address;
      modifiedApartmentDto1.Address = new AddressDto {Building = currentAddress.Building,
        City = currentAddress.City + "Modified1", Country = currentAddress.Country,
        Office = currentAddress.Office, Street = currentAddress.Street};

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var operations = mapper.Compare(new[] {originalApartmentDto0, originalApartmentDto1},
          new[] {modifiedApartmentDto1, modifiedApartmentDto0}).Operations;
        operations.Apply();
        var apartment0 = Query.Single<Apartment>(apartment0Key);
        ValidateApartment(modifiedApartmentDto0, apartment0);
        Assert.AreEqual(modifiedApartmentDto0.Description.Manager.Key,
          apartment0.Description.Manager.Key.Format());
        Assert.AreEqual(modifiedApartmentDto0.Description.Manager.Name,
          apartment0.Description.Manager.Name);
        var apartment1 = Query.Single<Apartment>(apartment1Key);
        ValidateApartment(modifiedApartmentDto1, apartment1);
      }
    }

    [Test]
    public void NewObjectKeysMappingTest()
    {
      Mapper mapper;
      var original = ServerCreateDtoGraphForKeysMappingTest(out mapper);

      var modified = Clone(original);
      var personDto2 = new SimplePersonDto {Key = Guid.NewGuid().ToString(), Name = "Person2"};
      modified.Add(personDto2);
      var personDto3 = new SimplePersonDto {Key = Guid.NewGuid().ToString(), Name = "Person3"};
      modified.Add(personDto3);

      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var comparisonResult = mapper.Compare(original, modified);
        var keyMapping = comparisonResult.KeyMapping;
        Assert.AreEqual(2, keyMapping.Count);
        var person2RealKey = Key.Parse(Domain, (string) keyMapping[personDto2.Key]);
        var person3RealKey = Key.Parse(Domain, (string) keyMapping[personDto3.Key]);
        comparisonResult.Operations.Apply();
        Query.Single<SimplePerson>(person2RealKey);
        Query.Single<SimplePerson>(person3RealKey);
        modified.RemoveAt(2);
        modified.RemoveAt(2);
        var personDto4 = new SimplePersonDto {Key = Guid.NewGuid().ToString(), Name = "Person4"};
        modified.Add(personDto4);
        comparisonResult = mapper.Compare(original, modified);
        keyMapping = comparisonResult.KeyMapping;
        Assert.AreEqual(1, keyMapping.Count);
        var person4RealKey = Key.Parse(Domain, (string) keyMapping[personDto4.Key]);
        comparisonResult.Operations.Apply();
        Query.Single<SimplePerson>(person4RealKey);
        Query.Single<SimplePerson>(person2RealKey);
        Query.Single<SimplePerson>(person3RealKey);
        tx.Complete();
      }
    }

    [Test]
    public void OptimisticOfflineLockTest()
    {
      Mapper mapper;
      var original = ServerCreateDtoGraphForOptimisticLockTest(out mapper);
      var modified = ClientModifiyDtoGraph(original);
      ServerApplyChanges(original, modified, mapper);
    }

    private PersonalProductDto ServerCreateDtoGraphForSimpleEntitiesMappingTest(out Mapper mapper)
    {
      PersonalProductDto productDto;
      mapper = new Mapper();
      mapper.MapType<PersonalProduct, PersonalProductDto, string>(p => p.Key.Format(), p => p.Key)
        .MapType<Employee, EmployeeDto, string>(e => e.Key.Format(), e => e.Key).Complete();
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var employee = new Employee {Age = 25, Name = "A", Position = "B"};
        var product = new PersonalProduct {Employee = employee, Name = "C"};
        productDto = (PersonalProductDto) mapper.Transform(product);
        tx.Complete();
      }
      return productDto;
    }

    private PublisherDto ServerCreateDtoGraphForCollectionMappingTest(out Mapper mapper)
    {
      PublisherDto publisherDto;
      mapper = new Mapper();
      mapper.MapType<Publisher, PublisherDto, string>(p => p.Key.Format(), p => p.Key)
        .MapType<BookShop, BookShopDto, string>(b => b.Key.Format(), b => b.Key).Complete();
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var bookShop0 = new BookShop {Name = "B0"};
        var bookShop1 = new BookShop {Name = "B1"};
        var bookShop2 = new BookShop {Name = "B2"};
        var publisher = new Publisher {Country = "ABC"};
        publisher.Distributors.Add(bookShop0);
        publisher.Distributors.Add(bookShop1);
        publisher.Distributors.Add(bookShop2);
        publisherDto = (PublisherDto) mapper.Transform(publisher);
        tx.Complete();
      }
      return publisherDto;
    }

    private BookShopDto ServerCreateDtoGraphForCustomEntitySetMappingTest(out Mapper mapper)
    {
      mapper = new Mapper();
      mapper.MapType<AnotherBookShop, BookShopDto, string>(abs => abs.Key.Format(), bs => bs.Key)
        .Ignore(bs => bs.Name).Ignore(bs => bs.Url)
        .MapType<Publisher, PublisherDto, string>(p => p.Key.Format(), p => p.Key)
        .Ignore(p => p.Distributors).Complete();
      BookShopDto bookShopDto;
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        foreach (var publisher in Query.All<Publisher>())
          publisher.Remove();
        foreach (var bookShop in Query.All<BookShop>())
          bookShop.Remove();
        var anotherBookShop = new AnotherBookShop();
        anotherBookShop.Suppliers.Add(new Publisher {Trademark = "A"});
        anotherBookShop.Suppliers.Add(new Publisher {Trademark = "B"});
        anotherBookShop.Suppliers.Add(new Publisher {Trademark = "C"});
        bookShopDto = (BookShopDto) mapper.Transform(anotherBookShop);
        tx.Complete();
      }
      return bookShopDto;
    }

    private void ServerCreateDtoGraphForStructureMappingTest(out ApartmentDto originalApartmentDto0,
      out ApartmentDto originalApartmentDto1, out Key apartment0Key, out Key apartment1Key, out Mapper mapper)
    {
      mapper = new Mapper();
      mapper.MapType<SimplePerson, SimplePersonDto, string>(p => p.Key.Format(), p => p.Key)
        .MapType<Apartment, ApartmentDto, string>(a => a.Key.Format(), a => a.Key)
        .MapStructure<Address, AddressDto>()
        .MapStructure<ApartmentDescription, ApartmentDescriptionDto>().Complete();
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var person0 = new SimplePerson {Name = "Name0"};
        var address0 = new Address {
          Building = 1, City = "City0", Country = "Country0", Office = 10, Street = "Street0"
        };
        var apartmentDescription0 = new ApartmentDescription {
          Area = 123.57, RentalFee = 10.5, Manager = new SimplePerson {Name = "Manager0"}
        };
        var apartment0 = new Apartment {
          Address = address0, Person = person0, Description = apartmentDescription0
        };
        apartment0Key = apartment0.Key;
        var person1 = new SimplePerson {Name = "Name1"};
        var address1 = new Address {
          Building = 2, City = "City1", Country = "Country1", Office = 11, Street = "Street1"
        };
        var apartment1 = new Apartment {Address = address1, Person = person1};
        apartment1Key = apartment1.Key;
        originalApartmentDto0 = (ApartmentDto) mapper.Transform(apartment0);
        originalApartmentDto1 = (ApartmentDto) mapper.Transform(apartment1);
        tx.Complete();
      }
    }

    private List<object> ServerCreateDtoGraphForKeysMappingTest(out Mapper mapper)
    {
      mapper = new Mapper();
      mapper.MapType<SimplePerson, SimplePersonDto, string>(sp => sp.Key.Format(), sp => sp.Key).Complete();
      List<object> original;
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var person0 = new SimplePerson {Name = "Person0"};
        var person1 = new SimplePerson {Name = "Person1"};
        original = (List<object>) mapper.Transform(new[] {person0, person1});
        tx.Complete();
      }
      return original;
    }

    private List<object> ServerCreateDtoGraphForOptimisticLockTest(out Mapper mapper)
    {
      mapper = new Mapper();
      var formatter = new BinaryFormatter();
      using (var stream = new MemoryStream()) {
        Func<VersionInfo, byte[]> serializer = version => {
          formatter.Serialize(stream, version);
          stream.Seek(0, SeekOrigin.Begin);
          var result = new byte[stream.Length];
          Array.Copy(stream.GetBuffer(), result, result.Length);
          stream.SetLength(0);
          return result;
        };
        mapper
          .MapType<SimplePerson, PersonWithVersionDto, string>(sp => sp.Key.Format(), sp => sp.Key)
          .MapProperty(p => serializer.Invoke(p.VersionInfo), p => p.Version)
          .MapStructure<VersionInfo, VersionInfo>().Complete();
        List<object> original;
        using (var session = Session.Open(Domain))
        using (var tx = Transaction.Open()) {
          var person0 = new SimplePerson {Name = "Person0"};
          var person1 = new SimplePerson {Name = "Person1"};
          original = (List<object>) mapper.Transform(new[] {person0, person1});
          tx.Complete();
        }
        return original;
      }
    }

    private static List<object> ClientModifiyDtoGraph(List<object> original)
    {
      var modified = Clone(original);
      ((PersonWithVersionDto) modified[0]).Name += "Modified0";
      modified.Add(new PersonWithVersionDto {Key = Guid.NewGuid().ToString(), Name = "Person3"});
      return modified;
    }

    private void ServerApplyChanges(List<object> original, List<object> modified, Mapper mapper)
    {
      var formatter = new BinaryFormatter();
      var deserializedVersions = new Dictionary<Key, VersionInfo>();
      Func<Key, VersionInfo> versionProvider = key => {
        VersionInfo result;
        if (!deserializedVersions.TryGetValue(key, out result)) {
          var keyString = key.Format();
          var foundPerson = modified.Cast<PersonWithVersionDto>().Where(m => m.Key==keyString)
            .First();
          using (var stream = new MemoryStream(foundPerson.Version))
            result = (VersionInfo) formatter.Deserialize(stream);
          deserializedVersions.Add(key, result);
        }
        return result;
      };
      using (var session = Session.Open(Domain))
      using (VersionValidator.Attach(session, versionProvider))
      using (var tx = Transaction.Open()) {
        var operations = mapper.Compare(original, modified).Operations;
        operations.Apply();
        tx.Complete();
      }

      // Validation of the stale object.
      ((PersonWithVersionDto) modified[0]).Name += "ModifiedAgain";
      using (var session = Session.Open(Domain))
      using (VersionValidator.Attach(session, versionProvider)) {
        var tx = Transaction.Open();
        try {
          var operations = mapper.Compare(original, modified).Operations;
          operations.Apply();
          tx.Complete();
        }
        finally {
          AssertEx.ThrowsInvalidOperationException(tx.Dispose);
        }
      }
    }

    private static void ValidateApartment(ApartmentDto modifiedApartmentDto, Apartment apartment)
    {
      Assert.AreEqual(modifiedApartmentDto.Person.Key, apartment.Person.Key.Format());
      Assert.AreEqual(modifiedApartmentDto.Address.Building, apartment.Address.Building);
      Assert.AreEqual(modifiedApartmentDto.Address.Country, apartment.Address.Country);
      Assert.AreEqual(modifiedApartmentDto.Address.City, apartment.Address.City);
    }

    private static T Clone<T>(T obj)
    {
      var serializer = new BinaryFormatter();
      using (var stream = new MemoryStream()) {
        serializer.Serialize(stream, obj);
        stream.Seek(0, SeekOrigin.Begin);
        return (T) serializer.Deserialize(stream);
      }
    }
  }
}