// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.02.24

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Building.DependencyGraph;
using Xtensive.Storage.Building.FixupActions;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Building
{
  [Serializable]
  internal static class ModelInspector
  {
    public static void Run()
    {
      var context = BuildingContext.Current;
      using (Log.InfoRegion("Inspecting initial model definition")) {
        foreach (var hierarchy in context.ModelDef.Hierarchies)
          Inspect(hierarchy);
        foreach (var typeDef in context.ModelDef.Types)
          Inspect(typeDef);
      }
    }

    public static void Inspect(HierarchyDef hierarchyDef)
    {
      var context = BuildingContext.Current;
      var root = hierarchyDef.Root;
      Log.Info("Inspecting hierarchy '{0}'", root.Name);
      Validator.EnsureHierarchyIsValid(hierarchyDef);

      // Check the presence of TypeId field
      FieldDef typeIdField = root.Fields[WellKnown.TypeIdFieldName];
      if (typeIdField==null)
        context.ModelInspectionResult.Actions.Enqueue(new AddTypeIdFieldAction(root));
      else
        context.ModelInspectionResult.Actions.Enqueue(new MarkFieldAsSystemAction(root, typeIdField));

      // Should TypeId field be added to key fields?
      if (hierarchyDef.IncludeTypeId && hierarchyDef.KeyFields.Find(f => f.Name == WellKnown.TypeIdFieldName) == null)
        context.ModelInspectionResult.Register(new AddTypeIdToKeyFieldsAction(hierarchyDef));

      context.ModelInspectionResult.Actions.Enqueue(new ReorderFieldsAction(hierarchyDef));

      foreach (var keyField in hierarchyDef.KeyFields) {
        FieldDef field = root.Fields[keyField.Name];
        InspectField(root, field, true);
      }

      context.ModelInspectionResult.Actions.Enqueue(new AddPrimaryIndexAction(hierarchyDef));
    }

    public static void Inspect(TypeDef typeDef)
    {
      var context = BuildingContext.Current;
      Log.Info("Inspecting type '{0}'", typeDef.Name);

      if (typeDef.IsInterface) {
        if (typeDef.UnderlyingType==typeof (IEntity))
          context.ModelInspectionResult.Register(new RemoveTypeAction(typeDef));
        else
          InspectBaseInterfaces(typeDef);
      }
      else {
        var fields = new List<FieldDef>(typeDef.Fields);
        if (typeDef.IsEntity) {
          Validator.EnsureUnderlyingTypeIsAspected(typeDef);
          if (typeDef.IsGenericTypeDefinition) {
            context.ModelInspectionResult.Register(new BuildGenericTypeInstancesAction(typeDef));
            context.ModelInspectionResult.Register(new RemoveTypeAction(typeDef));
            return;
          }
          HierarchyDef hierarchyDef = context.ModelDef.FindHierarchy(typeDef);
          if (hierarchyDef == null) {
            context.ModelInspectionResult.Register(new RemoveTypeAction(typeDef));
            return;
          }
          // We must skip key fields as they have been inspected already
          foreach (var keyField in hierarchyDef.KeyFields) {
            KeyField field = keyField;
            fields.RemoveAll(f => f.Name==field.Name);
          }
        }

        InspectAncestor(typeDef);
        foreach (var field in fields)
          InspectField(typeDef, field, false);
        InspectInterfaces(typeDef);
      }
    }

    #region Private members

    private static void InspectInterfaces(TypeDef implementor)
    {
      var context = BuildingContext.Current;

      Type[] interfaces = implementor.UnderlyingType.GetInterfaces();
      foreach (var item in interfaces) {
        var @interface = context.ModelDef.Types.TryGetValue(item);
        if (@interface!=null)
          RegisterDependency(@interface, implementor, EdgeKind.Implementation, EdgeWeight.High);
      }
    }

    private static void InspectField(TypeDef typeDef, FieldDef fieldDef, bool isKeyField)
    {
      var context = BuildingContext.Current;
//      if (fieldDef.UnderlyingProperty != null &&
//        fieldDef.UnderlyingProperty.DeclaringType.Assembly == Assembly.GetExecutingAssembly())
//        context.ModelInspectionResult.Actions.Enqueue(new MarkFieldAsSystemAction(typeDef, fieldDef));

      if (isKeyField) {
        if (fieldDef.IsNullable)
          context.ModelInspectionResult.Register(new MarkFieldAsNotNullableAction(typeDef, fieldDef));

        if (fieldDef.IsLazyLoad)
          throw new DomainBuilderException(string.Format(Strings.ExFieldXCanTBeLoadOnDemandAsItIsIncludedInPrimaryKey, fieldDef.Name));
      }

      if (fieldDef.IsPrimitive)
        return;

      if (fieldDef.IsStructure) {
        if (fieldDef.ValueType==typeDef.UnderlyingType)
          throw new DomainBuilderException(string.Format("Structure '{0}' can't contain field of the same type.", typeDef.Name));
        RegisterDependency(typeDef, FindTypeDef(fieldDef.ValueType), EdgeKind.Aggregation, EdgeWeight.High);
        return;
      }

      // Inspecting index for the reference field
      if (fieldDef.IsEntity) {
        IndexDef indexDef = typeDef.Indexes.Where(i => i.IsSecondary && i.KeyFields.Count==1 && i.KeyFields[0].Key==fieldDef.Name).FirstOrDefault();
        if (indexDef==null)
          context.ModelInspectionResult.Register(new AddSecondaryIndexAction(typeDef, fieldDef));
      }
      else {
        // Restriction for EntitySet properties only
        if (fieldDef.OnTargetRemove == OnRemoveAction.Cascade)
          throw new DomainBuilderException(string.Format("'{0}.{1}': '{2}' value is not acceptable for 'OnTargetRemove' property.", typeDef.Name, fieldDef.Name, fieldDef.OnTargetRemove));
      }

      Type referencedType = fieldDef.IsEntitySet ? fieldDef.ItemType : fieldDef.ValueType;
      TypeDef referencedTypeDef = FindTypeDef(referencedType);
//      TypeDef headDef = context.ModelDef.FindRoot(referencedTypeDef);
      RegisterDependency(typeDef, referencedTypeDef, EdgeKind.Reference, isKeyField ? EdgeWeight.High : EdgeWeight.Low);
    }

    private static void InspectAncestor(TypeDef descendant)
    {
      var context = BuildingContext.Current;

      TypeDef parent = context.ModelDef.Types.TryGetValue(descendant.UnderlyingType.BaseType);
      if (parent!=null)
        RegisterDependency(descendant, parent, EdgeKind.Inheritance, EdgeWeight.High);
    }

    private static void InspectBaseInterfaces(TypeDef typeDef)
    {
      var context = BuildingContext.Current;

      Type[] interfaces = typeDef.UnderlyingType.GetInterfaces();
      foreach (var @interface in interfaces) {
        var @base = context.ModelDef.Types.TryGetValue(@interface);
        if (@base != null)
          RegisterDependency(typeDef, @base, EdgeKind.Inheritance, EdgeWeight.High);
      }
    }

    #endregion

    #region Helper members

    private static TypeDef FindTypeDef(Type type)
    {
      var context = BuildingContext.Current;
      TypeDef result = context.ModelDef.Types.TryGetValue(type);
      if (result == null)
        throw new DomainBuilderException(
          String.Format(Strings.ExTypeXIsNotRegisteredInTheModel, type.GetFullName()));
      return result;
    }

    private static void RegisterDependency(TypeDef tail, TypeDef head, EdgeKind kind, EdgeWeight weight)
    {
      var edge = new Edge(tail, head, kind, weight);
      tail.OutgoingEdges.Add(edge);
      head.IncomingEdges.Add(edge);
    }

    #endregion
  }
}