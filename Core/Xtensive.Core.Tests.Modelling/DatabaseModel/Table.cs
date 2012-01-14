// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.18

using System;
using Xtensive.Modelling;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Core.Tests.Modelling.DatabaseModel
{
  [Serializable]
  public sealed class Table : NodeBase<Schema>
  {
    private PrimaryIndex primaryIndex;

    [Property(Priority = 100)]
    public PrimaryIndex PrimaryIndex {
      get { return primaryIndex; }
      set {
        ChangeProperty("PrimaryIndex", value, (x,v) => ((Table)x).primaryIndex = v);
      }
    }

    [Property(Priority = 200)]
    public SecondaryIndexCollection SecondaryIndexes { get; private set; }

    protected override Nesting CreateNesting()
    {
      return new Nesting<Table, Schema, TableCollection>(this, "Tables");
    }

    protected override void Initialize()
    {
      base.Initialize();
      if (SecondaryIndexes==null)
        SecondaryIndexes = new SecondaryIndexCollection(this);
    }


    public Table(Schema parent, string name)
      : base(parent, name)
    {
    }
  }
}