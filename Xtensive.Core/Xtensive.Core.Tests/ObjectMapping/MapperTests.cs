// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.09

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Collections;
using Xtensive.Core.ObjectMapping;
using Xtensive.Core.ObjectMapping.Model;
using Xtensive.Core.Tests.ObjectMapping.SourceModel;
using Xtensive.Core.Tests.ObjectMapping.TargetModel;

namespace Xtensive.Core.Tests.ObjectMapping
{
  [TestFixture]
  public sealed class MapperTests
  {
    [Test]
    public void DefaultTransformationOfPrimitivePropertiesTest()
    {
      var source = GetSourcePerson();
      var mapper = GetPersonOrderMapper();
      var target = (PersonDto) mapper.Transform(source);
      Assert.IsNotNull(target);
      AssertAreEqual(source, target);
    }

    [Test]
    public void DefaultTransformationOfComplexPropertiesTest()
    {
      var source = GetSourceOrder();
      var mapper = GetPersonOrderMapper();
      var target = (OrderDto) mapper.Transform(source);
      Assert.IsNotNull(target);
      Assert.AreEqual(source.Id.ToString(), target.Id);
      Assert.AreEqual(source.ShipDate, target.ShipDate);
      AssertAreEqual(source.Customer, target.Customer);
    }

    [Test]
    public void ComparisonOfObjectsContainingPrimitivePropertiesOnlyTest()
    {
      var source = GetSourcePerson();
      var mapper = GetPersonOrderMapper();
      var target = (PersonDto) mapper.Transform(source);
      var clone = (PersonDto) target.Clone();
      var modifiedDate = clone.BirthDate.AddDays(23);
      clone.BirthDate = modifiedDate;
      var modifiedFirstName = clone.FirstName + "!";
      clone.FirstName = modifiedFirstName;
      var eventRaisingCount = 0;
      Action<OperationInfo> validator = descriptor => {
        eventRaisingCount++;
        switch (descriptor.Property.SystemProperty.Name) {
        case "BirthDate":
          Assert.AreEqual(modifiedDate, descriptor.Value);
          break;
        case "FirstName":
          Assert.AreEqual(modifiedFirstName, descriptor.Value);
          break;
        default:
          Assert.Fail();
          break;
        }
      };
      ((DefaultModificationSet) mapper.Compare(target, clone)).Apply(validator);
      Assert.AreEqual(2, eventRaisingCount);
    }

    [Test]
    public void ComparisonOfObjectsWhenOnlyPrimitivePropertiesHaveBeenModifiedTest()
    {
      var source = GetSourceOrder();
      var mapper = GetPersonOrderMapper();
      var target = (OrderDto) mapper.Transform(source);
      var clone = (OrderDto) target.Clone();
      var modifiedShipDate = clone.ShipDate.AddDays(-5);
      clone.ShipDate = modifiedShipDate;
      var modifiedFirstName = clone.Customer.FirstName + "!";
      clone.Customer.FirstName = modifiedFirstName;
      var eventRaisingCount = 0;
      Action<OperationInfo> validator = descriptor => {
        eventRaisingCount++;
        if (ReferenceEquals(target, descriptor.Object)) {
          Assert.AreEqual("ShipDate", descriptor.Property.SystemProperty.Name);
          Assert.AreEqual(modifiedShipDate, descriptor.Value);
        }
        else if (ReferenceEquals(target.Customer, descriptor.Object)) {
          Assert.AreEqual("FirstName", descriptor.Property.SystemProperty.Name);
          Assert.AreEqual(modifiedFirstName, descriptor.Value);
        }
        else
          Assert.Fail();
      };
      ((DefaultModificationSet) mapper.Compare(target, clone)).Apply(validator);
      Assert.AreEqual(2, eventRaisingCount);
    }

