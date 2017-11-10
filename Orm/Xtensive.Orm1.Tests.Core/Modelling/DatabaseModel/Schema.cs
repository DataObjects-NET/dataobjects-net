// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.18

using System;
using System.Diagnostics;
using Xtensive.Modelling;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Orm.Tests.Core.Modelling.DatabaseModel
{
  [Serializable]
  public sealed class Schema : NodeBase<Database>
  {
    [Property]
    public TableCollection Tables { get; private set; }

    protected override Nesting CreateNesting()
    {
      return new Nesting<Schema, Database, SchemaCollection>(this, "Schemas");
    }

    protected override void Initialize()
    {
      base.Initialize();
      if (Tables==null)
        Tables = new TableCollection(this);
    }


    public Schema(Database parent, string name)
      : base(parent, name)
    {
    }
  }
}