// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.12.13

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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
    {}

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
    {}

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
      get
      {
        return Session.Query.All<Order>().Where(o => o.ShipVia==this);
      }
    }

    /// <summary>
    /// Tests subqery with 'this' scenario.
    /// </summary>
    public Order FirstOrder
    {
      get
      {
        return Session.Query.All<Order>().Where(o => o.ShipVia==this).FirstOrDefault();
      }
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
    public ProductType ProductType { get; protected set;}

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

    public string FullName { get { return FirstName + " " + LastName; } }
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
    public static void Fill(Domain domain)
    {
      var con = new SqlConnection(EnvironmentConfiguration.NorthwindConnectionString);
      con.Open();
      try
      {
        SqlTransaction transaction = con.BeginTransaction();
        SqlCommand cmd = con.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = "Select * from [dbo].[Categories]";
        var reader = cmd.ExecuteReader();

        using (var session = domain.OpenSession())
        using (var tr = session.OpenTransaction(System.Transactions.IsolationLevel.ReadCommitted))
        {
          #region  Categories

          var categories = new Dictionary<object, Category>();
          if (reader != null)
          {
            while (reader.Read())
            {
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
          if (reader != null)
          {
            while (reader.Read())
            {
              var customer = new Customer(reader.GetString(0));
              for (int i = 1; i < reader.FieldCount; i++)
              {
                string dbName = reader.GetName(i);
                string fieldName;
                Address address = customer.Address;
                switch (dbName)
                {
                  case "Address":
                    address.StreetAddress = (string)(!reader.IsDBNull(i) ? reader.GetValue(i) : null);
                    break;
                  case "City":
                  case "Region":
                  case "PostalCode":
                  case "Country":
                    fieldName = dbName;
                    address[fieldName] = !reader.IsDBNull(i) ? reader.GetValue(i) : string.Empty;
                    break;
                  default:
                    fieldName = dbName;
                    customer[fieldName] = !reader.IsDBNull(i) ? reader.GetValue(i) : null;
                    break;
                }
              }
              customers.Add(reader.GetValue(0), customer);
            }
            reader.Close();
          }

          #endregion

          #region Regions

          var regions = new Dictionary<object, Region>();
          cmd.CommandText = "Select * from [dbo].[Region]";
          reader = cmd.ExecuteReader();
          if (reader != null)
          {
            while (reader.Read())
            {
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
          if (reader != null)
          {
            while (reader.Read())
            {
              var supplier = new Supplier();
              for (int i = 1; i < reader.FieldCount; i++)
              {
                string dbName = reader.GetName(i);
                string fieldName;
                Address address = supplier.Address;
                switch (dbName)
                {
                  case "Address":
                    address.StreetAddress = (string)(!reader.IsDBNull(i) ? reader.GetValue(i) : null);
                    break;
                  case "City":
                  case "Region":
                  case "PostalCode":
                  case "Country":
                    fieldName = dbName;
                    address[fieldName] = !reader.IsDBNull(i) ? reader.GetValue(i) : null;
                    break;
                  default:
                    fieldName = dbName;
                    supplier[fieldName] = !reader.IsDBNull(i) ? reader.GetValue(i) : null;
                    break;
                }
              }
              suppliers.Add(reader.GetValue(0), supplier);
            }
            reader.Close();
          }

          #endregion

          #region Shippers

          var shippers = new Dictionary<object, Shipper>();
          cmd.CommandText = "Select * from [dbo].[Shippers]";
          reader = cmd.ExecuteReader();
          if (reader != null)
          {
            while (reader.Read())
            {
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
          if (reader != null)
          {
            while (reader.Read())
            {
              var discontinuedColumnIndex = reader.GetOrdinal("Discontinued");
              Product product = reader.GetBoolean(discontinuedColumnIndex)
                ? (Product)new DiscontinuedProduct()
                : new ActiveProduct();
              for (int i = 1; i < reader.FieldCount; i++)
                switch (i)
                {
                  case 2:
                    product.Supplier = !reader.IsDBNull(i) ? suppliers[reader.GetValue(i)] : null;
                    break;
                  case 3:
                    product.Category = !reader.IsDBNull(i) ? categories[reader.GetValue(i)] : null;
                    break;
                  default:
                    if (i != discontinuedColumnIndex)
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
          if (reader != null)
          {
            while (reader.Read())
            {
              var employee = new Employee();
              for (int i = 1; i < reader.FieldCount; i++)
              {
                if (i == 16)
                  continue;
                string dbName = reader.GetName(i);
                string fieldName;
                Address address = employee.Address;
                switch (dbName)
                {
                  case "Address":
                    address.StreetAddress = (string)(!reader.IsDBNull(i) ? reader.GetValue(i) : null);
                    break;
                  case "City":
                  case "Region":
                  case "PostalCode":
                  case "Country":
                    fieldName = dbName;
                    address[fieldName] = !reader.IsDBNull(i) ? reader.GetValue(i) : string.Empty;
                    break;
                  default:
                    fieldName = dbName;
                    employee[fieldName] = !reader.IsDBNull(i) ? reader.GetValue(i) : null;
                    break;
                }
              }
              employees.Add(reader.GetValue(0), employee);
            }
            reader.Close();
          }

          reader = cmd.ExecuteReader();
          if (reader != null)
          {
            while (reader.Read())
            {
              var employee = employees[reader.GetValue(0)];
              employee.ReportsTo = !reader.IsDBNull(16) ? employees[reader.GetValue(16)] : null;
            }
            reader.Close();
          }

          #endregion

          #region Territories

          var territories = new Dictionary<object, Territory>();
          cmd.CommandText = "Select * from [dbo].[Territories]";
          reader = cmd.ExecuteReader();
          if (reader != null)
          {
            while (reader.Read())
            {
              var territory = new Territory(reader.GetString(0));
              territory.TerritoryDescription = reader.GetString(1);
              territory.Region = regions[reader.GetInt32(2)];
              territories.Add(reader.GetValue(0), territory);
            }
            reader.Close();
          }

          #endregion

          #region EmployeeTerritories

          cmd.CommandText = "Select * from [dbo].[EmployeeTerritories]";
          reader = cmd.ExecuteReader();
          if (reader != null)
          {
            while (reader.Read())
            {
              var employee = employees[reader.GetValue(0)];
              var territory = territories[reader.GetValue(1)];
              territory.Employees.Add(employee);
            }
            reader.Close();
          }

          #endregion

          #region Orders

          var orders = new Dictionary<object, Order>();
          cmd.CommandText = "Select * from [dbo].[Orders]";
          reader = cmd.ExecuteReader(CommandBehavior.KeyInfo);
          if (reader != null)
          {
            while (reader.Read())
            {
              var order = new Order();
              for (int i = 1; i < reader.FieldCount; i++)
                switch (i)
                {
                  case 1:
                    order.Customer = !reader.IsDBNull(i) ? customers[reader.GetValue(i)] : null;
                    break;
                  case 2:
                    order.Employee = !reader.IsDBNull(i) ? employees[reader.GetValue(i)] : null;
                    break;
                  case 3:
                    order.OrderDate = !reader.IsDBNull(i) ? (DateTime?)reader.GetDateTime(i) : null;
                    break;
                  case 4:
                    order.RequiredDate = !reader.IsDBNull(i) ? (DateTime?)reader.GetDateTime(i) : null;
                    break;
                  case 5:
                    order.ShippedDate = !reader.IsDBNull(i) ? (DateTime?)reader.GetDateTime(i) : null;
                    break;
                  case 6:
                    order.ShipVia = !reader.IsDBNull(i) ? shippers[reader.GetValue(i)] : null;
                    break;
                  case 7:
                    order.Freight = reader.GetDecimal(i);
                    break;
                  default:
                    string dbName = reader.GetName(i);
                    string fieldName;
                    Address address = order.ShippingAddress;
                    switch (dbName)
                    {
                      case "ShipAddress":
                        address.StreetAddress = (string)(!reader.IsDBNull(i) ? reader.GetValue(i) : null);
                        break;
                      case "ShipCity":
                      case "ShipRegion":
                      case "ShipPostalCode":
                      case "ShipCountry":
                        fieldName = dbName.Substring(4);
                        address[fieldName] = !reader.IsDBNull(i) ? reader.GetValue(i) : null;
                        break;
                      default:
                        fieldName = dbName;
                        order[fieldName] = !reader.IsDBNull(i) ? reader.GetValue(i) : null;
                        break;
                    }
                    break;
                }
              orders.Add(reader.GetValue(0), order);
            }
            reader.Close();
          }

          foreach (var o in orders.Values)
            o.ProcessingTime = o.ShippedDate - o.OrderDate;

          #endregion

          #region OrderDetails

          cmd.CommandText = "Select * from [dbo].[Order Details]";
          reader = cmd.ExecuteReader();
          if (reader != null)
          {
            while (reader.Read())
            {
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
      }
      finally {
        con.Close();
      }
    }
  }
}