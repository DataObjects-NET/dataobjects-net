// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.05

using System;
using Xtensive.Storage.Building;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Tests
{
  public class TypeIdModifier : IDomainBuilder
  {
    protected KeyField TypeIdField { get; private set; }

    public virtual void Build(BuildingContext context, DomainModelDef model)
    {
      TypeIdField = new KeyField(WellKnown.TypeIdFieldName);
    }

    public static Type GetModifier(TypeIdBehavior typeIdBehavior)
    {
      switch (typeIdBehavior) {
        case TypeIdBehavior.Include:
          return typeof (IncludeTypeIdModifier);
        case TypeIdBehavior.Exclude:
          return typeof (ExcludeTypeIdModifier);
        default:
          return typeof (TypeIdModifier);
      }
    }
  }

  public class IncludeTypeIdModifier : TypeIdModifier
  {
    public override void Build(BuildingContext context, DomainModelDef model)
    {
      base.Build(context, model);
      foreach (HierarchyDef hierarchy in model.Hierarchies)
        hierarchy.IncludeTypeId = true;
    }
  }

  public class ExcludeTypeIdModifier : TypeIdModifier
  {
    public override void Build(BuildingContext context, DomainModelDef model)
    {
      base.Build(context, model);
      foreach (HierarchyDef hierarchy in model.Hierarchies) {
        hierarchy.IncludeTypeId = false;
        if (hierarchy.KeyFields.Contains(TypeIdField))
          hierarchy.KeyFields.Remove(TypeIdField);
      }
    }
  }
}