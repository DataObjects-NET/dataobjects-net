// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.09

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Collections;
using Xtensive.Core.ObjectMapping;
using Xtensive.Core.ObjectMapping.Model;
using Xtensive.Core.Testing;
using Xtensive.Core.Tests.ObjectMapping.SourceModel;
using Xtensive.Core.Tests.ObjectMapping.TargetModel;

namespace Xtensive.Core.Tests.ObjectMapping
{
  [TestFixture]
  public sealed class MapperTests
  {
    private const string inherited = "Inherited";
    private const string inheritedFromMammal = "InheritedFromMammal";
    private const string inheritedFlyingInsect = "InheritedFlyingInsect";
    private PropertyInfo petsProperty;
    private PropertyInfo customerProperty;
    private PropertyInfo firstNameProperty;
    private PropertyInfo lastNameProperty;
    private PropertyInfo birthDateProperty;

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      petsProperty = typeof (PetOwnerDto).GetProperty("Pets");
      customerProperty = typeof (OrderDto).GetProperty("Customer");
      firstNameProperty = typeof (PersonDto).GetProperty("FirstName");
      lastNameProperty = typeof (PersonDto).GetProperty("LastName");
      birthDateProperty = typeof (PersonDto).GetProperty("BirthDate");
    }

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
      ((DefaultOperationSet) mapper.Compare(target, clone)).Apply(validator);
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
      ((DefaultOperationSet) mapper.Compare(target, clone)).Apply(validator);
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
      ((DefaultOperationSet) mapper.Compare(null, null)).Apply(validator);
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
      ((DefaultOperationSet) mapper.Compare(null, target)).Apply(validator);
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
      ((DefaultOperationSet) mapper.Compare(target, null)).Apply(validator);
      Assert.AreEqual(2, eventRaisingCount);
      Assert.IsTrue(personCreated);
      Assert.IsTrue(orderCreated);
    }

    [Test]
    public void ComparisonWhenBothOfGraphRootsAreNullTest()
    {
      var mapper = GetPersonOrderMapper();
      var operations = mapper.Compare(null, null);
      Assert.IsTrue(operations.IsEmpty);
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
    public void MappingBuildingWhenTypesHavePropertyWhichShouldBeIgnoredTest()
    {
      var mapper = GetIgnorableMapper();
      var auxiliaryProperty = GetTargetProperty(mapper, typeof (IgnorableDto), "Auxiliary");
      var ignoredTargetProperty = GetTargetProperty(mapper, typeof (IgnorableDto), "Ignored");
      Assert.IsNull(auxiliaryProperty.SourceProperty);
      Assert.IsTrue(auxiliaryProperty.IsIgnored);
      Assert.IsNull(ignoredTargetProperty.SourceProperty);
      Assert.IsTrue(ignoredTargetProperty.IsIgnored);
    }

    [Test]
    public void TransformationWhenTypesHavePropertiesWhichShouldBeIgnoredTest()
    {
      var mapper = GetIgnorableMapper();
      var source = GetIgnorableSource();
      var target = (IgnorableDto) mapper.Transform(source);
      Assert.IsNotNull(target);
      Assert.AreEqual(source.Id, target.Id);
      Assert.AreNotEqual(source.Ignored, target.Ignored);
      Assert.IsNull(target.Ignored);
      Assert.IsNull(target.Auxiliary);
      Assert.AreEqual(source.IncludedReference.Id, target.IncludedReference.Id);
      Assert.AreEqual(source.IncludedReference.Date, target.IncludedReference.Date);
      Assert.IsNotNull(source.IgnoredReference);
      Assert.IsNull(target.IgnoredReference);
    }

    [Test]
    public void ComparisonWhenTypesHavePropertiesWhichShouldBeIgnoredTest()
    {
      var mapper = GetIgnorableMapper();
      var source = GetIgnorableSource();
      var original = (IgnorableDto) mapper.Transform(source);
      original.Ignored = "I";
      original.Auxiliary = "A";
      original.IgnoredReference = new IgnorableSubordinateDto {Date = DateTime.Now, Id = Guid.NewGuid()};
      var modified = (IgnorableDto) original.Clone();
      modified.Ignored = "1";
      modified.Auxiliary = "2";
      modified.IgnoredReference = new IgnorableSubordinateDto {Date = DateTime.Now.AddDays(1),
        Id = Guid.NewGuid()};
      var operations = mapper.Compare(original, modified);
      Assert.IsTrue(operations.IsEmpty);
      var newDate = modified.IncludedReference.Date.AddYears(20);
      modified.IncludedReference.Date = newDate;
      operations = mapper.Compare(original, modified);
      Assert.IsFalse(operations.IsEmpty);
      var operation = ((DefaultOperationSet) operations).Single();
      Assert.AreEqual(original.IncludedReference, operation.Object);
      Assert.AreEqual("Date", operation.Property.SystemProperty.Name);
      Assert.AreEqual(OperationType.SetProperty, operation.Type);
      Assert.AreEqual(newDate, operation.Value);
      Assert.AreNotEqual(original.IgnoredReference.Id, modified.IgnoredReference.Id);
      Assert.AreNotEqual(original.IgnoredReference.Date, modified.IgnoredReference.Date);
      Assert.AreNotEqual(original.Auxiliary, modified.Auxiliary);
      Assert.AreNotEqual(original.Ignored, modified.Ignored);
    }

    [Test]
    public void TransformationIncludingNullTest()
    {
      var mapper = GetPersonOrderMapper();
      var sourceOrder = new Order {Id = Guid.NewGuid()};
      var targetOrder = (OrderDto) mapper.Transform(sourceOrder);
      Assert.IsNull(targetOrder.Customer);
      sourceOrder = new Order {Id = Guid.NewGuid(), Customer = new Person {Id = 1}};
      targetOrder = (OrderDto) mapper.Transform(sourceOrder);
      Assert.IsNull(targetOrder.Customer.FirstName);
      Assert.IsNull(targetOrder.Customer.LastName);

      mapper = GetPetOwnerAnimalMapper();
      var sourcePetOwner = new PetOwner(null) {Id = 1};
      var targetPetOwner = (PetOwnerDto) mapper.Transform(sourcePetOwner);
      Assert.IsNull(targetPetOwner.FirstName);
      Assert.IsNull(targetPetOwner.LastName);
      Assert.IsNull(targetPetOwner.Pets);

      var pets = new HashSet<Animal> {new Animal()};
      sourcePetOwner = new PetOwner(pets);
      targetPetOwner = (PetOwnerDto) mapper.Transform(sourcePetOwner);
      Assert.IsNull(targetPetOwner.Pets.Single().Name);

      pets = new HashSet<Animal> {null};
      sourcePetOwner = new PetOwner(pets);
      targetPetOwner = (PetOwnerDto) mapper.Transform(sourcePetOwner);
      Assert.IsNull(targetPetOwner.Pets.Single());
    }

    [Test]
    public void TransformationWhenGraphRootIsCollectionAndContainsNullsTest()
    {
      var mapper = GetPersonOrderMapper();
      var source = new List<Order> {GetSourceOrder(1), null, GetSourceOrder(2), null, null};
      var target = ((List<object>) mapper.Transform(source)).Cast<OrderDto>().ToList();
      Assert.IsNotNull(target[0]);
      Assert.IsNull(target[1]);
      Assert.IsNotNull(target[2]);
      Assert.IsNull(target[3]);
      Assert.IsNull(target[4]);
    }

    [Test]
    public void ComparisonWhenOriginalReferencePropertyContainsNulTest()
    {
      var mapper = GetPersonOrderMapper();
      var sourceOrder = GetSourceOrder();
      var originalOrder = (OrderDto) mapper.Transform(sourceOrder);
      var modifiedOrder = (OrderDto) originalOrder.Clone();
      originalOrder.Customer = null;
      var operations = (DefaultOperationSet) mapper.Compare(originalOrder, modifiedOrder);
      Assert.AreEqual(5, operations.Count());
      var personCreationOperation = operations.First();
      var createdCustomer = modifiedOrder.Customer;
      ValidateObjectCreation(createdCustomer, personCreationOperation);
      ValidatePropertySettingOperation(createdCustomer, operations.Skip(1).First(),
        typeof (PersonDto).GetProperty("FirstName"), createdCustomer.FirstName);
      ValidatePropertySettingOperation(createdCustomer, operations.Skip(2).First(),
        typeof (PersonDto).GetProperty("LastName"), createdCustomer.LastName);
      ValidatePropertySettingOperation(createdCustomer, operations.Skip(3).First(),
        typeof (PersonDto).GetProperty("BirthDate"), createdCustomer.BirthDate);
      var customerSettingOperation = operations.Skip(4).Single();
      ValidatePropertySettingOperation(originalOrder, customerSettingOperation, customerProperty,
        createdCustomer);
    }

    [Test]
    public void ComparisonWhenModifiedReferencePropertyContainsNullTest()
    {
      var mapper = GetPersonOrderMapper();
      var sourceOrder = GetSourceOrder();
      var originalOrder = (OrderDto) mapper.Transform(sourceOrder);
      var modifiedOrder = (OrderDto) originalOrder.Clone();
      modifiedOrder.Customer = null;
      var operations = (DefaultOperationSet) mapper.Compare(originalOrder, modifiedOrder);
      Assert.AreEqual(2, operations.Count());
      var customerSettingOperation = operations.First();
      ValidatePropertySettingOperation(originalOrder, customerSettingOperation, customerProperty, null);
      var personRemovalOperation = operations.Skip(1).Single();
      ValidateObjectRemoval(originalOrder.Customer, personRemovalOperation);
    }

    [Test]
    public void ComparisonWhenOriginalCollectionIsNullTest()
    {
      var mapper = GetPetOwnerAnimalMapper();
      var source = new PetOwner(null) {Id = 1, BirthDate = DateTime.Now, FirstName = "A"};
      var original = (PetOwnerDto) mapper.Transform(source);
      var modified = (PetOwnerDto) original.Clone();
      modified.Pets = new List<AnimalDto>();
      var operations = (DefaultOperationSet) mapper.Compare(original, modified);
      Assert.IsTrue(operations.IsEmpty);
    }

    [Test]
    public void ComparisonWhenModifiedCollectionIsNullTest()
    {
      var mapper = GetPetOwnerAnimalMapper();
      var animal = new Animal();
      var pets = new HashSet<Animal> {animal};
      var source = new PetOwner(pets) {Id = 1, BirthDate = DateTime.Now, FirstName = "A"};
      var original = (PetOwnerDto) mapper.Transform(source);
      var modified = (PetOwnerDto) original.Clone();
      modified.Pets = null;
      var operations = (DefaultOperationSet) mapper.Compare(original, modified);
      Assert.AreEqual(2, operations.Count());
      var removedAnimal = original.Pets.Single();
      var itemRemovalOperation = operations.First();
      ValidateItemRemovalOperation(original, itemRemovalOperation, petsProperty, removedAnimal);
      var animalRemovalOperation = operations.Skip(1).Single();
      ValidateObjectRemoval(removedAnimal, animalRemovalOperation);
    }

    [Test]
    public void ComparisonWhenOriginalCollectionContainsNullTest()
    {
      var mapper = GetPetOwnerAnimalMapper();
      var animal = new Animal();
      var source = new PetOwner(new HashSet<Animal>{animal}) {
        Id = 1, BirthDate = DateTime.Now, FirstName = "A"
      };
      var original = (PetOwnerDto) mapper.Transform(source);
      var modified = (PetOwnerDto) original.Clone();
      original.Pets = new List<AnimalDto> {null};
      var createdAnimal = modified.Pets.Single();
      var operations = (DefaultOperationSet) mapper.Compare(original, modified);
      Assert.AreEqual(3, operations.Count());
      var animalCreationOperation = operations.First();
      ValidateObjectCreation(createdAnimal, animalCreationOperation);
      var propertyInfo = typeof (AnimalDto).GetProperty("Name");
      var nameSettingOperation = operations.Skip(1).First();
      ValidatePropertySettingOperation(createdAnimal, nameSettingOperation, propertyInfo, createdAnimal.Name);
      var petAdditionOperation = operations.Skip(2).Single();
      ValidateItemAdditionOperation(original, petAdditionOperation, petsProperty, createdAnimal);
    }

    [Test]
    public void ComparisonWhenModifiedCollectionContainsNullTest()
    {
      var mapper = GetPetOwnerAnimalMapper();
      var animal = new Animal();
      var source = new PetOwner(new HashSet<Animal>{animal}) {
        Id = 1, BirthDate = DateTime.Now, FirstName = "A"
      };
      var original = (PetOwnerDto) mapper.Transform(source);
      var modified = (PetOwnerDto) original.Clone();
      modified.Pets = null;
      var operations = (DefaultOperationSet) mapper.Compare(original, modified);
      Assert.AreEqual(2, operations.Count());
      var removedAnimal = original.Pets.Single();
      var itemRemovalOperation = operations.First();
      ValidateItemRemovalOperation(original, itemRemovalOperation, petsProperty, removedAnimal);
      var animalRemovalOperation = operations.Skip(1).Single();
      ValidateObjectRemoval(removedAnimal, animalRemovalOperation);
    }

    [Test]
    public void TransformationWhenGraphRootIsCollectionTest()
    {
      var mapper = GetAuthorBookMapper();
      var source = new HashSet<Author>();
      for (int i = 0; i < 5; i++) {
        var author = GetSourceAuthor();
        author.Name += i;
        author.Book.Price += i * 100;
        author.Book.Title.Text += i;
        source.Add(author);
      }
      var transformedList = (List<object>) mapper.Transform(source);
      var target = transformedList.Cast<AuthorDto>().ToList();
      Assert.AreEqual(source.Count, target.Count);
      target.Apply(ta => Assert.IsTrue(source.Any(sa => sa.Id == ta.Id && sa.Name + "!!!" == ta.Name
        && sa.Book.ISBN == ta.Book.ISBN && sa.Book.Price == ta.Book.Price
        && sa.Book.Title.Id == ta.Book.Title.Id && sa.Book.Title.Text == ta.Book.Title.Text
        && sa.Book.Title.Text == ta.Book.TitleText)));
    }

    [Test]
    public void ComparisonWhenGraphRootIsCollectionTest()
    {
      var mapper = GetAuthorBookMapper();
      var source = new HashSet<Author>();
      for (int i = 0; i < 5; i++) {
        var author = GetSourceAuthor();
        author.Name += i;
        author.Book.Price += i * 100;
        author.Book.Title.Text += i;
        source.Add(author);
      }
      var transformedList = (List<object>) mapper.Transform(source);
      var original = transformedList.Cast<AuthorDto>().ToList();
      var modified = original.Select(a => a.Clone()).Cast<AuthorDto>().ToList();
      var removedAuthor = original[1];
      modified.RemoveAt(1);
      var createdAuthor = (AuthorDto) mapper.Transform(GetSourceAuthor());
      modified.Add(createdAuthor);
      var operations = (DefaultOperationSet) mapper.Compare(original, modified);
      Assert.AreEqual(9, operations.Count());
      ValidateObjectCreation(createdAuthor, operations.First());
      ValidateObjectCreation(createdAuthor.Book, operations.Skip(1).First());
      ValidateObjectCreation(createdAuthor.Book.Title, operations.Skip(2).First());
      ValidatePropertySettingOperation(createdAuthor, operations.Skip(3).First(),
        typeof (AuthorDto).GetProperty("Book"), createdAuthor.Book);
      ValidatePropertySettingOperation(createdAuthor.Book, operations.Skip(4).First(),
        typeof (BookDto).GetProperty("Price"), createdAuthor.Book.Price);
      ValidatePropertySettingOperation(createdAuthor.Book.Title, operations.Skip(5).First(),
        typeof (TitleDto).GetProperty("Text"), createdAuthor.Book.Title.Text);
      ValidateObjectRemoval(removedAuthor, operations.Skip(6).First());
      ValidateObjectRemoval(removedAuthor.Book, operations.Skip(7).First());
      ValidateObjectRemoval(removedAuthor.Book.Title, operations.Skip(8).First());
    }

    [Test]
    public void ComparisonWhenOriginalIsCollectionButModifiedIsNotTest()
    {
      var mapper = GetPersonOrderMapper();
      var source = new List<Person> {GetSourcePerson(1), GetSourcePerson(2)};
      var original = (List<object>) mapper.Transform(source);
      var modified = new PersonDto {BirthDate = DateTime.Now, Id = 3};
      var operations = (DefaultOperationSet) mapper.Compare(original, modified);
      Assert.AreEqual(6, operations.Count());
      ValidateObjectCreation(modified, operations.First());
      ValidatePropertySettingOperation(modified, operations.Skip(1).First(), firstNameProperty,
        modified.FirstName);
      ValidatePropertySettingOperation(modified, operations.Skip(2).First(), lastNameProperty,
        modified.LastName);
      ValidatePropertySettingOperation(modified, operations.Skip(3).First(), birthDateProperty,
        modified.BirthDate);
      ValidateObjectRemoval(original[0], operations.Skip(4).First());
      ValidateObjectRemoval(original[1], operations.Skip(5).First());
    }

    [Test]
    public void ComparisonWhenModifiedIsCollectionButOriginalIsNotTest()
    {
      var mapper = GetPersonOrderMapper();
      var source = new Person {BirthDate = DateTime.Now, Id = 3};
      var original = (PersonDto) mapper.Transform(source);
      var modified = new List<PersonDto> {
        new PersonDto {BirthDate = DateTime.Now, Id = 1, FirstName = "A"},
        new PersonDto {BirthDate = DateTime.Now, Id = 2, FirstName = "B"}
      };
      var operations = (DefaultOperationSet) mapper.Compare(original, modified);
      Assert.AreEqual(9, operations.Count());
      ValidateObjectCreation(modified[0], operations.First());
      ValidateObjectCreation(modified[1], operations.Skip(1).First());
      ValidatePropertySettingOperation(modified[0], operations.Skip(2).First(), firstNameProperty,
        modified[0].FirstName);
      ValidatePropertySettingOperation(modified[0], operations.Skip(3).First(), lastNameProperty,
        modified[0].LastName);
      ValidatePropertySettingOperation(modified[0], operations.Skip(4).First(), birthDateProperty,
        modified[0].BirthDate);
      ValidatePropertySettingOperation(modified[1], operations.Skip(5).First(), firstNameProperty,
        modified[1].FirstName);
      ValidatePropertySettingOperation(modified[1], operations.Skip(6).First(), lastNameProperty,
        modified[1].LastName);
      ValidatePropertySettingOperation(modified[1], operations.Skip(7).First(), birthDateProperty,
        modified[1].BirthDate);
      ValidateObjectRemoval(original, operations.Skip(8).Single());
    }

    [Test]
    public void InheritedPropertyConverterMappingTest()
    {
      var mapper = GetCreatureHeirsMapperWithCustomConverters();
      var source = GetSourceCreatures();
      var target = ((List<object>) mapper.Transform(source)).Cast<CreatureDto>().ToList();
      Assert.AreEqual(source.Count, target.Count);
      for (var i = 0; i < target.Count; i++) {
        Assert.AreEqual(source[i].Id, target[i].Id);
        if (typeof (Mammal).IsAssignableFrom(source[i].GetType()))
          Assert.AreEqual(source[i].Name + inheritedFromMammal, target[i].Name);
        else if (typeof (FlyingInsect).IsAssignableFrom(source[i].GetType()))
          Assert.AreEqual(source[i].Name + inheritedFlyingInsect, target[i].Name);
        else
          Assert.AreEqual(source[i].Name + inherited, target[i].Name);
      }
      var sourceMammal = (Mammal) source[0];
      var targetMammal = (MammalDto) target[0];
      Assert.AreEqual(sourceMammal.Color, targetMammal.Color);
      Assert.AreEqual(sourceMammal.HasHair, targetMammal.HasHair);
      var sourceInsect = (Insect) source[1];
      var targetInsect = (InsectDto) target[1];
      Assert.AreEqual(sourceInsect.LegPairCount, targetInsect.LegPairCount);
      var sourceFlyingInsect = (FlyingInsect) source[3];
      var targetFlyingInsect = (FlyingInsectDto) target[3];
      Assert.AreEqual(sourceFlyingInsect.LegPairCount, targetFlyingInsect.LegPairCount);
      Assert.AreEqual(sourceFlyingInsect.WingPairCount, targetFlyingInsect.WingPairCount);
      var sourceLongBee = (LongBee) source[4];
      var targetLongBee = (LongBeeDto) target[4];
      Assert.AreEqual(sourceLongBee.LegPairCount, targetLongBee.LegPairCount);
      Assert.AreEqual(sourceLongBee.WingPairCount, targetLongBee.WingPairCount);
      Assert.AreEqual(sourceLongBee.Length, targetLongBee.Length);
      Assert.AreEqual(sourceLongBee.StripCount, targetLongBee.StripCount);
      var sourceCat = (Cat) source[5];
      var targetCat = (CatDto) target[5];
      Assert.AreEqual(sourceCat.Breed, targetCat.Breed);
      Assert.AreEqual(sourceCat.Color, targetCat.Color);
      Assert.AreEqual(sourceCat.HasHair, targetCat.HasHair);
      var sourceDolphin = (Dolphin) source[6];
      var targetDolphin = (DolphinDto) target[6];
      Assert.AreEqual(sourceDolphin.Color, targetDolphin.Color);
      Assert.AreEqual(sourceDolphin.HasHair, targetDolphin.HasHair);
      Assert.AreEqual(sourceDolphin.OceanAreal, targetDolphin.OceanAreal);
    }

    [Test]
    public void InheritedPropertyAttributesModelBuildingTest()
    {
      var mapper = GetCreatureHeirsMapperWithAttributesMapper();
      var model = mapper.MappingDescription;
      Func<Type, TargetPropertyDescription> namePropertyGetter =
        type => (TargetPropertyDescription) model.TargetTypes[type].Properties[type.GetProperty("Name")];
      Func<Type, TargetPropertyDescription> legPairCountPropertyGetter =
        type => (TargetPropertyDescription) model.TargetTypes[type]
          .Properties[type.GetProperty("LegPairCount")];
      
      var creatureDtoType = typeof (CreatureDto);
      var namePropertyDescription = namePropertyGetter.Invoke(creatureDtoType);
      Assert.IsFalse(namePropertyDescription.IsIgnored);
      Assert.IsTrue(namePropertyDescription.IsImmutable);
      var mammalDtoType = typeof (MammalDto);
      namePropertyDescription = namePropertyGetter.Invoke(mammalDtoType);
      Assert.IsTrue(namePropertyDescription.IsIgnored);
      Assert.IsFalse(namePropertyDescription.IsImmutable);
      namePropertyDescription = namePropertyGetter.Invoke(typeof (CatDto));
      Assert.IsTrue(namePropertyDescription.IsIgnored);
      Assert.IsFalse(namePropertyDescription.IsImmutable);
      namePropertyDescription = namePropertyGetter.Invoke(typeof (DolphinDto));
      Assert.IsTrue(namePropertyDescription.IsIgnored);
      Assert.IsFalse(namePropertyDescription.IsImmutable);
      namePropertyDescription = namePropertyGetter.Invoke(typeof (InsectDto));
      Assert.IsFalse(namePropertyDescription.IsIgnored);
      Assert.IsTrue(namePropertyDescription.IsImmutable);
      namePropertyDescription = namePropertyGetter.Invoke(typeof (FlyingInsectDto));
      Assert.IsFalse(namePropertyDescription.IsIgnored);
      Assert.IsTrue(namePropertyDescription.IsImmutable);
      namePropertyDescription = namePropertyGetter.Invoke(typeof (LongBeeDto));
      Assert.IsFalse(namePropertyDescription.IsIgnored);
      Assert.IsTrue(namePropertyDescription.IsImmutable);
      var legPairCountPropertyDescription = legPairCountPropertyGetter.Invoke(typeof (InsectDto));
      Assert.IsFalse(legPairCountPropertyDescription.IsIgnored);
      Assert.IsTrue(legPairCountPropertyDescription.IsImmutable);
      legPairCountPropertyDescription = legPairCountPropertyGetter.Invoke(typeof (FlyingInsectDto));
      Assert.IsFalse(legPairCountPropertyDescription.IsIgnored);
      Assert.IsTrue(legPairCountPropertyDescription.IsImmutable);
      legPairCountPropertyDescription = legPairCountPropertyGetter.Invoke(typeof (LongBeeDto));
      Assert.IsFalse(legPairCountPropertyDescription.IsIgnored);
      Assert.IsTrue(legPairCountPropertyDescription.IsImmutable);
    }

    [Test]
    public void ThrowWhenLimitOfGraphDepthHasBeenExceededTest()
    {
      const int depthLimit = 2;
      var mapper = GetRecursiveCompositionMapperWithGraphDepthLimit(depthLimit, GraphTruncationType.Throw);
      var source = new RecursiveComposition();
      source.Compose(3);
      AssertEx.ThrowsInvalidOperationException(() => mapper.Transform(source));
      source = new RecursiveComposition();
      source.Compose(2);
      var target = (RecursiveCompositionDto) mapper.Transform(source);
      Assert.AreEqual(source.Id, target.Id);
      Assert.AreEqual(source.Level, target.Level);
      Assert.AreEqual(source.Child.Id, target.Child.Id);
      Assert.AreEqual(source.Child.Level, target.Child.Level);
      Assert.AreEqual(source.Child.Child.Id, target.Child.Child.Id);
      Assert.AreEqual(source.Child.Child.Level, target.Child.Child.Level);

      mapper = GetRecursiveCompositionMapperWithGraphDepthLimit(0, GraphTruncationType.Throw);
      source = new RecursiveComposition();
      source.Compose(0);
      target = (RecursiveCompositionDto) mapper.Transform(source);
      Assert.AreEqual(source.Id, target.Id);
      Assert.AreEqual(source.Level, target.Level);
      Assert.AreEqual(source.Child, target.Child);
    }

    [Test]
    public void SetNullWhenLimitOfGraphDepthHasBeenExceededTest()
    {
      const int depthLimit = 2;
      var mapper = GetRecursiveCompositionMapperWithGraphDepthLimit(depthLimit, GraphTruncationType.SetNull);
      var source = new RecursiveComposition();
      source.Compose(3);
      var target = (RecursiveCompositionDto) mapper.Transform(source);
      Assert.IsNotNull(source.Child.Child.Child);
      Assert.IsNull(target.Child.Child.Child);
    }

    [Test]
    public void HandleChangeOfImmutablePropertyTest()
    {
      var mapper = new DefaultMapper();
      mapper.MapType<Person, PersonDto, int>(p => p.Id, p => p.Id).Immutable(p => p.FirstName).Complete();
      var source = GetSourcePerson(1);
      var target = (PersonDto) mapper.Transform(source);
      var modified = (PersonDto) target.Clone();
      modified.FirstName += "!";
      AssertEx.ThrowsInvalidOperationException(() => mapper.Compare(target, modified));

      mapper = new DefaultMapper();
      mapper.MapType<Person, PersonDto, int>(p => p.Id, p => p.Id).Immutable(p => p.FirstName)
        .Ignore(p => p.FirstName).Complete();
      target = (PersonDto) mapper.Transform(source);
      modified = (PersonDto) target.Clone();
      modified.FirstName += "!";
      var operations = mapper.Compare(target, modified);
      Assert.IsTrue(operations.IsEmpty);
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
      ((DefaultOperationSet) mapper.Compare(original, modified)).Apply(validator);
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
      ((DefaultOperationSet) mapper.Compare(target, clone)).Apply(validator);
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
      result
        .MapType<Person, PersonDto, int>(p => p.Id, p => p.Id)
        .MapType<Order, OrderDto, String>(o => o.Id.ToString(), o => o.Id).Complete();
      return result;
    }

    private static DefaultMapper GetAuthorBookMapper()
    {
      var result = new DefaultMapper();
      result.MapType<Author, AuthorDto, Guid>(a => a.Id, a => a.Id)
          .MapProperty(a => a.Name + "!!!", a => a.Name)
        .MapType<Book, BookDto, string>(b => b.ISBN, b => b.ISBN)
          .MapProperty(b => b.Title.Text, b => b.TitleText)
          .MapProperty(b => new TitleDto {Id = b.Title.Id, Text = b.Title.Text}, b => b.Title)
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
      result.MapType<Ignorable, IgnorableDto, Guid>(i => i.Id, i => i.Id)
        .Ignore(i => i.Auxiliary).Ignore(i => i.Ignored).Ignore(i => i.IgnoredReference)
        .MapType<IgnorableSubordinate, IgnorableSubordinateDto, Guid>(s => s.Id, s => s.Id).Complete();
      return result;
    }

    private static DefaultMapper GetCreatureHeirsMapperWithCustomConverters()
    {
      var result = new DefaultMapper();
      result.MapType<Creature, CreatureDto, Guid>(c => c.Id, c => c.Id)
          .MapProperty(c => c.Name + inherited, c => c.Name)
        .MapType<Insect, InsectDto, Guid>(i => i.Id, i => i.Id)
        .Inherit<InsectDto, LongBee, LongBeeDto>()
        .MapType<FlyingInsect, FlyingInsectDto, Guid>(f => f.Id, f => f.Id)
          .MapProperty(f => f.Name + inheritedFlyingInsect, f => f.Name)
        .MapType<Mammal, MammalDto, Guid>(m => m.Id, m => m.Id)
          .MapProperty(m => m.Name + inheritedFromMammal, m => m.Name)
        .Inherit<MammalDto, Cat, CatDto>()
        .Inherit<MammalDto, Dolphin, DolphinDto>().Complete();
      return result;
    }

    private static DefaultMapper GetCreatureHeirsMapperWithAttributesMapper()
    {
      var result = new DefaultMapper();
      result.MapType<Creature, CreatureDto, Guid>(c => c.Id, c => c.Id)
          .MapProperty(c => c.Name + inherited, c => c.Name)
        .MapType<Insect, InsectDto, Guid>(i => i.Id, i => i.Id)
          .Immutable(i => i.LegPairCount)
        .Inherit<InsectDto, LongBee, LongBeeDto>()
        .MapType<FlyingInsect, FlyingInsectDto, Guid>(f => f.Id, f => f.Id)
        .MapType<Mammal, MammalDto, Guid>(m => m.Id, m => m.Id)
          .Ignore(m => m.Name)
        .Inherit<MammalDto, Cat, CatDto>()
        .Inherit<MammalDto, Dolphin, DolphinDto>().Complete();
      return result;
    }

    private static DefaultMapper GetRecursiveCompositionMapperWithGraphDepthLimit(
      int depthLimit, GraphTruncationType action)
    {
      var mapperSettings = new MapperSettings {GraphTruncationType = action, GraphDepthLimit = depthLimit};
      var result = new DefaultMapper(mapperSettings);
      result.MapType<RecursiveComposition, RecursiveCompositionDto, Guid>(r => r.Id, r => r.Id).Complete();
      return result;
    }

    private static Person GetSourcePerson()
    {
      return GetSourcePerson(3);
    }

    private static Person GetSourcePerson(int id)
    {
      return new Person {
        BirthDate = DateTime.Now.AddYears(-20), FirstName = "John", LastName = "Smith", Id = id
      };
    }

    private static Order GetSourceOrder()
    {
      return GetSourceOrder(3);
    }

    private static Order GetSourceOrder(int personId)
    {
      return new Order {
        Customer = GetSourcePerson(personId), Id = Guid.NewGuid(), ShipDate = DateTime.Today.AddMonths(3)
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

    private static Ignorable GetIgnorableSource()
    {
      var ignoredSubordinate = new IgnorableSubordinate {Id = Guid.NewGuid(), Date = DateTime.Now};
      var includedSubordinate = new IgnorableSubordinate {Id = Guid.NewGuid(), Date = DateTime.Now.AddDays(10)};
      return new Ignorable {
        Id = Guid.NewGuid(), Ignored = "I", IgnoredReference = ignoredSubordinate,
        IncludedReference = includedSubordinate
      };
    }

    private static List<Creature> GetSourceCreatures()
    {
      var result = new List<Creature>();
      result.Add(new Mammal {Color = "Green", Name = "A"});
      result.Add(new Insect {LegPairCount = 3, Name = "B"});
      result.Add(new Creature {Name = "C"});
      result.Add(new FlyingInsect {LegPairCount = 2, Name = "IA"});
      result.Add(new LongBee {Length = 1, Name = "IB", StripCount = 3});
      result.Add(new Cat {Breed = "Siam", HasHair = true});
      result.Add(new Dolphin {Color = "Grey", OceanAreal = "Pacific", HasHair = false});
      return result;
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

    private static void ValidateObjectCreation(object obj, OperationInfo operationInfo)
    {
      ValidateObjectOperation(obj, operationInfo, OperationType.CreateObject);
    }

    private static void ValidateObjectRemoval(object obj, OperationInfo operationInfo)
    {
      ValidateObjectOperation(obj, operationInfo, OperationType.RemoveObject);
    }

    private static void ValidateObjectOperation(object obj, OperationInfo operationInfo,
      OperationType operationType)
    {
      Assert.AreEqual(obj, operationInfo.Object);
      Assert.AreEqual(operationType, operationInfo.Type);
      Assert.IsNull(operationInfo.Property);
      Assert.IsNull(operationInfo.Value);
    }

    private static void ValidateItemAdditionOperation(object obj, OperationInfo operationInfo,
      PropertyInfo propertyInfo, object item)
    {
      ValidatePropertyOperation(obj, operationInfo, propertyInfo, OperationType.AddItem, item);
    }

    private static void ValidateItemRemovalOperation(object obj, OperationInfo operationInfo,
      PropertyInfo propertyInfo, object item)
    {
      ValidatePropertyOperation(obj, operationInfo, propertyInfo, OperationType.RemoveItem, item);
    }

    private static void ValidatePropertySettingOperation(object obj, OperationInfo operationInfo,
      PropertyInfo propertyInfo, object value)
    {
      ValidatePropertyOperation(obj, operationInfo, propertyInfo, OperationType.SetProperty, value);
    }

    private static void ValidatePropertyOperation(object obj, OperationInfo operationInfo,
      PropertyInfo propertyInfo, OperationType operationType, object value)
    {
      Assert.AreSame(obj, operationInfo.Object);
      Assert.AreEqual(value, operationInfo.Value);
      Assert.AreEqual(propertyInfo, operationInfo.Property.SystemProperty);
      Assert.AreEqual(operationType, operationInfo.Type);
    }
  }
}