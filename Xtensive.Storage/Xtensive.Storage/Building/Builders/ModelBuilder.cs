// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.26

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Notifications;
using Xtensive.Core.Sorting;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Building.DependencyGraph;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;
using Activator=System.Activator;

namespace Xtensive.Storage.Building.Builders
{
  internal static class ModelBuilder
  {
    public static void Build()
    {
      BuildingContext context = BuildingContext.Current;
      
      // Initial model
      context.ModelDef = new DomainModelDef();
      ModelDefBuilder.Run();

      // Custom model updates
      BuildCustomDefinitions();

      // Inspecting model
      ModelInspector.Run();

      // Applying fixup actions
      if (context.ModelInspectionResult.HasActions) {

        // Add handlers for hierarchies and types that could be created as a result of FixupProcessor work
        context.ModelDef.Hierarchies.Inserted += OnHierarchyAdded;
        context.ModelDef.Types.Inserted += OnTypeAdded;

        // Applying fixup actions to the model.
        FixupActionProcessor.Run();
      }

      BuildModel();
    }

    private static void OnHierarchyAdded(object sender, CollectionChangeNotifierEventArgs<HierarchyDef> e)
    {
      ModelInspector.Inspect(e.Item);
      FixupActionProcessor.Run();
    }

    private static void OnTypeAdded(object sender, CollectionChangeNotifierEventArgs<TypeDef> e)
    {
      ModelInspector.Inspect(e.Item);
      FixupActionProcessor.Run();
    }

    private static void BuildCustomDefinitions()
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.CustomDefinitions)) {
        var context = BuildingContext.Current;
        foreach (var type in BuildingContext.Current.Configuration.Builders) {
          var builder = (IDomainBuilder) Activator.CreateInstance(type);
          builder.Build(context, context.ModelDef);
        }
      }
    }

    public static void BuildModel()
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.ActualModel)) {
        var context = BuildingContext.Current;

        List<Node<TypeDef, object>> loops;
        List<TypeDef> sequence = TopologicalSorter.Sort<TypeDef>(context.ModelDef.Types, TypeConnector, out loops);
        if (sequence==null)
          throw new DomainBuilderException(string.Format("At least one loop have been found in persistent type dependencies graph. Suspicious types: {0}", loops.Select(node => node.Item.Name).ToCommaDelimitedString()));
        context.Model = new DomainModel();
        BuildTypes(sequence);
        context.ModelUnlockKey = context.Model.GetUnlockKey();
        BuildAssociations();
        BuildColumns();
        IndexBuilder.BuildIndexes();
        BuildHierarchyColumns();
        context.Model.Lock(true);
      }
    }

    private static void BuildTypes(List<TypeDef> types)
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.Types)) {
        foreach (var typeDef in types)
          TypeBuilder.BuildType(typeDef);
      }
    }

    private static bool TypeConnector(TypeDef first, TypeDef second)
    {
      foreach (var info in second.OutgoingEdges)
        if (info.Weight == EdgeWeight.High && info.Head==first)
          return true;
      return false;
    }

    private static void BuildHierarchyColumns()
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.HierarchyColumns)) {
        foreach (var hierarchyInfo in BuildingContext.Current.Model.Hierarchies)
          HierarchyBuilder.BuildHierarchyColumns(hierarchyInfo);
      }
    }

    private static void BuildColumns()
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.Columns)) {
        foreach (var type in BuildingContext.Current.Model.Types) {
          type.Columns.Clear();
          type.Columns.AddRange(type.Fields.Where(f => f.Column!=null).Select(f => f.Column));
        }
      }
    }

    private static void BuildAssociations()
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.Associations)) {
        var context = BuildingContext.Current;
        foreach (var pair in context.PairedAssociations) {
          if (context.DiscardedAssociations.Contains(pair.First))
            continue;
          AssociationBuilder.BuildPairedAssociation(pair.First, pair.Second);
        }

        foreach (var ai in context.DiscardedAssociations)
          context.Model.Associations.Remove(ai);
        context.DiscardedAssociations.Clear();

        BuildEntitySetTypes(context.Model.Associations);
      }
    }

    private static void BuildEntitySetTypes(IEnumerable<AssociationInfo> associations)
    {
      var context = BuildingContext.Current;
      foreach (var association in associations) {
        if (!association.IsMaster)
          continue;

        var multiplicity = association.Multiplicity;
        if (!(multiplicity == Multiplicity.ZeroToMany || multiplicity == Multiplicity.ManyToMany))
          continue;

        var masterType = association.ReferencedType;
        var slaveType = association.ReferencingType;

        var genericDefinitionType = typeof (EntitySetItem<,>);
        var genericInstanceType = genericDefinitionType.MakeGenericType(masterType.UnderlyingType, slaveType.UnderlyingType);
        var underlyingType = TypeHelper.CreateDummyType(context.NameBuilder.Build(association), genericInstanceType, true);

        // Defining auxiliary type
        var underlyingTypeDef = ModelDefBuilder.DefineType(underlyingType);
        underlyingTypeDef.Name = association.Name;

        // HierarchyRootAttribute is not inherited so we must take it from the generic type definition or generic instance type
        var hra = genericInstanceType.GetAttribute<HierarchyRootAttribute>(AttributeSearchOptions.Default);
        // Defining the hierarchy
        var hierarchy = ModelDefBuilder.DefineHierarchy(underlyingTypeDef, hra);

        // Processing type properties
        ModelDefBuilder.ProcessProperties(underlyingTypeDef, hierarchy);

        // Getting fields
        var masterFieldDef = underlyingTypeDef.Fields[context.NameBuilder.EntitySetItemMasterFieldName];
        var slaveFieldDef = underlyingTypeDef.Fields[context.NameBuilder.EntitySetItemSlaveFieldName];

        // Updating fields names only if types differ.
        if (masterType!=slaveType) {
          masterFieldDef.MappingName = context.NameBuilder.NamingConvention.Apply(masterType.Name);
          slaveFieldDef.MappingName = context.NameBuilder.NamingConvention.Apply(slaveType.Name);
        }
        context.ModelDef.Hierarchies.Add(hierarchy);
        context.ModelDef.Types.Add(underlyingTypeDef);

        TypeBuilder.BuildType(underlyingTypeDef);
        association.UnderlyingType = context.Model.Types[underlyingType];
      }
    }
  }
}