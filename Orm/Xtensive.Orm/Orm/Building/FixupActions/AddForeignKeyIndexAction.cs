// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.05.29

using Xtensive.Orm.Building.Definitions;

namespace Xtensive.Orm.Building.FixupActions
{
  internal class AddForeignKeyIndexAction : FieldAction
  {
    public override void Run(FixupActionProcessor processor)
    {
      processor.Process(this);
    }

    public override string ToString()
    {
      return $"Add secondary index to '{Type.Name}.{Field.Name}'";
    }


    //  Constructors

    public AddForeignKeyIndexAction(TypeDef type, FieldDef field)
      : base(type, field)
    {
    }
  }
}