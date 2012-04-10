// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.09.14

using System;
using Xtensive.Orm.Building.Definitions;

namespace Xtensive.Orm.Building.FixupActions
{
  [Serializable]
  internal class CopyKeyFieldsAction : TypeAction
  {
    public HierarchyDef Source { get; private set; }

    public override void Run()
    {
      FixupActionProcessor.Process(this);
    }

    
    public override string ToString()
    {
      return string.Format("Copy key fields from '{0}' hierarchy root to '{1}' type.", Source.Name, Type.Name);
    }


    // Constructors

    public CopyKeyFieldsAction(TypeDef target, HierarchyDef source)
      : base(target)
    {
      Source = source;
    }
  }
}