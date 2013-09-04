// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace Xtensive.Orm.Weaver.Stages
{
  internal sealed class FindPersistentTypesStage : ProcessorStage
  {
    private readonly Dictionary<TypeIdentity, TypeInfo> processedTypes = new Dictionary<TypeIdentity, TypeInfo>();

    public override ActionResult Execute(ProcessorContext context)
    {
      var typesToInspect = context.TargetModule.GetTypes().Where(t => t.IsClass && t.BaseType!=null || t.IsInterface);
      foreach (var type in typesToInspect) {
        var result = InspectType(context, type);
        if (result.Kind!=PersistentTypeKind.None)
          context.PersistentTypes.Add(result);
      }
      return ActionResult.Success;
    }

    private TypeInfo InspectDefinedType(ProcessorContext context, TypeIdentity identity, TypeDefinition type)
    {
      if (type.IsClass) {
        var baseType = InspectType(context, type.BaseType);
        var kind = baseType.Kind;
        var result = new TypeInfo(kind, type, baseType);
        if (kind==PersistentTypeKind.Entity || kind==PersistentTypeKind.Structure) {
          foreach (var @interface in type.Interfaces)
            InspectType(context, @interface);
          InspectProperties(result);
        }
        processedTypes.Add(identity, result);
        return result;
      }
      else {
        var isPersistent = false;
        foreach (var @interface in type.Interfaces) {
          var current = InspectType(context, @interface);
          if (current.Kind==PersistentTypeKind.EntityInterface)
            isPersistent = true;
        }
        TypeInfo result;
        if (isPersistent) {
          result = new TypeInfo(PersistentTypeKind.EntityInterface, type);
          InspectProperties(result);
        }
        else
          result = new TypeInfo(PersistentTypeKind.None);
        processedTypes.Add(identity, result);
        return result;
      }
    }

    private TypeInfo InspectType(ProcessorContext context, TypeReference type)
    {
      if (type.IsGenericInstance)
        type = type.GetElementType();

      var identity = new TypeIdentity(type);

      TypeInfo existing;
      if (processedTypes.TryGetValue(identity, out existing))
        return existing;

      if (type.Scope.MetadataScopeType==MetadataScopeType.AssemblyNameReference) {
        var reference = (AssemblyNameReference) type.Scope;
        if (context.AssemblyChecker.IsFrameworkAssembly(reference)) {
          var result = new TypeInfo(PersistentTypeKind.None);
          processedTypes.Add(identity, result);
          return result;
        }
        if (reference.FullName==WellKnown.OrmAssemblyFullName) {
          var result = new TypeInfo(ClassifyOrmType(type));
          processedTypes.Add(identity, result);
          return result;
        }
      }

      return InspectDefinedType(context, identity, type.Resolve());
    }

    private PersistentTypeKind ClassifyOrmType(TypeReference type)
    {
      var comparer = WeavingHelper.TypeNameComparer;
      var name = type.FullName;
      if (comparer.Equals(name, WellKnown.EntityType))
        return PersistentTypeKind.Entity;
      if (comparer.Equals(name, WellKnown.StructureType))
        return PersistentTypeKind.Structure;
      if (comparer.Equals(name, WellKnown.EntitySetType))
        return PersistentTypeKind.EntitySet;
      if (comparer.Equals(name, WellKnown.EntityInterface))
        return PersistentTypeKind.EntityInterface;
      return PersistentTypeKind.None;
    }

    private PersistentTypeKind ClassifyExternalType(TypeDefinition type)
    {
      if (type.HasAttribute(WellKnown.EntityTypeAttribute))
        return PersistentTypeKind.Entity;
      if (type.HasAttribute(WellKnown.StructureTypeAttribute))
        return PersistentTypeKind.Structure;
      if (type.HasAttribute(WellKnown.EntitySetTypeAttribute))
        return PersistentTypeKind.EntitySet;
      if (type.HasAttribute(WellKnown.EntityInterfaceAttribute))
        return PersistentTypeKind.EntityInterface;
      return PersistentTypeKind.None;
    }

    private void InspectProperties(TypeInfo type)
    {
      foreach (var property in type.Definition.Properties) {
        var baseProperty = type.FindProperty(property.Name);
        var propertyInfo = new PropertyInfo(property);
        propertyInfo.IsAutomatic = property.IsAutoProperty();
        propertyInfo.IsPersistent = propertyInfo.IsInstance
          && (property.HasAttribute(WellKnown.FieldAttribute) || baseProperty!=null && baseProperty.IsPersistent);
        propertyInfo.IsKey = propertyInfo.IsPersistent
          && (property.HasAttribute(WellKnown.KeyAttribute) || baseProperty!=null && baseProperty.IsKey);
        propertyInfo.ExplicitlyImplementedInterface = property.FindExplicitlyImplementedInterface();
        type.Properties.Add(property.Name, propertyInfo);
      }
    }
  }
}