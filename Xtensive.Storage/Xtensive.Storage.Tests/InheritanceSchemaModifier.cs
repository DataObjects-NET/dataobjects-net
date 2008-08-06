// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.08.05

using System;
using Xtensive.Storage.Building;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests
{
  public abstract class InheritanceSchemaModifier : IDomainBuilder
  {
    protected InheritanceSchema Schema { get; private set; }

    public void Build(BuildingContext context, DomainModelDef model)
    {
      foreach (HierarchyDef hierarchy in model.Hierarchies)
        hierarchy.Schema = Schema;
    }

    public static Type GetModifier(InheritanceSchema schema)
    {
      switch (schema) {
        case InheritanceSchema.ConcreteTable:
          return typeof(ConcreteTableSchemaModifier);
        case InheritanceSchema.SingleTable:
          return typeof(SingleTableSchemaModifier);
        default:
          return typeof(ClassTableSchemaModifier);
      }
    }


    // Constructor

    protected InheritanceSchemaModifier(InheritanceSchema schema)
    {
      Schema = schema;
    }
  }

  public class ClassTableSchemaModifier : InheritanceSchemaModifier
  {
    public ClassTableSchemaModifier()
      : base(InheritanceSchema.ClassTable)
    {
    }
  }

  public class SingleTableSchemaModifier : InheritanceSchemaModifier
  {
    public SingleTableSchemaModifier()
      : base(InheritanceSchema.SingleTable)
    {
    }
  }

  public class ConcreteTableSchemaModifier : InheritanceSchemaModifier
  {
    public ConcreteTableSchemaModifier()
      : base(InheritanceSchema.ConcreteTable)
    {
    }
  }

}