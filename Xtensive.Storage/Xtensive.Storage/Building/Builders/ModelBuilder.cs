// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.26

using System;
using System.Linq;
using Xtensive.Core;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Building.Builders
{
  internal static class ModelBuilder
  {
    public static void Build()
    {
      using (Log.InfoRegion(String.Format(Strings.LogBuildingX, Strings.ModelDefinition)))
        BuildDefinition();
      using (Log.InfoRegion(String.Format(Strings.LogBuildingX, Strings.ActualModel)))
        BuildModel();
    }

    private static void BuildDefinition()
    {
      BuildingContext context = BuildingContext.Current;
      try {
        context.Definition = new DomainDef();
        using (Log.InfoRegion(String.Format(Strings.LogBuildingX, Strings.Types)))
          DefineTypes();
        using (Log.InfoRegion(String.Format(Strings.LogBuildingX, Strings.Services)))
          DefineServices();
      }
      catch (DomainBuilderException e) {
        context.RegisterError(e);
      }
      context.EnsureBuildSucceed();

      if (context.Configuration.Builders.Count == 0)
        return;
      
      using (Log.InfoRegion(String.Format(Strings.LogBuildingX, Strings.CustomDefinitions)))
        foreach (Type type in BuildingContext.Current.Configuration.Builders) {
          IDomainBuilder builder = (IDomainBuilder)Activator.CreateInstance(type);
          builder.Build(context, context.Definition);
        }
    }

    private static void BuildModel()
    {
      BuildingContext context = BuildingContext.Current;
      context.Model = new DomainInfo();
      BuildTypes();
      context.Model.Lock(true);
    }

    private static void DefineTypes()
    {
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

    private static void DefineServices()
    {
    }

    private static void BuildTypes()
    {
      BuildingContext context = BuildingContext.Current;

      // Types
      foreach (TypeDef typeDef in context.Definition.Types.Where(t => !t.IsInterface))
        try {
          TypeBuilder.BuildType(typeDef);
        }
        catch (DomainBuilderException e) {
          context.RegisterError(e);
        }

      // Associations
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

      // Columns
      foreach (TypeInfo type in context.Model.Types) {
        type.Columns.Clear();
        type.Columns.AddRange(type.Fields.Where(f => f.Column!=null).Select(f => f.Column));
      }

      // Indexes
      IndexBuilder.BuildIndexes();
      IndexBuilder.BuildAffectedIndexes();

      // Hirarchy columns
      foreach (HierarchyInfo hierarchyInfo in context.Model.Hierarchies)
        HierarchyBuilder.BuildHierarchyColumns(hierarchyInfo);
    }
  }
}