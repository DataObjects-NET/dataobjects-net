// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.20

using System;
using System.Diagnostics;
using Xtensive.Modelling.Attributes;

namespace Xtensive.Modelling.Tests.DatabaseModel
{
  [Serializable]
  public class Security : NodeBase<Server, Server>
  {
    [Property]
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
    }


    public Security(Server parent, string name)
      : base(parent, name)
    {
    }

    public Security(Server parent, string name, int index)
      : base(parent, name, index)
    {
    }
  }
}