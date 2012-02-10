// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.02.24

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Reflection;
using Xtensive.Orm.Building.Builders;
using Xtensive.Orm.Building.Definitions;
using Xtensive.Orm.Building.DependencyGraph;
using Xtensive.Orm.Building.FixupActions;
using Xtensive.Orm.Model;


namespace Xtensive.Orm.Building
{
  [Serializable]
  internal static class ModelInspector
  {
    public static void Run()
    {
      using (Log.InfoRegion(Strings.LogInspectingModelDefinition)) {
        InspectHierarchies();
        InspectTypes();
        InspectInterfaces();
        InspectReferences();
        InspectAbstractTypes();
      }
    }

    private static void InspectAbstractTypes()
    {
      var context = BuildingContext.Demand();
      foreach (var typeDef in context.ModelDef.Types.Where(td => td.IsAbstract)) {
        var hierarchyDef = context.ModelDef.FindHierarchy(typeDef);
        if (hierarchyDef != null) {
          var node = context.DependencyGraph.TryGetNode(typeDef);
          if (node == null || !node.IncomingEdges.Any(e => e.Kind == EdgeKind.Inheritance))
            context.ModelInspectionResult.Register(new MakeTypeNonAbstractAction(typeDef));
//            throw new DomainBuilderException(string.Format("Abstract class {0} does not have registered implementors.", typeDef.Name));
        }
      }
    }

    private static void InspectHierarchies()
    {
      foreach (var hierarchy in BuildingContext.Demand().ModelDef.Hierarchies)
        Inspect(hierarchy);
    }

    private static void InspectTypes()
    {
      foreach (var typeDef in BuildingContext.Demand().ModelDef.Types)
        Inspect(typeDef);
    }

    private static void InspectInterfaces()
    {
      var context = BuildingContext.Demand();

      foreach (var interfaceDef in context.ModelDef.Types.Where(t => t.IsInterface)) {

        var interfaceNode = context.DependencyGraph.TryGetNode(interfaceDef);

        // Are there any dependencies at all?
        if (interfaceNode == null) {
          context.ModelInspectionResult.Register(new RemoveTypeAction(interfaceDef));
          continue;
        }

        // Are there any implementors?
        var implementorEdges = interfaceNode.IncomingEdges.Where(e => e.Kind==EdgeKind.Implementation).ToList();

        // There is no implementors. If there are no references to the interface, it could be safely removed
        if (implementorEdges.Count == 0 && !interfaceNode.IncomingEdges.Any(e => e.Kind == EdgeKind.Reference)) {
          context.ModelInspectionResult.Register(new RemoveTypeAction(interfaceDef));
          continue;
        }

        HierarchyDef hierarchyDef;

        // There is only one implementor. Nothing else to do here
        if (implementorEdges.Count == 1) {
          hierarchyDef = context.ModelDef.FindHierarchy(implementorEdges[0].Tail.Value);
          if (hierarchyDef == null)
            throw new DomainBuilderException(string.Format("{0} implementors don't belong to any hierarchy.", interfaceDef.Name));
          context.ModelInspectionResult.SingleHierarchyInterfaces.Add(interfaceDef);
        }
        else {

          // Cleaning implementation edges. We need only direct implementors of interface
          var directImplementorEdges = new HashSet<Edge<TypeDef>>();
          foreach (var implementorEdge in implementorEdges) {

            if (context.ModelInspectionResult.RemovedTypes.Contains(implementorEdge.Tail.Value))
              continue;

            var implementorType = implementorEdge.Tail.Value.UnderlyingType;
            var interfaceType = implementorEdge.Head.Value.UnderlyingType;
            // if not explicit implementation
            var interfaceMap = implementorType.GetInterfaceMap(interfaceType);
            if (!interfaceMap.TargetMethods.Any(tm => tm.DeclaringType == implementorType && tm.IsPrivate)) {
              // Checking for ancestor-descendant connection
              foreach (var directImplementorEdge in directImplementorEdges) {
                var directImplementorType = directImplementorEdge.Tail.Value.UnderlyingType;

                // Implementor is a descendant of one of direct implementors
                if (implementorType.IsSubclassOf(directImplementorType)) {
                  interfaceNode.IncomingEdges.Remove(implementorEdge);
                  implementorEdge.Tail.OutgoingEdges.Remove(implementorEdge);
                  goto Next;
                }
                // Implementor is an ancestor of one of direct implementors
                if (directImplementorType.IsSubclassOf(implementorType)) {
                  directImplementorEdges.Remove(directImplementorEdge);
                  directImplementorEdges.Add(implementorEdge);
                  interfaceNode.IncomingEdges.Remove(directImplementorEdge);
                  directImplementorEdge.Tail.OutgoingEdges.Remove(directImplementorEdge);
                  goto Next;
                }
              }
            }
            // None of direct implementors is in the same hierarchy as implementor
            directImplementorEdges.Add(implementorEdge);
            Next:
            ;
          }

          // Find hierarchies for all direct implementors
          var hierarchies = new HashSet<HierarchyDef>();
          foreach (var edge in directImplementorEdges) {
            var implementor = edge.Tail.Value;
            var hierarchy = context.ModelDef.FindHierarchy(implementor);
            if (hierarchy!=null)
              hierarchies.Add(hierarchy);
            else
              context.ModelInspectionResult.Register(new RemoveTypeAction(implementor));
          }

          // TODO: what if hierarchies.Count == 0?
          var count = hierarchies.Count;
          if (count == 0)
            throw new DomainBuilderException(string.Format("{0} implementors don't belong to any hierarchy.", interfaceDef.Name));
          if (count == 1)
            context.ModelInspectionResult.SingleHierarchyInterfaces.Add(interfaceDef);
          else {
            HierarchyDef master = null;
            foreach (var candidate in hierarchies) {
              if (master==null) {
                master = candidate;
                continue;
              }
              Validator.ValidateHierarchyEquality(interfaceDef, master, candidate);
            }
          }

          hierarchyDef = hierarchies.First();
        }

        context.ModelInspectionResult.Register(new CopyKeyFieldsAction(interfaceDef, hierarchyDef));
        context.ModelInspectionResult.Register(new ReorderFieldsAction(interfaceDef, hierarchyDef));
        context.ModelInspectionResult.Register(new BuildImplementorListAction(interfaceDef));
        context.ModelInspectionResult.Register(new AddPrimaryIndexAction(interfaceDef));
      }
    }

