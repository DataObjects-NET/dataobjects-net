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
    private enum TypePersistence
    {
      None,
      Entity,
      Structure,
    }

    private readonly Dictionary<TypeIdentity, TypePersistence> processedTypes = new Dictionary<TypeIdentity, TypePersistence>();
    private readonly Dictionary<TypeIdentity, TypePersistence> externalTypes = new Dictionary<TypeIdentity, TypePersistence>();

    public override ActionResult Execute(ProcessorContext context)
    {
      var typesToInspect = context.TargetModule.Types.Where(t => t.IsClass && t.BaseType!=null);
      foreach (var type in typesToInspect) {
        switch (InspectType(context, type)) {
        case TypePersistence.Entity:
          context.EntityTypes.Add(type);
          break;
        case TypePersistence.Structure:
          context.StructureTypes.Add(type);
          break;
        }
      }
      return ActionResult.Success;
    }

    private TypePersistence InspectType(ProcessorContext context, TypeDefinition type)
    {
      var identity = new TypeIdentity(type);

      TypePersistence persistence;
      if (processedTypes.TryGetValue(identity, out persistence))
        return persistence;

      var thisAssembly = context.TargetModule;
      var ormAssembly = context.References.OrmAssembly;

      var baseType = type.BaseType;
      if (baseType.Scope==thisAssembly) {
        persistence = InspectType(context, baseType.Resolve());
        processedTypes.Add(identity, persistence);
        return persistence;
      }

      if (baseType.Scope==ormAssembly) {
        persistence = ClassifyOrmType(baseType);
        processedTypes.Add(identity, persistence);
        return persistence;
      }

      persistence = InspectExternalType(baseType);
      processedTypes.Add(identity, persistence);
      return persistence;
    }

    private TypePersistence InspectExternalType(TypeReference type)
    {
      var identity = new TypeIdentity(type);
      TypePersistence persistence;
      if (!externalTypes.TryGetValue(identity, out persistence)) {
        persistence = ClassifyExternalType(type.Resolve());
        externalTypes.Add(identity, persistence);
      }
      return persistence;
    }

    private TypePersistence ClassifyOrmType(TypeReference type)
    {
      var comparer = TypeIdentity.TypeNameComparer;
      if (comparer.Equals(type.Namespace, WellKnown.OrmNamespace)) {
        if (comparer.Equals(type.Name, WellKnown.EntityType))
          return TypePersistence.Entity;
        if (comparer.Equals(type.Name, WellKnown.StructureType))
          return TypePersistence.Structure;
      }
      return TypePersistence.None;
    }

    private TypePersistence ClassifyExternalType(TypeDefinition type)
    {
      var comparer = TypeIdentity.TypeNameComparer;
      foreach (var attribute in type.CustomAttributes) {
        var attributeType = attribute.AttributeType;
        if (comparer.Equals(attributeType.Namespace, WellKnown.OrmNamespace)) {
          if (comparer.Equals(attributeType.Name, WellKnown.EntityTypeAttribute))
            return TypePersistence.Entity;
          if (comparer.Equals(attributeType.Name, WellKnown.StructureTypeAttribute))
            return TypePersistence.Structure;
        }
      }
      return TypePersistence.None;
    }
  }
}