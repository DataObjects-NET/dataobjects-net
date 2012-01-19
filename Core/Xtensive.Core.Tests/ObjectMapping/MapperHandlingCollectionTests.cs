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
using Xtensive.ObjectMapping.Model;
using Xtensive.Testing;
using Xtensive.Tests.ObjectMapping.SourceModel;
using Xtensive.Tests.ObjectMapping.TargetModel;
using Action=Xtensive.Tests.ObjectMapping.SourceModel.Action;

namespace Xtensive.Tests.ObjectMapping
{
  [TestFixture]
  public sealed class MapperHandlingCollectionTests : MapperTestBase
  {
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
    public void TransformationOfPrimitiveCustomCollectionTest()
    {
      var mapping = new MappingBuilder()
        .MapType<PrimitiveCollectionContainer, PrimitiveCollectionContainerDto, Guid>(c => c.Id, c => c.Id)
        .Build();
      var mapper = new DefaultMapper(mapping);
      var source = new PrimitiveCollectionContainer();
      source.Collection.Add(3);
      source.Collection.Add(2);
      source.Collection.Add(1);
      var target = (PrimitiveCollectionContainerDto) mapper.Transform(source);
      Assert.AreEqual(3, target.Collection[0]);
      Assert.AreEqual(2, target.Collection[1]);
      Assert.AreEqual(1, target.Collection[2]);
    }

    [Test]
    public void TransformationOfComplexCustomCollectionTest()
    {
      var mapping = new MappingBuilder().MapType<ComplexCollectionContainer, ComplexCollectionContainerDto, Guid>(c => c.Id, c => c.Id)
        .MapType<Person, PersonDto, int>(p => p.Id, p => p.Id).Build();
      var mapper = new DefaultMapper(mapping);
      var source = new ComplexCollectionContainer();
      source.Collection.Add(GetSourcePerson(3));
      source.Collection.Add(GetSourcePerson(2));
      source.Collection.Add(GetSourcePerson(1));
      var target = (ComplexCollectionContainerDto) mapper.Transform(source);
      Assert.AreEqual(3, target.Collection[0].Id);
      Assert.AreEqual(2, target.Collection[1].Id);
      Assert.AreEqual(1, target.Collection[2].Id);
    }

    [Test]
    public void TransformationWhenGraphRootContainsPrimitiveObjectsTest()
    {
      var mapper = GetPersonStructureMapper();
      Person person;
      int intItem;
      string stringItem;
      Structure structureItem;
      var source = GetSourcePersonStructure(out person, out intItem, out stringItem, out structureItem);

      var target = (List<object>) mapper.Transform(source);
      Assert.AreEqual(source.Count, target.Count);
      Assert.AreEqual(((Person) source[0]).Id, ((PersonDto) target[0]).Id);
      Assert.AreEqual(((Person) source[0]).FirstName, ((PersonDto) target[0]).FirstName);
      Assert.AreEqual(intItem, target[1]);
      Assert.AreEqual(stringItem, target[2]);
      var targetStructureItem = (StructureDto) target[3];
      Assert.AreEqual(structureItem.DateTime, targetStructureItem.DateTime);
      Assert.AreEqual(structureItem.Int, targetStructureItem.Int);
      Assert.AreEqual(structureItem.String, targetStructureItem.String);

      var intArray = new[] {8, 9, 0};
      source.Add(intArray);
      AssertEx.ThrowsArgumentException(() => mapper.Transform(source));
    }

