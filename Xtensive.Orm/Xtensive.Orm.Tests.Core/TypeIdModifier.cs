// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.05

using System;
using Xtensive.Orm;
using Xtensive.Orm.Building;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Tests
{
  public class TypeIdModifier : IModule
  {
    public static bool IsEnabled;

    protected KeyField TypeIdField { get; private set; }

    public virtual void OnBuilt(Domain domain)
    {}

    public virtual void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
    {
      if (!IsEnabled)
        return;
      TypeIdField = new KeyField(WellKnown.TypeIdFieldName);
    }

    public static void ActivateModifier(TypeIdBehavior typeIdBehavior)
    {
      IncludeTypeIdModifier.IsEnabled = false;
      ExcludeTypeIdModifier.IsEnabled = false;
      IsEnabled = false;

      switch (typeIdBehavior) {
        case TypeIdBehavior.Include:
          IncludeTypeIdModifier.IsEnabled = true;
          break;
        case TypeIdBehavior.Exclude:
          ExcludeTypeIdModifier.IsEnabled = true;
          break;
        default:
          IsEnabled = true;
          break;
      }
    }
  }

  public class IncludeTypeIdModifier : TypeIdModifier
  {
    public new static bool IsEnabled;

    public override void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
    {
      if (!IsEnabled)
        return;
      base.OnDefinitionsBuilt(context, model);
      foreach (HierarchyDef hierarchy in model.Hierarchies)
        hierarchy.IncludeTypeId = true;
    }
  }

  public class ExcludeTypeIdModifier : TypeIdModifier
  {
    public new static bool IsEnabled;

    public override void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
    {
      if (!IsEnabled)
        return;
      base.OnDefinitionsBuilt(context, model);
      foreach (HierarchyDef hierarchy in model.Hierarchies) {
        hierarchy.IncludeTypeId = false;
        if (hierarchy.KeyFields.Contains(TypeIdField))
          hierarchy.KeyFields.Remove(TypeIdField);
      }
    }
  }
}