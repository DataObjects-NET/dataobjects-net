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
  public abstract class InheritanceSchemaModifier : IModule
  {
    protected InheritanceSchema Schema { get; private set; }

    public virtual void OnBuilt(Domain domain)
    {}

    public virtual void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
    {
      foreach (HierarchyDef hierarchy in model.Hierarchies)
        hierarchy.Schema = Schema;
    }

    public static void ActivateModifier(InheritanceSchema schema)
    {
      ConcreteTableSchemaModifier.IsEnabled = false;
      SingleTableSchemaModifier.IsEnabled = false;
      ClassTableSchemaModifier.IsEnabled = false;

      switch (schema) {
        case InheritanceSchema.ConcreteTable:
          ConcreteTableSchemaModifier.IsEnabled = true;
          break;
        case InheritanceSchema.SingleTable:
          SingleTableSchemaModifier.IsEnabled = true;
          break;
        default:
          ClassTableSchemaModifier.IsEnabled = true;
          break;
      }
    }


    // Constructors

    protected InheritanceSchemaModifier(InheritanceSchema schema)
    {
      Schema = schema;
    }
  }

  public class ClassTableSchemaModifier : InheritanceSchemaModifier
  {
    public static bool IsEnabled;

    public override void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
    {
      if (!IsEnabled)
        return;
      base.OnDefinitionsBuilt(context, model);
    }

    public ClassTableSchemaModifier()
      : base(InheritanceSchema.ClassTable)
    {
    }
  }

  public class SingleTableSchemaModifier : InheritanceSchemaModifier
  {
    public static bool IsEnabled;

    public override void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
    {
      if (!IsEnabled)
        return;
      base.OnDefinitionsBuilt(context, model);
    }

    public SingleTableSchemaModifier()
      : base(InheritanceSchema.SingleTable)
    {
    }
  }

  public class ConcreteTableSchemaModifier : InheritanceSchemaModifier
  {
    public static bool IsEnabled;

    public override void OnDefinitionsBuilt(BuildingContext context, DomainModelDef model)
    {
      if (!IsEnabled)
        return;
      base.OnDefinitionsBuilt(context, model);
    }

    public ConcreteTableSchemaModifier()
      : base(InheritanceSchema.ConcreteTable)
    {
    }
  }
}