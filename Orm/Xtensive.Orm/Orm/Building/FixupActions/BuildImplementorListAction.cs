// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.09.14

using Xtensive.Orm.Building.Definitions;

namespace Xtensive.Orm.Building.FixupActions
{
  internal class BuildImplementorListAction : TypeAction
  {
    public override void Run(FixupActionProcessor processor)
    {
      processor.Process(this);
    }

    public override string ToString()
    {
      return $"Build implementor list for '{Type.Name}' interface";
    }

    public BuildImplementorListAction(TypeDef type)
      : base(type)
    {
    }
  }
}