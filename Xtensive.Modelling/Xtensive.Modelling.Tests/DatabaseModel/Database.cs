// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.18

using System;
using System.Diagnostics;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Modelling.Tests.DatabaseModel
{
  [Serializable]
  public class Database : NodeBase<Server, Server>
  {
    private User owner;

    /// <exception cref="ArgumentOutOfRangeException"><c>value.Model</c> is out of range.</exception>
    [Property]
    public User Owner
    {
      get { return owner; }
      set {
        if (value!=null && value.Model!=this.Model)
          throw new ArgumentOutOfRangeException("value.Model");
        if (owner!=null)
          owner.UsageCount--;
        owner = value;
        if (owner!=null)
          owner.UsageCount++;
      }
    }

    [Property]
    public SchemaCollection Schemas { get; private set; }

    protected override void ValidateState()
    {
      base.ValidateState();
    }

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

    public Database(Server parent, string name, int index)
      : base(parent, name, index)
    {
    }
  }
}