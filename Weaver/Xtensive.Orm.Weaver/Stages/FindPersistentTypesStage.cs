// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

using System;
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
        var result = new TypeInfo(type, kind, baseType);
        if (kind==PersistentTypeKind.Entity || kind==PersistentTypeKind.Structure) {
          foreach (var @interface in type.Interfaces)
            result.Interfaces.Add(InspectType(context, @interface));
          InspectProperties(result);
        }
        processedTypes.Add(identity, result);
        return result;
      }
      else {
        var isPersistent = false;
        var interfaces = new List<TypeInfo>();
        foreach (var @interface in type.Interfaces) {
          var current = InspectType(context, @interface);
          interfaces.Add(current);
          if (current.Kind==PersistentTypeKind.EntityInterface)
            isPersistent = true;
        }
        var kind = isPersistent ? PersistentTypeKind.EntityInterface : PersistentTypeKind.None;
        var result = new TypeInfo(type, kind) {Interfaces = interfaces};
        InspectProperties(result);
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
          var result = new TypeInfo(type, PersistentTypeKind.None);
          processedTypes.Add(identity, result);
          return result;
        }
        if (reference.FullName==WellKnown.OrmAssemblyFullName) {
          var result = new TypeInfo(type, ClassifyOrmType(type));
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
        var propertyInfo = new PropertyInfo(type, property);
        if (propertyInfo.AnyAccessor==null)
          continue;
        propertyInfo.IsAutomatic = property.IsAutoProperty();
        propertyInfo.IsPersistent = propertyInfo.IsInstance && property.HasAttribute(WellKnown.FieldAttribute);
        propertyInfo.IsKey = propertyInfo.IsPersistent && property.HasAttribute(WellKnown.KeyAttribute);
        type.Properties.Add(property.Name, propertyInfo);
      }

      var baseType = type.BaseType;
      if (baseType!=null)
        foreach (var property in type.Properties.Values) {
          var baseProperty = baseType.FindProperty(property.Name);
          if (baseProperty==null)
            continue;
          property.BaseProperty = baseProperty;
          var thisAccessor = property.AnyAccessor;
          var baseAccessor = baseProperty.AnyAccessor;
          if (baseAccessor.IsVirtual && thisAccessor.IsVirtual && !thisAccessor.IsNewSlot) {
            property.IsOverride = true;
            InheritPersistence(property, baseProperty);
          }
          else {
            property.IsNew = true;
            var persistentBase = property.FindBase(p => p.IsPersistent);
            if (persistentBase!=null)
              property.PersistentName = WeavingHelper.BuildComplexPersistentName(type, property);
          }
        }

      var propertiesToImplement = new HashSet<PropertyInfo>(type.Interfaces.SelectMany(i => i.Properties.Values));

      // Exclude explicit implementations
      foreach (var property in type.Properties.Values) {
        var implementedAccessor = property.AnyAccessor.Overrides.FirstOrDefault();
        if (implementedAccessor==null)
          continue;
        var interfaceType = GetType(new TypeIdentity(implementedAccessor.DeclaringType));
        var implementedPropertyName = WeavingHelper.GetPropertyName(implementedAccessor.Name);
        var implementedProperty = interfaceType.FindProperty(implementedPropertyName);
        if (implementedProperty==null)
          continue;
        property.ImplementedProperties.Add(implementedProperty);
        property.IsExplicitInterfaceImplementation = true;
        property.PersistentName = WeavingHelper.BuildComplexPersistentName(interfaceType, implementedProperty);
        propertiesToImplement.Remove(implementedProperty);
        InheritPersistence(property, implementedProperty);
      }

      // Try associate remaining properties
      foreach (var property in propertiesToImplement) {
        PropertyInfo implementor;
        if (type.Properties.TryGetValue(property.Name, out implementor)) {
          implementor.ImplementedProperties.Add(property);
          InheritPersistence(implementor, property);
        }
      }
    }

    private TypeInfo GetType(TypeIdentity identity)
    {
      TypeInfo result;
      if (!processedTypes.TryGetValue(identity, out result))
        throw new InvalidOperationException(string.Format("Unable to find type '{0}'", identity.TypeName));
      return result;
    }

    private static void InheritPersistence(PropertyInfo property, PropertyInfo baseProperty)
    {
      if (baseProperty.IsPersistent)
        property.IsPersistent = true;
      if (baseProperty.IsKey)
        property.IsKey = true;
    }
  }
}