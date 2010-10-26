// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.10.10

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Xtensive.Orm.Tests.Storage.Prefetch.Model
{
  [Serializable]
  [HierarchyRoot]
  public class Simple : Entity
  {
    [Key, Field]
    public int Id { get; private set;}

    [Field, Version]
    public int VersionId { get; set;}

    [Field]
    public string Value { get; set;}
  }

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
  public class Customer : AdvancedPerson
  {
    [Field, Association(PairTo = "Customer", OnTargetRemove = OnRemoveAction.Clear)]
    public EntitySet<Order> Orders { get; private set; }

    [Field]
    public string City { get; set; }


    // Constructors

    public Customer()
    {}

    public Customer(int id)
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
  public class Employee : AdvancedPerson,
    IEnumerable<OrderDetail>
  {
    [Field, Association(PairTo = "Employee", OnTargetRemove = OnRemoveAction.Clear)]
    public EntitySet<Order> Orders { get; private set; }

    public IEnumerator<OrderDetail> GetEnumerator()
    {
      return Orders.SelectMany(o => o.Details).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }

  [Serializable]
  [HierarchyRoot]
  public class Order : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public int Number { get; set; }

    [Field]
    public Employee Employee { get; set; }

    [Field(LazyLoad = true)]
    public Customer Customer { get; set; }

    [Field, Association(PairTo = "Order", OnOwnerRemove = OnRemoveAction.Clear)]
    public EntitySet<OrderDetail> Details { get; private set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class OrderDetail : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public Order Order { get; set; }

    [Field]
    public Product Product { get; set; }

    [Field]
    public int Count { get; set; }
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

  [Serializable]
  [HierarchyRoot]
  public class AnotherTitle : Entity,
    ITitle
  {
    [Key, Field]
    public int Id { get; private set; }

    public string Text { get; set; }

    public Book Book { get; set; }
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
  public class Offer : Structure
  {
    [Field]
    public int Number { get; set; }

    [Field(LazyLoad = true)]
    public Book Book { get; set; }

    [Field]
    public IBookShop BookShop { get; set; }

    [Field(LazyLoad = true)]
    public int Lazy { get; set; }
  }

  [Serializable]
  public class IntermediateOffer : Structure
  {
    [Field]
    public int Number { get; set; }

    [Field]
    public OfferContainer AnotherContainer { get; set; }

    [Field]
    public Offer RealOffer { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class OfferContainer : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public Offer RealOffer { get; set; }

    [Field(LazyLoad = true)]
    public IntermediateOffer IntermediateOffer { get; set; }

    [Field]
    public string AuxField { get; set; }

    [Field(LazyLoad = true)]
    public int Lazy { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class LazyClass : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(LazyLoad = true)]
    public string LazyString { get; set; }

    [Field(LazyLoad = true)]
    public int LazyInt { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class IdOnly : Entity
  {
    [Field, Key]
    public int Id { get; private set; }
  }
}