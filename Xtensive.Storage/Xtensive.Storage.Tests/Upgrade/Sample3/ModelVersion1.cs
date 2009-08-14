// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.20

namespace Xtensive.Storage.Tests.Upgrade.Sample3.Model.Version1
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
    [Field]
    public string Department { get; set; }

    [Field]
    public bool IsHead { get; set; }

    public override string ToString()
    {
      return FullName + " (" + Department + ")";
    }
  }
}