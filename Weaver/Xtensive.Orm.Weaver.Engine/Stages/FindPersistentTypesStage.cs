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
      var typesToInspect = context.TargetModule.Types.Where(t => t.IsClass && t.BaseType!=null);
      foreach (var type in typesToInspect) {
        var kind = InspectType(context, type);
        switch (kind) {
        case PersistentTypeKind.Entity:
          context.EntityTypes.Add(CreateType(type, kind));
          break;
        case PersistentTypeKind.Structure:
          context.StructureTypes.Add(CreateType(type, kind));
          break;
        }
      }
      return ActionResult.Success;
    }

    private PersistentType CreateType(TypeDefinition definition, PersistentTypeKind kind)
    {
      var allProperties = definition.Properties.Where(IsPersistentProperty).Select(CreateProperty);
      return new PersistentType(definition, kind, allProperties);
    }

    private PersistentProperty CreateProperty(PropertyDefinition definition)
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

      var thisAssembly = context.TargetModule;
      var ormAssembly = context.References.OrmAssembly;

      var baseType = type.BaseType;
      if (baseType.Scope==thisAssembly) {
        kind = InspectType(context, baseType.Resolve());
        processedTypes.Add(identity, kind);
        return kind;
      }

      if (baseType.Scope==ormAssembly) {
        kind = ClassifyOrmType(baseType);
        processedTypes.Add(identity, kind);
        return kind;
      }

      kind = InspectExternalType(baseType);
      processedTypes.Add(identity, kind);
      return kind;
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
      if (comparer.Equals(type.FullName, WellKnown.EntityType))
        return PersistentTypeKind.Entity;
      if (comparer.Equals(type.FullName, WellKnown.StructureType))
        return PersistentTypeKind.Structure;
      return PersistentTypeKind.None;
    }

    private PersistentTypeKind ClassifyExternalType(TypeDefinition type)
    {
      if (type.HasAttribute(WellKnown.EntityTypeAttribute))
        return PersistentTypeKind.Entity;
      if (type.HasAttribute(WellKnown.StructureTypeAttribute))
        return PersistentTypeKind.Structure;
      return PersistentTypeKind.None;
    }

    private bool IsPersistentProperty(PropertyDefinition property)
    {
      if (property.IsStatic() || !property.IsAutoProperty())
        return false;
      if (property.HasAttribute(WellKnown.FieldAttribute))
        return true;
      if (property.GetMethod.IsVirtual || property.GetMethod.Overrides.Count > 0) {
        // Inspect declaring type
      }
    }
  }
}