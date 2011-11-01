// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System.Collections;
using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Core;
using Xtensive.Storage.Upgrade;
using System;

namespace Xtensive.Storage.Tests.Issues.IssueJira0208_IncorrectUpgradeSequence.Model.Version2
{
  [HierarchyRoot, Index("Field", Unique = true)]
  public class VeryUniqueEntity : Entity
  {
    [Key, Field]
    public long Id { get; private set; }

    [Field]
    public int Field { get; set; }
  }

  [HierarchyRoot]
  public class EntityToKeep1 : Entity
  {
    [Key, Field]
    public long Id { get; private set; }
  }

  [HierarchyRoot]
  public class EntityToKeep2 : Entity
  {
    [Key, Field]
    public long Id { get; private set; }
  }
}