// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.21

using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Xtensive.Orm.Weaver.Tasks;

namespace Xtensive.Orm.Weaver.Stages
{
  internal sealed class ModifyPersistentTypesStage : ProcessorStage
  {
    private IPersistentPropertyChecker propertyChecker;

    private TypeReference[][] entityFactorySignatures;
    private TypeReference[][] structureFactorySignatures;
    private TypeReference[][] entitySetFactorySignatures;

    public override ActionResult Execute(ProcessorContext context)
    {
      var references = context.References;

      entityFactorySignatures = new[] {
        new[] {references.SerializationInfo, references.StreamingContext},
        new[] {references.Session, references.EntityState},
      };

      structureFactorySignatures = new[] {
        new[] {references.SerializationInfo, references.StreamingContext},
        new[] {references.Session, references.Tuple},
        new[] {references.Persistent, references.FieldInfo},
      };

      entitySetFactorySignatures = new[] {
        new[] {references.SerializationInfo, references.StreamingContext},
        new[] {references.Entity, references.FieldInfo},
      };

      propertyChecker = (context.Language==SourceLanguage.CSharp)
        ? (IPersistentPropertyChecker) new CsPropertyChecker()
        : new VbPropertyChecker();

      foreach (var type in context.PersistentTypes)
        switch (type.Kind) {
        case PersistentTypeKind.Entity:
          ProcessEntity(context, type);
          break;
        case PersistentTypeKind.EntitySet:
          ProcessEntitySet(context, type);
          break;
        case PersistentTypeKind.EntityInterface:
          ProcessEntityInterface(context, type);
          break;
        case PersistentTypeKind.Structure:
          ProcessStructure(context, type);
          break;
        }

      if (propertyChecker.HasSkippedProperties) {
        context.Logger.Write(MessageCode.ErrorPersistentPropertiesWereNotProcessed);
        return ActionResult.Failure;
      }

      return ActionResult.Success;
    }

    private void ProcessEntityInterface(ProcessorContext context, TypeInfo type)
    {
      context.WeavingTasks.Add(new AddAttributeTask(type.Definition, context.References.EntityInterfaceAttributeConstructor));
    }

    private void ProcessEntitySet(ProcessorContext context, TypeInfo type)
    {
      var definition = type.Definition;

      foreach (var signature in entitySetFactorySignatures)
        context.WeavingTasks.Add(new ImplementFactoryTask(definition, signature));

      context.WeavingTasks.Add(new AddAttributeTask(definition, context.References.EntitySetTypeAttributeConstructor));
    }

    private void ProcessEntity(ProcessorContext context, TypeInfo type)
    {
      var definition = type.Definition;

      foreach (var signature in entityFactorySignatures)
        context.WeavingTasks.Add(new ImplementFactoryTask(definition, signature));

      ProcessFields(context, type);
      ProcessConstructors(context, type);

      context.WeavingTasks.Add(new AddAttributeTask(definition, context.References.EntityTypeAttributeConstructor));
    }

    private void ProcessStructure(ProcessorContext context, TypeInfo type)
    {
      var definition = type.Definition;

      foreach (var signature in structureFactorySignatures)
        context.WeavingTasks.Add(new ImplementFactoryTask(definition, signature));

      ProcessFields(context, type);
      ProcessConstructors(context, type);

      context.WeavingTasks.Add(new AddAttributeTask(definition, context.References.StructureTypeAttributeConstructor));
    }

    private Dictionary<PropertyInfo, int> GetPropertyToIndexMap(TypeInfo type)
    {
      if (type is null) {
        return new();
      }
      var r = GetPropertyToIndexMap(type.BaseType);
      int idx = r.Count == 0 ? 0 : r.Values.Max() + 1;
      if (idx == 0 && type.Kind == PersistentTypeKind.Entity) {
        idx = 1;   // for TypeId
      }
      foreach (var p in type.Properties.Values.Where(p => p.IsPersistent)
          .OrderBy(p => p.Definition.MetadataToken.ToInt32())) {
        r[p] = (p.IsOverride && p.BaseProperty.IsPersistent)
          ? r[p.BaseProperty]               // For overridden persistent property assign base property's index
          : idx++;
      }
      return r;
    }

    private void ProcessFields(ProcessorContext context, TypeInfo type)
    {
      var typeDefinition = type.Definition;
      var propertyToIndex = GetPropertyToIndexMap(type);

      foreach (var property in type.Properties.Values.Where(p => p.IsPersistent && propertyChecker.ShouldProcess(p, context))) {
        var persistentIndex = propertyToIndex[property];
        var propertyDefinition = property.Definition;
        // Backing field
        context.WeavingTasks.Add(new RemoveBackingFieldTask(typeDefinition, propertyDefinition));
        // Getter
        context.WeavingTasks.Add(new ImplementFieldAccessorTask(AccessorKind.Getter,
          typeDefinition, propertyDefinition, persistentIndex));
        // Setter
        if (property.IsKey)
          context.WeavingTasks.Add(new ImplementKeySetterTask(typeDefinition, propertyDefinition));
        else
          context.WeavingTasks.Add(new ImplementFieldAccessorTask(AccessorKind.Setter,
            typeDefinition, propertyDefinition, persistentIndex));
        if (property.PersistentName!=null)
          context.WeavingTasks.Add(new AddAttributeTask(propertyDefinition,
            context.References.OverrideFieldNameAttributeConstructor, property.PersistentName));
      }
    }

    private static void ProcessConstructors(ProcessorContext context, TypeInfo type)
    {
      var userConstructors = type.Definition.Methods
        .Where(m => m.IsConstructor && !m.IsStatic && !m.HasAttribute(WellKnown.CompilerGeneratedAttribute));
      foreach (var constructor in userConstructors)
        context.WeavingTasks.Add(new ImplementInitializablePatternTask(type.Definition, constructor));
    }
  }
}