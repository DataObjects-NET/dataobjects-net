// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.11.12

using System;

namespace Xtensive.Orm.Manual.Upgrade.Model_3
{
  [Serializable]
  [HierarchyRoot]
  public class Order : Entity
  {
    // ...

    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string ProductName { get; set; }

    [Field]
    public int Quantity { get; set; }

    [Field]
    public Person Customer { get; set;}
  }

  [Serializable]
  [HierarchyRoot]
  public class Person : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field]
    public string FullName { get; set; }
  }
}