    private static void InspectReferences()
    {
      
    }

    public static void Inspect(HierarchyDef hierarchyDef)
    {
      var context = BuildingContext.Demand();
      var root = hierarchyDef.Root;
      Log.Info(Strings.LogInspectingHierarchyX, root.Name);
      Validator.ValidateHierarchy(hierarchyDef);
      // Skip open generic hierarchies
      if (root.IsGenericTypeDefinition)
        return;

      // Check the presence of TypeId field
      FieldDef typeIdField;
      if (!root.Fields.TryGetValue(WellKnown.TypeIdFieldName, out typeIdField))
        context.ModelInspectionResult.Actions.Enqueue(new AddTypeIdFieldAction(root));
      else
        context.ModelInspectionResult.Actions.Enqueue(new MarkFieldAsSystemAction(root, typeIdField));

      // Should TypeId field be added to key fields?
      if (hierarchyDef.IncludeTypeId && hierarchyDef.KeyFields.Find(f => f.Name == WellKnown.TypeIdFieldName) == null)
        context.ModelInspectionResult.Register(new AddTypeIdToKeyFieldsAction(hierarchyDef));

      context.ModelInspectionResult.Actions.Enqueue(new ReorderFieldsAction(hierarchyDef));

      foreach (var keyField in hierarchyDef.KeyFields) {
        var field = root.Fields[keyField.Name];
        InspectField(root, field, true);
      }

      context.ModelInspectionResult.Actions.Enqueue(new AddPrimaryIndexAction(root));
    }

