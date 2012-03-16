// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.10

using System;
using System.Collections.Generic;

namespace Xtensive.Tests.ObjectMapping.SourceModel
{
  public interface IPerson
  {
    int Id { get; set; }

    string FirstName { get; set; }

    string LastName { get; set; }

    DateTime BirthDate { get; set; }
  }

  public class Person : IPerson
  {
    public int Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public DateTime BirthDate { get; set; }
  }

  public class Order
  {
    public Guid Id { get; set; }

    public Person Customer { get; set; }

    public DateTime ShipDate { get; set; }
  }

  public class Author
  {
    public Guid Id { get; set; }

    public string Name { get; set; }

    public Book Book { get; set; }
  }

  public class Book
  {
    public string ISBN { get; set; }

    public Title Title { get; set; }

    public double Price { get; set; }
  }

  public class Title
  {
    public Guid Id { get; set; }

    public string Text { get; set; }
  }

  public class PetOwner : Person
  {
    public HashSet<Animal> Pets { get; private set; }

    public PetOwner()
      : this(new HashSet<Animal>())
    {}

    public PetOwner(HashSet<Animal> pets)
    {
      Pets = pets;
    }
  }

  public class Animal
  {
    public Guid Id { get; private set; }

    public string Name { get; set; }

    public Animal()
    {
      Id = Guid.NewGuid();
    }
  }

  public abstract class CreatureBase
  {
    public abstract Guid Id { get; set; }

    public abstract string Name { get; set; }
  }

  public class Creature : CreatureBase
  {
    public override Guid Id { get; set; }

    public override string Name { get; set; }

    public Creature()
    {
      Id = Guid.NewGuid();
    }
  }

  public class Insect : Creature
  {
    public int LegPairCount { get; set; }
  }

  public class FlyingInsect : Insect
  {
    public int WingPairCount { get; set; }
  }

  public class LongBee : FlyingInsect
  {
    public int StripCount { get; set; }

    public double Length { get; set; }
  }

  public class Mammal : Creature
  {
    public bool HasHair { get; set; }

    public string Color { get; set; }
  }

  public class Cat : Mammal
  {
    public string Breed { get; set; }
  }

  public class Dolphin : Mammal
  {
    public string OceanAreal { get; set; }
  }

  public class Ignorable
  {
    public Guid Id { get; set; }

    public string Ignored { get; set; }

    public IgnorableSubordinate IgnoredReference { get; set; }

    public IgnorableSubordinate IncludedReference { get; set; }
  }

  public class IgnorableSubordinate
  {
    public Guid Id { get; set; }

    public DateTime Date { get; set; }
  }

  public class RecursiveComposition
  {
    public Guid Id { get; private set; }

    public int Level { get; private set; }

    public Simplest AuxReference { get; set; }

    public RecursiveComposition Child { get; private set; }

    public void Compose(int levelCount)
    {
      var current = this;
      for (var i = 0; i < levelCount; i++) {
        current.Child = new RecursiveComposition {Level = Level + i + 1};
        current = current.Child;
      }
    }

    public RecursiveComposition()
    {
      Id = Guid.NewGuid();
    }
  }

  public class Simplest
  {
    public Guid Id { get; private set; }

    public Simplest Value { get; set; }

    public Simplest()
    {
      Id = Guid.NewGuid();
    }
  }

  public class PrimitiveCollectionContainer
  {
    private PrimitiveCollection collection;

    public Guid Id { get; private set; }

    public PrimitiveCollection Collection
    {
      get {
        if (collection==null)
          collection = new PrimitiveCollection();
        return collection;
      }
    }
  }

  public class PrimitiveCollection : List<int>
  {}

  public class ComplexCollectionContainer
  {
    private ComplexCollection collection;

    public Guid Id { get; private set; }

    public ComplexCollection Collection
    {
      get {
        if (collection==null)
          collection = new ComplexCollection();
        return collection;
      }
    }
  }

  public class ComplexCollection : List<Person>
  {}

  public class StructureContainer
  {
    public Guid Id { get; set; }

    public string AuxString { get; set; }

    public Structure Structure { get; set; }

