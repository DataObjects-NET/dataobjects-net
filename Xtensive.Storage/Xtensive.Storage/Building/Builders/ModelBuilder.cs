// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.26

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core.Collections;
using Xtensive.Core.Diagnostics;
using Xtensive.Core.Notifications;
using Xtensive.Core.Reflection;
using Xtensive.Core.Sorting;
using Xtensive.Core.Threading;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Building.DependencyGraph;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Building.Builders
{
  internal static class ModelBuilder
  {
    private const string GeneratedTypeNameFormat = "{0}.EntitySetItems.{1}";

    private static ThreadSafeDictionary<string, Type> generatedTypes
      = ThreadSafeDictionary<string, Type>.Create(new object());

    public static void Run()
    {
      BuildingContext context = BuildingContext.Current;

      // Model definition building
      context.ModelDef = new DomainModelDef();
      ModelDefBuilder.Run();

      // Custom model definition actions
      BuildCustomDefinitions();

      CleanModelDef();

      // Inspecting model definition
      ModelInspector.Run();

      // Applying fixup actions
      if (context.ModelInspectionResult.HasActions) {
        // Add handlers for hierarchies and types that could be created as a result of FixupProcessor work
        // This is done to inspect them right after construction with the help of ModelInspector
        context.ModelDef.Hierarchies.Inserted += OnHierarchyAdded;
        context.ModelDef.Types.Inserted += OnTypeAdded;

        // Applying fixup actions to the model definition.
        FixupActionProcessor.Run();
      }

      BuildModel();
    }

    private static void CleanModelDef()
    {
      var modelDef = BuildingContext.Current.ModelDef;

      var ientityDef = modelDef.Types[typeof (IEntity)];
      if (ientityDef != null)
        modelDef.Types.Remove(ientityDef);
    }

    private static void BuildCustomDefinitions()
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.CustomDefinitions)) {
        var context = BuildingContext.Current;
        foreach (var module in context.BuilderConfiguration.Modules)
          module.OnDefinitionsBuilt(context, context.ModelDef);
      }
    }

    public static void BuildModel()
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.ActualModel)) {
        var context = BuildingContext.Current;

        context.Model = new DomainModel();
        var typeSequence = GetTypeBuildSequence(context);
        BuildTypes(typeSequence);
        BuildAssociations();
        BuildIndexes();
        context.Model.UpdateState(true);
      }
    }

    private static void BuildTypes(IEnumerable<Node<TypeDef>> nodes)
    {
      var context = BuildingContext.Current;

      using (Log.InfoRegion(Strings.LogBuildingX, Strings.Types)) {
        // Building types, system fields and hierarchies
        foreach (var node in nodes) {
          var typeDef = node.Value;
          TypeBuilder.BuildType(typeDef);
        }
      }
      using (Log.InfoRegion(Strings.LogBuildingX, "Fields"))
        foreach (var node in nodes) {
          var typeDef = node.Value;
          var typeInfo = context.Model.Types[typeDef.UnderlyingType];
          TypeBuilder.BuildFields(typeDef, typeInfo);
        }
    }

    private static void BuildAssociations()
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.Associations)) {
        var context = BuildingContext.Current;
        foreach (var pair in context.PairedAssociations) {
          if (context.DiscardedAssociations.Contains(pair.First))
            continue;
          if (!context.Model.Associations.Contains(pair.First))
            continue;
          AssociationBuilder.BuildPairedAssociation(pair.First, pair.Second);
        }

        foreach (var ai in context.DiscardedAssociations)
          context.Model.Associations.Remove(ai);
        context.DiscardedAssociations.Clear();

        foreach (var association in context.Model.Associations) {
          if (association.IsPaired)
            continue;
          if (!association.OnOwnerRemove.HasValue)
            association.OnOwnerRemove = OnRemoveAction.Clear;
          if (!association.OnTargetRemove.HasValue)
            association.OnTargetRemove = OnRemoveAction.Deny;
        }

        BuildAuxiliaryTypes(context.Model.Associations);
      }
    }

    private static void BuildIndexes()
    {
      IndexBuilder.BuildIndexes();
    }

    private static void BuildAuxiliaryTypes(IEnumerable<AssociationInfo> associations)
    {
      var context = BuildingContext.Current;
      foreach (var association in associations) {
        if (!association.IsMaster)
          continue;

        var multiplicity = association.Multiplicity;
        if (!(multiplicity==Multiplicity.ZeroToMany || multiplicity==Multiplicity.ManyToMany))
          continue;

        var masterType = association.OwnerType;
        var slaveType = association.TargetType;

        var genericDefinitionType = typeof (EntitySetItem<,>);
        var genericInstanceType = genericDefinitionType.MakeGenericType(masterType.UnderlyingType, slaveType.UnderlyingType);

        var underlyingTypeName = String.Format(GeneratedTypeNameFormat,
          masterType.UnderlyingType.Namespace,
          context.NameBuilder.BuildAssociationName(association));
        var underlyingType = generatedTypes.GetValue(underlyingTypeName,
          (_underlyingTypeName, _genericInstanceType) =>
            TypeHelper.CreateInheritedDummyType(_underlyingTypeName, _genericInstanceType, true),
          genericInstanceType);

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
        var masterFieldDef = underlyingTypeDef.Fields[WellKnown.MasterFieldName];
        var slaveFieldDef = underlyingTypeDef.Fields[WellKnown.SlaveFieldName];

        // Updating fields names only if types differ.
        if (masterType!=slaveType) {
          try {
            masterFieldDef.MappingName = context.NameBuilder.ApplyNamingRules(masterType.Name);
          }
          catch (DomainBuilderException) {
          }

          try {
            slaveFieldDef.MappingName = context.NameBuilder.ApplyNamingRules(slaveType.Name);
          }
          catch (DomainBuilderException) {
          }
        }
        context.ModelDef.Hierarchies.Add(hierarchy);
        context.ModelDef.Types.Add(underlyingTypeDef);

        var auxiliaryType = TypeBuilder.BuildType(underlyingTypeDef);
        auxiliaryType.IsAuxiliary = true;
        TypeBuilder.BuildFields(underlyingTypeDef, auxiliaryType);
        association.AuxiliaryType = auxiliaryType;
        if (association.IsPaired)
          association.Reversed.AuxiliaryType = auxiliaryType;
      }
    }

    #region Event handlers

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

    #endregion

    #region Topological sort helpers

    private static List<Node<TypeDef>> GetTypeBuildSequence(BuildingContext context)
    {
      List<Node<Node<TypeDef>, object>> loops;
      List<Node<TypeDef>> result = TopologicalSorter.Sort(context.DependencyGraph.Nodes, TypeConnector, out loops);
      if (result==null)
        throw new DomainBuilderException(String.Format(Strings.ExAtLeastOneLoopHaveBeenFoundInPersistentTypeDependenciesGraphSuspiciousTypesX, loops.Select(node => node.Item.Value.Name).ToCommaDelimitedString()));
      return result;
    }

    private static bool TypeConnector(Node<TypeDef> first, Node<TypeDef> second)
    {
      foreach (var info in second.OutgoingEdges)
        if (info.Weight==EdgeWeight.High && info.Head==first)
          return true;
      return false;
    }

    #endregion
  }
}