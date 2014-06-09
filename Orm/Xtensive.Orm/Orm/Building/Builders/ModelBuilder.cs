// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.26

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Building.DependencyGraph;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Model;
using Xtensive.Reflection;
using Xtensive.Sorting;
using FieldAttributes = Xtensive.Orm.Model.FieldAttributes;
using TypeHelper = Xtensive.Reflection.TypeHelper;

namespace Xtensive.Orm.Building.Builders
{
  internal sealed class ModelBuilder
  {
    private const string GeneratedTypeNameFormat = "{0}.EntitySetItems.{1}";

    private static ThreadSafeDictionary<string, Type> GeneratedTypes = ThreadSafeDictionary<string, Type>.Create(new object());

    private readonly BuildingContext context;
    private readonly TypeBuilder typeBuilder;
    private readonly ModelDefBuilder modelDefBuilder;

    private readonly HashSet<TypeInfo> typesWithProcessedInheritedAssociations = new HashSet<TypeInfo>();
    private readonly Dictionary<TypeInfo, List<Pair<AssociationInfo, string>>> pairedAssociationsToReverse
      = new Dictionary<TypeInfo, List<Pair<AssociationInfo, string>>>();

    public static void Run(BuildingContext context)
    {
      new ModelBuilder(context).Run();
    }

    private void Run()
    {
      // Model definition building
      BuildModelDefinition();
      // Applying mapping rules
      StorageMappingBuilder.Run(context);
      // Invoke user-defined transformations
      ApplyCustomDefinitions();
      // Clean-up
      RemoveTemporaryDefinitions();
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

        modelDefBuilder.ProcessTypes();
        InspectAndProcessGeneratedEntities();
        BuildModel();
        monitor.Detach();
      }
      else {
        // Simply build model
        BuildModel();
      }
    }

    private void BuildModelDefinition()
    {
      using (BuildLog.InfoRegion(Strings.LogBuildingX, Strings.ModelDefinition)) {
        context.ModelDef = new DomainModelDef(modelDefBuilder, context.Validator);
        using (BuildLog.InfoRegion(Strings.LogDefiningX, Strings.Types))
          modelDefBuilder.ProcessTypes();
      }
    }

    private void InspectAndProcessGeneratedEntities()
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

    private void ApplyCustomDefinitions()
    {
      using (BuildLog.InfoRegion(Strings.LogBuildingX, Strings.CustomDefinitions))
      using (new BuildingScope(context)) { // Activate context for compatibility with previous versions
        foreach (var module in context.Modules)
          module.OnDefinitionsBuilt(context, context.ModelDef);
      }
    }

    private void RemoveTemporaryDefinitions()
    {
      var modelDef = context.ModelDef;
      var ientityDef = modelDef.Types.TryGetValue(typeof (IEntity));
      if (ientityDef != null)
        modelDef.Types.Remove(ientityDef);
    }

    private void BuildModel()
    {
      using (BuildLog.InfoRegion(Strings.LogBuildingX, Strings.ActualModel)) {
        context.Model = new DomainModel();
        BuildTypes(GetTypeBuildSequence());
        BuildAssociations();
        FindAndMarkInboundAndOutboundTypes(context);
        IndexBuilder.BuildIndexes(context);
        context.Model.UpdateState();
        ValidateMappingConfiguration();
        BuildDatabaseDependencies();
        BuildPrefetchActions();
      }
    }

    private void BuildDatabaseDependencies()
    {
      if (context.Configuration.IsMultidatabase)
        DatabaseDependencyBuilder.Run(context);
    }

    private void ValidateMappingConfiguration()
    {
      if (context.Configuration.IsMultidatabase || context.Configuration.IsMultischema)
        StorageMappingValidator.Run(context);
    }

    private void BuildPrefetchActions()
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

    private void BuildTypes(IEnumerable<TypeDef> typeDefs)
    {
      using (BuildLog.InfoRegion(Strings.LogBuildingX, Strings.Types)) {
        // Building types, system fields and hierarchies
        foreach (var typeDef in typeDefs) {
          typeBuilder.BuildType(typeDef);
        }
      }
      using (BuildLog.InfoRegion(Strings.LogBuildingX, "Fields"))
        foreach (var typeDef in typeDefs) {
          var typeInfo = context.Model.Types[typeDef.UnderlyingType];
          typeBuilder.BuildFields(typeDef, typeInfo);
          typeBuilder.BuildTypeDiscriminatorMap(typeDef, typeInfo);
        }
    }

