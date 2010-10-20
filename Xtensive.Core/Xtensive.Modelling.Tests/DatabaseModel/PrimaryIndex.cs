// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.18

using System;
using System.Diagnostics;

namespace Xtensive.Modelling.Tests.DatabaseModel
{
  [Serializable]
  public sealed class PrimaryIndex : Index
  {
    protected override Nesting CreateNesting()
    {
      return new Nesting<PrimaryIndex, Table, PrimaryIndex>(this, "PrimaryIndex");
    }

    
    public PrimaryIndex(Table parent, string name)
      : base(parent, name)
    {
    }
  }
}