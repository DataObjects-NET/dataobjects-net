// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.21

using System.Linq;
using Mono.Cecil;
using Xtensive.Orm.Weaver.Tasks;

namespace Xtensive.Orm.Weaver.Stages
{
  internal sealed class ModifyPersistentTypesStage : ProcessorStage
  {
    private TypeReference[][] entityFactorySignatures;
    private TypeReference[][] structureFactorySignatures;

    public override ActionResult Execute(ProcessorContext context)
    {
      var references = context.References;

      entityFactorySignatures = new[] {
        new[] {references.EntityState},
        new[] {references.Session, references.EntityState},
        new[] {references.SerializationInfo, references.StreamingContext},
      };

      structureFactorySignatures = new[] {
        new[] {references.Tuple},
        new[] {references.Session, references.Tuple},
        new[] {references.Persistent, references.FieldInfo},
        new[] {references.SerializationInfo, references.StreamingContext},
      };

      foreach (var type in context.EntityTypes)
        ProcessEntity(context, type);

      foreach (var type in context.StructureTypes)
        ProcessStructure(context, type);

      return ActionResult.Success;
    }

    private void ProcessEntity(ProcessorContext context, PersistentType type)
    {
      var definition = type.Definition;
      
      foreach (var signature in entityFactorySignatures)
        context.WeavingTasks.Add(new AddFactoryTask(definition, signature));

      var userConstructors = type.Definition.Methods
        .Where(m => m.IsConstructor && !m.IsStatic && !m.HasAttribute(WellKnown.CompilerGeneratedAttribute));
      foreach (var constructor in userConstructors) {
        context.WeavingTasks.Add(new ImplementInitializablePatternTask(type.Definition, constructor));
      }

      ProcessFields(context, type);

      context.WeavingTasks.Add(new AddAttributeTask(definition, context.References.EntityTypeAttributeConstructor));
    }

    private void ProcessStructure(ProcessorContext context, PersistentType type)
    {
      var definition = type.Definition;

      foreach (var signature in structureFactorySignatures)
        context.WeavingTasks.Add(new AddFactoryTask(definition, signature));

      ProcessFields(context, type);

      context.WeavingTasks.Add(new AddAttributeTask(definition, context.References.StructureTypeAttributeConstructor));
    }

    private void ProcessFields(ProcessorContext context, PersistentType type)
    {
      foreach (var property in type.Properties) {
        var typeDefinition = type.Definition;
        var propertyDefinition = property.Definition;
        // Getter
        context.WeavingTasks.Add(new ImplementFieldAccessorTask(AccessorKind.Getter,
          typeDefinition, propertyDefinition, property.ExplicitlyImplementedInterface));
        // Setter
        if (property.IsKey)
          context.WeavingTasks.Add(new DisableKeySetterTask(typeDefinition, propertyDefinition));
        else
          context.WeavingTasks.Add(new ImplementFieldAccessorTask(AccessorKind.Setter,
            typeDefinition, propertyDefinition, property.ExplicitlyImplementedInterface));
        // Backing field
        context.WeavingTasks.Add(new RemoveBackingFieldTask(typeDefinition, propertyDefinition,
          property.ExplicitlyImplementedInterface));
      }
    }
  }
}