    [Test]
    public void ComparisonOfObjectsWhenReferencedObjectHasBeenReplacedTest()
    {
      var source = GetSourceOrder();
      var mapper = GetPersonOrderMapper();
      var target = (OrderDto) mapper.Transform(source);
      var clone = (OrderDto) target.Clone();
      var modifiedShipDate = clone.ShipDate.AddDays(-5);
      clone.ShipDate = modifiedShipDate;
      var modifiedCustomer = new PersonDto {
        BirthDate = DateTime.Now.AddYears(25), FirstName = "Stewart", LastName = "Smith", Id = 4
      };
      clone.Customer = modifiedCustomer;

      ValidateComparisonOfObjectsWhenReferencedObjectHasBeenReplaced(mapper, target, clone, modifiedShipDate,
        modifiedCustomer);
    }

    [Test]
    public void TransformationUsingCustomMappingTest()
    {
      var source = GetSourceAuthor();
      var mapper = GetAuthorBookMapper();
      var target = (AuthorDto) mapper.Transform(source);
      Assert.IsNotNull(target);
      Assert.AreEqual(source.Name + "!!!", target.Name);
      Assert.AreEqual(source.Book.ISBN, target.Book.ISBN);
      Assert.AreEqual(source.Book.Title.Text, target.Book.TitleText);
      Assert.AreEqual(source.Book.Title.Id, target.Book.Title.Id);
    }

    [Test]
    public void TransformationWhenGraphRootIsNullTest()
    {
      var mapper = GetPersonOrderMapper();
      Assert.IsNull(mapper.Transform(null));
      Action<OperationInfo> validator = descriptor => Assert.Fail();
      ((DefaultModificationSet) mapper.Compare(null, null)).Apply(validator);
    }

    [Test]
    public void ComparisonWhenOriginalGraphRootIsNullTest()
    {
      var mapper = GetPersonOrderMapper();
      var target = (OrderDto) mapper.Transform(GetSourceOrder());
      var eventRaisingCount = 0;
      var personCreated = false;
      var orderCreated = false;
      var orderPropertyCounts = CreateCountsForMutableProperties(typeof (OrderDto), mapper);
      var customerPropertyCounts = CreateCountsForMutableProperties(typeof (PersonDto), mapper);
      Action<OperationInfo> validator = descriptor => {
        switch (descriptor.Type) {
        case OperationType.CreateObject:
          if (ReferenceEquals(target, descriptor.Object))
            orderCreated = true;
          else if (ReferenceEquals(target.Customer, descriptor.Object))
            personCreated = true;
          else
            Assert.Fail();
          break;
        case OperationType.SetProperty:
            if (ReferenceEquals(target, descriptor.Object))
              orderPropertyCounts[descriptor.Property.SystemProperty.Name] += 1;
          else if (ReferenceEquals(target.Customer, descriptor.Object))
              customerPropertyCounts[descriptor.Property.SystemProperty.Name] += 1;
          else
            Assert.Fail();
          break;
        default:
            Assert.Fail();
          break;
        }
        eventRaisingCount++;
      };
      ((DefaultModificationSet) mapper.Compare(null, target)).Apply(validator);
      Assert.AreEqual(7, eventRaisingCount);
      Assert.IsTrue(orderPropertyCounts.All(pair => pair.Value == 1));
      Assert.IsTrue(customerPropertyCounts.All(pair => pair.Value == 1));
      Assert.IsTrue(personCreated);
      Assert.IsTrue(orderCreated);
    }

    [Test]
    public void ComparisonWhenModifiedGraphRootIsNullTest()
    {
      var mapper = GetPersonOrderMapper();
      var target = (OrderDto) mapper.Transform(GetSourceOrder());
      var eventRaisingCount = 0;
      var personCreated = false;
      var orderCreated = false;
      Action<OperationInfo> validator = descriptor => {
        Assert.AreEqual(OperationType.RemoveObject, descriptor.Type);
        eventRaisingCount++;
        if (ReferenceEquals(target, descriptor.Object))
          orderCreated = true;
        else if (ReferenceEquals(target.Customer, descriptor.Object))
          personCreated = true;
        else
          Assert.Fail();
      };
      ((DefaultModificationSet) mapper.Compare(target, null)).Apply(validator);
      Assert.AreEqual(2, eventRaisingCount);
      Assert.IsTrue(personCreated);
      Assert.IsTrue(orderCreated);
    }

