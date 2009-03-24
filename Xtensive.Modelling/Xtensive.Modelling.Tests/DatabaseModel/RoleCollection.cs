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
  public class RoleCollection : NodeCollectionBase<Role, Security>, 
    IUnorderedNodeCollection
  {
    internal RoleCollection(Security parent)
      : base(parent, "Roles")
    {
    }
  }
}