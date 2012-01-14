// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.05.28

using System;
using Xtensive.Orm.Building.Definitions;

namespace Xtensive.Orm.Building.FixupActions
{
  [Serializable]
  internal class ReorderFieldsAction : HierarchyAction
  {
    public TypeDef Target { get; private set; }

    public override void Run()
    {
      FixupActionProcessor.Process(this);
    }

    public override string ToString()
    {
      return string.Format("Reorder fields in '{0}' type.", Hierarchy.Root.Name);
    }


    // Constructors

    public ReorderFieldsAction(HierarchyDef hierarchy)
      : this(hierarchy.Root, hierarchy)
    {
    }

    public ReorderFieldsAction(TypeDef target, HierarchyDef hierarchy)
      : base(hierarchy)
    {
      Target = target;
    }
  }
}