// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.05.28

using Xtensive.Orm.Building.Definitions;

namespace Xtensive.Orm.Building.FixupActions
{
  internal class ReorderFieldsAction : HierarchyAction
  {
    public TypeDef Target { get; private set; }

    public override void Run(FixupActionProcessor processor)
    {
      processor.Process(this);
    }

    public override string ToString()
    {
      return $"Reorder fields in '{Hierarchy.Root.Name}' type.";
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