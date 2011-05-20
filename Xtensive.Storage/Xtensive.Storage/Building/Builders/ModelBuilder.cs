// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.26

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Notifications;
using Xtensive.Core.Reflection;
using Xtensive.Core.Sorting;
using Xtensive.Core.Threading;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Building.DependencyGraph;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;
using TypeHelper=Xtensive.Core.Reflection.TypeHelper;

namespace Xtensive.Storage.Building.Builders
{
  internal static class ModelBuilder
  {
    private const string GeneratedTypeNameFormat = "{0}.EntitySetItems.{1}";

    private static ThreadSafeDictionary<string, Type> generatedTypes
      = ThreadSafeDictionary<string, Type>.Create(new object());

    public static void Run()
    {
      var context = BuildingContext.Demand();

      // Model definition building
      context.ModelDef = new DomainModelDef();
      ModelDefBuilder.Run();

      ApplyCustomDefinitions();
      RemoveTemporaryDefinitions();

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

        ModelDefBuilder.ProcessTypes();
        InspectAndProcessGeneratedEntities(context);
      }
      BuildModel();
      context.ModelDef.Hierarchies.Inserted -= OnHierarchyAdded;
      context.ModelDef.Types.Inserted -= OnTypeAdded;
    }

    private static void InspectAndProcessGeneratedEntities(BuildingContext context)
    {
      foreach (var hieararchy in context.ModelInspectionResult.GeneratedHierarchies)
        ModelInspector.Inspect(hieararchy);
      foreach (var type in context.ModelInspectionResult.GeneratedTypes)
        ModelInspector.Inspect(type);
      context.ModelInspectionResult.GeneratedHierarchies.Clear();
      context.ModelInspectionResult.GeneratedTypes.Clear();

      if (context.ModelInspectionResult.HasActions)
        // Applying fixup actions for generated entities.
        FixupActionProcessor.Run();
    }

