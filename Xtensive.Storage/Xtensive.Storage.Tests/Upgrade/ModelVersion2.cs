// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System;

namespace Xtensive.Storage.Tests.Upgrade.Model.Version2
{
  // BusinessContact renamed to Person
  // Person renamed to BusinessContact
  // Fields LastName and FirstName moved from Employee to BusinessContact (old Person)
  // Field Order.ProcessingTime removed
  
  public class Address : Structure
  {
    [Field(Length = 15)]
    public string City { get; set; }

    [Field(Length = 15)]
    public string Country { get; set; }
  }

  [Index("FirstName")]
  [HierarchyRoot]
  public class BusinessContact : Entity
  {
    [Field, KeyField]
    public int Id { get; private set; }

    [Field]
    public Address Address { get; set; }

    [Field(Length = 24)]
    public string Phone { get; set; }

    [Field(Length = 20)]
    public string LastName { get; set; }

    [Field(Length = 10)]
    public string FirstName { get; set; }
  }
  
  [Index("CompanyName")]
  public class Person : BusinessContact
  {
    [Field(Length = 40)]
    public string CompanyName { get; set; }

    [Field(Length = 30)]
    public string ContactName { get; set; }
  }

  [Index("HireDate")]
  public class Employee : BusinessContact
  {
    [Field]
    public DateTime? HireDate { get; set; }

    [Field]
    public Employee ReportsTo { get; set; }

    [Field(PairTo = "ReportsTo")]
    public EntitySet<Employee> ReportingEmployees { get; set; }

    [Field(PairTo = "Employee")]
    public EntitySet<Order> Orders { get; private set; }

    public string FullName { get { return FirstName + " " + LastName; } }
  }

  [Index("OrderDate")]
  [Index("Freight")]
  [HierarchyRoot]
  public class Order : Entity
  {
    [Field, KeyField]
    public int Id { get; private set; }

    [Field]
    public Employee Employee { get; set; }

    [Field]
    public Person Customer { get; set; }

    [Field]
    public DateTime OrderDate { get; set; }

    [Field(Length = 128)]
    public string ProductName { get; set; }

    [Field]
    public decimal? Freight { get; set; }

    [Field]
    public Address ShippingAddress { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format("OrderId: {0}; OrderDate: {1}.", Id, OrderDate);
    }
  }
}