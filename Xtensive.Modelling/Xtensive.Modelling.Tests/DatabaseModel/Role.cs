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
  public class Role : NodeBase<Security>
  {
    protected override Nesting CreateNesting()
    {
      return new Nesting<Role, Security, RoleCollection>(this, "Roles");
    }


    public Role(Security parent, string name)
      : base(parent, name)
    {
    }

    public Role(Security parent, string name, int index)
      : base(parent, name, index)
    {
    }
  }
}