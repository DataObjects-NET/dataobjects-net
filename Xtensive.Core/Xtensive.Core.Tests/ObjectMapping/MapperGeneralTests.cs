// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.09

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.ObjectMapping;
using Xtensive.ObjectMapping.Model;
using Xtensive.Testing;
using Xtensive.Tests.ObjectMapping.SourceModel;
using Xtensive.Tests.ObjectMapping.TargetModel;

namespace Xtensive.Tests.ObjectMapping
{
  [TestFixture]
  public sealed class MapperGeneralTests : MapperTestBase
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
      Action<Operation> validator = descriptor => {
        eventRaisingCount++;
        switch (descriptor.PropertyPath[0].SystemProperty.Name) {
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
      ((DefaultOperationLog) mapper.Compare(target, clone).Operations).ForEach(validator);
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
      Action<Operation> validator = descriptor => {
        eventRaisingCount++;
        if (ReferenceEquals(target, descriptor.Object)) {
          Assert.AreEqual("ShipDate", descriptor.PropertyPath[0].SystemProperty.Name);
          Assert.AreEqual(modifiedShipDate, descriptor.Value);
        }
        else if (ReferenceEquals(target.Customer, descriptor.Object)) {
          Assert.AreEqual("FirstName", descriptor.PropertyPath[0].SystemProperty.Name);
          Assert.AreEqual(modifiedFirstName, descriptor.Value);
        }
        else
          Assert.Fail();
      };
      ((DefaultOperationLog) mapper.Compare(target, clone).Operations).ForEach(validator);
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
      var operations = mapper.Compare(original, modified).Operations;
      Assert.IsTrue(operations.Count==0);
      var newDate = modified.IncludedReference.Date.AddYears(20);
      modified.IncludedReference.Date = newDate;
      operations = mapper.Compare(original, modified).Operations;
      Assert.IsFalse(operations.Count==0);
      var operation = ((DefaultOperationLog) operations).Single();
      Assert.AreEqual(original.IncludedReference, operation.Object);
      Assert.AreEqual("Date", operation.PropertyPath[0].SystemProperty.Name);
      Assert.AreEqual(OperationType.SetProperty, operation.Type);
      Assert.AreEqual(newDate, operation.Value);
      Assert.AreNotEqual(original.IgnoredReference.Id, modified.IgnoredReference.Id);
      Assert.AreNotEqual(original.IgnoredReference.Date, modified.IgnoredReference.Date);
      Assert.AreNotEqual(original.Auxiliary, modified.Auxiliary);
      Assert.AreNotEqual(original.Ignored, modified.Ignored);
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
      EnumerableExtensions.ForEach(target, ta => Assert.IsTrue(source.Any(sa => sa.Id == ta.Id && sa.Name + "!!!" == ta.Name
        && sa.Book.ISBN == ta.Book.ISBN && sa.Book.Price == ta.Book.Price
        && sa.Book.Title.Id == ta.Book.Title.Id && sa.Book.Title.Text == ta.Book.Title.Text
        && sa.Book.Title.Text == ta.Book.TitleText)));
    }

    [Test]
    public void TransformationWhenGraphRootIsEnumerableButNotCollectionTest()
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
      var transformedList = (List<object>) mapper.Transform(source.AsQueryable());
      var target = transformedList.Cast<AuthorDto>().ToList();
      Assert.AreEqual(source.Count, target.Count);
      EnumerableExtensions.ForEach(target, ta => Assert.IsTrue(source.Any(sa => sa.Id == ta.Id && sa.Name + "!!!" == ta.Name
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
      var operations = (DefaultOperationLog) mapper.Compare(original, modified).Operations;
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
      var operations = (DefaultOperationLog) mapper.Compare(original, modified).Operations;
      Assert.AreEqual(6, operations.Count());
      ValidateObjectCreation(modified, operations.First());
      ValidatePropertySettingOperation(modified, operations.Skip(1).First(), FirstNameProperty,
        modified.FirstName);
      ValidatePropertySettingOperation(modified, operations.Skip(2).First(), LastNameProperty,
        modified.LastName);
      ValidatePropertySettingOperation(modified, operations.Skip(3).First(), BirthDateProperty,
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
      var operations = (DefaultOperationLog) mapper.Compare(original, modified).Operations;
      Assert.AreEqual(9, operations.Count());
      ValidateObjectCreation(modified[0], operations.First());
      ValidateObjectCreation(modified[1], operations.Skip(1).First());
      ValidatePropertySettingOperation(modified[0], operations.Skip(2).First(), FirstNameProperty,
        modified[0].FirstName);
      ValidatePropertySettingOperation(modified[0], operations.Skip(3).First(), LastNameProperty,
        modified[0].LastName);
      ValidatePropertySettingOperation(modified[0], operations.Skip(4).First(), BirthDateProperty,
        modified[0].BirthDate);
      ValidatePropertySettingOperation(modified[1], operations.Skip(5).First(), FirstNameProperty,
        modified[1].FirstName);
      ValidatePropertySettingOperation(modified[1], operations.Skip(6).First(), LastNameProperty,
        modified[1].LastName);
      ValidatePropertySettingOperation(modified[1], operations.Skip(7).First(), BirthDateProperty,
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
        type => (TargetPropertyDescription) model.GetTargetType(type).Properties[type.GetProperty("Name")];
      Func<Type, TargetPropertyDescription> legPairCountPropertyGetter =
        type => (TargetPropertyDescription) model.GetTargetType(type)
          .Properties[type.GetProperty("LegPairCount")];
      
      var creatureDtoType = typeof (CreatureDto);
      var namePropertyDescription = namePropertyGetter.Invoke(creatureDtoType);
      Assert.IsFalse(namePropertyDescription.IsIgnored);
      Assert.IsTrue(namePropertyDescription.IsChangeTrackingDisabled);
      var mammalDtoType = typeof (MammalDto);
      namePropertyDescription = namePropertyGetter.Invoke(mammalDtoType);
      Assert.IsTrue(namePropertyDescription.IsIgnored);
      Assert.IsFalse(namePropertyDescription.IsChangeTrackingDisabled);
      namePropertyDescription = namePropertyGetter.Invoke(typeof (CatDto));
      Assert.IsTrue(namePropertyDescription.IsIgnored);
      Assert.IsFalse(namePropertyDescription.IsChangeTrackingDisabled);
      namePropertyDescription = namePropertyGetter.Invoke(typeof (DolphinDto));
      Assert.IsTrue(namePropertyDescription.IsIgnored);
      Assert.IsFalse(namePropertyDescription.IsChangeTrackingDisabled);
      namePropertyDescription = namePropertyGetter.Invoke(typeof (InsectDto));
      Assert.IsFalse(namePropertyDescription.IsIgnored);
      Assert.IsTrue(namePropertyDescription.IsChangeTrackingDisabled);
      namePropertyDescription = namePropertyGetter.Invoke(typeof (FlyingInsectDto));
      Assert.IsFalse(namePropertyDescription.IsIgnored);
      Assert.IsTrue(namePropertyDescription.IsChangeTrackingDisabled);
      namePropertyDescription = namePropertyGetter.Invoke(typeof (LongBeeDto));
      Assert.IsFalse(namePropertyDescription.IsIgnored);
      Assert.IsTrue(namePropertyDescription.IsChangeTrackingDisabled);
      var legPairCountPropertyDescription = legPairCountPropertyGetter.Invoke(typeof (InsectDto));
      Assert.IsFalse(legPairCountPropertyDescription.IsIgnored);
      Assert.IsTrue(legPairCountPropertyDescription.IsChangeTrackingDisabled);
      legPairCountPropertyDescription = legPairCountPropertyGetter.Invoke(typeof (FlyingInsectDto));
      Assert.IsFalse(legPairCountPropertyDescription.IsIgnored);
      Assert.IsTrue(legPairCountPropertyDescription.IsChangeTrackingDisabled);
      legPairCountPropertyDescription = legPairCountPropertyGetter.Invoke(typeof (LongBeeDto));
      Assert.IsFalse(legPairCountPropertyDescription.IsIgnored);
      Assert.IsTrue(legPairCountPropertyDescription.IsChangeTrackingDisabled);
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
    public void SetDefaultValueWhenLimitOfGraphDepthHasBeenExceededTest()
    {
      const int depthLimit = 2;
      var mapper = GetRecursiveCompositionMapperWithGraphDepthLimit(depthLimit,
        GraphTruncationType.SetDefaultValue);
      var source = new RecursiveComposition();
      source.Compose(3);
      var target = (RecursiveCompositionDto) mapper.Transform(source);
      Assert.IsNotNull(source.Child.Child.Child);
      Assert.IsNull(target.Child.Child.Child);
    }

    [Test]
    public void SkipDetectionOfPropertyChangeTest()
    {
      var mapping = new MappingBuilder()
        .MapType<Person, PersonDto, int>(p => p.Id, p => p.Id).TrackChanges(p => p.FirstName, false)
        .Build();
      var mapper = new DefaultMapper(mapping);
      var source = GetSourcePerson(1);
      var target = (PersonDto) mapper.Transform(source);
      var modified = (PersonDto) target.Clone();
      modified.FirstName += "!";
      var operations = mapper.Compare(target, modified).Operations;
      Assert.IsTrue(operations.Count==0);

      mapping = new MappingBuilder()
        .MapType<Person, PersonDto, int>(p => p.Id, p => p.Id)
        .TrackChanges(p => p.FirstName, false)
        .IgnoreProperty(p => p.FirstName).Build();
      mapper = new DefaultMapper(mapping);
      target = (PersonDto) mapper.Transform(source);
      modified = (PersonDto) target.Clone();
      modified.FirstName += "!";
      operations = mapper.Compare(target, modified).Operations;
      Assert.IsTrue(operations.Count==0);
    }

    [Test]
    public void SkipSettingIgnoredPropertiesWhenObjectHasBeenCreatedTest()
    {
      var mapping = new MappingBuilder()
        .MapType<Person, PersonDto, int>(p => p.Id, p => p.Id)
        .IgnoreProperty(p => p.FirstName).Build();
      var mapper = new DefaultMapper(mapping);
      var createdObject = new PersonDto {
        BirthDate = DateTime.Now, FirstName = "FirstName", LastName = "LastName"
      };
      var operations = ((DefaultOperationLog) mapper.Compare(null, createdObject).Operations).ToList();
      Assert.AreEqual(3, operations.Count);
      ValidateObjectCreation(createdObject, operations[0]);
      ValidatePropertyOperation(createdObject, operations[1], p => p.LastName,
        createdObject.LastName, OperationType.SetProperty);
      ValidatePropertyOperation(createdObject, operations[2], p => p.BirthDate,
        createdObject.BirthDate, OperationType.SetProperty);
    }

    [Test]
    public void DynamicExpansionOfSourceHierarchyTest()
    {
      var settings = new MapperSettings {EnableDynamicSourceHierarchies = true};
      var mapping = new MappingBuilder()
        .MapType<CreatureBase, CreatureDto, Guid>(c => c.Id, c => c.Id).Build();

      var source = new List<Creature> {
        new Creature {Id = Guid.NewGuid(), Name = "Name0"}, new LongBee {Id = Guid.NewGuid(), Name = "Name1"},
        new Mammal {Id = Guid.NewGuid(), Name = "Name2"}
      };
      var target = ((List<object>) new DefaultMapper(mapping, settings).Transform(source))
        .Cast<CreatureDto>().ToList();
      for (var i = 0; i < source.Count; i++) {
        Assert.AreEqual(source[i].Id, target[i].Id);
        Assert.AreEqual(source[i].Name, target[i].Name);
      }
    }

    [Test]
    public void NullablePropertyTransformationTest()
    {
      var mapping = new MappingBuilder()
        .MapType<NullableDateTimeContainer, NullableDateTimeContainerDto, Guid>(n => n.Id, n => n.Id)
        .Build();

      var source = new NullableDateTimeContainer {NullableDateTime = DateTime.Now};
      var target = (NullableDateTimeContainerDto) new DefaultMapper(mapping).Transform(source);

      Assert.AreEqual(source.Id, target.Id);
      Assert.AreEqual(source.NullableDateTime, target.NullableDateTime);
    }

    [Test]
    public void NullablePropertyComparisonTest()
    {
      var mapping = new MappingBuilder()
        .MapType<NullableDateTimeContainer, NullableDateTimeContainerDto, Guid>(n => n.Id, n => n.Id)
        .Build();

      var source = new NullableDateTimeContainer {NullableDateTime = DateTime.Now};
      var original = (NullableDateTimeContainerDto) new DefaultMapper(mapping).Transform(source);
      var newValue = original.NullableDateTime.Value.AddDays(5);
      var modified = new NullableDateTimeContainerDto {
        Id = original.Id,
        NullableDateTime = newValue
      };

      var operations = ((DefaultOperationLog) new DefaultMapper(mapping)
        .Compare(original, modified).Operations).ToList();
      Assert.AreEqual(1, operations.Count);
      ValidatePropertyOperation(original, operations[0], n => n.NullableDateTime,
        newValue, OperationType.SetProperty);
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
      Action<Operation> validator = descriptor => {
        eventRaisingCount++;
        if (ReferenceEquals(target, descriptor.Object)) {
          Assert.AreSame(target, descriptor.Object);
          switch (descriptor.PropertyPath[0].SystemProperty.Name) {
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
            Assert.IsNull(descriptor.PropertyPath);
            Assert.IsNull(descriptor.Value);
          }
          else {
            customerPropertyCounts[descriptor.PropertyPath[0].SystemProperty.Name] += 1;
            var expectedValue = descriptor.PropertyPath[0].SystemProperty.GetValue(modifiedCustomer, null);
            Assert.AreEqual(expectedValue, descriptor.Value);
            Assert.AreEqual(OperationType.SetProperty, descriptor.Type);
          }
        }
        else if (ReferenceEquals(target.Customer, descriptor.Object)) {
          oldCustomerRemoved = true;
          Assert.AreEqual(OperationType.RemoveObject, descriptor.Type);
          Assert.IsNull(descriptor.PropertyPath);
          Assert.IsNull(descriptor.Value);
        }
        else
          Assert.Fail();
      };
      ((DefaultOperationLog) mapper.Compare(target, clone).Operations).ForEach(validator);
      Assert.AreEqual(7, eventRaisingCount);
      Assert.IsTrue(customerPropertyCounts.All(pair => pair.Value == 1));
      Assert.IsTrue(shipDateModified);
      Assert.IsTrue(customerModified);
      Assert.IsTrue(newCustomerCreated);
      Assert.IsTrue(oldCustomerRemoved);
    }
  }
}