    private void PreprocessAssociations()
    {
      foreach (var typeInfo in context.Model.Types.Where(t => t.IsEntity && !t.IsAuxiliary)) {
        
        // pair integrity escalation and consistency check
        typesWithProcessedInheritedAssociations.Add(typeInfo);
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
            if (pairedAssociationsToReverse.TryGetValue(typeInfo, out pairedToReverse))
              foreach (var pair in pairedToReverse)
                AssociationBuilder.BuildReversedAssociation(context, pair.First, pair.Second);
            var field = refField;
            var pairedAssociations = context.PairedAssociations
              .Where(pa => field.Associations.Contains(pa.First))
              .ToList();
            if (pairedAssociations.Count > 0) {
              foreach (var paired in pairedAssociations) {
                paired.First.Ancestors.AddRange(interfaceAssociations);
                if (paired.First.TargetType.IsInterface || typesWithProcessedInheritedAssociations.Contains(paired.First.TargetType))
                  AssociationBuilder.BuildReversedAssociation(context, paired.First, paired.Second);
                else {
                  List<Pair<AssociationInfo, string>> pairs;
                  if (!pairedAssociationsToReverse.TryGetValue(paired.First.TargetType, out pairs)) {
                    pairs = new List<Pair<AssociationInfo, string>>();
                    pairedAssociationsToReverse.Add(paired.First.TargetType, pairs);
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
            if (!context.Model.Associations.Contains(association))
              context.Model.Associations.Add(association);
          }
        }
      }
    }


    private void BuildAssociations()
    {
      using (BuildLog.InfoRegion(Strings.LogBuildingX, Strings.Associations)) {
        PreprocessAssociations();
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
          TryAddForeignKeyIndex(association);
          if (association.IsPaired)
            continue;
          if (!association.OnOwnerRemove.HasValue)
            association.OnOwnerRemove = association.OwnerField.IsEntitySet ? OnRemoveAction.Clear : OnRemoveAction.None;
          if (!association.OnTargetRemove.HasValue)
            association.OnTargetRemove = OnRemoveAction.Deny;
        }

        BuildAuxiliaryTypes(context.Model.Associations);
      }
    }

    private void TryAddForeignKeyIndex(AssociationInfo association)
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
      var indexDef = modelDefBuilder.DefineIndex(typeDef, attribute);
      typeDef.Indexes.Add(indexDef);
    }

    private void BuildAuxiliaryTypes(IEnumerable<AssociationInfo> associations)
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

        var underlyingType = GenerateAuxiliaryType(association);

        // Defining auxiliary type
        var underlyingTypeDef = modelDefBuilder.DefineType(underlyingType);
        underlyingTypeDef.Name = association.Name;
        underlyingTypeDef.MappingName = context.NameBuilder.BuildAuxiliaryTypeMappingName(association);
        // Copy mapping information from master type
        underlyingTypeDef.MappingSchema = association.OwnerType.MappingSchema;
        underlyingTypeDef.MappingDatabase = association.OwnerType.MappingDatabase;

        // HierarchyRootAttribute is not inherited so we must take it from the generic type definition or generic instance type
        var hra = typeof (EntitySetItem<,>).GetAttribute<HierarchyRootAttribute>(AttributeSearchOptions.Default);
        // Defining the hierarchy
        var hierarchy = modelDefBuilder.DefineHierarchy(underlyingTypeDef, hra);

        // Processing type properties
        modelDefBuilder.ProcessProperties(underlyingTypeDef, hierarchy);

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

      InspectAndProcessGeneratedEntities();

