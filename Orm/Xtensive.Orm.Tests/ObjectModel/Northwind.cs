// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.01

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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

  public class DataBaseFiller
  {
    public static void Fill(Domain domain)
    {
      var con = new SqlConnection(TestConfiguration.Instance.NorthwindConnectionString);
      con.Open();
      SqlTransaction transaction = con.BeginTransaction();
      SqlCommand cmd = con.CreateCommand();
      cmd.Transaction = transaction;
      cmd.CommandText = "Select * from [dbo].[Categories]";
      var reader = cmd.ExecuteReader();

      using (var session = domain.OpenSession())
      using (var tr = session.OpenTransaction()) {
        #region  Categories

        var categories = new Dictionary<object, Category>();
        if (reader!=null) {
          while (reader.Read()) {
            var category = new Category();
            for (int i = 1; i < reader.FieldCount; i++)
              category[reader.GetName(i)] = !reader.IsDBNull(i) ? reader.GetValue(i) : null;
            categories.Add(reader.GetValue(0), category);
          }
          reader.Close();
        }

        #endregion

        #region Customers

        var customers = new Dictionary<object, Customer>();
        cmd.CommandText = "Select * from [dbo].[Customers]";
        reader = cmd.ExecuteReader();
        if (reader!=null) {
          while (reader.Read()) {
            var customer = new Customer(reader.GetString(0));
            for (int i = 1; i < reader.FieldCount; i++)
              customer[reader.GetName(i)] = !reader.IsDBNull(i) ? reader.GetValue(i) : null;
            customers.Add(reader.GetValue(0), customer);
          }
          reader.Close();
        }

        #endregion

        #region Regions

        var regions = new Dictionary<object, Region>();
        cmd.CommandText = "Select * from [dbo].[Region]";
        reader = cmd.ExecuteReader();
        if (reader!=null) {
          while (reader.Read()) {
            var region = new Region();
            for (int i = 1; i < reader.FieldCount; i++)
              region[reader.GetName(i)] = !reader.IsDBNull(i) ? reader.GetValue(i) : null;
            regions.Add(reader.GetValue(0), region);
          }
          reader.Close();
        }

        #endregion

        #region Suppliers

        var suppliers = new Dictionary<object, Supplier>();
        cmd.CommandText = "Select * from [dbo].[Suppliers]";
        reader = cmd.ExecuteReader();
        if (reader!=null) {
          while (reader.Read()) {
            var supplier = new Supplier();
            for (int i = 1; i < reader.FieldCount; i++)
              supplier[reader.GetName(i)] = !reader.IsDBNull(i) ? reader.GetValue(i) : null;
            suppliers.Add(reader.GetValue(0), supplier);
          }
          reader.Close();
        }

        #endregion

        #region Shippers

        var shippers = new Dictionary<object, Shipper>();
        cmd.CommandText = "Select * from [dbo].[Shippers]";
        reader = cmd.ExecuteReader();
        if (reader!=null) {
          while (reader.Read()) {
            var shipper = new Shipper();
            for (int i = 1; i < reader.FieldCount; i++)
              shipper[reader.GetName(i)] = !reader.IsDBNull(i) ? reader.GetValue(i) : null;
            shippers.Add(reader.GetValue(0), shipper);
          }
          reader.Close();
        }

        #endregion

        #region Products

        var products = new Dictionary<object, Product>();
        cmd.CommandText = "Select * from [dbo].[Products]";
        reader = cmd.ExecuteReader(CommandBehavior.KeyInfo);
        if (reader!=null) {
          while (reader.Read()) {
            var product = new Product();
            for (int i = 1; i < reader.FieldCount; i++)
              switch (i) {
              case 2:
                product.Supplier = !reader.IsDBNull(i) ? suppliers[reader.GetInt32(i)] : null;
                break;
              case 3:
                product.Category = !reader.IsDBNull(i) ? categories[reader.GetInt32(i)] : null;
                break;
              default:
                product[reader.GetName(i)] = !reader.IsDBNull(i) ? reader.GetValue(i) : null;
                break;
              }
            products.Add(reader.GetValue(0), product);
          }
          reader.Close();
        }

        #endregion

        #region Employees

        var employees = new Dictionary<object, Employee>();
        cmd.CommandText = "Select * from [dbo].[Employees]";
        reader = cmd.ExecuteReader();
        if (reader!=null) {
          while (reader.Read()) {
            var employee = new Employee();
            for (int i = 1; i < reader.FieldCount; i++)
              if (i!=16)
                employee[reader.GetName(i)] = !reader.IsDBNull(i) ? reader.GetValue(i) : null;
            employees.Add(reader.GetValue(0), employee);
          }
          reader.Close();
        }

        reader = cmd.ExecuteReader();
        if (reader!=null) {
          while (reader.Read()) {
            var employee = employees[reader.GetValue(0)];
            bool isNull = reader.IsDBNull(16);
            if (!isNull) {
              int employeeId = reader.GetInt32(16);
              var reportsTo = employees[employeeId];
              if (reportsTo == null)
                throw new NullReferenceException("Employee is null.");
              employee.ReportsTo = reportsTo;
            }
          }
          reader.Close();
        }

        #endregion

        #region Territories

        var territories = new Dictionary<object, Territory>();
        cmd.CommandText = "Select * from [dbo].[Territories]";
        reader = cmd.ExecuteReader();
        if (reader!=null) {
          while (reader.Read()) {
            var territory = new Territory(reader.GetString(0));
            territory.TerritoryDescription = reader.GetString(1);
            territory.Region = regions[reader.GetValue(2)];
            territories.Add(reader.GetValue(0), territory);
          }
          reader.Close();
        }

        #endregion

        #region EmployeeTerritories

        cmd.CommandText = "Select * from [dbo].[EmployeeTerritories]";
        reader = cmd.ExecuteReader();
        if (reader!=null) {
          while (reader.Read()) {
            var territory = territories[reader.GetString(1)];
            var employee = employees[reader.GetInt32(0)];
            if (employee == null)
              throw new NullReferenceException("Employee is null.");
            territory.Employees.Add(employee);
          }
          reader.Close();
        }

        #endregion

        #region Orders

        var orders = new Dictionary<object, Order>();
        cmd.CommandText = "Select * from [dbo].[Orders]";
        reader = cmd.ExecuteReader(CommandBehavior.KeyInfo);
        if (reader!=null) {
          while (reader.Read()) {
            var order = new Order();
            for (int i = 1; i < reader.FieldCount; i++)
              switch (i) {
              case 1:
                order.Customer = !reader.IsDBNull(i) ? customers[reader.GetValue(i)]: null;
                break;
              case 2:
                order.Employee = !reader.IsDBNull(i) ? employees[reader.GetValue(i)] : null;
                break;
              case 6:
                order.ShipVia = !reader.IsDBNull(i) ? shippers[reader.GetValue(i)] : null;
                break;
              default:
                order[reader.GetName(i)] = !reader.IsDBNull(i) ? reader.GetValue(i) : null;
                break;
              }
            orders.Add(reader.GetValue(0), order);
          }
          reader.Close();
        }

        #endregion

        #region OrderDetails

        cmd.CommandText = "Select * from [dbo].[Order Details]";
        reader = cmd.ExecuteReader();
        if (reader!=null) {
          while (reader.Read()) {
            var order = orders[reader.GetValue(0)];
            var product = products[reader.GetValue(1)];
            var orderDetails = new OrderDetails(order, product);

            for (int i = 2; i < reader.FieldCount; i++)
              orderDetails[reader.GetName(i)] = !reader.IsDBNull(i) ? reader.GetValue(i) : null;
          }
          reader.Close();
        }

        #endregion

        Session.Current.SaveChanges();
        tr.Complete();
      }

      transaction.Commit();
      con.Close();
    }
  }
}