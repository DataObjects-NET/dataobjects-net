// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.06.12

using System;
using Xtensive.Orm.Building.Definitions;

namespace Xtensive.Orm.Building.FixupActions
{
  [Serializable]
  internal class AddPrimaryIndexAction : TypeAction
  {
    public override void Run()
    {
      FixupActionProcessor.Process(this);
    }

    public override string ToString()
    {
      return string.Format("Add primary index to '{0}'", Type.Name);
    }

    // Constructors

    public AddPrimaryIndexAction(TypeDef typeDef)
      : base(typeDef)
    {
    }
  }
}