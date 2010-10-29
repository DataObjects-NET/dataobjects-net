// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2010.01.13

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.ObjectMapping;
using Xtensive.ObjectMapping.Model;
using Xtensive.Tests.ObjectMapping.SourceModel;
using Xtensive.Tests.ObjectMapping.TargetModel;

namespace Xtensive.Tests.ObjectMapping
{
  public class MapperTestBase
  {
    protected const string inherited = "Inherited";
    protected const string inheritedFromMammal = "InheritedFromMammal";
    protected const string inheritedFlyingInsect = "InheritedFlyingInsect";

    protected PropertyInfo PetsProperty;
    protected PropertyInfo CustomerProperty;
    protected PropertyInfo FirstNameProperty;
    protected PropertyInfo LastNameProperty;
    protected PropertyInfo BirthDateProperty;

    [TestFixtureSetUp]
    public void TestFixtureSetUp()
    {
      PetsProperty = typeof (PetOwnerDto).GetProperty("Pets");
      CustomerProperty = typeof (OrderDto).GetProperty("Customer");
      FirstNameProperty = typeof (PersonDto).GetProperty("FirstName");
      LastNameProperty = typeof (PersonDto).GetProperty("LastName");
      BirthDateProperty = typeof (PersonDto).GetProperty("BirthDate");
    }

    protected static DefaultMapper GetPersonOrderMapper()
    {
      var mapping = new MappingBuilder().MapType<Person, PersonDto, int>(p => p.Id, p => p.Id)
        .MapType<Order, OrderDto, String>(o => o.Id.ToString(), o => o.Id).Build();
      return new DefaultMapper(mapping);
    }

    protected static DefaultMapper GetPetOwnerAnimalMapper()
    {
      var mapping = new MappingBuilder().MapType<PetOwner, PetOwnerDto, int>(o => o.Id, o => o.Id)
        .MapType<Animal, AnimalDto, Guid>(a => a.Id, a => a.Id).Build();
      return new DefaultMapper(mapping);
    }

    protected static DefaultMapper GetAuthorBookMapper()
    {
      var mapping = new MappingBuilder().MapType<Author, AuthorDto, Guid>(a => a.Id, a => a.Id)
          .MapProperty(a => a.Name + "!!!", a => a.Name)
        .MapType<Book, BookDto, string>(b => b.ISBN, b => b.ISBN)
          .MapProperty(b => b.Title.Text, b => b.TitleText)
          .MapProperty(b => new TitleDto {Id = b.Title.Id, Text = b.Title.Text}, b => b.Title)
        .MapType<Title, TitleDto, Guid>(t => t.Id, t => t.Id).Build();
      return new DefaultMapper(mapping);
    }

    protected static DefaultMapper GetIgnorableMapper()
    {
      var mapping = new MappingBuilder().MapType<Ignorable, IgnorableDto, Guid>(i => i.Id, i => i.Id)
        .IgnoreProperty(i => i.Auxiliary).IgnoreProperty(i => i.Ignored).IgnoreProperty(i => i.IgnoredReference)
        .MapType<IgnorableSubordinate, IgnorableSubordinateDto, Guid>(s => s.Id, s => s.Id).Build();
      return new DefaultMapper(mapping);
    }

    protected static DefaultMapper GetCreatureHeirsMapperWithCustomConverters()
    {
      var mapping = new MappingBuilder().MapType<Creature, CreatureDto, Guid>(c => c.Id, c => c.Id)
          .MapProperty(c => c.Name + inherited, c => c.Name)
        .MapType<Insect, InsectDto, Guid>(i => i.Id, i => i.Id)
        .Inherit<InsectDto, LongBee, LongBeeDto>()
        .MapType<FlyingInsect, FlyingInsectDto, Guid>(f => f.Id, f => f.Id)
          .MapProperty(f => f.Name + inheritedFlyingInsect, f => f.Name)
        .MapType<Mammal, MammalDto, Guid>(m => m.Id, m => m.Id)
          .MapProperty(m => m.Name + inheritedFromMammal, m => m.Name)
        .Inherit<MammalDto, Cat, CatDto>()
        .Inherit<MammalDto, Dolphin, DolphinDto>().Build();
      return new DefaultMapper(mapping);
    }

