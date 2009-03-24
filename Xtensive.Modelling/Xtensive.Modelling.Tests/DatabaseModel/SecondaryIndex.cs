// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.18

using System;
using System.Diagnostics;

namespace Xtensive.Modelling.Tests.DatabaseModel
{
  [Serializable]
  public class SecondaryIndex : Index
  {
    protected override Nesting CreateNesting()
    {
      return new Nesting<SecondaryIndex, Table, SecondaryIndexCollection>(this, "SecondaryIndexes");
    }

    
    public SecondaryIndex(Table parent, string name)
      : base(parent, name)
    {
    }

    public SecondaryIndex(Table parent, string name, int index)
      : base(parent, name, index)
    {
    }
  }
}