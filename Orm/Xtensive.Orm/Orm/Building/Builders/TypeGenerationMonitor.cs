// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.13

using Xtensive.Orm.Building.Definitions;

namespace Xtensive.Orm.Building.Builders
{
  internal sealed class TypeGenerationMonitor
  {
    private readonly BuildingContext context;

    public void Attach()
    {
      context.ModelDef.Hierarchies.Added += OnHierarchyAdded;
      context.ModelDef.Types.Added += OnTypeAdded;
    }

    public void Detach()
    {
      context.ModelDef.Hierarchies.Added -= OnHierarchyAdded;
      context.ModelDef.Types.Added -= OnTypeAdded;
    }

    private void OnHierarchyAdded(object sender, HierarchyDefCollectionChangedEventArgs e)
    {
      context.ModelInspectionResult.GeneratedHierarchies.Add(e.Item);
    }

    private void OnTypeAdded(object sender, TypeDefCollectionChangedEventArgs e)
    {
      context.ModelInspectionResult.GeneratedTypes.Add(e.Item);
    }

    // Constructors

    public TypeGenerationMonitor(BuildingContext context)
    {
      this.context = context;
    }
  }
}