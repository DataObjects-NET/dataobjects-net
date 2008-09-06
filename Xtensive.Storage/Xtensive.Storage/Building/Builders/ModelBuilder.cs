// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.26

using System;
using System.Linq;
using Xtensive.Core;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Building.Builders
{
  internal static class ModelBuilder
  {
    public static void Build()
    {
      BuildDefinition();
      BuildModel();
    }

    private static void BuildDefinition()
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.ModelDefinition)) {
        BuildingContext context = BuildingContext.Current;
        try {
          context.Definition = new DomainModelDef();

          DefineTypes();
          DefineServices();
        }
        catch (DomainBuilderException e) {
          context.RegisterError(e);
        }
        context.EnsureBuildSucceed();

        if (context.Configuration.Builders.Count==0)
          return;

        BuildCustomDefinitions();
      }
    }

    private static void BuildCustomDefinitions()
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.CustomDefinitions)) {
        BuildingContext context = BuildingContext.Current;
        foreach (Type type in BuildingContext.Current.Configuration.Builders) {
          IDomainBuilder builder = (IDomainBuilder) Activator.CreateInstance(type);
          builder.Build(context, context.Definition);
        }
      }
    }

    private static void BuildModel()
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.ActualModel)) {
        BuildingContext context = BuildingContext.Current;
        context.Model = new DomainModel();
        BuildTypes();
        context.Model.Lock(true);
      }
    }

    private static void DefineTypes()
    {
      using (Log.InfoRegion(Strings.LogDefiningX, Strings.Types)) {
        BuildingContext context = BuildingContext.Current;
        foreach (Type type in context.Configuration.Types) {
          try {
            TypeDef typeDef = TypeBuilder.DefineType(type);
            context.Definition.Types.Add(typeDef);
            IndexBuilder.DefineIndexes(typeDef);
          }
          catch (DomainBuilderException e) {
            context.RegisterError(e);
          }
        }
      }
    }

    private static void DefineServices()
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.Services)) {

      }
    }

    private static void BuildTypes()
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.Types)) {
        BuildingContext context = BuildingContext.Current;

        foreach (TypeDef typeDef in context.Definition.Types.Where(t => !t.IsInterface))
          try {
            TypeBuilder.BuildType(typeDef);
          }
          catch (DomainBuilderException e) {
            context.RegisterError(e);
          }

        BuildAssociations();
        BuildColumns();
        IndexBuilder.BuildIndexes();
        BuildHierarchyColumns();
      }
    }

    private static void BuildHierarchyColumns()
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.HierarchyColumns)) {
        foreach (HierarchyInfo hierarchyInfo in BuildingContext.Current.Model.Hierarchies)
          HierarchyBuilder.BuildHierarchyColumns(hierarchyInfo);
      }
    }

    private static void BuildColumns()
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.Columns)) {
        foreach (TypeInfo type in BuildingContext.Current.Model.Types) {
          type.Columns.Clear();
          type.Columns.AddRange(type.Fields.Where(f => f.Column!=null).Select(f => f.Column));
        }
      }
    }

    private static void BuildAssociations()
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.Associations)) {
        BuildingContext context = BuildingContext.Current;
        foreach (Pair<AssociationInfo, string> pair in context.PairedAssociations) {
          if (context.DiscardedAssociations.Contains(pair.First))
            continue;
          try {
            AssociationBuilder.BuildPairedAssociation(pair.First, pair.Second);
          }
          catch (DomainBuilderException e) {
            context.RegisterError(e);
          }
        }

        foreach (AssociationInfo ai in context.DiscardedAssociations)
          context.Model.Associations.Remove(ai);
        context.DiscardedAssociations.Clear();

        foreach (AssociationInfo association in context.Model.Associations) {
          if (association.EntityType!=null) {

            TypeDef typeDef = TypeBuilder.DefineType(association.EntityType);
            FieldDef idFieldDef = new FieldDef(typeof(int));
            idFieldDef.Name = "Id";
            typeDef.Fields.Add(idFieldDef);
            FieldDef leftFieldDef = new FieldDef(association.ReferencingType.UnderlyingType);
            leftFieldDef.Name = "Left";
            typeDef.Fields.Add(leftFieldDef);
            FieldDef rightFieldDef = new FieldDef(association.ReferencedType.UnderlyingType);
            rightFieldDef.Name = "Right";
            typeDef.Fields.Add(rightFieldDef);
            context.Definition.Types.Add(typeDef); 
            TypeBuilder.BuildType(typeDef);
          }
        }
      }
    }
  }
}