    [Test]
    public void ComparisonWhenRootsContainsValueObjectsTest()
    {
      var mapper = GetPersonStructureMapper();
      Person person;
      int intItem;
      string stringItem;
      Structure structureItem;
      var source = GetSourcePersonStructure(out person, out intItem, out stringItem, out structureItem);
      var original = (List<object>) mapper.Transform(source);
      var modified = Clone(original);
      var newFirstName = ((PersonDto) modified[0]).FirstName + "Modified0";
      ((PersonDto) modified[0]).FirstName = newFirstName;
      modified[1] = ((int) modified[1]) + 3;
      modified[2] = ((string) modified[2]) + "Modified1";
      var modifiedStructureItem = (StructureDto) modified[3];
      modifiedStructureItem.Int += 5;
      var operations = ((DefaultOperationLog) mapper.Compare(original, modified).Operations).ToList();
      Assert.AreEqual(1, operations.Count);
      ValidatePropertyOperation((PersonDto) original[0], operations[0], p => p.FirstName,
        newFirstName, OperationType.SetProperty);

      var intArray = new[] {8, 9, 0};
      modified.Add(intArray);
      AssertEx.ThrowsArgumentException(() => mapper.Compare(original, modified));
    }

    [Test]
    public void TransformationOfValueTypesCollectionTest()
    {
      var mapper = GetAccountMapper();
      var source = GetSourceAccount();
      var target = (AccountDto) mapper.Transform(source);
      Assert.AreEqual(source.Id, target.Id);
      Assert.IsTrue(source.AccessRights
        .All(ar => target.AccessRights
          .Where(tar => tar.Action==ar.Action && tar.ObjectId.SequenceEqual(ar.ObjectId)).Count()==1));
      Assert.IsTrue(source.PasswordHash.SequenceEqual(target.PasswordHash));
    }

    [Test]
    public void ComparisonOfValueTypesCollectionTest()
    {
      var mapper = GetAccountMapper();
      var source = GetSourceAccount();
      var original = (AccountDto) mapper.Transform(source);
      var modified = Clone(original);
      modified.PasswordHash[3] += 5;
      var oldAccessRight = modified.AccessRights[0];
      modified.AccessRights[0] = new AccessRightDto {
        Action = SourceModel.Action.Write, ObjectId = oldAccessRight.ObjectId
      };
      modified.AccessRights[0].ObjectId[2] += 7;
      var operations = ((DefaultOperationLog) mapper.Compare(original, modified).Operations).ToList();
      Assert.AreEqual(1, operations.Count);
      ValidatePropertyOperation(original, operations[0], a => a.PasswordHash,
        modified.PasswordHash, OperationType.SetProperty);
    }

    [Test]
    public void EnsureDepthLimitWhenCollectionPropertyLocatedOnGraphTruncationPointTest()
    {
      var settings = new MapperSettings {
        GraphDepthLimit = 1, GraphTruncationType = GraphTruncationType.SetDefaultValue
      };
      var mapping = new MappingBuilder()
        .MapType<CollectionContainer, CollectionContainerDto, Guid>(c => c.Id, c => c.Id).Build();
      var mapper = new DefaultMapper(mapping, settings);
      var source0 = new CollectionContainer {AuxInt = 1};
      var source10 = new CollectionContainer {AuxInt = 11};
      var source11 = new CollectionContainer {AuxInt = 12};
      source0.Collection.Add(source10);
      source0.Collection.Add(source11);
      var source20 = new CollectionContainer {AuxInt = 20};
      var source21 = new CollectionContainer {AuxInt = 21};
      source10.Collection.Add(source20);
      source10.Collection.Add(source21);
      var target = (CollectionContainerDto) mapper.Transform(source0);
      Assert.IsNull(target.Collection[0].Collection);
      Assert.IsNull(target.Collection[1].Collection);
    }

