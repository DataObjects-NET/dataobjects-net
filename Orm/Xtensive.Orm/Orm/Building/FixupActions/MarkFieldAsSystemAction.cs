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
  internal class MarkFieldAsSystemAction : FieldAction
  {
    public override void Run()
    {
      FixupActionProcessor.Process(this);
    }

    public override string ToString()
    {
      return string.Format("Mark '{0}.{1}' field as system.", Type.Name, Field.Name);
    }


    // Constructors

    public MarkFieldAsSystemAction(TypeDef typeDef, FieldDef fieldDef)
      : base(typeDef, fieldDef)
    {
    }
  }
}