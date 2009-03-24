// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.18

using System;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Modelling.Tests.DatabaseModel
{
  [Serializable]
  public class Table : NodeBase<Schema>
  {
    private PrimaryIndex primaryIndex;

    [Property]
    public PrimaryIndex PrimaryIndex {
      get { return primaryIndex; }
      set {
        ChangeProperty("PrimaryIndex", value, (x,v) => ((Table)x).primaryIndex = v);
      }
    }

    [Property]
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

    public Table(Schema parent, string name, int index)
      : base(parent, name, index)
    {
    }
  }
}