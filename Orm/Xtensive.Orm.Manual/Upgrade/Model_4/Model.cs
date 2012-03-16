// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.11.19

using System;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Manual.Upgrade.Model_4
{
  [Serializable]
  [HierarchyRoot]
  public class Order : Entity
  {
    // ...

    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public Product Product { get; set; }

    [Field, Obsolete, Recycled]
    public string ProductName { get; set; }

    [Field]
    public int Quantity { get; set; }

    [Field]
    public Person Customer { get; set;}

    public Order(Session session)
      : base(session)
    {}
  }

  [Serializable]
  [HierarchyRoot]
  public class Product : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string Name { get; set; }

    public Product(Session session)
      : base(session)
    {}
  }

  [Serializable]
  [HierarchyRoot]
  public class Person : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string FullName { get; set; }

    public Person(Session session)
      : base(session)
    {}
  }
}