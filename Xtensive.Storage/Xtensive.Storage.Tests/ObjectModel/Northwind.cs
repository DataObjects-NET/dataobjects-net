// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.01

using System;
using System.Data;
using System.Data.SqlClient;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Attributes;

namespace Xtensive.Storage.Tests.ObjectModel.Northwind
{

  #region Model

  [Entity(MappingName = "Categories")]
  [HierarchyRoot(typeof (KeyGenerator), "Id")]
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
  public class Customer : Entity
  {
    [Field(Length = 5, MappingName = "CustomerId")]
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
      : base(Tuple.Create(id))
    {
    }
  }

  [HierarchyRoot(typeof (KeyGenerator), "Id")]
  public class Region : Entity
  {
    [Field(MappingName = "RegionId")]
    public int Id { get; private set; }

    [Field(Length = 50)]
    public string RegionDescription { get; set; }
  }

  [Entity(MappingName = "Suppliers")]
  [HierarchyRoot(typeof (KeyGenerator), "Id")]
  public class Supplier : Entity
  {
    [Field(MappingName = "SupplierId")]
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

  [Entity(MappingName = "Shippers")]
  [HierarchyRoot(typeof (KeyGenerator), "Id")]
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
  [HierarchyRoot(typeof (KeyGenerator), "Id")]
  public class Product : Entity
  {
    [Field(MappingName = "ProductId")]
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
  [HierarchyRoot(typeof (KeyGenerator), "Id")]
  public class Employee : Entity
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
  [HierarchyRoot(typeof (KeyGenerator), "Id")]
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

  #endregion

  public class DataBaseFiller
  {
    private static readonly SqlConnection con = new SqlConnection("Data Source=localhost;"
      + "Initial Catalog = Northwind;"
        + "Integrated Security=SSPI;");

