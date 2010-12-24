// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.11.11

using System;
using System.Diagnostics;

namespace Xtensive.Orm.Manual.Upgrade.Model_2
{
  [Serializable]
  [HierarchyRoot]
  public class Order : Entity
  {
    [Key, Field]
    public int Id { get; private set;}

    [Field]
    public string ProductName { get; set; }

    [Field]
    public Customer Customer { get; set;}

    [Field]
    public int Quantity { get; set; }

    public Order(Session session)
      : base(session)
    {}
  }

  [Serializable]
  [HierarchyRoot]
  public class Customer : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    public Customer(Session session)
      : base(session)
    {}
  }
}