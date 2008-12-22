// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.13

using System;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Attributes;


namespace Xtensive.Storage.Tests.ObjectModel.NorthwindDO
{
  public class Address : Structure
  {
    [Field(Length = 60)]
    public string StreetAddress { get; set; }

    [Field(Length = 15)]
    public string City { get; set; }

    [Field(Length = 15)]
    public string Region { get; set; }

    [Field(Length = 10)]
    public string PostalCode { get; set; }

    [Field(Length = 15)]
    public string Country { get; set; }

  }

  public abstract class Person : Entity
  {
    [Field]
    public Address Address { get; set; }

    [Field(Length = 24)]
    public string Phone { get; set; }

    [Field(Length = 24)]
    public string Fax { get; set; }


    // Constructors

    protected Person()
    {}

    protected Person(Tuple tuple)
      : base(tuple)
    {}
  }

  public abstract class BusinessContact : Person
  {
    [Field(Length = 40)]
    public string CompanyName { get; set; }

    [Field(Length = 30)]
    public string ContactName { get; set; }

    [Field(Length = 30)]
    public string ContactTitle { get; set; }


    // Constructors

    protected BusinessContact()
    {}

    protected BusinessContact(Tuple tuple)
      : base(tuple)
    {
    }
  }

  [Entity(MappingName = "Categories")]
  [HierarchyRoot(typeof(KeyGenerator), "Id")]
  public class Category : Entity
  {
    [Field(MappingName = "CategoryId")]
    public int Id { get; private set; }

    [Field(Length = 15)]
    public string CategoryName { get; set; }

    [Field]
    public string Description { get; set; }

    [Field(LazyLoad = true, Length = 1073741823)]
    public byte[] Picture { get; set; }
  }

  [Entity(MappingName = "Customers")]
  [HierarchyRoot("Id")]
  public class Customer : BusinessContact
  {
    [Field(Length = 5, MappingName = "CustomerId")]
    public string Id { get; private set; }


    // Constructors

    public Customer(string id)
      : base(Tuple.Create(id))
    {
    }
  }

  [HierarchyRoot(typeof(KeyGenerator), "Id")]
  public class Region : Entity
  {
    [Field(MappingName = "RegionId")]
    public int Id { get; private set; }

    [Field(Length = 50)]
    public string RegionDescription { get; set; }
  }

  [Entity(MappingName = "Suppliers")]
  [HierarchyRoot(typeof(KeyGenerator), "Id")]
  public class Supplier : BusinessContact
  {
    [Field]
    public int Id { get; private set; }

    [Field]
    public string HomePage { get; set; }
  }

  [Entity(MappingName = "Shippers")]
  [HierarchyRoot(typeof(KeyGenerator), "Id")]
  public class Shipper : Entity
  {
    [Field(MappingName = "ShipperId")]
    public int Id { get; private set; }

    [Field(Length = 40)]
    public string CompanyName { get; set; }

    [Field(Length = 24)]
    public string Phone { get; set; }
  }

  [Entity(MappingName = "Products")]
  [HierarchyRoot(typeof(KeyGenerator), "Id")]
  public class Product : Entity
  {
    [Field]
    public int Id { get; private set; }

    [Field(Length = 40)]
    public string ProductName { get; set; }

    [Field]
    public Supplier Supplier { get; set; }

    [Field]
    public Category Category { get; set; }

    [Field(Length = 20)]
    public string QuantityPerUnit { get; set; }

    [Field]
    public decimal UnitPrice { get; set; }

    [Field]
    public short UnitsInStock { get; set; }

    [Field]
    public short UnitsOnOrder { get; set; }

    [Field]
    public short ReorderLevel { get; set; }

    [Field]
    public bool Discontinued { get; set; }
  }

  [Entity(MappingName = "Employees")]
  [HierarchyRoot(typeof(KeyGenerator), "Id")]
  public class Employee : Person
  {
    [Field]
    public int Id { get; private set; }

    [Field(Length = 20)]
    public string LastName { get; set; }

    [Field(Length = 10)]
    public string FirstName { get; set; }

    [Field(Length = 30)]
    public string Title { get; set; }

    [Field(Length = 25)]
    public string TitleOfCourtesy { get; set; }

    [Field]
    public DateTime? BirthDate { get; set; }

    [Field]
    public DateTime? HireDate { get; set; }

    [Field(Length = 24)]
    public string HomePhone { get; set; }

    [Field(Length = 4)]
    public string Extension { get; set; }

    [Field(Length = 1073741823)]
    public byte[] Photo { get; set; }

    [Field]
    public string Notes { get; set; }

    [Field]
    public Employee ReportsTo { get; set; }

    [Field(Length = 255)]
    public string PhotoPath { get; set; }

    [Field]
    public EntitySet<Territory> Territories { get; private set; }
  }

  [Entity(MappingName = "Territories")]
  [HierarchyRoot("Id")]
  public class Territory : Entity
  {
    [Field(Length = 20, MappingName = "TerritoryId")]
    public string Id { get; private set; }

    [Field(Length = 50)]
    public string TerritoryDescription { get; set; }

    [Field]
    public Region Region { get; set; }

    [Field(PairTo = "Territories")]
    public EntitySet<Employee> Employees { get; private set; }

    // Constructors

    public Territory(string id)
      : base(Tuple.Create(id))
    {
    }
  }

  [Entity(MappingName = "Orders")]
  [HierarchyRoot(typeof(KeyGenerator), "Id")]
  public class Order : Entity
  {
    [Field(MappingName = "OrderId")]
    public int Id { get; private set; }

    [Field]
    public Customer Customer { get; set; }

    [Field]
    public Employee Employee { get; set; }

    [Field]
    public DateTime? OrderDate { get; set; }

    [Field]
    public DateTime? RequiredDate { get; set; }

    [Field]
    public DateTime? ShippedDate { get; set; }

    [Field]
    public Shipper ShipVia { get; set; }

    [Field]
    public decimal Freight { get; set; }

    [Field(Length = 60)]
    public string ShipName { get; set; }

    [Field]
    public Address ShippingAddress { get; set; }
  }

  [Entity(MappingName = "OrderDetails")]
  [HierarchyRoot("Order", "Product")]
  public class OrderDetails : Entity
  {
    [Field]
    public Order Order { get; private set; }

    [Field]
    public Product Product { get; private set; }

    [Field]
    public decimal UnitPrice { get; set; }

    [Field]
    public short Quantity { get; set; }

    [Field]
    public float Discount { get; set; }


    // Constructors

    public OrderDetails(Order order, Product product)
      : base(Tuple.Create(order.Id, product.Id))
    {
    }
  }
}