// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.09.19

using System;

namespace Xtensive.Storage.Tests.Issues.Issue_Jira0180_ChangeNullabilityViaUpgradeHints.Model.Version2
{
  [Serializable]
  [HierarchyRoot]
  public class Person : Entity
  {
    [Key, Field]
    public long Id { get; private set; }
    
    [Field(Nullable = false)]
    public string Name { get; set; }

    [Field]
    public decimal Weight { get; set; }
  }
}