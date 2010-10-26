// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.06.02

using System;
using Xtensive.Orm.Building.Definitions;

namespace Xtensive.Orm.Building.FixupActions
{
  [Serializable]
  internal class AddTypeIdFieldAction : TypeAction
  {
    public override void Run()
    {
      FixupActionProcessor.Process(this);
    }

    public override string ToString()
    {
      return string.Format("Add TypeId field to '{0}' type.", Type.Name);
    }


    // Constructors

    public AddTypeIdFieldAction(TypeDef type)
      : base(type)
    {
    }
  }
}