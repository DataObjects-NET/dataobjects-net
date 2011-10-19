// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System;

namespace Xtensive.Orm.Tests.Issues.IssueJira0208_IncorrectUpgradeSequence.Model.Version1
{
  [HierarchyRoot]
  public class EntityToRemove : Entity
  {
    [Key, Field]
    public long Id { get; private set; }
  }

  [HierarchyRoot]
  public class EntityToKeep : Entity
  {
    [Key, Field]
    public long Id { get; private set; }

    [Field]
    public EntityToRemove EvilRef { get; set; }

    public EntityToKeep(EntityToRemove evilRef)
    {
      EvilRef = evilRef;
    }
  }
}