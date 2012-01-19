// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.04

using System;

namespace Xtensive.Orm.Tests.Storage.Performance.CrudModel
{
  [Serializable]
  [HierarchyRoot]
  public sealed class NonPairedSimplestContainerItem : Entity
  {
    [Field, Key]
    public long Id { get; private set; }
  }
}