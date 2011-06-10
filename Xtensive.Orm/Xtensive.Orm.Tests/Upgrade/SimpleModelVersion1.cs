// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.01.15

using System;
using System.Diagnostics;

namespace Xtensive.Orm.Tests.Upgrade.Model.SimpleVersion1
{
  [Serializable]
  public class Address : Structure
  {
    [Field(Length = 15)]
    public string City { get; set; }

    [Field(Length = 15)]
    public string Country { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Person : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public Address Address { get; set; }

    [Field(Length = 24)]
    public string Phone { get; set; }

    [Field(Length = 10)]
    public string PassportNumber { get; set; }
  }

  [Serializable]
  [Index("CompanyName")]
  public class BusinessContact : Person
  {
    [Field(Length = 40)]
    public string CompanyName { get; set; }

    [Field(Length = 30)]
    public string ContactName { get; set; }
  }

  [Serializable]
  [Index("FirstName")]
  [Index("HireDate", "LastName")]
  public class Employee : Person
  {
    [Field(Length = 20)]
    public string LastName { get; set; }

    [Field(Length = 10)]
    public string FirstName { get; set; }

    [Field]
    public Address HomeAddress { get; set; }

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

    [Field(Length = 10)]
    public string OrderNumber { get; set; }

    [Field]
    public TimeSpan? ProcessingTime { get; set; }

    [Field]
    public Employee Employee { get; set; }

    [Field]
    public BusinessContact Customer { get; set; }

    [Field]
    public DateTime OrderDate { get; set; }

    [Field(Length = 128)]
    public string ProductName { get; set; }

    [Field]
    public decimal? Freight { get; set; }

    [Field]
    public Address ShippingAddress { get; set; }

    [Field]
    public OrderData Data { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format("OrderId: {0}; OrderDate: {1}.", Id, OrderDate);
    }
  }

  public class OrderData : Structure
  {
    [Field]
    public int Value1 { get; set; }

    [Field]
    public int Value2 { get; set; }
  }
}