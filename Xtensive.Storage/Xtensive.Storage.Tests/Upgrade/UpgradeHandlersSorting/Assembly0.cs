// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.06.25

using System;
using System.Diagnostics;
using Xtensive.Storage;

namespace UpgradeHandlersSorting.Model
{
  [Serializable]
  [HierarchyRoot]
  public class Simple0 : Entity
  {
    [Field, Key]
    public int Id
    {
      get { return GetFieldValue<int>("Id");}
    }

    public Simple0()
    {}

    protected Simple0(EntityState state)
      : base(state)
    {}
  }
}