    [Test]
    public void CollectionTransformationTest()
    {
      var mapper = GetPetOwnerAnimalMapper();
      var source = GetSourcePetOwner();
      var target = (PetOwnerDto) mapper.Transform(source);
      Assert.IsNotNull(target);
      AssertAreEqual(source, target);
      Assert.AreEqual(source.Pets.Count, target.Pets.Count);
      foreach (var animalDto in target.Pets)
        Assert.IsNotNull(source.Pets.Where(p => p.Id == animalDto.Id).Single());
    }

    [Test]
    public void CollectionComparisonTest()
    {
      var mapper = GetPetOwnerAnimalMapper();
      var source = GetSourcePetOwner();
      var original = (PetOwnerDto) mapper.Transform(source);
      var modified = (PetOwnerDto) original.Clone();
      const int removedIndex = 1;
      var key = modified.Pets[removedIndex].Id;
      var removedAnimal0 = original.Pets.Single(p => p.Id == key);
      modified.Pets.RemoveAt(removedIndex);
      key = modified.Pets[removedIndex].Id;
      var removedAnimal1 = original.Pets.Single(p => p.Id == key);
      modified.Pets.RemoveAt(removedIndex);
      var newAnimal = new AnimalDto {Id = Guid.NewGuid(), Name = "N"};
      modified.Pets.Add(newAnimal);

      ValidateCollectionComparison(mapper, original, modified, newAnimal, removedAnimal0, removedAnimal1);
    }

    [Test]
    public void BuildMappingWhenTypesHavePropertyWhichShouldBeIgnoredTest()
    {
      var mapper = GetIgnorableMapper();
      var auxiliaryProperty = GetTargetProperty(mapper, typeof (IgnorableDto), "Auxiliary");
      var ignoredTargetProperty = GetTargetProperty(mapper, typeof (IgnorableDto), "Ignored");
      Assert.IsNull(auxiliaryProperty.SourceProperty);
      Assert.IsTrue(auxiliaryProperty.IsIgnored);
      Assert.IsNull(ignoredTargetProperty.SourceProperty);
      Assert.IsTrue(ignoredTargetProperty.IsIgnored);
    }

    private static void ValidateCollectionComparison(DefaultMapper mapper, PetOwnerDto original,
      PetOwnerDto modified, AnimalDto newAnimal, AnimalDto removedAnimal0, AnimalDto removedAnimal1)
    {
      var itemRemovalPublished0 = false;
      var itemRemovalPublished1 = false;
      var creationPublished = false;
      var removalPublished0 = false;
      var removalPublished1 = false;
      var additionPublished = false;
      var petPropertyCounts = CreateCountsForMutableProperties(typeof (AnimalDto), mapper);
      const string petsName = "Pets";
      var eventRaisingCount = 0;
      Action<OperationInfo> validator = descriptor => {
        eventRaisingCount++;
        switch (descriptor.Type) {
        case OperationType.AddItem:
          Assert.AreEqual(original, descriptor.Object);
          Assert.AreEqual(petsName, descriptor.Property.SystemProperty.Name);
          Assert.AreEqual(newAnimal, descriptor.Value);
          additionPublished = true;
          break;
        case OperationType.RemoveItem:
          if (ReferenceEquals(removedAnimal0, descriptor.Value))
            itemRemovalPublished0 = true;
          else if (ReferenceEquals(removedAnimal1, descriptor.Value))
            itemRemovalPublished1 = true;
          else
            Assert.Fail();
          Assert.AreEqual(petsName, descriptor.Property.SystemProperty.Name);
          Assert.AreEqual(original, descriptor.Object);
          break;
        case OperationType.CreateObject:
          creationPublished = true;
          Assert.AreEqual(newAnimal, descriptor.Object);
          Assert.IsNull(descriptor.Property);
          Assert.IsNull(descriptor.Value);
          break;
        case OperationType.RemoveObject:
          if (ReferenceEquals(removedAnimal0, descriptor.Object))
            removalPublished0 = true;
          else if (ReferenceEquals(removedAnimal1, descriptor.Object))
            removalPublished1 = true;
          else
            Assert.Fail();
          Assert.IsNull(descriptor.Property);
          Assert.IsNull(descriptor.Value);
          break;
        case OperationType.SetProperty:
          petPropertyCounts[descriptor.Property.SystemProperty.Name] += 1;
          Assert.AreSame(newAnimal, descriptor.Object);
          var expectedValue = descriptor.Property.SystemProperty.GetValue(newAnimal, null);
          Assert.AreEqual(expectedValue, descriptor.Value);
          break;
        default:
          Assert.Fail();
          break;
        }
      };
      ((DefaultModificationSet) mapper.Compare(original, modified)).Apply(validator);
      Assert.AreEqual(7, eventRaisingCount);
      Assert.IsTrue(itemRemovalPublished0);
      Assert.IsTrue(itemRemovalPublished1);
      Assert.IsTrue(removalPublished0);
      Assert.IsTrue(removalPublished1);
      Assert.IsTrue(creationPublished);
      Assert.IsTrue(additionPublished);
    }

