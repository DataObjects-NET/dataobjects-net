// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.06.02

using Xtensive.Orm.Building.Definitions;

namespace Xtensive.Orm.Building.FixupActions
{
  internal abstract class FieldAction : TypeAction
  {
    public FieldDef Field { get; private set; }

    // Constructors

    public FieldAction(TypeDef type, FieldDef field)
      : base(type)
    {
      Field = field;
    }
  }
}