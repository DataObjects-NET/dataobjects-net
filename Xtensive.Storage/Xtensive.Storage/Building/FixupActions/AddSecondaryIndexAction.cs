// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.05.29

using System;
using Xtensive.Storage.Building.Definitions;

namespace Xtensive.Storage.Building.FixupActions
{
  [Serializable]
  internal class AddSecondaryIndexAction : FieldAction
  {
    public override void Run()
    {
      FixupActionProcessor.Process(this);
    }

    public override string ToString()
    {
      return string.Format("Add secondary index to '{0}.{1}'", Type.Name, Field.Name);
    }


    //  Constructors

    public AddSecondaryIndexAction(TypeDef type, FieldDef field)
      : base(type, field)
    {
    }
  }
}