    private static void ValidateComparisonOfObjectsWhenReferencedObjectHasBeenReplaced(DefaultMapper mapper,
      OrderDto target, OrderDto clone, DateTime modifiedShipDate, PersonDto modifiedCustomer)
    {
      var shipDateModified = false;
      var customerModified = false;
      var newCustomerCreated = false;
      var oldCustomerRemoved = false;
      var eventRaisingCount = 0;
      var customerPropertyCounts = CreateCountsForMutableProperties(typeof (PersonDto), mapper);
      Action<OperationInfo> validator = descriptor => {
        eventRaisingCount++;
        if (ReferenceEquals(target, descriptor.Object)) {
          Assert.AreSame(target, descriptor.Object);
          switch (descriptor.Property.SystemProperty.Name) {
          case "ShipDate":
            shipDateModified = true;
            Assert.AreEqual(modifiedShipDate, descriptor.Value);
            break;
          case "Customer":
            customerModified = true;
            Assert.AreSame(modifiedCustomer, descriptor.Value);
            var descriptorCustomer = (PersonDto) descriptor.Value;
            Assert.AreEqual(modifiedCustomer.BirthDate, descriptorCustomer.BirthDate);
            Assert.AreEqual(modifiedCustomer.FirstName, descriptorCustomer.FirstName);
            Assert.AreEqual(modifiedCustomer.Id, descriptorCustomer.Id);
            Assert.AreEqual(modifiedCustomer.LastName, descriptorCustomer.LastName);
            break;
          default:
            Assert.Fail();
            break;
          }
        }
        else if (ReferenceEquals(modifiedCustomer, descriptor.Object)) {
          if (!newCustomerCreated) {
            newCustomerCreated = true;
            Assert.AreEqual(OperationType.CreateObject, descriptor.Type);
            Assert.IsNull(descriptor.Property);
            Assert.IsNull(descriptor.Value);
          }
          else {
            customerPropertyCounts[descriptor.Property.SystemProperty.Name] += 1;
            var expectedValue = descriptor.Property.SystemProperty.GetValue(modifiedCustomer, null);
            Assert.AreEqual(expectedValue, descriptor.Value);
            Assert.AreEqual(OperationType.SetProperty, descriptor.Type);
          }
        }
        else if (ReferenceEquals(target.Customer, descriptor.Object)) {
          oldCustomerRemoved = true;
          Assert.AreEqual(OperationType.RemoveObject, descriptor.Type);
          Assert.IsNull(descriptor.Property);
          Assert.IsNull(descriptor.Value);
        }
        else
          Assert.Fail();
      };
      ((DefaultModificationSet) mapper.Compare(target, clone)).Apply(validator);
      Assert.AreEqual(7, eventRaisingCount);
      Assert.IsTrue(customerPropertyCounts.All(pair => pair.Value == 1));
      Assert.IsTrue(shipDateModified);
      Assert.IsTrue(customerModified);
      Assert.IsTrue(newCustomerCreated);
      Assert.IsTrue(oldCustomerRemoved);
    }

