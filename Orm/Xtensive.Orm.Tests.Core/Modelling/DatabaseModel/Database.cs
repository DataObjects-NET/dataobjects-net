// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.18

using System;
using Xtensive.Modelling;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Orm.Tests.Core.Modelling.DatabaseModel
{
  [Serializable]
  public sealed class Database : NodeBase<Server>
  {
    private User owner;

    [Property(Priority = -1000)]
    public User Owner
    {
      get { return owner; }
      set {
        ChangeProperty("Owner", value, (x,v) => ((Database)x).owner = v);
      }
    }

    [Property]
    public SchemaCollection Schemas { get; private set; }

    protected override Nesting CreateNesting()
    {
      return new Nesting<Database, Server, DatabaseCollection>(this, "Databases");
    }

    protected override void Initialize()
    {
      base.Initialize();
      if (Schemas==null)
        Schemas = new SchemaCollection(this);
    }


    public Database(Server parent, string name)
      : base(parent, name)
    {
    }
  }
}