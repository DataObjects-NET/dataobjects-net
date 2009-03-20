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
  public class Schema : NodeBase<Database, Server>
  {
    protected override Nesting CreateNesting()
    {
      return new Nesting<Schema, Database, SchemaCollection>(this, "Schemas");
    }


    public Schema(Database parent, string name)
      : base(parent, name)
    {
    }

    public Schema(Database parent, string name, int index)
      : base(parent, name, index)
    {
    }
  }
}