    private static DefaultMapper GetPersonOrderMapper()
    {
      var result = new DefaultMapper();
      result.MapType<Person, PersonDto, int>(p => p.Id, p => p.Id)
        .MapType<Order, OrderDto, String>(o => o.Id.ToString(), o => o.Id).Complete();
      return result;
    }

    private static DefaultMapper GetAuthorBookMapper()
    {
      var result = new DefaultMapper();
      result.MapType<Author, AuthorDto, Guid>(a => a.Id, a => a.Id)
          .Map(a => a.Name + "!!!", a => a.Name)
        .MapType<Book, BookDto, string>(b => b.ISBN, b => b.ISBN)
          .Map(b => b.Title.Text, b => b.TitleText)
          .Map(b => new TitleDto {Id = b.Title.Id, Text = b.Title.Text}, b => b.Title)
        .MapType<Title, TitleDto, Guid>(t => t.Id, t => t.Id).Complete();
      return result;
    }

    private static DefaultMapper GetPetOwnerAnimalMapper()
    {
      var result = new DefaultMapper();
      result.MapType<PetOwner, PetOwnerDto, int>(o => o.Id, o => o.Id)
        .MapType<Animal, AnimalDto, Guid>(a => a.Id, a => a.Id).Complete();
      return result;
    }

    private static DefaultMapper GetIgnorableMapper()
    {
      var result = new DefaultMapper();
      result.MapType<Ignorable, IgnorableDto, Guid>(c => c.Id, c => c.Id)
        .Ignore(c => c.Auxiliary).Ignore(c => c.Ignored).Complete();
      return result;
    }

    private static Person GetSourcePerson()
    {
      return new Person {
        BirthDate = DateTime.Now.AddYears(-20), FirstName = "John", LastName = "Smith", Id = 3
      };
    }

    private static Order GetSourceOrder()
    {
      return new Order {
        Customer = GetSourcePerson(), Id = Guid.NewGuid(), ShipDate = DateTime.Today.AddMonths(3)
      };
    }

    private static Author GetSourceAuthor()
    {
      var title = new Title {Id = Guid.NewGuid(), Text = "T"};
      var book = new Book {ISBN = Guid.NewGuid().ToString(), Price = 25.0, Title = title};
      return new Author {Book = book, Id = Guid.NewGuid(), Name = "A"};
    }

    private static PetOwner GetSourcePetOwner()
    {
      var petOwner = new PetOwner {
        BirthDate = DateTime.Now.AddYears(20), FirstName = "A", Id = 10, LastName = "B"
      };
      petOwner.Pets.Add(new Animal());
      petOwner.Pets.Add(new Animal());
      petOwner.Pets.Add(new Animal());
      petOwner.Pets.Add(new Animal());
      petOwner.Pets.Add(new Animal());
      return petOwner;
    }
    
    private static void AssertAreEqual(Person source, PersonDto target)
    {
      Assert.AreEqual(source.BirthDate, target.BirthDate);
      Assert.AreEqual(source.FirstName, target.FirstName);
      Assert.AreEqual(source.Id, target.Id);
      Assert.AreEqual(source.LastName, target.LastName);
    }

    private static Dictionary<string, int> CreateCountsForMutableProperties(Type type, DefaultMapper mapper)
    {
      var result = new Dictionary<string, int>();
      mapper.MappingDescription.TargetTypes[type].Properties.Select(pair => pair.Value)
        .Cast<TargetPropertyDescription>().Where(p => !p.IsImmutable)
        .Apply(p => result.Add(p.SystemProperty.Name, 0));
      return result;
    }

    private static TargetPropertyDescription GetTargetProperty(MapperBase mapper, Type type,
      string propertyName)
    {
      return (TargetPropertyDescription) mapper.MappingDescription.TargetTypes[type]
        .Properties[type.GetProperty(propertyName)];
    }

    private static SourcePropertyDescription GetSourceProperty(MapperBase mapper, Type type,
      string propertyName)
    {
      return (SourcePropertyDescription) mapper.MappingDescription.SourceTypes[type]
        .Properties[type.GetProperty(propertyName)];
    }
  }
}