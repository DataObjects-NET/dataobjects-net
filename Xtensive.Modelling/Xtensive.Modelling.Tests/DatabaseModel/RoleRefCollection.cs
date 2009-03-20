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
  public class RoleRefCollection : NodeCollectionBase<RoleRef, User>,
    IUnorederedNodeCollection
  {
    public RoleRefCollection(Node parent, string name)
      : base(parent, name)
    {
    }
  }
}