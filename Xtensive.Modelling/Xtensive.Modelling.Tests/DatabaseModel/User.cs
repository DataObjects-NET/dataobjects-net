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
  public class User : NodeBase<Security>
  {
    private string password;

    [Property]
    public string Password
    {
      get { return password; }
      set {
        ChangeProperty("Password", value, (x,v) => ((User)x).password = v);
      }
    }

    [Property]
    public RoleRefCollection Roles { get; private set; }

    protected override Nesting CreateNesting()
    {
      return new Nesting<User, Security, UserCollection>(this, "Users");
    }

    protected override void Initialize()
    {
      base.Initialize();
      if (Roles==null)
        Roles = new RoleRefCollection(this, "Roles");
    }


    public User(Security parent, string name)
      : base(parent, name)
    {
    }

    public User(Security parent, string name, int index)
      : base(parent, name, index)
    {
    }
  }
}