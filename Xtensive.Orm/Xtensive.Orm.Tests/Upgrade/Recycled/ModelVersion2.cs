// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.20

using Xtensive.Orm.Upgrade;
using System;

namespace Xtensive.Orm.Tests.Upgrade.Recycled.Model.Version2
{
  [Serializable]
  [HierarchyRoot]
  public class Person : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }


    // Constructors

    public Person()
    {
    }

    public Person(int id)
      : base(id)
    {
    }
  }

  [Serializable]
  public class Customer : Person
  {
    [Field(Length = 256)]
    public string Address { get; set; }

    [Field(Length = 24)]
    public string Phone { get; set; }


    // Constructors

    public Customer()
    {
    }

    public Customer(int id)
      : base(id)
    {
    }
  }

  [Serializable]
  public class Employee : Person
  {
    [Field(Length = 30)]
    public string CompanyName { get; set; }


    // Constructors

    public Employee()
    {
    }

    public Employee(int id)
      : base(id)
    {
    }
  }

  [Serializable]
  [HierarchyRoot]
  public class Order : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    [Recycled("Employee"), Obsolete]
    public RcEmployee RcEmployee { get; set; }

    [Field]
    [Recycled("Customer"), Obsolete]
    public RcCustomer RcCustomer { get; set; }

    [Field]
    public Person Employee { get; set; }

    [Field]
    public Person Customer { get; set; }

    [Field(Length = 128)]
    public string ProductName { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format("Id={0}, Customer={1}, Employee={2}, ProductName={3}",
        Id,
        Customer!=null ? Customer.Name : "null",
        Employee!=null ? Employee.Name : "null",
        ProductName ?? "null");
    }
  }

  [Serializable]
  [Recycled("Xtensive.Storage.Tests.Upgrade.Recycled.Model.Version1.Customer")]
  [HierarchyRoot]
  public class RcCustomer : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Length = 256)]
    public string Address { get; set; }

    [Field(Length = 24)]
    public string Phone { get; set; }

    [Field(Length = 30)]
    public string Name{ get; set;}
  }

  [Serializable]
  [Recycled("Xtensive.Storage.Tests.Upgrade.Recycled.Model.Version1.Employee")]
  [HierarchyRoot]
  public class RcEmployee : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Length = 30)]
    public string Name { get; set; }

    [Field(Length = 30)]
    public string CompanyName { get; set; }
  }
}