// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.20

using System;
using System.Diagnostics;

namespace Xtensive.Modelling.Tests.DatabaseModel
{
  [Serializable]
  public class RoleRef : Ref<Role, User>
  {
    protected override Nesting CreateNesting()
    {
      return new Nesting<RoleRef, User, RoleRefCollection>(this, "Roles");
    }

    public RoleRef(User parent, Role role)
      : base(parent, role.Name)
    {
      Value = role;
    }
  }
}