// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.20

using System;
using System.Diagnostics;
using Xtensive.Modelling;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Core.Tests.Modelling.DatabaseModel
{
  [Serializable]
  public sealed class Security : NodeBase<Server>
  {
    [Property(Priority = 100)]
    public RoleCollection Roles { get; private set; }

    [Property(Priority = 200)]
    public UserCollection Users { get; private set; }

    protected override Nesting CreateNesting()
    {
      return new Nesting<Security, Server, Security>(this, "Security");
    }

    protected override void Initialize()
    {
      base.Initialize();
      if (Users==null)
        Users = new UserCollection(this);
      if (Roles==null)
        Roles = new RoleCollection(this);
    }


    public Security(Server parent, string name)
      : base(parent, name)
    {
    }
  }
}