    public CompositeStructure0 CompositeStructure { get; set; }


    // Constructors

    public StructureContainer()
    {
      Id = Guid.NewGuid();
    }
  }

  public interface IStructure
  {
    int Int { get; set; }

    string String { get; set; }

    DateTime DateTime { get; set; }
  }

  public class Structure
  {
    public int Int { get; set; }

    public string String { get; set; }

    public DateTime DateTime { get; set; }
  }

  public class CompositeStructure0
  {
    public int AuxInt { get; set; }

    public CompositeStructure1 Structure { get; set; }
  }

  public class CompositeStructure1
  {
    public double AuxDouble { get; set; }

    public CompositeStructure2 Structure { get; set; }
  }

  public class CompositeStructure2
  {
    public int AuxInt { get; set; }

    public StructureContainer StructureContainer { get; set; }
  }

  public class Account
  {
    public Guid Id { get; private set; }

    public byte[] PasswordHash { get; set; }

    public List<AccessRight> AccessRights { get; set; }


    // Constructor

    public Account()
    {
      Id = Guid.NewGuid();
    }
  }

  public enum Action
  {
    Read,
    Write
  }

  public class AccessRight
  {
    public long[] ObjectId { get; set; }

    public Action Action { get; set; }
  }

  public class CollectionContainer
  {
    public Guid Id { get; set; }

    public int AuxInt { get; set; }

    public List<CollectionContainer> Collection { get; private set; }


    // Constructors

    public CollectionContainer()
    {
      Id = Guid.NewGuid();
      Collection = new List<CollectionContainer>();
    }
  }

  public class ObjectContainer
  {
    public Guid Id { get; set; }

    public int Int { get; set; }

    public object Object { get; set; }

    public List<object> ObjectCollection { get; set; }
    
    
    // Constructors

    public ObjectContainer()
    {
      Id = Guid.NewGuid();
    }
  }

  public class SourceGetterSetterExample
  {
    private readonly int readOnly;
    private DateTime writeOnly;

    public Guid Id { get; private set; }

    public int ReadOnly { get { return readOnly; } }

    public DateTime WriteOnly { set { writeOnly = value; } }

    public string WithProtectedGetter { protected get; set; }

    public double WithInternalGetter { internal get; set; }


    // Constructors

    public SourceGetterSetterExample()
    {
      Id = Guid.NewGuid();
      readOnly = new Random().Next();
    }
  }

  public class TargetGetterSetterExample
  {
    public Guid Id { get; set; }

    public int ReadOnly { get; set; }

    public DateTime WriteOnly { get; set; }

    public string WithProtectedGetter { get; set; }

    public double WithInternalGetter { get; set; }

    public string WithProtectedSetter { get; set; }

    public double WithInternalSetter { get; set; }
  }

  public class ArrayContainer
  {
    public Guid Id { get; private set; }

    public int[] IntArray { get; set; }

    public ArrayElement[] EntityArray { get; set; }

    
    // Constructors

    public ArrayContainer()
    {
      Id = Guid.NewGuid();
    }
  }

  public class ArrayElement
  {
    public Guid Id { get; private set; }

    public string Aux { get; set; }

    public ArrayElement[] NestedElements { get; set; }


    // Constructors

    public ArrayElement()
    {
      Id = Guid.NewGuid();
    }
  }

  public class NullableDateTimeContainer
  {
    public Guid Id { get; set; }

    public DateTime? NullableDateTime { get; set; }

    
    // Constructors

    public NullableDateTimeContainer()
    {
      Id = Guid.NewGuid();
    }
  }

  public class InconsistentNullableContainer
  {
    public Guid Id { get; set; }

    public int? Int { get; set; }

    public char Char { get; set; }

    
    // Constructors

    public InconsistentNullableContainer()
    {
      Id = Guid.NewGuid();
    }
  }

  public class InconsistentPrimitiveContainer
  {
    public Guid Id { get; set; }

    public int Int { get; set; }

    public double Double { get; set; }

    
    // Constructors

    public InconsistentPrimitiveContainer()
    {
      Id = Guid.NewGuid();
    }
  }
}