    public static void Inspect(TypeDef typeDef)
    {
      var context = BuildingContext.Demand();
      Log.Info(Strings.LogInspectingTypeX, typeDef.Name);

      if (typeDef.IsInterface) {
         // Remove open generic interface
        if (typeDef.IsGenericTypeDefinition) {
          context.ModelInspectionResult.Register(new RemoveTypeAction(typeDef));
          return;
        }

        // Base interfaces
        foreach (var @interface in context.ModelDef.Types.FindInterfaces(typeDef.UnderlyingType))
          context.DependencyGraph.AddEdge(typeDef, @interface, EdgeKind.Inheritance, EdgeWeight.High);

        // Fields
        foreach (var field in typeDef.Fields)
          InspectField(typeDef, field, false);
        return;
      }

      if (typeDef.IsStructure) {

        if (typeDef.UnderlyingType.IsGenericTypeDefinition) {
          context.ModelInspectionResult.Register(new RemoveTypeAction(typeDef));
          return;
        }

        // Ancestor
        var parent = context.ModelDef.Types.FindAncestor(typeDef);
        if (parent!=null)
          context.DependencyGraph.AddEdge(typeDef, parent, EdgeKind.Inheritance, EdgeWeight.High);

        // Fields
        foreach (var field in typeDef.Fields)
          InspectField(typeDef, field, false);
        return;
      }

      if (typeDef.IsEntity) {
        Validator.EnsureUnderlyingTypeIsAspected(typeDef);
        // Should we remove it or not?
        var hierarchyDef = context.ModelDef.FindHierarchy(typeDef);
        if (hierarchyDef == null) {
          context.ModelInspectionResult.Register(new RemoveTypeAction(typeDef));
          return;
        }

        // Register closed generic types
        if (typeDef.IsGenericTypeDefinition) {
          var allGenericConstraintsAreEntity = typeDef.UnderlyingType.GetGenericArguments()
            .SelectMany(ga => ga.GetGenericParameterConstraints())
            .All(constraintType => typeof (IEntity).IsAssignableFrom(constraintType));
          // Skip automatic registration for open generic types whicn constraints are not IEntity
          if (!allGenericConstraintsAreEntity)
            return;

          if (!typeDef.IsAbstract)
            context.ModelInspectionResult.Register(new BuildGenericTypeInstancesAction(typeDef));
          context.ModelInspectionResult.Register(new RemoveTypeAction(typeDef));
          return;
        }

        // Ancestor
        var parent = context.ModelDef.Types.FindAncestor(typeDef);
        if (parent!=null)
          context.DependencyGraph.AddEdge(typeDef, parent, EdgeKind.Inheritance, EdgeWeight.High);

        // Interfaces
        foreach (var @interface in context.ModelDef.Types.FindInterfaces(typeDef.UnderlyingType)) {
          context.DependencyGraph.AddEdge(typeDef, @interface, EdgeKind.Implementation, EdgeWeight.High);
          context.Interfaces.Add(@interface);
        }
       
        Validator.ValidateType(typeDef, hierarchyDef);
        // We should skip key fields inspection as they have been already inspected
        foreach (var field in typeDef.Fields.Where(f => !hierarchyDef.KeyFields.Any(kf => kf.Name == f.Name)))
          InspectField(typeDef, field, false);
      }
    }

    #region Private members

    private static void InspectField(TypeDef typeDef, FieldDef fieldDef, bool isKeyField)
    {
      var context = BuildingContext.Demand();
      if ((fieldDef.Attributes & (FieldAttributes.ManualVersion | FieldAttributes.AutoVersion)) > 0)
        Validator.ValidateVersionField(fieldDef, isKeyField);

      Validator.ValidateFieldType(typeDef, fieldDef.ValueType, isKeyField);

      if (isKeyField && fieldDef.IsNullable)
        context.ModelInspectionResult.Register(new MarkFieldAsNotNullableAction(typeDef, fieldDef));
      
      if (fieldDef.IsPrimitive) {
        if (fieldDef.ValueType==typeof (Key) && !fieldDef.IsNotIndexed)
          context.ModelInspectionResult.Register(new AddForeignKeyIndexAction(typeDef, fieldDef));
        return;
      }

      if (fieldDef.IsStructure) {
        Validator.ValidateStructureField(typeDef, fieldDef);
        context.DependencyGraph.AddEdge(typeDef, GetTypeDef(fieldDef.ValueType), EdgeKind.Aggregation, EdgeWeight.High);
        return;
      }

      if (fieldDef.IsEntity) {
        if (!fieldDef.IsNotIndexed)
          context.ModelInspectionResult.Register(new AddForeignKeyIndexAction(typeDef, fieldDef));
      }
      else
        Validator.ValidateEntitySetField(typeDef, fieldDef);

      var referencedType = fieldDef.IsEntitySet ? fieldDef.ItemType : fieldDef.ValueType;
      var referencedTypeDef = GetTypeDef(referencedType);

      if (!referencedTypeDef.IsInterface) {
        var hierarchyDef = context.ModelDef.FindHierarchy(referencedTypeDef);
        if (hierarchyDef==null)
          throw new DomainBuilderException(string.Format(Strings.ExHierarchyIsNotFoundForTypeX, referencedType.GetShortName()));
      }
      context.DependencyGraph.AddEdge(typeDef, referencedTypeDef, EdgeKind.Reference, isKeyField ? EdgeWeight.High : EdgeWeight.Low);
    }


    #endregion

    #region Helper members

    private static TypeDef GetTypeDef(Type type)
    {
      var context = BuildingContext.Demand();
      var result = context.ModelDef.Types.TryGetValue(type);
      if (result!=null)
        return result;
      // Do not register requested type automatically
//      if (type.IsGenericType) {
//        var genericDef = type.GetGenericTypeDefinition();
//        var alreadyRegisteredForGeneration = context.ModelInspectionResult.Actions
//          .OfType<BuildGenericTypeInstancesAction>()
//          .Any(action => action.Type.UnderlyingType == genericDef);
//        if (alreadyRegisteredForGeneration)
//          return ModelDefBuilder.ProcessType(type);
//      }

      throw new DomainBuilderException(
        String.Format(Strings.ExTypeXIsNotRegisteredInTheModel, type.GetFullName()));
    }

    #endregion
  }
}