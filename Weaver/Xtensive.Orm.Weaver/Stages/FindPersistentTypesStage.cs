// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;

namespace Xtensive.Orm.Weaver.Stages
{
  internal sealed class FindPersistentTypesStage : ProcessorStage
  {
    private readonly Dictionary<TypeIdentity, TypeInfo> processedTypes = new Dictionary<TypeIdentity, TypeInfo>();
    private Func<TypeDefinition, PropertyDefinition, bool> autoPropertyChecker;

    public override ActionResult Execute(ProcessorContext context)
    {
      if (context.Language==SourceLanguage.VbNet)
        autoPropertyChecker = IsVbAutoProperty;
      else
        autoPropertyChecker = IsCSharpAutoProperty;

      var typesToInspect = context.TargetModule.GetTypes()
        .Where(t => t.IsClass && t.BaseType!=null || t.IsInterface);
      foreach (var type in typesToInspect) {
        var result = InspectType(context, type);
        if (result.Kind!=PersistentTypeKind.None)
          context.PersistentTypes.Add(result);
      }

      context.SkipProcessing = context.PersistentTypes.Count==0;
      return ActionResult.Success;
    }

    private TypeInfo InspectDefinedType(ProcessorContext context, TypeIdentity identity, TypeDefinition type)
    {
      if (type.IsClass) {
        var baseType = InspectType(context, type.BaseType);
        var kind = baseType.Kind;
        var result = new TypeInfo(type, kind, baseType);
        var expectPersistentProperties = kind==PersistentTypeKind.Entity || kind==PersistentTypeKind.Structure;
        if (expectPersistentProperties)
          foreach (var @interface in type.Interfaces)
            result.Interfaces.Add(InspectType(context, @interface));
        processedTypes.Add(identity, result);
        if (expectPersistentProperties)
          InspectProperties(context, result);
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
        processedTypes.Add(identity, result);
        InspectProperties(context, result);
        return result;
      }
    }

    private TypeInfo InspectType(ProcessorContext context, TypeReference type)
    {
      type = type.StripGenericParameters();

      var identity = new TypeIdentity(type);

      TypeInfo existing;
      if (processedTypes.TryGetValue(identity, out existing))
        return existing;

      AssemblyNameReference reference;
      if (type.Scope.MetadataScopeType==MetadataScopeType.AssemblyNameReference)
        reference = (AssemblyNameReference) type.Scope;
      else if (type.Scope.MetadataScopeType==MetadataScopeType.ModuleDefinition) {
        var defenition = (ModuleDefinition) type.Scope;
        reference = defenition.Assembly.Name;
      }
      else {
        // If type.Scope.MetadataScoreType==MetadataScoreType.ModuleReference
        // then unable to understand which assembly is it, 
        // because ModuleReference contains only name of Module.
        throw new InvalidOperationException("Unable to inspect ModuleReference");
      }
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
      if (comparer.Equals(name, WellKnown.EntityInterfaceType))
        return PersistentTypeKind.EntityInterface;
      return PersistentTypeKind.None;
    }

    private void InspectProperties(ProcessorContext context, TypeInfo type)
    {
      foreach (var property in type.Definition.Properties) {
        var propertyInfo = new PropertyInfo(type, property);
        if (propertyInfo.AnyAccessor==null)
          continue;
        propertyInfo.IsAutomatic = autoPropertyChecker.Invoke(type.Definition, property);
        propertyInfo.IsPersistent = propertyInfo.IsInstance && property.HasAttribute(WellKnown.FieldAttribute);
        propertyInfo.IsKey = propertyInfo.IsPersistent && property.HasAttribute(WellKnown.KeyAttribute);
        type.Properties.Add(propertyInfo.PropertySignatureName, propertyInfo);
      }

      var baseType = type.BaseType;
      if (baseType!=null)
        foreach (var property in type.Properties.Values) {
          var baseProperty = baseType.FindProperty(property.PropertySignatureName);
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
        var interfaceType = GetType(new TypeIdentity(implementedAccessor.DeclaringType.StripGenericParameters()));
        var implementedPropertySignature = WeavingHelper.GetPropertySignatureName(implementedAccessor);
        var implementedProperty = interfaceType.FindProperty(implementedPropertySignature);
        if (implementedProperty==null)
          continue;
        property.ImplementedProperties.Add(implementedProperty);
        property.IsExplicitInterfaceImplementation = true;
        if (context.Language!=SourceLanguage.VbNet)
          property.PersistentName = WeavingHelper.BuildComplexPersistentName(interfaceType, implementedProperty);
        propertiesToImplement.Remove(implementedProperty);
        InheritPersistence(property, implementedProperty);
      }

      // Try associate remaining properties
      foreach (var property in propertiesToImplement) {
        PropertyInfo implementor;
        if (type.Properties.TryGetValue(property.PropertySignatureName, out implementor)) {
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

    private static bool IsCSharpAutoProperty(TypeDefinition type, PropertyDefinition property)
    {
      return property.GetMethod!=null && property.SetMethod!=null
        && property.GetMethod.HasAttribute(WellKnown.CompilerGeneratedAttribute)
        && property.SetMethod.HasAttribute(WellKnown.CompilerGeneratedAttribute);
    }

    private static bool IsVbAutoProperty(TypeDefinition type, PropertyDefinition property)
    {
      if (property.GetMethod==null || property.SetMethod==null)
        return false;
      var backingFieldIndex = type.Fields.IndexOf("_" + property.Name);
      if (backingFieldIndex < 0)
        return false;
      var backingField = type.Fields[backingFieldIndex];
      return backingField.IsPrivate;
    }
  }
}