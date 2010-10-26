// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.01.15

using System;
using System.Diagnostics;

namespace Xtensive.Orm.Tests.Upgrade.Model.SimpleVersion2
{
  // BusinessContact renamed to Person
  // Person renamed to BusinessContact
  // Fields LastName and FirstName moved from Employee to BusinessContact (old Person)
  // Field Order.ProcessingTime removed
  // Field Order.OrderNumber renamed to Order.Number
  // Type of field Order.Number changed to int
  // Type of field BusinessContact.PassportNumber (old Person.PassportNumber) changed to int

  [Serializable]
  public class Address : Structure
  {
    [Field(Length = 15)]
    public string City { get; set; }

    [Field(Length = 15)]
    public string Country { get; set; }
  }

  [Serializable]
  [Index("FirstName")]
  [HierarchyRoot]
  public class BusinessContact : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public int PassportNumber { get; set; }

    [Field]
    public Address BusinessAddress { get; set; }

    [Field(Length = 24)]
    public string Phone { get; set; }

    [Field(Length = 20)]
    public string LastName { get; set; }

    [Field(Length = 10)]
    public string FirstName { get; set; }
  }

  [Serializable]
  [Index("CompanyName")]
  public class Person : BusinessContact
  {
    [Field(Length = 40)]
    public string CompanyName { get; set; }

    [Field(Length = 30)]
    public string ContactName { get; set; }
  }

  [Serializable]
  [Index("HireDate")]
  public class Employee : BusinessContact
  {
    [Field]
    public Address Address { get; set; }

    [Field]
    public DateTime? HireDate { get; set; }

    [Field]
    public Employee ReportsTo { get; set; }

    [Field, Association(PairTo = "ReportsTo")]
    public EntitySet<Employee> ReportingEmployees { get; set; }

    [Field, Association(PairTo = "Employee")]
    public EntitySet<Order> Orders { get; private set; }

    public string FullName { get { return FirstName + " " + LastName; } }
  }

  [Serializable]
  [Index("OrderDate")]
  [Index("Freight")]
  [HierarchyRoot]
  public class Order : Entity
  {
    [Field, Key]
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
    public int Number { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format("OrderId: {0}; OrderDate: {1}.", Id, OrderDate);
    }
  }
}