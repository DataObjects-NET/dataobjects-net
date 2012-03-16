// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System;

namespace Xtensive.Orm.Tests.Upgrade.Recycled.Model.Version1
{
  [Serializable]
  [HierarchyRoot]
  public class Customer : Entity
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
  [HierarchyRoot]
  public class Employee : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field(Length = 30)]
    public string CompanyName { get; set; }

    [Field(Length = 30)]
    public string Name { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Order : Entity
  {
    [Field, Key]
    public int Id { get; private set; }

    [Field]
    public Employee Employee { get; set; }

    [Field]
    public Customer Customer { get; set; }

    [Field(Length = 128)]
    public string ProductName { get; set; }
    
  }
}