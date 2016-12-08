// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.13

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Model;


namespace Xtensive.Orm.Tests.ObjectModel.NorthwindDO
{
  public interface IHasFreight : IEntity
  {
    [Field]
    decimal? Freight { get; set; }
  }

  public enum ProductType
  {
    Active,
    Discontinued
  }

  [Serializable]
  public class Address : Structure
  {
    [Field(Length = 60), FullText("English")]
    public string StreetAddress { get; set; }

    [Field(Length = 15)]
    public string City { get; set; }

    [Field(Length = 15)]
    public string Region { get; set; }

    [Field(Length = 10)]
    public string PostalCode { get; set; }

    [Field(Length = 15)]
    public string Country { get; set; }

    public string JustAProperty { get; set; }
  }

  [Serializable]
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
    {
    }

    protected Person(string id)
      : base(id)
    {
    }

    protected Person(int id)
      : base(id)
    {
    }
  }

  [Serializable]
  public abstract class BusinessContact : Person
  {
    [Field(Length = 40, Indexed = true)]
    public string CompanyName { get; set; }

    [Field(Length = 30)]
    public string ContactName { get; set; }

    [Field(Length = 30)]
    public string ContactTitle { get; set; }


    // Constructors

    protected BusinessContact()
    {
    }

    protected BusinessContact(string id)
      : base(id)
    {
    }

    protected BusinessContact(int id)
      : base(id)
    {
    }
  }

  [Serializable]
  [TableMapping("Categories")]
  [HierarchyRoot]
  public class Category : Entity
  {
    [Field, FieldMapping("CategoryId"), Key]
    public int Id { get; private set; }

    [FullText("English")]
    [Field(Length = 15, Indexed = true)]
    public string CategoryName { get; set; }

    [FullText("English")]
    [Field]
    public string Description { get; set; }

    [Field(LazyLoad = true, Length = 1073741823)]
    public byte[] Picture { get; set; }

    [Field, Association(PairTo = "Category")]
    public EntitySet<Product> Products { get; private set; }
  }

  [Serializable]
  [KeyGenerator(KeyGeneratorKind.None)]
  [TableMapping("Customers")]
  [HierarchyRoot]
  public class Customer : BusinessContact
  {
    [Field(Length = 5), FieldMapping("CustomerId"), Key]
    public string Id { get; private set; }

    [Field, Association(PairTo = "Customer")]
    public EntitySet<Order> Orders { get; private set; }

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
  public class Supplier : BusinessContact
  {
    [Field, FieldMapping("SupplierId"), Key]
    public int Id { get; private set; }

    [Field]
    public string HomePage { get; set; }

    [Field, Association(PairTo = "Supplier")]
    public EntitySet<Product> Products { get; private set; }
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

    /// <summary>
    /// Tests subqery with 'this' scenario.
    /// </summary>
    public IQueryable<Order> Orders
    {
      get { return Session.Query.All<Order>().Where(o => o.ShipVia==this); }
    }

    /// <summary>
    /// Tests subqery with 'this' scenario.
    /// </summary>
    public Order FirstOrder
    {
      get { return Session.Query.All<Order>().Where(o => o.ShipVia==this).FirstOrDefault(); }
    }
  }

  [Serializable]
  [TableMapping("Products")]
  [HierarchyRoot(InheritanceSchema.SingleTable)]
  [Index("Category", "Supplier", "UnitPrice")]
  public abstract class Product : Entity
  {
    [Field, FieldMapping("ProductId"), Key]
    public int Id { get; private set; }

    [FullText("English")]
    [Field(Length = 40, Indexed = true)]
    public string ProductName { get; set; }

    [Field, FieldMapping("Seller")]
    public Supplier Supplier { get; set; }

    [Field]
    public Category Category { get; set; }

    [Field]
    public ProductType ProductType { get; protected set; }

    [Field(Indexed = true)]
    public decimal UnitPrice { get; set; }

    [Field]
    public short UnitsInStock { get; set; }

    [Field]
    public short UnitsOnOrder { get; set; }

    [Field]
    public short ReorderLevel { get; set; }
  }

  /// <summary>
  /// Just for Linq type inheritance tests.
  /// </summary>
  [Serializable]
  public abstract class IntermediateProduct : Product
  {
    [Field(Length = 20)]
    public string QuantityPerUnit { get; set; }
  }

  [Serializable]
  public class ActiveProduct : IntermediateProduct
  {
    public ActiveProduct()
    {
      ProductType = ProductType.Active;
    }
  }

  [Serializable]
  public class DiscontinuedProduct : IntermediateProduct
  {
    public DiscontinuedProduct()
    {
      ProductType = ProductType.Discontinued;
    }
  }

  [Serializable]
  [TableMapping("Employees")]
  [HierarchyRoot]
  [Index("FirstName")]
  [Index("BirthDate")]
  [Index("Title")]
  [Index("HireDate", "LastName", "Title")]
  public class Employee : Person
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

    [Field(Length = 24)]
    public string HomePhone { get; set; }

    [Field(Length = 4)]
    public string Extension { get; set; }

    [Field(Length = 1073741823, LazyLoad = true)]
    public byte[] Photo { get; set; }

    [Field]
    public string Notes { get; set; }

    [Field]
    public Employee ReportsTo { get; set; }

    [Field, Association(PairTo = "ReportsTo")]
    public EntitySet<Employee> ReportingEmployees { get; set; }

    [Field(Length = 255)]
    public string PhotoPath { get; set; }

    [Field, Association(PairTo = "Employee")]
    public EntitySet<Order> Orders { get; private set; }

    [Field]
    public EntitySet<Territory> Territories { get; private set; }

    public string FullName
    {
      get { return FirstName + " " + LastName; }
    }
  }

  [Serializable]
  [KeyGenerator(KeyGeneratorKind.None)]
  [TableMapping("Territories")]
  [HierarchyRoot]
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
  [Index("OrderDate")]
  [Index("ShipName")]
  [Index("Freight")]
  [HierarchyRoot]
  public class Order : Entity,
    IHasFreight
  {
    [Field, FieldMapping("OrderId"), Key]
    public int Id { get; private set; }

    [Field]
    public TimeSpan? ProcessingTime { get; set; }

    [Field]
    public Shipper ShipVia { get; set; }

    [Field]
    public Employee Employee { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field]
    public DateTime? OrderDate { get; set; }

    [Field]
    public DateTime? RequiredDate { get; set; }

    [Field]
    public DateTime? ShippedDate { get; set; }

    public decimal? Freight { get; set; }

    [Field(Length = 60)]
    public string ShipName { get; set; }

    [Field]
    public Address ShippingAddress { get; set; }

    [Field, Association(PairTo = "Order")]
    public EntitySet<OrderDetails> OrderDetails { get; private set; }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format("OrderId: {0}; OrderDate: {1}; RequiredDate: {2}.", Id, OrderDate, RequiredDate);
    }
  }

  [Serializable]
  [KeyGenerator(KeyGeneratorKind.None)]
  [TableMapping("OrderDetails")]
  [HierarchyRoot]
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

  public class DataBaseFiller
  {
    private abstract class Importer
    {
      public abstract void Import(Dictionary<string, object> fields, ImportContext importContext);
    }

    private class CategoryImporter : Importer
    {
      public override void Import(Dictionary<string, object> fields, ImportContext importContext)
      {
        var category = new Category();
        category.CategoryName = (String) fields["CategoryName"];
        category.Description = (String) fields["Description"];
        category.Picture = (Byte[]) fields["Picture"];
        importContext.Categories.Add(fields["CategoryID"], category);
      }
    }

    private class CustomerImporter : Importer
    {
      public override void Import(Dictionary<string, object> fields, ImportContext importContext)
      {
        var customer = new Customer((String) fields["CustomerID"]);
        customer.CompanyName = (String) fields["CompanyName"];
        customer.ContactName = (String) fields["ContactName"];
        customer.ContactTitle = (String) fields["ContactTitle"];
        customer.Fax = (String) fields["Fax"];
        customer.Phone = (String) fields["Phone"];
        customer.Address = new Address();
        customer.Address.City = (String) fields["City"];
        customer.Address.Country = (String) fields["Country"];
        customer.Address.PostalCode = (String) fields["PostalCode"];
        customer.Address.Region = (String) fields["Region"];
        customer.Address.StreetAddress = (String) fields["Address"];
        importContext.Customers.Add(fields["CustomerID"], customer);
      }
    }

    private class RegionImporter : Importer
    {
      public override void Import(Dictionary<string, object> fields, ImportContext importContext)
      {
        var region = new Region();
        region.RegionDescription = (String) fields["RegionDescription"];
        importContext.Regions.Add(fields["RegionID"], region);
      }
    }

    private class SupplierImporter : Importer
    {
      public override void Import(Dictionary<string, object> fields, ImportContext importContext)
      {
        var supplier = new Supplier();
        supplier.Phone = (String) fields["Phone"];
        supplier.HomePage = (String) fields["HomePage"];
        supplier.Fax = (String) fields["Fax"];
        supplier.ContactTitle = (String) fields["ContactTitle"];
        supplier.ContactName = (String) fields["ContactName"];
        supplier.CompanyName = (String) fields["CompanyName"];
        supplier.Address = new Address();
        supplier.Address.City = (String) fields["City"];
        supplier.Address.Country = (String) fields["Country"];
        supplier.Address.Region = (String) fields["Region"];
        supplier.Address.PostalCode = (String) fields["PostalCode"];
        supplier.Address.StreetAddress = (String) fields["Address"];
        importContext.Suppliers.Add(fields["SupplierID"], supplier);
      }
    }

    private class ShipperImporter : Importer
    {
      public override void Import(Dictionary<string, object> fields, ImportContext importContext)
      {
        var shipper = new Shipper();
        shipper.CompanyName = (String) fields["CompanyName"];
        shipper.Phone = (String) fields["Phone"];
        importContext.Shippers.Add(fields["ShipperID"], shipper);
      }
    }

    private class ProductImporter : Importer
    {
      public override void Import(Dictionary<string, object> fields, ImportContext importContext)
      {
        var product = (bool) fields["Discontinued"]
          ? (Product) new DiscontinuedProduct()
          : new ActiveProduct();
        product.ProductName = (String) fields["ProductName"];
        product.Supplier = (Supplier) importContext.Suppliers[fields["SupplierID"]];
        product.Category = (Category) importContext.Categories[fields["CategoryID"]];
        product.UnitPrice = (Decimal) fields["UnitPrice"];
        product.UnitsInStock = (Int16) fields["UnitsInStock"];
        product.UnitsOnOrder = (Int16) fields["UnitsOnOrder"];
        product.ReorderLevel = (Int16) fields["ReorderLevel"];
        importContext.Products.Add(fields["ProductID"], product);
      }
    }

    private class EmployeeImporter : Importer
    {
      public override void Import(Dictionary<string, object> fields, ImportContext importContext)
      {
        var employee = new Employee();
        employee.LastName = (String) fields["LastName"];
        employee.FirstName = (String) fields["FirstName"];
        employee.Title = (String) fields["Title"];
        employee.TitleOfCourtesy = (String) fields["TitleOfCourtesy"];
        employee.BirthDate = (DateTime?) fields["BirthDate"];
        employee.HireDate = (DateTime?) fields["HireDate"];
        employee.HomePhone = (String) fields["HomePhone"];
        employee.Extension = (String) fields["Extension"];
        employee.Photo = (Byte[]) fields["Photo"];
        employee.Notes = (String) fields["Notes"];
        employee.PhotoPath = (String) fields["PhotoPath"];
        employee.Address = new Address();
        employee.Address.City = (String) fields["City"];
        employee.Address.Country = (String) fields["Country"];
        employee.Address.PostalCode = (String) fields["PostalCode"];
        employee.Address.Region = (String) fields["Region"];
        employee.Address.StreetAddress = (String) fields["Address"];
        importContext.Employees.Add(fields["EmployeeID"], employee);
      }
    }

    private class EmployeeImporterReportsTo : Importer
    {
      public override void Import(Dictionary<string, object> fields, ImportContext importContext)
      {
        var value = fields["ReportsTo"];
        if (value!=null)
          ((Employee) importContext.Employees[fields["EmployeeID"]]).ReportsTo = (Employee) importContext.Employees[fields["ReportsTo"]];
      }
    }

    private class TerritoryImporter : Importer
    {
      public override void Import(Dictionary<string, object> fields, ImportContext importContext)
      {
        var territory = new Territory((String) fields["TerritoryID"]);
        territory.TerritoryDescription = (String) fields["TerritoryDescription"];
        territory.Region = (Region) importContext.Regions[fields["RegionID"]];
        importContext.Territories.Add(fields["TerritoryID"], territory);
      }
    }

    private class TerritoryEmployeeImporter : Importer
    {
      public override void Import(Dictionary<string, object> fields, ImportContext importContext)
      {
        var employee = importContext.Employees[fields["EmployeeID"]];
        var territory = (Territory) importContext.Territories[fields["TerritoryID"]];
        territory.Employees.Add(employee);
      }
    }

    private class OrderImporter : Importer
    {
      public override void Import(Dictionary<string, object> fields, ImportContext importContext)
      {
        var order = new Order();
        order.Customer = (Customer) importContext.Customers[fields["CustomerID"]];
        order.Employee = (Employee) importContext.Employees[fields["EmployeeID"]];
        order.ShipVia = (Shipper) importContext.Shippers[fields["ShipVia"]];
        order.OrderDate = (DateTime?) fields["OrderDate"];
        order.RequiredDate = (DateTime?) fields["RequiredDate"];
        order.ShippedDate = (DateTime?) fields["ShippedDate"];
        order.Freight = (Decimal?) fields["Freight"];
        order.ShipName = (String) fields["ShipName"];
        order.ShippingAddress = new Address();
        order.ShippingAddress.City = (String) fields["ShipCity"];
        order.ShippingAddress.Country = (String) fields["ShipCountry"];
        order.ShippingAddress.Region = (String) fields["ShipRegion"];
        order.ShippingAddress.PostalCode = (String) fields["ShipPostalCode"];
        order.ShippingAddress.StreetAddress = (String) fields["ShipAddress"];
        importContext.Orders.Add(fields["OrderID"], order);
      }
    }

    private class OrderDetailsImporter : Importer
    {
      public override void Import(Dictionary<string, object> fields, ImportContext importContext)
      {
        var order = (Order) importContext.Orders[fields["OrderID"]];
        var product = (Product) (importContext.Products[fields["ProductID"]]);
        var orderDetail = new OrderDetails(order, product);
        orderDetail.Discount = (Single) fields["Discount"];
        orderDetail.UnitPrice = (Decimal) fields["UnitPrice"];
        orderDetail.Quantity = (Int16) fields["Quantity"];
      }
    }

    public class ImportContext
    {
      public Dictionary<object, Entity> Categories { get; private set; }
      public Dictionary<object, Entity> Customers { get; private set; }
      public Dictionary<object, Entity> Regions { get; private set; }
      public Dictionary<object, Entity> Suppliers { get; private set; }
      public Dictionary<object, Entity> Shippers { get; private set; }
      public Dictionary<object, Entity> Products { get; private set; }
      public Dictionary<object, Entity> Employees { get; private set; }
      public Dictionary<object, Entity> Territories { get; private set; }
      public Dictionary<object, Entity> Orders { get; private set; }

      public ImportContext()
      {
        Categories = new Dictionary<object, Entity>();
        Customers = new Dictionary<object, Entity>();
        Regions = new Dictionary<object, Entity>();
        Suppliers = new Dictionary<object, Entity>();
        Shippers = new Dictionary<object, Entity>();
        Products = new Dictionary<object, Entity>();
        Employees = new Dictionary<object, Entity>();
        Territories = new Dictionary<object, Entity>();
        Orders = new Dictionary<object, Entity>();
      }
    }

    public static void Fill(Domain domain)
    {
      var path = @"Northwind.xml";
      var xmlTables = ReadXml(path);
      using (var session = domain.OpenSession(new SessionConfiguration("Legacy", SessionOptions.ServerProfile | SessionOptions.AutoActivation)))
      using (var tr = session.OpenTransaction(System.Transactions.IsolationLevel.ReadCommitted)) {
        var importContext = new ImportContext();
        Import(xmlTables.First(t => t.Name=="Categories"), importContext, new CategoryImporter());
        Import(xmlTables.First(t => t.Name=="Customers"), importContext, new CustomerImporter());
        Import(xmlTables.First(t => t.Name=="Region"), importContext, new RegionImporter());
        Import(xmlTables.First(t => t.Name=="Suppliers"), importContext, new SupplierImporter());
        Import(xmlTables.First(t => t.Name=="Shippers"), importContext, new ShipperImporter());
        Import(xmlTables.First(t => t.Name=="Products"), importContext, new ProductImporter());
        Import(xmlTables.First(t => t.Name=="Employees"), importContext, new EmployeeImporter());
        Import(xmlTables.First(t => t.Name=="Employees"), importContext, new EmployeeImporterReportsTo());
        Import(xmlTables.First(t => t.Name=="Territories"), importContext, new TerritoryImporter());
        Import(xmlTables.First(t => t.Name=="EmployeeTerritories"), importContext, new TerritoryEmployeeImporter());
        Import(xmlTables.First(t => t.Name=="Orders"), importContext, new OrderImporter());
        Import(xmlTables.First(t => t.Name=="Order_Details"), importContext, new OrderDetailsImporter());
        Session.Current.SaveChanges();
        tr.Complete();
      }
    }

    private static void Import(XmlTable node, ImportContext importContext, Importer importer)
    {
      foreach (var row in node.Rows) {
        var fields = GetFields(row, node.ColumnTypes);
        importer.Import(fields, importContext);
      }
    }

    private static Dictionary<string, object> GetFields(XElement row, Dictionary<string, string> columnTypes)
    {
      var fields = new Dictionary<string, object>();
      var elements = row.Elements().ToList();
      for (int i = 0; i < elements.Count(); i++) {
        var value = elements[i].Value;
        object obj = null;
        if (!string.IsNullOrEmpty(value)) {
          obj = ConvertFieldType(columnTypes[elements[i].Name.LocalName], elements[i].Value);
        }
        fields.Add(elements[i].Name.LocalName, obj);
      }
      return fields;
    }

    private static object ConvertFieldType(string columnType, string text)
    {
      var type = Type.GetType(columnType);
      switch (columnType) {
      case "System.Byte[]":
        return Convert.FromBase64String(text);
      case "System.Decimal":
        return Decimal.Parse(text, CultureInfo.InvariantCulture);
      case "System.Single":
        return Single.Parse(text, CultureInfo.InvariantCulture);
      case "System.DateTime":
        return DateTime.Parse(text);
      default:
        return Convert.ChangeType(text, type, CultureInfo.InvariantCulture);
      }
    }

    private static List<XmlTable> ReadXml(string path)
    {
      var doc = XDocument.Load(path);
      var root = doc.Element("root");
      if (root==null)
        throw new Exception("Read xml error");
      var tables = root.Elements();
      var list = new List<XmlTable>();

      foreach (var table in tables) {
        var xmlTable = new XmlTable();
        xmlTable.Name = table.Name.LocalName;
        xmlTable.ColumnTypes = table.Element("Columns").Elements().ToDictionary(key => key.Name.LocalName, value => value.Value);
        xmlTable.Rows = table.Element("Rows").Elements();
        list.Add(xmlTable);
      }
      return list;
    }

    private class XmlTable
    {
      public string Name { get; set; }
      public Dictionary<string, string> ColumnTypes { get; set; }
      public IEnumerable<XElement> Rows { get; set; }
    }
  }
}