    private static void ApplyCustomDefinitions()
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.CustomDefinitions)) {
        var context = BuildingContext.Demand();
        foreach (var module in context.BuilderConfiguration.Modules)
          module.OnDefinitionsBuilt(context, context.ModelDef);
      }
    }

    private static void RemoveTemporaryDefinitions()
    {
      var modelDef = BuildingContext.Demand().ModelDef;
      var ientityDef = modelDef.Types[typeof (IEntity)];
      if (ientityDef != null)
        modelDef.Types.Remove(ientityDef);
    }

    public static void BuildModel()
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.ActualModel)) {
        var context = BuildingContext.Demand();

        context.Model = new DomainModel();
        var typeSequence = GetTypeBuildSequence(context);
        BuildTypes(typeSequence);
        BuildAssociations();
        BuildIndexes();
        context.Model.UpdateState(true);
        BuildPrefetchActions();
      }
    }

    private static void BuildPrefetchActions()
    {
      var context = BuildingContext.Demand();
      var model = context.Model;
      var domain = context.Domain;
      foreach (var type in context.Model.Types.Entities) {
        var associations = type.GetOwnerAssociations()
          .Where(a => a.OnOwnerRemove.In(OnRemoveAction.Cascade, OnRemoveAction.Clear))
          .ToList();
        if (associations.Count <= 0)
          continue;
        var actionContainer = new PrefetchActionContainer(type, associations);
        var action = actionContainer.BuildPrefetchAction();
        domain.PrefetchActionMap.Add(type, action);
      }
    } 

    private static void BuildTypes(IEnumerable<TypeDef> typeDefs)
    {
      var context = BuildingContext.Demand();

      using (Log.InfoRegion(Strings.LogBuildingX, Strings.Types)) {
        // Building types, system fields and hierarchies
        foreach (var typeDef in typeDefs) {
          TypeBuilder.BuildType(typeDef);
        }
      }
      using (Log.InfoRegion(Strings.LogBuildingX, "Fields"))
        foreach (var typeDef in typeDefs) {
          var typeInfo = context.Model.Types[typeDef.UnderlyingType];
          TypeBuilder.BuildFields(typeDef, typeInfo);
        }
    }

    private static void PreprocessAssociations()
    {
      var context = BuildingContext.Demand();
      foreach (var typeInfo in context.Model.Types.Where(t => t.IsEntity && !t.IsAuxiliary)) {
        
        // pair integrity escalation and consistency check
        context.TypesWithProcessedInheritedAssociations.Add(typeInfo);
        var refFields = typeInfo.Fields.Where(f => f.IsEntity || f.IsEntitySet).ToList();
        // check for interface fields
        foreach (var refField in refFields) {
          var parentIsPaired = false;
          var interfaceIsPaired = false;
          var interfaceAssociations = new List<AssociationInfo>();
          var inheritedAssociations = new List<AssociationInfo>();
          if (refField.IsDeclared && refField.IsInterfaceImplementation) {
            var implementedInterfaceFields = typeInfo.FieldMap
              .GetImplementedInterfaceFields(refField);

            foreach (var interfaceField in implementedInterfaceFields) {
              var field = interfaceField;
              interfaceAssociations.AddRange(field.Associations);
              interfaceIsPaired |= context.PairedAssociations.Any(pa => field.Associations.Contains(pa.First));
            }
          }
          if (refField.IsInherited) {
            var ancestor = typeInfo.GetAncestor();
            var field = ancestor.Fields[refField.Name];
            inheritedAssociations.AddRange(field.Associations);
            parentIsPaired |= context.PairedAssociations.Any(pa => field.Associations.Contains(pa.First));
          }

          if (!parentIsPaired && !interfaceIsPaired) {
            List<Pair<AssociationInfo, string>> pairedToReverse;
            if (context.PairedAssociationsToReverse.TryGetValue(typeInfo, out pairedToReverse))
              foreach (var pair in pairedToReverse)
                AssociationBuilder.BuildReversedAssociation(pair.First, pair.Second);
            var field = refField;
            var pairedAssociations = context.PairedAssociations
              .Where(pa => field.Associations.Contains(pa.First))
              .ToList();
            if (pairedAssociations.Count > 0) {
              foreach (var paired in pairedAssociations) {
                paired.First.Ancestors.AddRange(interfaceAssociations);
                if (paired.First.TargetType.IsInterface || context.TypesWithProcessedInheritedAssociations.Contains(paired.First.TargetType))
                  AssociationBuilder.BuildReversedAssociation(paired.First, paired.Second);
                else {
                  List<Pair<AssociationInfo, string>> pairs;
                  if (!context.PairedAssociationsToReverse.TryGetValue(paired.First.TargetType, out pairs)) {
                    pairs = new List<Pair<AssociationInfo, string>>();
                    context.PairedAssociationsToReverse.Add(paired.First.TargetType, pairs);
                  }
                  pairs.Add(paired);
                }
              }
              continue;
            }
          }

          var fieldCopy = refField;
          if (!parentIsPaired)
            context.PairedAssociations.RemoveAll(pa => fieldCopy.Associations.Contains(pa.First));
          Func<AssociationInfo, bool> associationFilter = a => context.PairedAssociations
            .Any(pa => a.TargetType.UnderlyingType.IsAssignableFrom(pa.First.OwnerType.UnderlyingType)
              && pa.Second == a.OwnerField.Name
              && a.OwnerType == pa.First.TargetType);
          var associationsToKeep = refField.IsInterfaceImplementation
            ? refField.Associations
                .Where(associationFilter)
                .ToList()
            : refField.Associations.Count > 1
              ? refField.Associations
                  .Where(associationFilter)
                  .ToList()
              : refField.Associations.ToList();
          var associationsToRemove = refField.Associations
            .Except(associationsToKeep)
            .ToList();

          foreach (var association in associationsToRemove) {
            context.Model.Associations.Remove(association);
            refField.Associations.Remove(association);
          }
          foreach (var association in associationsToKeep) {
            var interfaceAssociationsToRemove = interfaceAssociations
              .Where(ia => ia.OwnerType != association.OwnerType)
              .ToList();
            association.Ancestors.AddRange(interfaceAssociationsToRemove);
            foreach (var interfaceAssociation in interfaceAssociationsToRemove)
              interfaceAssociations.Remove(interfaceAssociation);
          }
          refField.Associations.AddRange(interfaceAssociations);
          foreach (var association in inheritedAssociations) {
            if (!refField.Associations.Contains(association.Name))
              refField.Associations.Add(association);
          }
        }
      }
    }


    private static void BuildAssociations()
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.Associations)) {
        PreprocessAssociations();
        var context = BuildingContext.Demand();
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
          if (association.Multiplicity.In(Multiplicity.OneToOne, Multiplicity.ZeroToOne)) {
            Func<IndexDef, bool> predicate = 
              i => i.IsSecondary && i.KeyFields.Count==1 && 
              i.KeyFields[0].Key==association.OwnerField.Name;
            var typeDef = context.ModelDef.Types[association.OwnerType.UnderlyingType];
            if (!typeDef.Indexes.Any(predicate)) {
              var attribute = new IndexAttribute(association.OwnerField.Name);
              var indexDef = ModelDefBuilder.DefineIndex(typeDef, attribute);
              typeDef.Indexes.Add(indexDef);
            }

          }
          if (association.IsPaired)
            continue;
          if (!association.OnOwnerRemove.HasValue)
            association.OnOwnerRemove = 
              association.OwnerField.IsNullable
              ? OnRemoveAction.Clear
              : OnRemoveAction.None;
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
      var context = BuildingContext.Demand();
      var list = new List<Pair<AssociationInfo, TypeDef>>();
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
        underlyingTypeDef.MappingName = 
          context.NameBuilder.BuildAuxiliaryTypeMappingName(association);

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
        if (masterType != slaveType) {
          try {
            if (!masterType.Name.Contains("."))
              masterFieldDef.MappingName = context.NameBuilder.ApplyNamingRules(masterType.Name);
          }
          catch(DomainBuilderException){}
          try {
            if (!slaveType.Name.Contains("."))
              slaveFieldDef.MappingName = context.NameBuilder.ApplyNamingRules(slaveType.Name);
          }
          catch (DomainBuilderException){}
        }

        context.ModelDef.Hierarchies.Add(hierarchy);
        context.ModelDef.Types.Add(underlyingTypeDef);
        list.Add(new Pair<AssociationInfo, TypeDef>(association, underlyingTypeDef));
      }

      InspectAndProcessGeneratedEntities(context);

      // Build auxiliary types
      foreach (var pair in list) {
        var auxiliaryType = TypeBuilder.BuildType(pair.Second);
        auxiliaryType.IsAuxiliary = true;
        TypeBuilder.BuildFields(pair.Second, auxiliaryType);
        pair.First.AuxiliaryType = auxiliaryType;
        if (pair.First.IsPaired)
          pair.First.Reversed.AuxiliaryType = auxiliaryType;
      }
    }

    #region Event handlers

    private static void OnHierarchyAdded(object sender, CollectionChangeNotifierEventArgs<HierarchyDef> e)
    {
      var context = BuildingContext.Demand();
      context.ModelInspectionResult.GeneratedHierarchies.Add(e.Item);
    }

    private static void OnTypeAdded(object sender, CollectionChangeNotifierEventArgs<TypeDef> e)
    {
      var context = BuildingContext.Demand();
      context.ModelInspectionResult.GeneratedTypes.Add(e.Item);
    }

    #endregion

    #region Topological sort helpers

    private static IEnumerable<TypeDef> GetTypeBuildSequence(BuildingContext context)
    {
      List<Node<Node<TypeDef>, object>> loops;
      var result = TopologicalSorter.Sort(context.DependencyGraph.Nodes, TypeConnector, out loops);
      if (result==null)
        throw new DomainBuilderException(String.Format(Strings.ExAtLeastOneLoopHaveBeenFoundInPersistentTypeDependenciesGraphSuspiciousTypesX, loops.Select(node => node.Item.Value.Name).ToCommaDelimitedString()));
      var dependentTypes = result.Select(n => n.Value);
      var independentTypes = context.ModelDef.Types.Except(dependentTypes);
      return independentTypes.Concat(dependentTypes);
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