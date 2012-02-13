// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.09.08

using System;
using Xtensive.Orm.Building.Definitions;

namespace Xtensive.Orm.Building.FixupActions
{
  internal class MakeTypeNonAbstractAction : TypeAction
  {
    public override void Run(FixupActionProcessor processor)
    {
      processor.Process(this);
    }

    public override string ToString()
    {
      return string.Format("Make type '{0}' non-abstract.", Type.Name);
    }

    public MakeTypeNonAbstractAction(TypeDef type)
      : base(type)
    {
    }
  }
}