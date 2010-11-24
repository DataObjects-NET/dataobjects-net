// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.16

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.ObjectMapping;
using Xtensive.Testing;
using Xtensive.Orm.Disconnected;
using Xtensive.Orm.ObjectMapping;
using Xtensive.Orm.Operations;
using Xtensive.Storage.Providers;
using Xtensive.Orm.Tests.Storage.ObjectMapping.Model;
using GraphComparisonResult = Xtensive.Orm.ObjectMapping.GraphComparisonResult;

namespace Xtensive.Orm.Tests.Storage.ObjectMapping
{
  [TestFixture]
  public sealed class MapperTest : AutoBuildTest
  {
    protected override Xtensive.Orm.Configuration.DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.Register(typeof(Person).Assembly, typeof(Person).Namespace);
      return config;
    }

    [Test]
    public void SimpleEntitiesMappingTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
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

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var modificationSet = mapper.Compare(productDto, modifiedProductDto).Operations;
        modificationSet.Replay();
        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var product = session.Query.All<PersonalProduct>().Single();
        Assert.AreEqual(1, session.Query.All<Employee>().Count());
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
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
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

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var modifications = mapper.Compare(publisherDto, modifiedPublisherDto).Operations;
        modifications.Replay();
        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var publisher = session.Query.All<Publisher>().Single();
        Assert.AreEqual(4, session.Query.All<BookShop>().Count());
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
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
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

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var operations = mapper.Compare(bookShopDto, modifiedBookShopDto).Operations;
        operations.Replay();
        var newPublisher = session.Query.All<Publisher>().Where(p => p.Trademark==newPublisherDto.Trademark).Single();
        Assert.AreEqual("D", newPublisher.Trademark);
        var bookShop = session.Query.All<AnotherBookShop>().Single();
        Assert.AreEqual(4, bookShop.Suppliers.Count());
        tx.Complete();
      }
    }

    [Test]
    public void StructureMappingTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
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

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var operations = mapper.Compare(new[] {originalApartmentDto0, originalApartmentDto1},
          new[] {modifiedApartmentDto1, modifiedApartmentDto0}).Operations;
        operations.Replay();
        var apartment0 = session.Query.Single<Apartment>(apartment0Key);
        ValidateApartment(modifiedApartmentDto0, apartment0);
        Assert.AreEqual(modifiedApartmentDto0.Description.Manager.Key,
          apartment0.Description.Manager.Key.Format());
        Assert.AreEqual(modifiedApartmentDto0.Description.Manager.Name,
          apartment0.Description.Manager.Name);
        var apartment1 = session.Query.Single<Apartment>(apartment1Key);
        ValidateApartment(modifiedApartmentDto1, apartment1);
      }
    }

    [Test]
    public void NewObjectKeysMappingTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
      Mapper mapper;
      var original = ServerCreateDtoGraphForKeysMappingTest(out mapper);

      var modified = Clone(original);
      var personDto2 = new SimplePersonDto {Key = Guid.NewGuid().ToString(), Name = "Person2"};
      modified.Add(personDto2);
      var personDto3 = new SimplePersonDto {Key = Guid.NewGuid().ToString(), Name = "Person3"};
      modified.Add(personDto3);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var comparisonResult = mapper.Compare(original, modified);
        var keyMapping = comparisonResult.KeyMapping;
        Assert.AreEqual(2, keyMapping.Count);
        var person2RealKey = Key.Parse(Domain, (string) keyMapping[personDto2.Key]);
        var person3RealKey = Key.Parse(Domain, (string) keyMapping[personDto3.Key]);
        comparisonResult.Operations.Replay();
        session.Query.Single<SimplePerson>(person2RealKey);
        session.Query.Single<SimplePerson>(person3RealKey);
        modified.RemoveAt(2);
        modified.RemoveAt(2);
        var personDto4 = new SimplePersonDto {Key = Guid.NewGuid().ToString(), Name = "Person4"};
        modified.Add(personDto4);
        comparisonResult = mapper.Compare(original, modified);
        keyMapping = comparisonResult.KeyMapping;
        Assert.AreEqual(1, keyMapping.Count);
        var person4RealKey = Key.Parse(Domain, (string) keyMapping[personDto4.Key]);
        comparisonResult.Operations.Replay();
        session.Query.Single<SimplePerson>(person4RealKey);
        session.Query.Single<SimplePerson>(person2RealKey);
        session.Query.Single<SimplePerson>(person3RealKey);
        tx.Complete();
      }
    }

    [Test]
    public void OptimisticOfflineLockTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
      Mapper mapper;
      var original = ServerCreateDtoGraphForOptimisticLockTest(out mapper);
      var modified = ClientModifiyDtoGraph(original);
      ServerApplyChanges(original, modified, mapper);
    }

    [Test]
    public void CreateObjectUsingCustomPrimitiveKeyValuesTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
      var rnd = new Random();
      var mapping = new MappingBuilder()
        .MapType<CustomPerson, CustomPersonDto, string>(cp => cp.Key.Format(), cp => cp.Key,
          dto => new object[] {dto.Id}).TrackChanges(cp => cp.Id, false).Build();
      var mapper = new Mapper(mapping);
      List<object> originalPersonDtos;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var customPerson0 = new CustomPerson(rnd.Next()) {AuxString = "Auxiliary0", Name = "Name0"};
        var customPerson1 = new CustomPerson(rnd.Next()) {AuxString = "Auxiliary1", Name = "Name1"};
        originalPersonDtos = (List<object>) mapper.Transform(new[] {customPerson0, customPerson1});
        tx.Complete();
      }

      var modifiedPersonDtos = Clone(originalPersonDtos);
      var newPersonDto = new CustomPersonDto {
        AuxString = "AuxNew", Id = rnd.Next(), Name = "NewName", Key = Guid.NewGuid().ToString()
      };
      modifiedPersonDtos.Add(newPersonDto);

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var result = mapper.Compare(originalPersonDtos, modifiedPersonDtos);
        result.Operations.Replay();
        Session.Current.SaveChanges();

        Action<CustomPersonDto, CustomPerson> validator = (personDto, person) => {
          Assert.AreEqual(personDto.AuxString, person.AuxString);
          Assert.AreEqual(personDto.Name, person.Name);
          Assert.AreEqual(personDto.Id, person.Id);
        };
        var personDto0 = (CustomPersonDto) originalPersonDtos[0];
        var personDto1 = (CustomPersonDto) originalPersonDtos[1];
        var person0 = session.Query.Single<CustomPerson>(personDto0.Id);
        validator.Invoke(personDto0, person0);
        var person1 = session.Query.Single<CustomPerson>(personDto1.Id);
        validator.Invoke(personDto1, person1);
        var newPerson = session.Query.Single<CustomPerson>(newPersonDto.Id);
        validator.Invoke(newPersonDto, newPerson);
        tx.Complete();
      }
    }

    [Test]
    public void CreateObjectUsingCustomComplexKeyValuesTest()
    {
      Require.AllFeaturesSupported(ProviderFeatures.Savepoints);
      var mapping = new MappingBuilder()
        .MapType<CompositeKeyRoot, CompositeKeyRootDto, string>(c => c.Key.Format(),
          c => c.Key, c => new object[] {c.FirstId, c.SecondId})
          .TrackChanges(c => c.FirstId, false).TrackChanges(c => c.SecondId, false)
        .MapType<CompositeKeyFirstLevel0, CompositeKeyFirstLevel0Dto, string>(c => c.Key.Format(),
          c => c.Key, c => new object[] {c.FirstId, c.SecondId})
          .TrackChanges(c => c.FirstId, false).TrackChanges(c => c.SecondId, false)
        .MapType<CompositeKeyFirstLevel1, CompositeKeyFirstLevel1Dto, string>(c => c.Key.Format(),
          c => c.Key, c => new object[] {c.FirstId, c.SecondId})
          .TrackChanges(c => c.FirstId, false).TrackChanges(c => c.SecondId, false)
        .MapType<CompositeKeySecondLevel0, CompositeKeySecondLevel0Dto, string>(c => c.Key.Format(),
          c => c.Key, c => new object[] {c.FirstId, c.SecondId})
          .TrackChanges(c => c.FirstId, false).TrackChanges(c => c.SecondId, false)
        .Build();

      var firstLevel1Dto = new CompositeKeyFirstLevel1Dto {
        Key = Guid.NewGuid().ToString(), FirstId = Guid.NewGuid(), SecondId = DateTime.Now.AddDays(1).Date,
        Aux = 10
      };
      var firstLevel0Dto = new CompositeKeyFirstLevel0Dto {
        Key = Guid.NewGuid().ToString(), FirstId = Guid.NewGuid(), SecondId = firstLevel1Dto, Aux = "11"
      };
      var secondLevel0Dto = new CompositeKeySecondLevel0Dto {
        Key = Guid.NewGuid().ToString(), FirstId = 1, SecondId = "12", Aux = 13, Reference = firstLevel0Dto
      };
      var target = new CompositeKeyRootDto {
        Key = Guid.NewGuid().ToString(), FirstId = firstLevel0Dto, SecondId = secondLevel0Dto,
        Aux = DateTime.Now.AddDays(2).Date
      };

      ReadOnlyDictionary<object, object> keyMapping;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var mapper = new Mapper(mapping);
        using (var comparisonResult = mapper.Compare(null, target)) {
          comparisonResult.Operations.Replay();
          keyMapping = comparisonResult.KeyMapping;
        }
        tx.Complete();
      }

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var root = session.Query.Single<CompositeKeyRoot>(Key.Parse(Domain, (string) keyMapping[target.Key]));
        Assert.AreEqual(target.Aux, root.Aux);
        Assert.AreEqual(Key.Parse(Domain, (string)keyMapping[target.FirstId.Key]), root.FirstId.Key);
        Assert.AreEqual(Key.Parse(Domain, (string)keyMapping[target.SecondId.Key]), root.SecondId.Key);
        var firstLevel0 = root.FirstId;
        Assert.AreEqual(firstLevel0Dto.Aux, firstLevel0.Aux);
        Assert.AreEqual(firstLevel0Dto.FirstId, firstLevel0.FirstId);
        Assert.AreEqual(Key.Parse(Domain, (string)keyMapping[firstLevel0Dto.SecondId.Key]), firstLevel0.SecondId.Key);
        var firstLevel1 = firstLevel0.SecondId;
        Assert.AreEqual(firstLevel1Dto.Aux, firstLevel1.Aux);
        Assert.AreEqual(firstLevel1Dto.FirstId, firstLevel1.FirstId);
        Assert.AreEqual(firstLevel1Dto.SecondId, firstLevel1.SecondId);
        var secondLevel0 = root.SecondId;
        Assert.AreEqual(secondLevel0Dto.Aux, secondLevel0.Aux);
        Assert.AreEqual(secondLevel0Dto.FirstId, secondLevel0.FirstId);
        Assert.AreEqual(secondLevel0Dto.SecondId, secondLevel0.SecondId);
        Assert.AreEqual(Key.Parse(Domain, (string)keyMapping[secondLevel0Dto.Reference.Key]), firstLevel0.Key);
        Assert.AreSame(secondLevel0.Reference, firstLevel0);
      }
    }

    [Test]
    public void SerializationTest()
    {
      Mapper mapper;
      var productDto = ServerCreateDtoGraphForOptimisticLockTest(out mapper);

      var modifiedProductDto = Clone(productDto);
      var product = ((PersonWithVersionDto) modifiedProductDto[0]);
      var productNewName = product.Name + "!!!";
      product.Name = productNewName;
      var newProductDto = new PersonWithVersionDto {Key = Guid.NewGuid().ToString(), Name = "New Employee"};
      modifiedProductDto[1] = newProductDto;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var comparisonResult = mapper.Compare(productDto, modifiedProductDto);
        Assert.IsFalse(comparisonResult.Operations.Count==0);
        var binaryFormatter = new BinaryFormatter();
        comparisonResult.VersionInfoProvider
          .Invoke(Key.Parse(Domain, ((PersonWithVersionDto)modifiedProductDto[0]).Key));
        TestSerialization(comparisonResult, binaryFormatter.Serialize, binaryFormatter.Deserialize);
        var dataContractSerializer = new DataContractSerializer(typeof (GraphComparisonResult),
          new[] {
            typeof (PersonWithVersionDto), 
            typeof (OperationLog),
            typeof (Ref<Entity>), 
            typeof (EntityCreateOperation), 
            typeof (EntityFieldSetOperation),
            typeof (EntitiesRemoveOperation),
            typeof (EntitySetClearOperation),
            typeof (EntitySetItemAddOperation),
            typeof (EntitySetItemRemoveOperation),
            typeof (Xtensive.Orm.Model.FieldInfoRef)});
        TestSerialization(comparisonResult, dataContractSerializer.WriteObject,
          dataContractSerializer.ReadObject);
      }
    }

    private static void TestSerialization(GraphComparisonResult expectedResult,
      Action<Stream, GraphComparisonResult> serializer, Func<Stream, object> deserializer)
    {
      var binaryFormatter = new BinaryFormatter();
      GraphComparisonResult actualResult;
      using (var stream = new MemoryStream()) {
        serializer.Invoke(stream, expectedResult);
        stream.Seek(0, SeekOrigin.Begin);
        actualResult = (GraphComparisonResult) deserializer.Invoke(stream);
      }
      var originalField = typeof (GraphComparisonResult).GetField("original",
        BindingFlags.NonPublic | BindingFlags.Instance);
      var modifiedField = typeof (GraphComparisonResult).GetField("modified",
        BindingFlags.NonPublic | BindingFlags.Instance);
      var expectedOriginal = (Dictionary<object, object>) originalField.GetValue(expectedResult);
      var actualOriginal = (Dictionary<object, object>) originalField.GetValue(actualResult);
      Assert.IsTrue(expectedOriginal.Select(p => p.Key).SequenceEqual(actualOriginal.Select(p => p.Key)));
      var expectedModified = (Dictionary<object, object>) modifiedField.GetValue(expectedResult);
      var actualModified = (Dictionary<object, object>) modifiedField.GetValue(actualResult);
      Assert.IsTrue(expectedModified.Select(p => p.Key).SequenceEqual(actualModified.Select(p => p.Key)));
      var expectedOperations = (OperationLog) actualResult.Operations;
      var actualOperations = (OperationLog) actualResult.Operations;
      Assert.IsTrue(expectedResult.KeyMapping.SequenceEqual(actualResult.KeyMapping));
      Assert.IsNotNull(actualResult.VersionInfoProvider);
    }

    private PersonalProductDto ServerCreateDtoGraphForSimpleEntitiesMappingTest(out Mapper mapper)
    {
      PersonalProductDto productDto;
      var mapping = new MappingBuilder()
        .MapType<Entity, IdentifiableDto, string>(p => p.Key.Format(), p => p.Key)
        .Inherit<IdentifiableDto, PersonalProduct, PersonalProductDto>()
        .Inherit<IdentifiableDto, Employee, EmployeeDto>().Build();
      mapper = new Mapper(mapping);
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
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
      var mapping = new MappingBuilder()
        .MapType<Publisher, PublisherDto, string>(p => p.Key.Format(), p => p.Key)
        .MapType<BookShop, BookShopDto, string>(b => b.Key.Format(), b => b.Key).Build();
      mapper = new Mapper(mapping);
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
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
      var mapping = new MappingBuilder()
        .MapType<AnotherBookShop, BookShopDto, string>(abs => abs.Key.Format(), bs => bs.Key)
        .IgnoreProperty(bs => bs.Name).IgnoreProperty(bs => bs.Url)
        .MapType<Publisher, PublisherDto, string>(p => p.Key.Format(), p => p.Key)
        .IgnoreProperty(p => p.Distributors).Build();
      mapper = new Mapper(mapping);
      BookShopDto bookShopDto;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        foreach (var publisher in session.Query.All<Publisher>())
          publisher.Remove();
        foreach (var bookShop in session.Query.All<BookShop>())
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
      var mapping = new MappingBuilder()
        .MapType<SimplePerson, SimplePersonDto, string>(p => p.Key.Format(), p => p.Key)
        .MapType<Apartment, ApartmentDto, string>(a => a.Key.Format(), a => a.Key)
        .MapStructure<Address, AddressDto>()
        .MapStructure<ApartmentDescription, ApartmentDescriptionDto>().Build();
      mapper = new Mapper(mapping);
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
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
      var mapping = new MappingBuilder()
        .MapType<SimplePerson, SimplePersonDto, string>(sp => sp.Key.Format(), sp => sp.Key).Build();
      mapper = new Mapper(mapping);
      List<object> original;
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var person0 = new SimplePerson {Name = "Person0"};
        var person1 = new SimplePerson {Name = "Person1"};
        original = (List<object>) mapper.Transform(new[] {person0, person1});
        tx.Complete();
      }
      return original;
    }

    private List<object> ServerCreateDtoGraphForOptimisticLockTest(out Mapper mapper)
    {
      var formatter = new BinaryFormatter();
      using (var stream = new MemoryStream()) {
        Func<VersionInfo, byte[]> serializer = version => {
          formatter.Serialize(stream, version);
          stream.Seek(0, SeekOrigin.Begin);
          var result = new byte[stream.Length];
          stream.Read(result, 0, result.Length);
          stream.SetLength(0);
          return result;
        };
        var mapping = new MappingBuilder()
          .MapType<SimplePerson, PersonWithVersionDto, string>(sp => sp.Key.Format(), sp => sp.Key)
          .MapProperty(p => serializer.Invoke(p.VersionInfo), p => p.Version).Build();
        mapper = new Mapper(mapping);
        List<object> original;
        using (var session = Domain.OpenSession())
        using (var tx = session.OpenTransaction()) {
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
      using (var session = Domain.OpenSession()) {
        using (var result = mapper.Compare(original, modified))
        using (VersionValidator.Attach(session, result.VersionInfoProvider))
        using (var tx = session.OpenTransaction()) {
          result.Operations.Replay();
          tx.Complete();
        }
      }

      // Validation of the stale object.
      ((PersonWithVersionDto) modified[0]).Name += "ModifiedAgain";
      using (var session = Domain.OpenSession()) {
        using (var result = mapper.Compare(original, modified))
        using (VersionValidator.Attach(session, result.VersionInfoProvider)) {
          AssertEx.Throws<VersionConflictException>(() => {
            using (var tx = session.OpenTransaction()) {
              result.Operations.Replay();
              tx.Complete();
            }
          });
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