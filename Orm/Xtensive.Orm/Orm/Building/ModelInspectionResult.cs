// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.05.28

using System.Collections.Generic;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Building.FixupActions;

namespace Xtensive.Orm.Building
{
  internal class ModelInspectionResult
  {
    public HashSet<HierarchyDef> GeneratedHierarchies { get; private set; }

    public HashSet<TypeDef> GeneratedTypes { get; private set; }

    public Queue<FixupAction> Actions { get; private set; }

    public List<TypeDef> SingleHierarchyInterfaces { get; private set; }

    public HashSet<TypeDef> RemovedTypes { get; private set; }

    public bool HasActions
    {
      get { return Actions.Count > 0; }
    }

    public void Register(FixupAction action)
    {
      Actions.Enqueue(action);
      var ra = action as RemoveTypeAction;
      if (ra != null)
        RemovedTypes.Add(ra.Type);
    }


    // Constructor

    public ModelInspectionResult()
    {
      Actions = new Queue<FixupAction>();
      SingleHierarchyInterfaces = new List<TypeDef>();
      RemovedTypes = new HashSet<TypeDef>();
      GeneratedHierarchies = new HashSet<HierarchyDef>();
      GeneratedTypes = new HashSet<TypeDef>();
    }
  }
}