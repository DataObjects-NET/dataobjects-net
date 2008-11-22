// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.05

using System;
using Xtensive.Storage.Building;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Model;
using Xtensive.Storage.Providers;

namespace Xtensive.Storage.Tests
{
  public class TypeIdModifier : IDomainBuilder
  {
    protected KeyField TypeIdField { get; private set; }

    public virtual void Build(BuildingContext context, DomainModelDef model)
    {
      TypeIdField = new KeyField(context.NameBuilder.TypeIdFieldName, typeof(int));
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
        if (!hierarchy.KeyFields.ContainsKey(TypeIdField))
          hierarchy.KeyFields.Add(TypeIdField);
    }
  }

  public class ExcludeTypeIdModifier : TypeIdModifier
  {
    public override void Build(BuildingContext context, DomainModelDef model)
    {
      base.Build(context, model);
      foreach (HierarchyDef hierarchy in model.Hierarchies)
        if (hierarchy.KeyFields.ContainsKey(TypeIdField))
          hierarchy.KeyFields.Remove(TypeIdField);
    }
  }
}