// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.26

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Core;
using Xtensive.Notifications;
using Xtensive.Reflection;
using Xtensive.Sorting;
using Xtensive.Threading;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Building.DependencyGraph;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Model;

using TypeHelper=Xtensive.Reflection.TypeHelper;

namespace Xtensive.Orm.Building.Builders
{
  internal static class ModelBuilder
  {
    private const string GeneratedTypeNameFormat = "{0}.EntitySetItems.{1}";

    private static ThreadSafeDictionary<string, Type> generatedTypes
      = ThreadSafeDictionary<string, Type>.Create(new object());

    public static void Run(BuildingContext context)
    {
      // Model definition building
      ModelDefBuilder.Run(context);
      // Invoke user-defined transformations
      ApplyCustomDefinitions(context);
      // Clean-up
      RemoveTemporaryDefinitions(context);
      // Inspecting model definition
      ModelInspector.Run(context);

      // Applying fixup actions
      if (context.ModelInspectionResult.HasActions) {
        // Add handlers for hierarchies and types that could be created as a result of FixupProcessor work
        // This is done to inspect them right after construction with the help of ModelInspector
        var monitor = new TypeGenerationMonitor(context);
        monitor.Attach();

        // Applying fixup actions to the model definition.
        FixupActionProcessor.Run(context);

        ModelDefBuilder.ProcessTypes(context);
        InspectAndProcessGeneratedEntities(context);
        BuildModel(context);
        monitor.Detach();
      }
      else {
        // Simply build model
        BuildModel(context);
      }
    }

    private static void InspectAndProcessGeneratedEntities(BuildingContext context)
    {
      foreach (var hieararchy in context.ModelInspectionResult.GeneratedHierarchies)
        ModelInspector.Inspect(context, hieararchy);
      foreach (var type in context.ModelInspectionResult.GeneratedTypes)
        ModelInspector.Inspect(context, type);
      context.ModelInspectionResult.GeneratedHierarchies.Clear();
      context.ModelInspectionResult.GeneratedTypes.Clear();

      if (context.ModelInspectionResult.HasActions)
        // Applying fixup actions for generated entities.
        FixupActionProcessor.Run(context);
    }

    private static void ApplyCustomDefinitions(BuildingContext context)
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.CustomDefinitions)) {
        foreach (var module in context.BuilderConfiguration.Modules)
          module.OnDefinitionsBuilt(context, context.ModelDef);
      }
    }

    private static void RemoveTemporaryDefinitions(BuildingContext context)
    {
      var modelDef = context.ModelDef;
      var ientityDef = modelDef.Types.TryGetValue(typeof (IEntity));
      if (ientityDef != null)
        modelDef.Types.Remove(ientityDef);
    }

    public static void BuildModel(BuildingContext context)
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.ActualModel)) {
        context.Model = new DomainModel();
        BuildTypes(context, GetTypeBuildSequence(context));
        BuildAssociations(context);
        IndexBuilder.BuildIndexes(context);
        context.Model.UpdateState(true);
        ValidateMappingConfiguration(context);
        BuildPrefetchActions(context);
      }
    }

    private static void ValidateMappingConfiguration(BuildingContext context)
    {
      if (context.Model.IsMultidatabase || context.Model.IsMultischema)
        StorageMappingValidator.Run(context);
    }

    private static void BuildPrefetchActions(BuildingContext context)
    {
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

    private static void BuildTypes(BuildingContext context, IEnumerable<TypeDef> typeDefs)
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.Types)) {
        // Building types, system fields and hierarchies
        foreach (var typeDef in typeDefs) {
          TypeBuilder.BuildType(context, typeDef);
        }
      }
      using (Log.InfoRegion(Strings.LogBuildingX, "Fields"))
        foreach (var typeDef in typeDefs) {
          var typeInfo = context.Model.Types[typeDef.UnderlyingType];
          TypeBuilder.BuildFields(context, typeDef, typeInfo);
          TypeBuilder.BuildTypeDiscriminatorMap(context, typeDef, typeInfo);
        }
    }

    private static void PreprocessAssociations(BuildingContext context)
    {
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
                AssociationBuilder.BuildReversedAssociation(context, pair.First, pair.Second);
            var field = refField;
            var pairedAssociations = context.PairedAssociations
              .Where(pa => field.Associations.Contains(pa.First))
              .ToList();
            if (pairedAssociations.Count > 0) {
              foreach (var paired in pairedAssociations) {
                paired.First.Ancestors.AddRange(interfaceAssociations);
                if (paired.First.TargetType.IsInterface || context.TypesWithProcessedInheritedAssociations.Contains(paired.First.TargetType))
                  AssociationBuilder.BuildReversedAssociation(context, paired.First, paired.Second);
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


    private static void BuildAssociations(BuildingContext context)
    {
      using (Log.InfoRegion(Strings.LogBuildingX, Strings.Associations)) {
        PreprocessAssociations(context);
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
          TryAddForeignKeyIndex(context, association);
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

        BuildAuxiliaryTypes(context, context.Model.Associations);
      }
    }

    private static void TryAddForeignKeyIndex(BuildingContext context, AssociationInfo association)
    {
      if (!association.Multiplicity.In(Multiplicity.OneToOne, Multiplicity.ZeroToOne))
        return;
      var typeDef = context.ModelDef.Types[association.OwnerType.UnderlyingType];
      var field = association.OwnerField;
      if ((field.Attributes & FieldAttributes.NotIndexed) != 0)
        return;
      bool addIndex = true;
      while (field.Parent!=null) {
        field = field.Parent;
        addIndex = addIndex && field.IsStructure;
      }
      if (!addIndex)
        return;
      Func<IndexDef, bool> isIndexForField = i => i.IsSecondary && i.KeyFields.Count==1 && i.KeyFields[0].Key==association.OwnerField.Name;
      if (typeDef.Indexes.Any(isIndexForField))
        return;
      var attribute = new IndexAttribute(association.OwnerField.Name);
      var indexDef = ModelDefBuilder.DefineIndex(context, typeDef, attribute);
      typeDef.Indexes.Add(indexDef);
    }

    private static void BuildAuxiliaryTypes(BuildingContext context, IEnumerable<AssociationInfo> associations)
    {
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
        var underlyingTypeDef = ModelDefBuilder.DefineType(context, underlyingType);
        underlyingTypeDef.Name = association.Name;
        underlyingTypeDef.MappingName = 
          context.NameBuilder.BuildAuxiliaryTypeMappingName(association);

        // HierarchyRootAttribute is not inherited so we must take it from the generic type definition or generic instance type
        var hra = genericInstanceType.GetAttribute<HierarchyRootAttribute>(AttributeSearchOptions.Default);
        // Defining the hierarchy
        var hierarchy = ModelDefBuilder.DefineHierarchy(context, underlyingTypeDef, hra);

        // Processing type properties
        ModelDefBuilder.ProcessProperties(context, underlyingTypeDef, hierarchy);

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
        var auxiliaryType = TypeBuilder.BuildType(context, pair.Second);
        auxiliaryType.IsAuxiliary = true;
        TypeBuilder.BuildFields(context, pair.Second, auxiliaryType);
        pair.First.AuxiliaryType = auxiliaryType;
        if (pair.First.IsPaired)
          pair.First.Reversed.AuxiliaryType = auxiliaryType;
      }
    }

    #region Event handlers


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