    public static void Fill(Domain domain)
    {
      con.Open();
      SqlTransaction transaction = con.BeginTransaction();
      SqlCommand cmd = con.CreateCommand();
      cmd.Transaction = transaction;
      cmd.CommandText = "Select * from [dbo].[Categories]";
      var reader = cmd.ExecuteReader();

      using (domain.OpenSession())
      using (var tr = Transaction.Open()) {
        #region  Categories

        if (reader!=null) {
          while (reader.Read()) {
            var category = new Category();
            for (int i = 1; i < reader.FieldCount; i++)
              category[reader.GetName(i)] = !reader.IsDBNull(i) ? reader.GetValue(i) : null;
          }
          reader.Close();
        }

        #endregion

        #region Customers

        cmd.CommandText = "Select * from [dbo].[Customers]";
        reader = cmd.ExecuteReader();
        if (reader!=null) {
          while (reader.Read()) {
            var customer = new Customer(reader.GetString(0));
            for (int i = 1; i < reader.FieldCount; i++)
              customer[reader.GetName(i)] = !reader.IsDBNull(i) ? reader.GetValue(i) : null;
          }
          reader.Close();
        }

        #endregion

        #region Regions

        cmd.CommandText = "Select * from [dbo].[Region]";
        reader = cmd.ExecuteReader();
        if (reader!=null) {
          while (reader.Read()) {
            var region = new Region();
            for (int i = 1; i < reader.FieldCount; i++)
              region[reader.GetName(i)] = !reader.IsDBNull(i) ? reader.GetValue(i) : null;
          }
          reader.Close();
        }

        #endregion

        #region Suppliers

        cmd.CommandText = "Select * from [dbo].[Suppliers]";
        reader = cmd.ExecuteReader();
        if (reader!=null) {
          while (reader.Read()) {
            var supplier = new Supplier();
            for (int i = 1; i < reader.FieldCount; i++)
              supplier[reader.GetName(i)] = !reader.IsDBNull(i) ? reader.GetValue(i) : null;
          }
          reader.Close();
        }

        #endregion

        #region Shippers

        cmd.CommandText = "Select * from [dbo].[Shippers]";
        reader = cmd.ExecuteReader();
        if (reader!=null) {
          while (reader.Read()) {
            var shipper = new Shipper();
            for (int i = 1; i < reader.FieldCount; i++)
              shipper[reader.GetName(i)] = !reader.IsDBNull(i) ? reader.GetValue(i) : null;
          }
          reader.Close();
        }

        #endregion

        #region Products

        cmd.CommandText = "Select * from [dbo].[Products]";
        reader = cmd.ExecuteReader(CommandBehavior.KeyInfo);
        if (reader!=null) {
          while (reader.Read()) {
            var product = new Product();
            for (int i = 1; i < reader.FieldCount; i++)
              switch (i) {
              case 2:
                product.Supplier = !reader.IsDBNull(i) ? Key.Create<Supplier>(
                  Tuple.Create(reader.GetInt32(i))).Resolve<Supplier>() : null;
                break;
              case 3:
                product.Category = !reader.IsDBNull(i) ? Key.Create<Category>(
                  Tuple.Create(reader.GetInt32(i))).Resolve<Category>() : null;
                break;
              default:
                product[reader.GetName(i)] = !reader.IsDBNull(i) ? reader.GetValue(i) : null;
                break;
              }
          }
          reader.Close();
        }

        #endregion

        #region Employees

        cmd.CommandText = "Select * from [dbo].[Employees]";
        reader = cmd.ExecuteReader();
        if (reader!=null) {
          while (reader.Read()) {
            var employee = new Employee();
            for (int i = 1; i < reader.FieldCount; i++)
              if (i!=16)
                employee[reader.GetName(i)] = !reader.IsDBNull(i) ? reader.GetValue(i) : null;
          }
          reader.Close();
        }

        reader = cmd.ExecuteReader();
        if (reader!=null) {
          while (reader.Read()) {
            var employee = Key.Create<Employee>(Tuple.Create(reader.GetInt32(0))).Resolve<Employee>();
            bool isNull = reader.IsDBNull(16);
            if (!isNull) {
              int employeeId = reader.GetInt32(16);
              Key key = Key.Create<Employee>(Tuple.Create(employeeId));
              var reportsTo = key.Resolve<Employee>();
              employee.ReportsTo = reportsTo;
            }
          }
          reader.Close();
        }

        #endregion

        #region Territories

        cmd.CommandText = "Select * from [dbo].[Territories]";
        reader = cmd.ExecuteReader();
        if (reader!=null) {
          while (reader.Read()) {
            new Territory(reader.GetString(0))
              {
                TerritoryDescription = reader.GetString(1),
                Region = Key.Create<Region>(Tuple.Create(reader.GetInt32(2))).Resolve<Region>()
              };
          }
          reader.Close();
        }

        #endregion

        #region EmployeeTerritories

        cmd.CommandText = "Select * from [dbo].[EmployeeTerritories]";
        reader = cmd.ExecuteReader();
        if (reader!=null) {
          while (reader.Read()) {
            var territory = Key.Create<Territory>(Tuple.Create(reader.GetString(1))).Resolve<Territory>();
            territory.Employees.Add(Key.Create<Employee>(Tuple.Create(reader.GetInt32(0))).Resolve<Employee>());
          }
          reader.Close();
        }

        #endregion

        #region Orders

        cmd.CommandText = "Select * from [dbo].[Orders]";
        reader = cmd.ExecuteReader(CommandBehavior.KeyInfo);
        if (reader!=null) {
          while (reader.Read()) {
            var order = new Order();
            for (int i = 1; i < reader.FieldCount; i++)
              switch (i) {
              case 1:
                order.Customer = !reader.IsDBNull(i) ? Key.Create<Customer>(
                  Tuple.Create(reader.GetString(i))).Resolve<Customer>() : null;
                break;
              case 2:
                order.Employee = !reader.IsDBNull(i) ? Key.Create<Employee>(
                  Tuple.Create(reader.GetInt32(i))).Resolve<Employee>() : null;
                break;
              case 6:
                order.ShipVia = !reader.IsDBNull(i) ? Key.Create<Shipper>(
                  Tuple.Create(reader.GetInt32(i))).Resolve<Shipper>() : null;
                break;
              default:
                order[reader.GetName(i)] = !reader.IsDBNull(i) ? reader.GetValue(i) : null;
                break;
              }
          }
          reader.Close();
        }

        #endregion

        #region OrderDetails

        cmd.CommandText = "Select Min(OrderID) from [dbo].[Orders]";
        reader = cmd.ExecuteReader();
        int minValue = -1;
        if (reader!=null) {
          reader.Read();
          minValue = reader.GetInt32(0) - 1;
          reader.Close();
        }

        cmd.CommandText = "Select * from [dbo].[Order Details]";
        reader = cmd.ExecuteReader();
        if (reader!=null) {
          while (reader.Read()) {
            var order = Key.Create<Order>(Tuple.Create(reader.GetInt32(0) - minValue)).Resolve<Order>();
            var product = Key.Create<Product>(Tuple.Create(reader.GetInt32(1))).Resolve<Product>();
            var orderDetails = new OrderDetails(order, product);

            for (int i = 2; i < reader.FieldCount; i++)
              orderDetails[reader.GetName(i)] = !reader.IsDBNull(i) ? reader.GetValue(i) : null;
          }
          reader.Close();
        }

        #endregion

        Session.Current.Persist();
        tr.Complete();
      }
      transaction.Commit();
      con.Close();
    }
  }
}