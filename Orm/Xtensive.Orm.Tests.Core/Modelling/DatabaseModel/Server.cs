// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.18

using System;
using Xtensive.Modelling;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Orm.Tests.Core.Modelling.DatabaseModel
{
  [Serializable]
  public sealed class Server : NodeBase<Server>, 
    IModel
  {
    private ActionSequence actions;
    private Security security;

    public ActionSequence Actions {
      get { return actions; }
      set {
        this.EnsureIsEditable();
        actions = value;
      }
    }

    [Property]
    public DatabaseCollection Databases { get; private set; }

    [Property(Priority = -1000)]
    public Security Security {
      get { return security; }
      set {
        ChangeProperty("Security", value, (x,v) => ((Server)x).security = v);
      }
    }

    protected override Nesting CreateNesting()
    {
      return new Nesting<Server, Server, Server>(this);
    }

    protected override void Initialize()
    {
      base.Initialize();
      if (Databases==null)
        Databases = new DatabaseCollection(this);
    }


    public Server(string name)
      : base(null, name)
    {
    }
  }
}