// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.20

using Xtensive.Storage.Upgrade;
using System;

namespace Xtensive.Storage.Tests.Upgrade.Sample3.Model.Version2
{
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

  public class Employee : Person
  {
    [Field, Obsolete, Recycled("Department")]
    public string RcDepartment { get; set; }

    [Field, Obsolete, Recycled]
    public bool IsHead { get; set; }

    [Field]
    public string DepartmentName { get; set; }
  }
}