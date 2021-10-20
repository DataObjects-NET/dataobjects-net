// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.06.02

using Xtensive.Orm.Building.Definitions;

namespace Xtensive.Orm.Building.FixupActions
{
  internal class MarkFieldAsSystemAction : FieldAction
  {
    public override void Run(FixupActionProcessor processor)
    {
      processor.Process(this);
    }

    public override string ToString()
    {
      return $"Mark '{Type.Name}.{Field.Name}' field as system.";
    }


    // Constructors

    public MarkFieldAsSystemAction(TypeDef typeDef, FieldDef fieldDef)
      : base(typeDef, fieldDef)
    {
    }
  }
}