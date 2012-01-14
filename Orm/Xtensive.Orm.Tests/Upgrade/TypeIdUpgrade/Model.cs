// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.12.28

using System;
using System.Diagnostics;

namespace Xtensive.Orm.Tests.Upgrade.TypeIdUpgrade.Model
{
  [Serializable]
  [HierarchyRoot]
  public class Person : Entity
  {
    [Key, Field]
    public long Id { get; private set; }

    [Field]
    public string FirstName { get; set; }

    [Field]
    public string LastName { get; set; }
  }

  [Serializable]
  public class Employee : Person
  {
    [Field]
    public string DepartmentName { get; set; }
  }
}