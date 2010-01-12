// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.08.04

using System;

namespace Xtensive.Storage.Tests.Storage.Performance.CrudModel
{
  [Serializable]
  [HierarchyRoot]
  public class PairedSimplestContainerItem : Entity
  {
    [Field, Key]
    public long Id { get; private set; }

    [Field]
    [Association(PairTo = "Items")]
    public SimplestContainer Owner { get; private set; }
  }
}