    [Test]
    public void ArrayTransformationTest()
    {
      var mapping = GetArrayContainerMapping();
      var source = GetSourceArrayContainer();
      var mapper = new DefaultMapper(mapping);
      var target = (ArrayContainerDto) mapper.Transform(source);
      Assert.AreEqual(source.Id, target.Id);
      Assert.IsTrue(source.IntArray.SequenceEqual(target.IntArray));
      Action<ArrayElement[], ArrayElementDto[]> validator = null;
      validator = (sourceArray, targetArray) => {
        Assert.AreEqual(sourceArray.Length, targetArray.Length);
        for (var i = 0; i < sourceArray.Length; i++) {
          var sourceItem = sourceArray[i];
          var targetItem = targetArray[i];
          Assert.AreEqual(sourceItem.Id, targetItem.Id);
          Assert.AreEqual(sourceItem.Aux, targetItem.Aux);
          if (sourceItem.NestedElements!=null)
            validator(sourceItem.NestedElements, targetItem.NestedElements);
        }
      };
      validator.Invoke(source.EntityArray, target.EntityArray);
    }

    [Test]
    public void ArrayComparisonTest()
    {
      var mapping = GetArrayContainerMapping();
      var source = GetSourceArrayContainer();
      var mapper = new DefaultMapper(mapping);
      var original = (ArrayContainerDto) mapper.Transform(source);
      var modified = Clone(original);
      var removedElement = original.EntityArray[1].NestedElements[0];
      var newArrayElementDto = new ArrayElementDto {
        Id = Guid.NewGuid(), Aux = "NewElement", NestedElements = new[] {
          new ArrayElementDto {Id = Guid.NewGuid(), Aux = "NewElement10"},
          new ArrayElementDto {Id = Guid.NewGuid(), Aux = "NewElement11"}
        }
      };
      modified.EntityArray[1].NestedElements[0] = newArrayElementDto;
      var operations = ((DefaultOperationLog) mapper.Compare(original, modified).Operations).ToList();
      Assert.AreEqual(11, operations.Count);
      ValidateObjectCreation(newArrayElementDto, operations[0]);
      ValidateObjectCreation(newArrayElementDto.NestedElements[0], operations[1]);
      ValidateObjectCreation(newArrayElementDto.NestedElements[1], operations[2]);
      ValidatePropertyOperation(newArrayElementDto, operations[3], e => e.Aux,
        newArrayElementDto.Aux, OperationType.SetProperty);
      var nestedElementsProperty = typeof (ArrayElementDto).GetProperty("NestedElements");
      ValidateItemAdditionOperation(newArrayElementDto, operations[4], nestedElementsProperty,
        newArrayElementDto.NestedElements[0]);
      ValidateItemAdditionOperation(newArrayElementDto, operations[5], nestedElementsProperty,
        newArrayElementDto.NestedElements[1]);
      ValidatePropertyOperation(newArrayElementDto.NestedElements[0], operations[6],
        e => e.Aux, newArrayElementDto.NestedElements[0].Aux, OperationType.SetProperty);
      ValidatePropertyOperation(newArrayElementDto.NestedElements[1], operations[7],
        e => e.Aux, newArrayElementDto.NestedElements[1].Aux, OperationType.SetProperty);
      ValidateItemAdditionOperation(original.EntityArray[1], operations[8], nestedElementsProperty,
        newArrayElementDto);
      ValidateItemRemovalOperation(original.EntityArray[1], operations[9], nestedElementsProperty,
        removedElement);
      ValidateObjectRemoval(removedElement, operations[10]);
    }
    
    private static DefaultMapper GetPersonStructureMapper()
    {
      var mapping = new MappingBuilder().MapType<Person, PersonDto, int>(p => p.Id, p => p.Id)
        .MapStructure<Structure, StructureDto>().Build();
      return new DefaultMapper(mapping);
    }

    private static DefaultMapper GetAccountMapper()
    {
      var mapping = new MappingBuilder()
        .MapType<Account, AccountDto, Guid>(a => a.Id, a => a.Id)
          .TrackChanges(a => a.AccessRights, false)
        .MapStructure<AccessRight, AccessRightDto>()
        .Build();
      return new DefaultMapper(mapping);
    }

