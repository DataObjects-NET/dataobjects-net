// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.01

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Tests.ObjectModel.NorthwindDO;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Tests.ObjectModel.Northwind
{

  #region Model

  [Serializable]
  [TableMapping("Categories")]
  [HierarchyRoot]
  public class Category : Entity
  {
    [Field, FieldMapping("CategoryId"), Key]
    public int Id { get; private set; }

    [Field(Length = 15)]
    public string CategoryName { get; set; }

    [Field]
    public string Description { get; set; }

    [Field(LazyLoad = true, Length = 1073741823)]
    public byte[] Picture { get; set; }
  }

  [Serializable]
  [TableMapping("Customers")]
  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class Customer : Entity
  {
    [Field(Length = 5), FieldMapping("CustomerId"), Key]
    public string Id { get; private set; }

    [Field(Length = 40)]
    public string CompanyName { get; set; }

    [Field(Length = 30)]
    public string ContactName { get; set; }

    [Field(Length = 30)]
    public string ContactTitle { get; set; }

    [Field(Length = 60)]
    public string Address { get; set; }

    [Field(Length = 15)]
    public string City { get; set; }

    [Field(Length = 15)]
    public string Region { get; set; }

    [Field(Length = 10)]
    public string PostalCode { get; set; }

    [Field(Length = 15)]
    public string Country { get; set; }

    [Field(Length = 24)]
    public string Phone { get; set; }

    [Field(Length = 24)]
    public string Fax { get; set; }

    // Constructors

    public Customer(string id)
      : base(id)
    {
    }
  }

  [Serializable]
  [HierarchyRoot]
  public class Region : Entity
  {
    [Field, FieldMapping("RegionId"), Key]
    public int Id { get; private set; }

    [Field(Length = 50)]
    public string RegionDescription { get; set; }
  }

  [Serializable]
  [TableMapping("Suppliers")]
  [HierarchyRoot]
  public class Supplier : Entity
  {
    [Field, FieldMapping("SupplierId"), Key]
    public int Id { get; private set; }

    [Field(Length = 40)]
    public string CompanyName { get; set; }

    [Field(Length = 30)]
    public string ContactName { get; set; }

    [Field(Length = 30)]
    public string ContactTitle { get; set; }

    [Field(Length = 60)]
    public string Address { get; set; }

    [Field(Length = 15)]
    public string City { get; set; }

    [Field(Length = 15)]
    public string Region { get; set; }

    [Field(Length = 10)]
    public string PostalCode { get; set; }

    [Field(Length = 15)]
    public string Country { get; set; }

    [Field(Length = 24)]
    public string Phone { get; set; }

    [Field(Length = 24)]
    public string Fax { get; set; }

    [Field]
    public string HomePage { get; set; }
  }

  [Serializable]
  [TableMapping("Shippers")]
  [HierarchyRoot]
  public class Shipper : Entity
  {
    [Field, FieldMapping("ShipperId"), Key]
    public int Id { get; private set; }

    [Field(Length = 40)]
    public string CompanyName { get; set; }

    [Field(Length = 24)]
    public string Phone { get; set; }
  }

  [Serializable]
  [TableMapping("Products")]
  [HierarchyRoot]
  public class Product : Entity
  {
    [Field, FieldMapping("ProductId"), Key]
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

  [Serializable]
  [TableMapping("Employees")]
  [HierarchyRoot]
  public class Employee : Entity
  {
    [Field, Key]
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

    [Field(Length = 60)]
    public string Address { get; set; }

    [Field(Length = 15)]
    public string City { get; set; }

    [Field(Length = 15)]
    public string Region { get; set; }

    [Field(Length = 10)]
    public string PostalCode { get; set; }

    [Field(Length = 15)]
    public string Country { get; set; }

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

  [Serializable]
  [TableMapping("Territories")]
  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class Territory : Entity
  {
    [Field(Length = 20), FieldMapping("TerritoryId"), Key]
    public string Id { get; private set; }

    [Field(Length = 50)]
    public string TerritoryDescription { get; set; }

    [Field]
    public Region Region { get; set; }

    [Field, Association(PairTo = "Territories")]
    public EntitySet<Employee> Employees { get; private set; }

    // Constructors

    public Territory(string id)
      : base(id)
    {
    }
  }

  [Serializable]
  [TableMapping("Orders")]
  [HierarchyRoot]
  public class Order : Entity
  {
    [Field, FieldMapping("OrderId"), Key]
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

    [Field(Length = 60)]
    public string ShipAddress { get; set; }

    [Field(Length = 15)]
    public string ShipCity { get; set; }

    [Field(Length = 15)]
    public string ShipRegion { get; set; }

    [Field(Length = 10)]
    public string ShipPostalCode { get; set; }

    [Field(Length = 15)]
    public string ShipCountry { get; set; }
  }

  [Serializable]
  [TableMapping("OrderDetails")]
  [HierarchyRoot]
  [KeyGenerator(KeyGeneratorKind.None)]
  public class OrderDetails : Entity
  {
    [Field, Key(0)]
    public Order Order { get; private set; }

    [Field, Key(1)]
    public Product Product { get; private set; }

    [Field]
    public decimal UnitPrice { get; set; }

    [Field]
    public short Quantity { get; set; }

    [Field]
    public float Discount { get; set; }


    // Constructors

    public OrderDetails(Order order, Product product)
      : base(order, product)
    {
    }
  }

  #endregion
}