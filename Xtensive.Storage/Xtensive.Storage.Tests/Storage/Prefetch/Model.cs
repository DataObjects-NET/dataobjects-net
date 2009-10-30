// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.10.10

namespace Xtensive.Storage.Tests.Storage.Prefetch.Model
{
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

  [HierarchyRoot]
  public abstract class Person : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field(Length = 50)]
    public string Name { get; set; }
  }

  public abstract class AdvancedPerson : Person
  {
    [Field]
    public int Age { get; set; }
  }

  public class Customer : AdvancedPerson
  {
    [Field, Association(PairTo = "Customer", OnTargetRemove = OnRemoveAction.Clear)]
    public EntitySet<Order> Orders { get; private set; }

    [Field]
    public string City { get; set; }
  }

  public class Supplier : AdvancedPerson
  {
    [Field, Association(PairTo = "Supplier")]
    public EntitySet<Product> Products { get; private set;}
  }

  public class Employee : AdvancedPerson
  {
    [Field, Association(PairTo = "Employee", OnTargetRemove = OnRemoveAction.Clear)]
    public EntitySet<Order> Orders { get; private set; }
  }

  [HierarchyRoot]
  public class Order : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public int Number { get; set; }

    [Field]
    public Employee Employee { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field, Association(PairTo = "Order", OnOwnerRemove = OnRemoveAction.Clear)]
    public EntitySet<OrderDetail> Details { get; private set; }
  }

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

  [HierarchyRoot]
  public abstract class AbstractProduct : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }
  }

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

  public class PersonalProduct : AbstractProduct
  {
    [Field]
    public Employee Employee { get; set; }
  }

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

  [HierarchyRoot]
  public class ReferenceToSelf : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public int AuxField { get; set; }

    [Field]
    public ReferenceToSelf Reference { get; set; }
  }
}