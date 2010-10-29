// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.03

using System;

namespace Xtensive.Orm.Tests.Storage.Performance.CrudModel
{
  [Serializable]
  [HierarchyRoot]
  public sealed class SimplestContainer : Entity
  {
    [Key, Field]
    public long Id { get; private set; }

    [Field]
    [Association(OnOwnerRemove = OnRemoveAction.Cascade)]
    public EntitySet<NonPairedSimplestContainerItem> DistantItems { get; private set; }

    [Field]
    [Association(OnOwnerRemove = OnRemoveAction.Cascade, PairTo = "Owner")]
    public EntitySet<PairedSimplestContainerItem> Items { get; private set; }
  }
}