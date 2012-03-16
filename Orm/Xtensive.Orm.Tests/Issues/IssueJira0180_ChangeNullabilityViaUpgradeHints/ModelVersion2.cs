// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.09.19

using System;

namespace Xtensive.Orm.Tests.Issues.IssueJira0180_ChangeNullabilityViaUpgradeHints.Model.Version2
{
  [HierarchyRoot]
  public class Person : Entity
  {
    [Key, Field]
    public long Id { get; private set; }
    
    [Field(Nullable = false)] // Made non-nullable
    public string Name { get; set; }

    [Field] // Made non-nullable
    public decimal Weight { get; set; }

    // Age is removed

    // Car is removed

    [Field(Nullable = false)] // Made non-nullable
    public Phone Phone { get; set; }
  }

  [HierarchyRoot]
  public class Car : Entity
  {
    [Key, Field]
    public long Id { get; private set; }
  }

  [HierarchyRoot]
  public class Phone : Entity
  {
    [Key, Field]
    public long Id { get; private set; }
  }
}