    protected static DefaultMapper GetCreatureHeirsMapperWithAttributesMapper()
    {
      var mapping = new MappingBuilder().MapType<Creature, CreatureDto, Guid>(c => c.Id, c => c.Id)
          .MapProperty(c => c.Name + inherited, c => c.Name)
        .MapType<Insect, InsectDto, Guid>(i => i.Id, i => i.Id)
          .TrackChanges(i => i.LegPairCount, false)
        .Inherit<InsectDto, LongBee, LongBeeDto>()
        .MapType<FlyingInsect, FlyingInsectDto, Guid>(f => f.Id, f => f.Id)
        .MapType<Mammal, MammalDto, Guid>(m => m.Id, m => m.Id)
          .IgnoreProperty(m => m.Name)
        .Inherit<MammalDto, Cat, CatDto>()
        .Inherit<MammalDto, Dolphin, DolphinDto>().Build();
      return new DefaultMapper(mapping);
    }

    protected static DefaultMapper GetRecursiveCompositionMapperWithGraphDepthLimit(
      int depthLimit, GraphTruncationType action)
    {
      var mapperSettings = new MapperSettings {GraphTruncationType = action, GraphDepthLimit = depthLimit};
      var mapping = new MappingBuilder()
        .MapType<RecursiveComposition, RecursiveCompositionDto, Guid>(r => r.Id, r => r.Id)
        .MapType<Simplest, SimplestDto, Guid>(s => s.Id, s => s.Id).Build();
      return new DefaultMapper(mapping, mapperSettings);
    }

    protected static DefaultMapper GetStructureContainerMapper(int? graphDepthLimit,
      GraphTruncationType truncationType)
    {
      var settings = new MapperSettings {
        GraphDepthLimit = graphDepthLimit, GraphTruncationType = truncationType
      };
      var mapping = new MappingBuilder()
        .MapStructure<Structure, StructureDto>()
        .MapType<StructureContainer, StructureContainerDto, Guid>(s => s.Id, s => s.Id)
        .MapStructure<CompositeStructure0, CompositeStructure0Dto>()
        .MapStructure<CompositeStructure1, CompositeStructure1Dto>()
          .MapProperty(c => (int) c.AuxDouble, c => c.AuxInt)
        .MapStructure<CompositeStructure2, CompositeStructure2Dto>().Build();
      return new DefaultMapper(mapping, settings);
    }

    protected static DefaultMapper GetStructureContainerMapper()
    {
      return GetStructureContainerMapper(null, GraphTruncationType.Default);
    }

    protected static Person GetSourcePerson()
    {
      return GetSourcePerson(3);
    }

    protected static Person GetSourcePerson(int id)
    {
      return new Person {
        BirthDate = DateTime.Now.AddYears(-20), FirstName = "John", LastName = "Smith", Id = id
      };
    }

    protected static Order GetSourceOrder()
    {
      return GetSourceOrder(3);
    }

    protected static Order GetSourceOrder(int personId)
    {
      return new Order {
        Customer = GetSourcePerson(personId), Id = Guid.NewGuid(), ShipDate = DateTime.Today.AddMonths(3)
      };
    }

    protected static Author GetSourceAuthor()
    {
      var title = new Title {Id = Guid.NewGuid(), Text = "T"};
      var book = new Book {ISBN = Guid.NewGuid().ToString(), Price = 25.0, Title = title};
      return new Author {Book = book, Id = Guid.NewGuid(), Name = "A"};
    }

    protected static PetOwner GetSourcePetOwner()
    {
      return GetSourcePetOwner(10);
    }

    protected static PetOwner GetSourcePetOwner(int id)
    {
      var petOwner = new PetOwner {
        BirthDate = DateTime.Now.AddYears(20), FirstName = "A", Id = id, LastName = "B"
      };
      petOwner.Pets.Add(new Animal());
      petOwner.Pets.Add(new Animal());
      petOwner.Pets.Add(new Animal());
      petOwner.Pets.Add(new Animal());
      petOwner.Pets.Add(new Animal());
      return petOwner;
    }

    protected static Ignorable GetIgnorableSource()
    {
      var ignoredSubordinate = new IgnorableSubordinate {Id = Guid.NewGuid(), Date = DateTime.Now};
      var includedSubordinate = new IgnorableSubordinate {Id = Guid.NewGuid(), Date = DateTime.Now.AddDays(10)};
      return new Ignorable {
        Id = Guid.NewGuid(), Ignored = "I", IgnoredReference = ignoredSubordinate,
        IncludedReference = includedSubordinate
      };
    }

    protected static List<Creature> GetSourceCreatures()
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

