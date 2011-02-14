// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.05.20

using System;

namespace Xtensive.Storage.Tests.Upgrade.UpgradeAndNamingRules.Model.Version1
{
  [Serializable]
  [HierarchyRoot]
  public class Person : Entity
  {
    [Key, Field]
    public long Id { get; private set; }
  }
}