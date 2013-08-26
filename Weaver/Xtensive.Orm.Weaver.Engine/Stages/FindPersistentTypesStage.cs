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
    private readonly Dictionary<TypeIdentity, PersistentTypeKind> processedTypes = new Dictionary<TypeIdentity, PersistentTypeKind>();
    private readonly Dictionary<TypeIdentity, PersistentTypeKind> externalTypes = new Dictionary<TypeIdentity, PersistentTypeKind>();

    public override ActionResult Execute(ProcessorContext context)
    {
      var typesToInspect = context.TargetModule.Types.Where(t => t.IsClass && t.BaseType!=null || t.IsInterface);
      foreach (var type in typesToInspect) {
        var kind = InspectType(context, type);
        if (kind!=PersistentTypeKind.None)
          context.PersistentTypes.Add(CreatePersistentType(type, kind));
      }
      return ActionResult.Success;
    }

    private PersistentType CreatePersistentType(TypeDefinition definition, PersistentTypeKind kind)
    {
      var allProperties = kind==PersistentTypeKind.EntitySet
        ? Enumerable.Empty<PersistentProperty>()
        : definition.Properties.Where(IsPersistentProperty).Select(CreatePersistentProperty);
      return new PersistentType(definition, kind, allProperties);
    }

    private PersistentProperty CreatePersistentProperty(PropertyDefinition definition)
    {
      var result = new PersistentProperty(definition);
      result.IsKey = definition.HasAttribute(WellKnown.KeyAttribute);
      var overriddenGetter = definition.GetMethod.Overrides.FirstOrDefault();
      if (overriddenGetter!=null)
        result.ExplicitlyImplementedInterface = overriddenGetter.DeclaringType;
      return result;
    }

    private PersistentTypeKind InspectType(ProcessorContext context, TypeDefinition type)
    {
      var identity = new TypeIdentity(type);

      PersistentTypeKind kind;
      if (processedTypes.TryGetValue(identity, out kind))
        return kind;

      if (type.IsClass) {
        kind = ClassifyType(context, type.BaseType);
        processedTypes.Add(identity, kind);
        return kind;
      }

      kind = PersistentTypeKind.None;
      foreach (var baseInterface in type.Interfaces) {
        kind = ClassifyType(context, baseInterface);
        if (kind!=PersistentTypeKind.None)
          break;
      }
      processedTypes.Add(identity, kind);
      return kind;
    }

    private PersistentTypeKind ClassifyType(ProcessorContext context, TypeReference type)
    {
      if (type.IsGenericInstance)
        type = type.GetElementType();

      var thisAssembly = context.TargetModule;
      var ormAssembly = context.References.OrmAssembly;

      if (type.Scope==thisAssembly)
        return InspectType(context, type.Resolve());

      if (type.Scope==ormAssembly)
        return ClassifyOrmType(type);

      return InspectExternalType(type);
    }

    private PersistentTypeKind InspectExternalType(TypeReference type)
    {
      var identity = new TypeIdentity(type);
      PersistentTypeKind kind;
      if (!externalTypes.TryGetValue(identity, out kind)) {
        kind = ClassifyExternalType(type.Resolve());
        externalTypes.Add(identity, kind);
      }
      return kind;
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

    private bool IsPersistentProperty(PropertyDefinition property)
    {
      return !property.IsStatic() && property.IsAutoProperty() && property.HasAttribute(WellKnown.FieldAttribute);
    }
  }
}