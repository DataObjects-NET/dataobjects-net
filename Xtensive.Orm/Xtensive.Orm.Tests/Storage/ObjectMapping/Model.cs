// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.10.10

using System;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Tests.Storage.ObjectMapping.Model
{
  [Serializable]
  [HierarchyRoot]
  public abstract class Person : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field(Length = 50)]
    public string Name { get; set; }


    // Constructors

    protected Person()
    {}

    protected Person(int id)
      : base(id)
    {}
  }

  [Serializable]
  public abstract class AdvancedPerson : Person
  {
    [Field]
    public int Age { get; set; }

    // Constructors

    protected AdvancedPerson()
    {}

    protected AdvancedPerson(int id)
      : base(id)
    {}
  }

  [Serializable]
  public class Supplier : AdvancedPerson
  {
    [Field, Association(PairTo = "Supplier")]
    public EntitySet<Product> Products { get; private set;}
  }

  [Serializable]
  public class Employee : AdvancedPerson
  {
    [Field]
    public string Position { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public abstract class AbstractProduct : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }
  }

  [Serializable]
  public class Product : AbstractProduct
  {
    [Field]
    public Supplier Supplier { get; set; }
  }

  public interface IHasCategory : IEntity
  {
    [Field]
    string Category { get; set; }
  }

  [Serializable]
  public class PersonalProduct : AbstractProduct
  {
    [Field]
    public Employee Employee { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Book : Entity,
    IHasCategory
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public EntitySet<Author> Authors { get; private set; }

    public string Category { get; set; }

    [Field]
    public ITitle Title { get; set; }

    [Field]
    [Association(PairTo = "Book", OnOwnerRemove = OnRemoveAction.Clear,
      OnTargetRemove = OnRemoveAction.Clear)]
    public EntitySet<ITitle> TranslationTitles { get; private set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Author : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    [Field]
    [Association(PairTo = "Authors", OnOwnerRemove = OnRemoveAction.Clear,
      OnTargetRemove = OnRemoveAction.Clear)]
    public EntitySet<Book> Books { get; private set; }
  }

  public interface ITitle : IEntity
  {
    [Field]
    string Text { get; set; }

    [Field]
    Book Book { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Title : Entity,
    ITitle
  {
    [Key, Field]
    public int Id { get; private set; }

    public string Text { get; set; }

    public Book Book { get; set; }

    [Field]
    public string Language { get; set; }
  }

  public interface IPublisher : IEntity
  {
    [Field]
    string Trademark { get; set; }

    [Field]
    EntitySet<IBookShop> Distributors { get; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Publisher : Entity,
    IPublisher
  {
    [Key, Field]
    public int Id { get; private set; }

    public string Trademark { get; set; }

    [Field]
    public string Country { get; set; }

    public EntitySet<IBookShop> Distributors { get; private set; }
  }

  public interface IBookShop : IEntity
  {
    [Field]
    string Url { get; set; }

    [Field]
    [Association(PairTo = "Distributors", OnOwnerRemove = OnRemoveAction.Clear,
      OnTargetRemove = OnRemoveAction.Clear)]
    EntitySet<IPublisher> Suppliers { get; }
  }

  [Serializable]
  [HierarchyRoot]
  public class BookShop : Entity,
    IBookShop
  {
    [Key, Field]
    public int Id { get; private set; }

    public string Url { get; set; }

    [Field]
    public string Name { get; set; }

    public EntitySet<IPublisher> Suppliers { get; private set; }
  }

  public interface IReferenceToSelf : IEntity
  {
    [Field]
    IReferenceToSelf Reference { get; set; }
  }
  
  [Serializable]
  [HierarchyRoot]
  public class ReferenceToSelf : Entity,
    IReferenceToSelf
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public int AuxField { get; set; }

    public IReferenceToSelf Reference { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class AnotherBookShop : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public PublisherSet Suppliers { get; private set; }
  }

  public class PublisherSet : EntitySet<Publisher>
  {
    public PublisherSet(Entity owner, FieldInfo field)
      : base(owner, field)
    {}
  }

  [Serializable]
  public class SimplePerson : Person
  {}

  [Serializable]
  [HierarchyRoot]
  public class Apartment : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public SimplePerson Person { get; set; }

    [Field]
    public Address Address { get; set; }

    [Field]
    public ApartmentDescription Description { get; set; }
  }

  public class Address : Structure
  {
    [Field]
    public string Country { get; set; }

    [Field]
    public string City { get; set; }

    [Field]
    public string Street { get; set; }

    [Field]
    public int Building { get; set; }

    [Field]
    public int Office { get; set; }
  }

  public class ApartmentDescription : Structure
  {
    [Field]
    public double RentalFee { get; set; }

    [Field]
    public double Area { get; set; }

    [Field]
    public SimplePerson Manager { get; set; }
  }

  public class CustomPerson : Person
  {
    [Field]
    public string AuxString { get; set; }

    public CustomPerson(int id)
      : base(id)
    {}
  }

  [Serializable]
  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class CompositeKeyRoot : Entity
  {
    [Key(0), Field]
    public CompositeKeyFirstLevel0 FirstId { get; private set; }

    [Key(1), Field]
    public CompositeKeySecondLevel0 SecondId { get; private set; }

    [Field]
    public DateTime? Aux { get; set; }


    // Constructor

    public CompositeKeyRoot(CompositeKeyFirstLevel0 firstId, CompositeKeySecondLevel0 secondId)
      : base(firstId, secondId)
    {}
  }

  [Serializable]
  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class CompositeKeyFirstLevel0 : Entity
  {
    [Key(0), Field]
    public Guid FirstId { get; private set; }

    [Key(1), Field]
    public CompositeKeyFirstLevel1 SecondId { get; private set; }

    [Field]
    public string Aux { get; set; }


    // Constructors

    public CompositeKeyFirstLevel0(Guid firstId, CompositeKeyFirstLevel1 secondId)
      : base(firstId, secondId)
    {}
  }

  [Serializable]
  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class CompositeKeyFirstLevel1 : Entity
  {
    [Key(0), Field]
    public Guid FirstId { get; private set; }

    [Key(1), Field]
    public DateTime SecondId { get; private set; }

    [Field]
    public int Aux { get; set; }


    // Constructors

    public CompositeKeyFirstLevel1(Guid firstId, DateTime secondId)
      : base(firstId, secondId)
    {}
  }

  [Serializable]
  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class CompositeKeySecondLevel0 : Entity
  {
    [Key(0), Field]
    public int FirstId { get; private set; }

    [Key(1), Field]
    public string SecondId { get; private set; }

    [Field]
    public CompositeKeyFirstLevel0 Reference { get; set; }

    [Field]
    public int Aux { get; set; }


    // Constructors

    public CompositeKeySecondLevel0(int firstId, string secondId)
      : base(firstId, secondId)
    {}
  }
}