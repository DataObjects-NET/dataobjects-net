// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.06.12

using System;
using Xtensive.Storage.Building.Definitions;

namespace Xtensive.Storage.Building.FixupActions
{
  [Serializable]
  internal class MarkFieldAsNotNullableAction : FieldAction
  {
    public override void Run()
    {
      FixupActionProcessor.Process(this);
    }

    public override string ToString()
    {
      return string.Format("Mark '{0}' field as not nullable.", Field.Name);
    }

    // Constructors

    public MarkFieldAsNotNullableAction(TypeDef type, FieldDef field)
      : base(type, field)
    {
    }
  }
}