      // Build auxiliary types
      foreach (var pair in list) {
        var association = pair.First;
        var auxTypeDef = pair.Second;

        var auxiliaryType = typeBuilder.BuildType(auxTypeDef);
        auxiliaryType.IsAuxiliary = true;
        typeBuilder.BuildFields(auxTypeDef, auxiliaryType);
        association.AuxiliaryType = auxiliaryType;
        if (association.IsPaired)
          association.Reversed.AuxiliaryType = auxiliaryType;
      }
    }

    private Type GenerateAuxiliaryType(AssociationInfo association)
    {
      var masterType = association.OwnerType.UnderlyingType;
      var slaveType = association.TargetType.UnderlyingType;
      var baseType = typeof (EntitySetItem<,>).MakeGenericType(masterType, slaveType);

      var typeName = string.Format(GeneratedTypeNameFormat,
        masterType.Namespace,
        context.NameBuilder.BuildAssociationName(association));

      var result = GeneratedTypes.GetValue(typeName,
        (_typeName, _baseType) =>
          TypeHelper.CreateInheritedDummyType(_typeName, _baseType, true),
        baseType);

      return result;
    }

    private void FindAndMarkInboundAndOutboundTypes(BuildingContext context)
    {
      var inputRefCountDictionary = InitReferencesOfTypesDictionary(context.Model.Types);
      var outputRefCountDictionary = InitReferencesOfTypesDictionary(context.Model.Types);

      MarkAuxiliaryTypesAsOutboundOnly(context.Model.Types);

      var associations = GetMasterOrNonPairedAssociations(context.Model.Associations);
      foreach (var association in associations) {
        switch (association.Multiplicity) {
          case Multiplicity.ZeroToOne:
          case Multiplicity.ManyToOne: {
            RegiserReferences(outputRefCountDictionary, association.OwnerType);
            RegiserReferences(inputRefCountDictionary, association.TargetType);
            break;
          }
          case Multiplicity.OneToMany: {
            RegiserReferences(inputRefCountDictionary, association.OwnerType);
            RegiserReferences(outputRefCountDictionary, association.TargetType);
            break;
          }
          case Multiplicity.OneToOne: {
            RegiserReferences(inputRefCountDictionary, association.OwnerType, association.TargetType);
            RegiserReferences(outputRefCountDictionary, association.OwnerType, association.TargetType);
            break;
          }
          case Multiplicity.ManyToMany:
          case Multiplicity.ZeroToMany: {
            RegiserReferences(inputRefCountDictionary, association.OwnerType, association.TargetType);
            break;
          }
        }
      }
      MarkTypesAsInboundOnly(outputRefCountDictionary);
      MarkTypesAsOutboundOnly(inputRefCountDictionary);
    }

    private void RegiserReferences(Dictionary<TypeInfo, int> referenceRegistrator, params TypeInfo[] typesToRegisterReferences)
    {
      foreach (var type in typesToRegisterReferences) {
        var typeImplementors = type.GetImplementors();
        var descendantTypes = type.GetDescendants(true);
        if (typeImplementors.Any())
        {
          foreach (var implementor in typeImplementors)
            if (referenceRegistrator.ContainsKey(implementor))
              referenceRegistrator[implementor] += 1;
        }
        else {
          if (referenceRegistrator.ContainsKey(type))
            referenceRegistrator[type] += 1;
          if (descendantTypes.Any()) {
            foreach (var descendant in descendantTypes) {
              if (referenceRegistrator.ContainsKey(descendant))
                referenceRegistrator[descendant] += 1;
            }
          }
        }
      }
    }

    private void MarkAuxiliaryTypesAsOutboundOnly(IEnumerable<TypeInfo> typesToMark)
    {
      var auxiliary = typesToMark.Where(el => el.IsAuxiliary);
      foreach (var typeInfo in auxiliary)
        typeInfo.IsOutboundOnly = true;
    }

    private void MarkTypesAsInboundOnly(Dictionary<TypeInfo, int> outputRefCountDictionary)
    {
      foreach (var output in outputRefCountDictionary.Where(el => el.Value==0))
        output.Key.IsInboundOnly = true;
    }

    private void MarkTypesAsOutboundOnly(Dictionary<TypeInfo, int> inputRefCountDictionary)
    {
      foreach (var input in inputRefCountDictionary.Where(el => el.Value == 0))
        input.Key.IsOutboundOnly = true;
    }

    private IEnumerable<AssociationInfo> GetMasterOrNonPairedAssociations(IEnumerable<AssociationInfo> allAssociations)
    {
      return allAssociations.Where(el => el.IsMaster || !el.IsPaired);
    }

    private Dictionary<TypeInfo,int> InitReferencesOfTypesDictionary(TypeInfoCollection allTypes)
    {
      var referencesOfTypeDictionary = new Dictionary<TypeInfo, int>();
      var entityTypes = allTypes.Where(el => el.IsEntity && !el.IsInterface && !el.IsStructure && !el.IsSystem && !el.IsAuxiliary);
      foreach (var type in entityTypes) {
        referencesOfTypeDictionary.Add(type,0);
      }
      return referencesOfTypeDictionary;
    }

    #region Topological sort helpers

    private IEnumerable<TypeDef> GetTypeBuildSequence()
    {
      List<Node<Node<TypeDef>, object>> loops;
      var result = TopologicalSorter.Sort(context.DependencyGraph.Nodes, TypeConnector, out loops);
      if (result==null)
        throw new DomainBuilderException(string.Format(
          Strings.ExAtLeastOneLoopHaveBeenFoundInPersistentTypeDependenciesGraphSuspiciousTypesX,
          loops.Select(node => node.Item.Value.Name).ToCommaDelimitedString()));
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

    // Constructors

    private ModelBuilder(BuildingContext context)
    {
      this.context = context;
      modelDefBuilder = new ModelDefBuilder(context);
      context.ModelDefBuilder = modelDefBuilder;
      typeBuilder = new TypeBuilder(context);
    }
  }
}