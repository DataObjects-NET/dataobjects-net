// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.03

namespace Xtensive.Storage.Tests.Storage.Performance.CrudModel
{
  [HierarchyRoot]
  public sealed class SimplestCollection : Entity
  {
    [Key, Field]
    public long Id { get; private set; }

    [Field]
    [Association(OnTargetRemove = OnRemoveAction.Clear)]
    public EntitySet<Simplest> Items { get; private set; }
  }
}