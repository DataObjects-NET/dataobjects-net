// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.18

using System;
using System.Diagnostics;
using Xtensive.Modelling;

namespace Xtensive.Core.Tests.Modelling.DatabaseModel
{
  [Serializable]
  public sealed class SecondaryIndex : Index
  {
    protected override Nesting CreateNesting()
    {
      return new Nesting<SecondaryIndex, Table, SecondaryIndexCollection>(this, "SecondaryIndexes");
    }

    
    public SecondaryIndex(Table parent, string name)
      : base(parent, name)
    {
    }
  }
}