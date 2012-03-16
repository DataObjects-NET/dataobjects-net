// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System.Collections;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Upgrade;
using System;

namespace Xtensive.Orm.Tests.Upgrade.Sample3.Model.Version2
{
  [Serializable]
  [HierarchyRoot]
  public abstract class Person : Entity
  {
    [Key, Field]
    public long Id { get; private set; }

    [Field]
    public string FirstName { get; set; }

    [Field]
    public string LastName { get; set; }

    public string FullName { get { return FirstName + " " + LastName; } }

    [Field]
    public string ContactPhone { get; set; }
  }

  [Serializable]
  public class Employee : Person
  {
    [Field, Obsolete, Recycled("Department")]
    public string RcDepartment { get; set; }

    [Field, Obsolete, Recycled]
    public bool IsHead { get; set; }

    [Field]
    public string DepartmentName { get; set; }
  }

  [Serializable]
  [HierarchyRoot]
  public class Order : Entity
  {
    [Key, Field]
    public long Id { get; private set; }

    [Field]
    public Employee Seller { get; set; }

    [Field, Obsolete, Recycled]
    public int Amount { get; set;}

    [Field, Obsolete, Recycled]
    public string ProductName{ get; set;}

    [Field, Association(PairTo = "Order", OnTargetRemove = OnRemoveAction.Clear)]
    public EntitySet<OrderItem> Items { get; private set; }

    [Field]
    public DateTime? OrderDate { get; set; }

    public override string ToString()
    {
      var productNames = Items.Select(item => item.ProductName).ToCommaDelimitedString();
      return string.Format("Order {{\tSeller = {0}\n\tProducts = {1}\n}}",
        Seller.FullName, productNames);
    }

  }

  [Serializable]
  [HierarchyRoot]
  public class OrderItem : Entity
  {
    [Key, Field]
    public long Id { get; private set; }

    [Field]
    public Order Order { get; private set; }

    [Field]
    public string ProductName { get; set; }

    [Field]
    public int Amount { get; set; }
    
    public OrderItem(Order order)
    {
      Order = order;
    }
  }
}