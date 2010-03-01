// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.05.28

using System;
using Xtensive.Storage.Building.Definitions;

namespace Xtensive.Storage.Building.FixupActions
{
  [Serializable]
  internal abstract class TypeAction : FixupAction
  {
    public TypeDef Type { get; private set; }

    protected TypeAction(TypeDef type)
    {
      Type = type;
    }
  }
}