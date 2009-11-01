// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.26

using System;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Diagnostics;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Model;

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
      using (Log.InfoRegion("Building storage definition")) {
        BuildingContext buildingContext = BuildingScope.Context;
        buildingContext.Definition = new DomainDef();
        DefineTypes();
        DefineServices();

        if (buildingContext.Configuration.Builders.Count == 0)
          return;

        Log.Info("Entering custom storage definition.");
        foreach (Type type in BuildingScope.Context.Configuration.Builders) {
          IDomainBuilder builder = (IDomainBuilder)Activator.CreateInstance(type);
          builder.Build(buildingContext, buildingContext.Definition);
        }
      }
    }

    private static void BuildModel()
    {
      BuildingContext buildingContext = BuildingScope.Context;
      using (Log.InfoRegion("Building storage model")) {
        buildingContext.Model = new DomainInfo();
        BuildTypes();
        buildingContext.Model.Lock(true);
      }
    }

    private static void DefineTypes()
    {
      BuildingContext buildingContext = BuildingScope.Context;
      using (Log.InfoRegion("Defining types")) {
        using (var scope = new LogCaptureScope(buildingContext.Logger)) {
          foreach (Type type in buildingContext.Configuration.Types)
            using (LogCaptureScope typeScope = new LogCaptureScope(buildingContext.Logger)) {
              if (buildingContext.Definition.Types.Contains(type)) {
                Log.Error("Type '{0}' is already defined.", type.FullName);
                continue;
              }

              TypeDef typeDef = TypeBuilder.DefineType(type);

              if (buildingContext.Definition.Types.Contains(typeDef.Name)) {
                Log.Error("Type with name '{0}' is already defined.",
                          typeDef.Name);
                continue;
              }
              if (!typeScope.IsCaptured(LogEventTypes.Error)) {
                buildingContext.Definition.Types.Add(typeDef);
                IndexBuilder.DefineIndexes(typeDef);
              }
            }

          if (scope.IsCaptured(LogEventTypes.Error))
            throw new DomainBuilderException(
              "Some errors have been occurred during types definition process. See error log for details.");
        }
      }
    }

    private static void DefineServices()
    {
    }

    private static void BuildTypes()
    {
      BuildingContext buildingContext = BuildingScope.Context;
      using (Log.InfoRegion("Building types")) {
        using (var scope = new LogCaptureScope(buildingContext.Logger)) {
          foreach (TypeDef typeDef in buildingContext.Definition.Types.Where(t => !t.IsInterface))
            TypeBuilder.BuildType(typeDef);
          foreach (FieldInfo field in buildingContext.ComplexFields)
            FieldBuilder.BuildComplexField(field);
          foreach (Pair<AssociationInfo, string> pair in buildingContext.PairedAssociations)
            AssociationBuilder.BuildPairedAssociation(pair.First, pair.Second);
          foreach (TypeInfo type in buildingContext.Model.Types) {
            type.Columns.Clear();
            type.Columns.AddRange(type.Fields.Where(f => f.Column != null).Select(f => f.Column));
          }
          IndexBuilder.BuildIndexes();
          IndexBuilder.BuildAffectedIndexes();
          foreach (HierarchyInfo hierarchyInfo in buildingContext.Model.Hierarchies)
            HierarchyBuilder.BuildHierarchyColumns(hierarchyInfo);

          if (scope.IsCaptured(LogEventTypes.Error))
            throw new DomainBuilderException(
              "Some errors have been occurred during type building process. See error log for details.");
        }
      }
    }
  }
}