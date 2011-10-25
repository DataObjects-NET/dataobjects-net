// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System;

namespace Xtensive.Orm.Tests.Issues.IssueJira0208_IncorrectUpgradeSequence.Model.Version1
{
  [HierarchyRoot]
  public class EntityToRemove1 : Entity
  {
    [Key, Field]
    public long Id { get; private set; }
  }

  [HierarchyRoot]
  public class EntityToKeep1 : Entity
  {
    [Key, Field]
    public long Id { get; private set; }

    [Field]
    public EntityToRemove1 EvilRef { get; set; }

    public EntityToKeep1(EntityToRemove1 evilRef)
    {
      EvilRef = evilRef;
    }
  }

  // Same as 1 but with NOT NULL constraint

  [HierarchyRoot]
  public class EntityToRemove2 : Entity
  {
    [Key, Field]
    public long Id { get; private set; }
  }

  [HierarchyRoot]
  public class EntityToKeep2 : Entity
  {
    [Key, Field]
    public long Id { get; private set; }

    [Field(Nullable = false)]
    public EntityToRemove2 EvilRef { get; set; }

    public EntityToKeep2(EntityToRemove2 evilRef)
    {
      EvilRef = evilRef;
    }
  }
}