    protected static StructureContainer GetSourceStructureContainer()
    {
      var compositeStructure = new CompositeStructure0 {
        AuxInt = 1, Structure = new CompositeStructure1 {
          AuxDouble = 2, Structure = new CompositeStructure2 {
            AuxInt = 3, StructureContainer = new StructureContainer {
              AuxString = "S1", Structure = new Structure {DateTime = DateTime.Now}
            }
          }
        }
      };
      var result = new StructureContainer {
        AuxString = "1",
        Structure = new Structure {DateTime = DateTime.Now, Int = 2, String = "3"},
        CompositeStructure = compositeStructure
      };
      return result;
    }

    protected static Dictionary<string, int> CreateCountsForMutableProperties(Type type, DefaultMapper mapper)
    {
      var result = new Dictionary<string, int>();
      mapper.MappingDescription.GetTargetType(type).Properties.Select(pair => pair.Value)
        .Cast<TargetPropertyDescription>().Where(p => !p.IsChangeTrackingDisabled)
        .ForEach(p => result.Add(p.SystemProperty.Name, 0));
      return result;
    }

    protected static void ValidatePropertyOperation(object obj, Operation operation,
      PropertyInfo propertyInfo, OperationType operationType, object value)
    {
      Assert.AreSame(obj, operation.Object);
      Assert.AreEqual(value, operation.Value);
      Assert.AreEqual(propertyInfo, operation.PropertyPath[0].SystemProperty);
      Assert.AreEqual(operationType, operation.Type);
    }

    protected static void ValidateObjectRemoval(object obj, Operation operation)
    {
      ValidateObjectOperation(obj, operation, OperationType.RemoveObject);
    }

    protected static void ValidateObjectOperation(object obj, Operation operation,
      OperationType operationType)
    {
      Assert.AreEqual(obj, operation.Object);
      Assert.AreEqual(operationType, operation.Type);
      Assert.IsNull(operation.PropertyPath);
      Assert.IsNull(operation.Value);
    }

    protected static void ValidateObjectCreation(object obj, Operation operation)
    {
      ValidateObjectOperation(obj, operation, OperationType.CreateObject);
    }

    protected static void ValidatePropertySettingOperation(object obj, Operation operation,
      PropertyInfo propertyInfo, object value)
    {
      ValidatePropertyOperation(obj, operation, propertyInfo, OperationType.SetProperty, value);
    }

    protected static void ValidateItemAdditionOperation(object obj, Operation operation,
      PropertyInfo propertyInfo, object item)
    {
      ValidatePropertyOperation(obj, operation, propertyInfo, OperationType.AddItem, item);
    }

    protected static void ValidateItemRemovalOperation(object obj, Operation operation,
      PropertyInfo propertyInfo, object item)
    {
      ValidatePropertyOperation(obj, operation, propertyInfo, OperationType.RemoveItem, item);
    }

    protected static void AssertAreEqual(Person source, PersonDto target)
    {
      Assert.AreEqual(source.BirthDate, target.BirthDate);
      Assert.AreEqual(source.FirstName, target.FirstName);
      Assert.AreEqual(source.Id, target.Id);
      Assert.AreEqual(source.LastName, target.LastName);
    }

    protected static TargetPropertyDescription GetTargetProperty(DefaultMapper mapper, Type type,
      string propertyName)
    {
      return (TargetPropertyDescription) mapper.MappingDescription.GetTargetType(type)
        .Properties[type.GetProperty(propertyName)];
    }

    protected static void ValidatePropertyOperation<T>(T obj, Operation operation,
      Expression<Func<T, object>> propertyPath, object value, OperationType operationType)
    {
      var expression = propertyPath.Body;
      if (expression.NodeType == ExpressionType.Convert)
        expression = ((UnaryExpression) expression).Operand;
      var properties = new Stack<PropertyInfo>();
      while (expression.NodeType == ExpressionType.MemberAccess) {
        var propertyExpression = (MemberExpression) expression;
        properties.Push((PropertyInfo) propertyExpression.Member);
        expression = propertyExpression.Expression;
      }
      var i = 0;
      foreach (var property in properties)
        Assert.AreEqual(property, operation.PropertyPath[i++].SystemProperty);
      Assert.AreEqual(i, operation.PropertyPath.Count);
      ValidatePropertyOperation(obj, operation, operation.PropertyPath[0].SystemProperty,
        operationType, value);
    }

    protected static T Clone<T>(T source)
    {
      using (var stream = new MemoryStream()) {
        var serializer = new BinaryFormatter();
        serializer.Serialize(stream, source);
        stream.Seek(0, SeekOrigin.Begin);
        return (T) serializer.Deserialize(stream);
      }
    }
  }
}