    private static List<object> GetSourcePersonStructure(out Person person, out int intItem,
      out string stringItem, out Structure structureItem)
    {
      var source = new List<object>();
      person = new Person {BirthDate = DateTime.Now, FirstName = "Name"};
      source.Add(person);
      intItem = 5;
      source.Add(intItem);
      stringItem = "String";
      source.Add(stringItem);
      structureItem = new Structure {DateTime = DateTime.Now, Int = 3, String = "String"};
      source.Add(structureItem);
      return source;
    }

    private static MappingDescription GetArrayContainerMapping()
    {
      return new MappingBuilder()
        .MapType<ArrayContainer, ArrayContainerDto, Guid>(a => a.Id, a => a.Id)
        .MapType<ArrayElement, ArrayElementDto, Guid>(a => a.Id, a => a.Id)
        .Build();
    }

    private static Account GetSourceAccount()
    {
      return new Account {
        AccessRights = new List<AccessRight> {
          new AccessRight {Action = SourceModel.Action.Read, ObjectId = new[] {1L, 2, 3, 4, 5}},
          new AccessRight {Action = SourceModel.Action.Write, ObjectId = new[] {0L, 9, 8, 7, 6}}
        },
        PasswordHash = new byte[] {1, 8, 89, 29, 50, 77}
      };
    }

    private static ArrayContainer GetSourceArrayContainer()
    {
      return new ArrayContainer {IntArray = new[] {6, 8, 2, 9, 7}, EntityArray = new[] {
          new ArrayElement {
            Aux = "Element00", NestedElements = new[] {
              new ArrayElement {Aux = "Element10"}, new ArrayElement {Aux = "Element11"},
              new ArrayElement {Aux = "Element12"}
          }},
          new ArrayElement {Aux = "Element01", NestedElements = new[] {
              new ArrayElement {Aux = "Element13"}, new ArrayElement {Aux = "Element14"},
              new ArrayElement {Aux = "Element15"}
          }}
        }
      };
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
      Action<Operation> validator = descriptor => {
        eventRaisingCount++;
        switch (descriptor.Type) {
        case OperationType.AddItem:
          Assert.AreEqual(original, descriptor.Object);
          Assert.AreEqual(petsName, descriptor.PropertyPath[0].SystemProperty.Name);
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
          Assert.AreEqual(petsName, descriptor.PropertyPath[0].SystemProperty.Name);
          Assert.AreEqual(original, descriptor.Object);
          break;
        case OperationType.CreateObject:
          creationPublished = true;
          Assert.AreEqual(newAnimal, descriptor.Object);
          Assert.IsNull(descriptor.PropertyPath);
          Assert.IsNull(descriptor.Value);
          break;
        case OperationType.RemoveObject:
          if (ReferenceEquals(removedAnimal0, descriptor.Object))
            removalPublished0 = true;
          else if (ReferenceEquals(removedAnimal1, descriptor.Object))
            removalPublished1 = true;
          else
            Assert.Fail();
          Assert.IsNull(descriptor.PropertyPath);
          Assert.IsNull(descriptor.Value);
          break;
        case OperationType.SetProperty:
          petPropertyCounts[descriptor.PropertyPath[0].SystemProperty.Name] += 1;
          Assert.AreSame(newAnimal, descriptor.Object);
          var expectedValue = descriptor.PropertyPath[0].SystemProperty.GetValue(newAnimal, null);
          Assert.AreEqual(expectedValue, descriptor.Value);
          break;
        default:
          Assert.Fail();
          break;
        }
      };
      ((DefaultOperationLog) mapper.Compare(original, modified).Operations).ForEach(validator);
      Assert.AreEqual(7, eventRaisingCount);
      Assert.IsTrue(itemRemovalPublished0);
      Assert.IsTrue(itemRemovalPublished1);
      Assert.IsTrue(removalPublished0);
      Assert.IsTrue(removalPublished1);
      Assert.IsTrue(creationPublished);
      Assert.IsTrue(additionPublished);
    }
  }
}