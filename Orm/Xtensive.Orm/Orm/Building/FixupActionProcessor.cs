// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.05.28

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Building.DependencyGraph;
using Xtensive.Orm.Building.FixupActions;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Building
{
  internal sealed class FixupActionProcessor
  {
    #region Nested type: GenericArgumentCombinator

    private class GenericArgumentCombinator
    {
      private readonly Type[] current;
      private readonly List<Type> result;
      private readonly Type typeDefinition;
      private readonly Type[][] candidateTypes;

      public static List<Type> Generate(Type typeDefinition, Type[][] candidateTypes)
      {
        var combinator = new GenericArgumentCombinator(typeDefinition, candidateTypes);
        combinator.Generate(0);
        return combinator.result;
      }

      private void Generate(int argumentPosition)
      {
        if (argumentPosition < current.Length - 1)
          foreach (var candidate in candidateTypes[argumentPosition]) {
            current[argumentPosition] = candidate;
            Generate(argumentPosition + 1);
          }
        else
          foreach (var candidate in candidateTypes[argumentPosition]) {
            current[argumentPosition] = candidate;
            result.Add(typeDefinition.MakeGenericType(current));
          }
      }

      private GenericArgumentCombinator(Type typeDefinition, Type[][] candidateTypes)
      {
        current = new Type[candidateTypes.Length];
        result = new List<Type>();
        this.typeDefinition = typeDefinition;
        this.candidateTypes = candidateTypes;
      }
    }

    #endregion

    private readonly BuildingContext context;

    public static void Run(BuildingContext context)
    {
      new FixupActionProcessor(context).ProcessAll();
    }

    private void ProcessAll()
    {
      using (Log.InfoRegion(Strings.LogProcessingFixupActions))
        while (context.ModelInspectionResult.Actions.Count > 0) {
          var action = context.ModelInspectionResult.Actions.Dequeue();
          Log.Info(string.Format(Strings.LogExecutingActionX, action));
          action.Run(this);
        }
    }

    public void Process(AddTypeIdToKeyFieldsAction action)
    {
      action.Hierarchy.KeyFields.Add(new KeyField(WellKnown.TypeIdFieldName));
    }

    public void Process(ReorderFieldsAction action)
    {
      var target = action.Target;
      var buffer = new List<FieldDef>(target.Fields.Count);

      foreach (var keyField in action.Hierarchy.KeyFields) {
        var fieldDef = target.Fields[keyField.Name];
        buffer.Add(fieldDef);
        target.Fields.Remove(fieldDef);
      }
      if (!action.Hierarchy.IncludeTypeId) {
        var typeIdField = target.Fields[WellKnown.TypeIdFieldName];
        buffer.Add(typeIdField);
        target.Fields.Remove(typeIdField);
      }
      buffer.AddRange(target.Fields);
      target.Fields.Clear();
      target.Fields.AddRange(buffer);
    }

    public void Process(RemoveTypeAction action)
    {
      context.DependencyGraph.RemoveNode(action.Type);
      context.ModelDef.Types.Remove(action.Type);
    }

    public void Process(MakeTypeNonAbstractAction action)
    {
      action.Type.IsAbstract = false;
    }

    public void Process(BuildGenericTypeInstancesAction action)
    {
      // Making a copy of already built hierarchy set to avoid recursiveness
      var hierarchies = context.ModelDef.Hierarchies
        .Where(h => !h.Root.IsGenericTypeDefinition)
        .ToList();
      var type = action.Type;
      var parameters = type.UnderlyingType.GetGenericArguments();
      var inheritors = context.ModelDef.Types
        .Where(t => t.IsEntity)
        .ToLookup(t => t.UnderlyingType.BaseType, t => t.UnderlyingType);

      // Remove hierarchy
      var hierarchyToRemove = context.ModelDef.Hierarchies.SingleOrDefault(h => h.Root == type);
      if (hierarchyToRemove != null) 
        context.ModelDef.Hierarchies.Remove(hierarchyToRemove);

      // Building all possible generic argument substitutions
      var arguments = type.UnderlyingType.GetGenericArguments();
      var typeSubstitutions = new Type[arguments.Length][];
      for (var i = 0; i < arguments.Length; i++) {
        var argument = arguments[i];
        var constraints = argument.GetGenericParameterConstraints()
          .ToList();
        if (constraints.Count == 0 || !constraints.Any(c => typeof (IEntity).IsAssignableFrom(c)))
          return; // No IEntity / Entity constraints
        var queue = new Queue<Type>(
          from hierarchy in hierarchies
          let root = hierarchy.Root
          where 
            !root.IsSystem && 
            !root.IsAutoGenericInstance &&
            !root.IsAbstract
          select root.UnderlyingType);
        var types = new List<Type>();
        while (queue.Count > 0) {
          var candidate = queue.Dequeue();
          if (constraints.All(ct => ct.IsAssignableFrom(candidate)))
            // First suitable descendant is found
            types.Add(candidate); 
          else
            // Suitable descendant is not found, let's study its descendant
            foreach (var inheritor in inheritors[candidate])
              queue.Enqueue(inheritor);
        }
        if (types.Count == 0)
          return; // One of arguments has no matching types, so nothing to register at all
        typeSubstitutions[i] = types.ToArray();
      }
      foreach (var instanceType in GenericArgumentCombinator.Generate(type.UnderlyingType, typeSubstitutions)) {
        var typeDef = ModelDefBuilder.ProcessType(context, instanceType);
        typeDef.Attributes |= TypeAttributes.AutoGenericInstance;
        // Copy schema/database from first generic argument
        var originatingType = instanceType.GetGenericArguments()[0];
        var originatingEntity = context.ModelDef.Types[originatingType];
        typeDef.MappingDatabase = originatingEntity.MappingDatabase;
        typeDef.MappingSchema = originatingEntity.MappingSchema;
      }
    }

    public void Process(AddForeignKeyIndexAction action)
    {
      var type = action.Type;
      Func<IndexDef, bool> predicate = 
        i => i.IsSecondary && 
        i.KeyFields.Count==1 && 
        i.KeyFields[0].Key==action.Field.Name;
      if (type.Indexes.Any(predicate))
        return;
      var queue = new Queue<TypeDef>();
      var interfaces = new HashSet<TypeDef>();
      queue.Enqueue(type);
      while (queue.Count > 0) {
        var item = queue.Dequeue();
        foreach (var @interface in context.ModelDef.Types.FindInterfaces(item.UnderlyingType)) {
          queue.Enqueue(@interface);
          interfaces.Add(@interface);
        }
      }
      if (interfaces.SelectMany(i => i.Indexes).Any(predicate))
        return;

      var attribute = new IndexAttribute(action.Field.Name);
      var indexDef = ModelDefBuilder.DefineIndex(context, type, attribute);
      type.Indexes.Add(indexDef);
    }

    public void Process(AddPrimaryIndexAction action)
    {
      var type = action.Type;

      var primaryIndexes = type.Indexes.Where(i => i.IsPrimary).ToList();
      if (primaryIndexes.Count > 0)
        foreach (var primaryIndex in primaryIndexes)
          type.Indexes.Remove(primaryIndex);

      var generatedIndex = new IndexDef(action.Type) {IsPrimary = true};
      generatedIndex.Name = context.NameBuilder.BuildIndexName(type, generatedIndex);

      TypeDef hierarchyRoot;
      if (type.IsInterface) {
        var implementor = type.Implementors.First();
        hierarchyRoot = implementor;
      }
      else
        hierarchyRoot = type;

      var hierarchyDef = context.ModelDef.FindHierarchy(hierarchyRoot);

      foreach (KeyField pair in hierarchyDef.KeyFields)
        generatedIndex.KeyFields.Add(pair.Name, pair.Direction);

      // Check if user added secondary index equal to auto-generated primary index
      var userDefinedIndex = type.Indexes.Where(i => (i.KeyFields.Count==generatedIndex.KeyFields.Count) && i.KeyFields.SequenceEqual(generatedIndex.KeyFields)).FirstOrDefault();
      if (userDefinedIndex != null) {
        type.Indexes.Remove(userDefinedIndex);
        generatedIndex.FillFactor = userDefinedIndex.FillFactor;
        if (!string.IsNullOrEmpty(userDefinedIndex.MappingName))
          generatedIndex.MappingName = userDefinedIndex.MappingName;
      }

      // Gather "is clustered" flag from hierarchy.
      generatedIndex.IsClustered = hierarchyDef.IsClustered;

      type.Indexes.Add(generatedIndex);
    }

    public void Process(MarkFieldAsSystemAction action)
    {
      action.Field.IsSystem = true;
      if (action.Type.IsEntity && action.Field.Name == WellKnown.TypeIdFieldName)
        action.Field.IsTypeId = true;
    }

    public void Process(AddTypeIdFieldAction action)
    {
      FieldDef fieldDef = ModelDefBuilder.DefineField(context, typeof (Entity).GetProperty(WellKnown.TypeIdFieldName));
      fieldDef.IsTypeId = true;
      fieldDef.IsSystem = true;
      action.Type.Fields.Add(fieldDef);
    }

    public void Process(MarkFieldAsNotNullableAction action)
    {
      action.Field.IsNullable = false;
    }

    public void Process(CopyKeyFieldsAction action)
    {
      var target = action.Type;
      var source = action.Source;

      Action<FieldDef> createField = sourceFieldDef => {
        if (target.Fields.Contains(sourceFieldDef.Name)) 
          return;
        var fieldDef = target.DefineField(sourceFieldDef.Name, sourceFieldDef.ValueType);
        fieldDef.Attributes = sourceFieldDef.Attributes;
        if (!string.IsNullOrEmpty(sourceFieldDef.MappingName))
         fieldDef.MappingName = sourceFieldDef.MappingName;
      };

      foreach (var keyField in source.KeyFields) {
        var sourceFieldDef = source.Root.Fields[keyField.Name];
        createField(sourceFieldDef);
      }
      // copy system fields
      foreach (var sourceFieldDef in source.Root.Fields.Where(f => f.IsSystem)) {
        createField(sourceFieldDef);
      }
    }

    public void Process(BuildImplementorListAction action)
    {
      var node = context.DependencyGraph.TryGetNode(action.Type);
      node.Value.Implementors.AddRange(node.IncomingEdges.Where(e => e.Kind==EdgeKind.Implementation).Select(e => e.Tail.Value));
    }

    // Constructors

    private FixupActionProcessor(BuildingContext context)
    {
      this.context = context;
    }
  }
}