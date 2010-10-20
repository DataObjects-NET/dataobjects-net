// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.01.14

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using NUnit.Framework;
using Xtensive.ObjectMapping;
using Xtensive.Tests.ObjectMapping.SourceModel;
using Xtensive.Tests.ObjectMapping.TargetModel;

namespace Xtensive.Tests.ObjectMapping
{
  [TestFixture]
  public sealed class MapperHandlingNullTests : MapperTestBase
  {
    [Test]
    public void TransformationWhenGraphRootIsNullTest()
    {
      var mapper = GetPersonOrderMapper();
      Assert.IsNull(mapper.Transform(null));
      Action<Operation> validator = descriptor => Assert.Fail();
      ((DefaultOperationLog) mapper.Compare(null, null).Operations).ForEach(validator);
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
      Action<Operation> validator = descriptor => {
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
              orderPropertyCounts[descriptor.PropertyPath[0].SystemProperty.Name] += 1;
          else if (ReferenceEquals(target.Customer, descriptor.Object))
              customerPropertyCounts[descriptor.PropertyPath[0].SystemProperty.Name] += 1;
          else
            Assert.Fail();
          break;
        default:
            Assert.Fail();
          break;
        }
        eventRaisingCount++;
      };
      ((DefaultOperationLog) mapper.Compare(null, target).Operations).ForEach(validator);
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
      Action<Operation> validator = descriptor => {
        Assert.AreEqual(OperationType.RemoveObject, descriptor.Type);
        eventRaisingCount++;
        if (ReferenceEquals(target, descriptor.Object))
          orderCreated = true;
        else if (ReferenceEquals(target.Customer, descriptor.Object))
          personCreated = true;
        else
          Assert.Fail();
      };
      ((DefaultOperationLog) mapper.Compare(target, null).Operations).ForEach(validator);
      Assert.AreEqual(2, eventRaisingCount);
      Assert.IsTrue(personCreated);
      Assert.IsTrue(orderCreated);
    }

    [Test]
    public void ComparisonWhenBothOfGraphRootsAreNullTest()
    {
      var mapper = GetPersonOrderMapper();
      var operations = mapper.Compare(null, null).Operations;
      Assert.IsTrue(operations.Count==0);
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
      var operations = (DefaultOperationLog) mapper.Compare(originalOrder, modifiedOrder).Operations;
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
      ValidatePropertySettingOperation(originalOrder, customerSettingOperation, CustomerProperty,
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
      var operations = (DefaultOperationLog) mapper.Compare(originalOrder, modifiedOrder).Operations;
      Assert.AreEqual(2, operations.Count());
      var customerSettingOperation = operations.First();
      ValidatePropertySettingOperation(originalOrder, customerSettingOperation, CustomerProperty, null);
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
      var operations = (DefaultOperationLog) mapper.Compare(original, modified).Operations;
      Assert.IsTrue(operations.Count==0);
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
      var operations = (DefaultOperationLog) mapper.Compare(original, modified).Operations;
      Assert.AreEqual(2, operations.Count());
      var removedAnimal = original.Pets.Single();
      var itemRemovalOperation = operations.First();
      ValidateItemRemovalOperation(original, itemRemovalOperation, PetsProperty, removedAnimal);
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
      var operations = (DefaultOperationLog) mapper.Compare(original, modified).Operations;
      Assert.AreEqual(3, operations.Count());
      var animalCreationOperation = operations.First();
      ValidateObjectCreation(createdAnimal, animalCreationOperation);
      var propertyInfo = typeof (AnimalDto).GetProperty("Name");
      var nameSettingOperation = operations.Skip(1).First();
      ValidatePropertySettingOperation(createdAnimal, nameSettingOperation, propertyInfo, createdAnimal.Name);
      var petAdditionOperation = operations.Skip(2).Single();
      ValidateItemAdditionOperation(original, petAdditionOperation, PetsProperty, createdAnimal);
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
      var operations = (DefaultOperationLog) mapper.Compare(original, modified).Operations;
      Assert.AreEqual(2, operations.Count());
      var removedAnimal = original.Pets.Single();
      var itemRemovalOperation = operations.First();
      ValidateItemRemovalOperation(original, itemRemovalOperation, PetsProperty, removedAnimal);
      var animalRemovalOperation = operations.Skip(1).Single();
      ValidateObjectRemoval(removedAnimal, animalRemovalOperation);
    }

    [Test]
    public void TransformationWhenNullablePropertyHasNullValueTest()
    {
      var mapping = new MappingBuilder()
        .MapType<NullableDateTimeContainer, NullableDateTimeContainerDto, Guid>(n => n.Id, n => n.Id)
        .Build();

      var source = new NullableDateTimeContainer {NullableDateTime = null};
      var target = (NullableDateTimeContainerDto) new DefaultMapper(mapping).Transform(source);

      Assert.AreEqual(source.Id, target.Id);
      Assert.AreEqual(null, target.NullableDateTime);
    }

    [Test]
    public void ComparisonWhenNullablePropertyHasNullValueTest()
    {
      var mapping = new MappingBuilder()
        .MapType<NullableDateTimeContainer, NullableDateTimeContainerDto, Guid>(n => n.Id, n => n.Id)
        .Build();

      var source = new NullableDateTimeContainer {NullableDateTime = DateTime.Now};
      var original = (NullableDateTimeContainerDto) new DefaultMapper(mapping).Transform(source);
      var modified = new NullableDateTimeContainerDto {
        Id = original.Id,
        NullableDateTime = null
      };

      var operations = ((DefaultOperationLog) new DefaultMapper(mapping)
        .Compare(original, modified).Operations).ToList();
      Assert.AreEqual(1, operations.Count);
      ValidatePropertyOperation(original, operations[0], n => n.NullableDateTime,
        null, OperationType.SetProperty);
    }
  }
}