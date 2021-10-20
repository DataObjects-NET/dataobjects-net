// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.09.14

using Xtensive.Orm.Building.Definitions;

namespace Xtensive.Orm.Building.FixupActions
{
  internal class CopyKeyFieldsAction : TypeAction
  {
    public HierarchyDef Source { get; private set; }

    public override void Run(FixupActionProcessor processor)
    {
      processor.Process(this);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      return $"Copy key fields from '{Source.Name}' hierarchy root to '{Type.Name}' type.";
    }


    // Constructors

    public CopyKeyFieldsAction(TypeDef target, HierarchyDef source)
      : base(target)
    {
      Source = source;
    }
  }
}