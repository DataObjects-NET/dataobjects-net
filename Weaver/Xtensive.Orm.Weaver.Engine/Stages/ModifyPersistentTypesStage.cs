// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.21

using Mono.Cecil;
using Xtensive.Orm.Weaver.Tasks;

namespace Xtensive.Orm.Weaver.Stages
{
  internal sealed class ModifyPersistentTypesStage : ProcessorStage
  {
    public override ActionResult Execute(ProcessorContext context)
    {
      foreach (var type in context.EntityTypes)
        ProcessEntity(context, type);

      foreach (var type in context.StructureTypes)
        ProcessStructure(context, type);

      return ActionResult.Success;
    }

    private void ProcessEntity(ProcessorContext context, TypeDefinition type)
    {
      context.WeavingTasks.Add(new AddAttributeTask(type, context.References.EntityTypeAttributeConstructor));

      var references = context.References;
      var signatures = new[] {
        new[] {references.EntityState},
        new[] {references.Session, references.EntityState},
        new[] {references.SerializationInfo, references.StreamingContext},
      };

      foreach (var signature in signatures)
        context.WeavingTasks.Add(new AddFactoryTask(type, signature));

      ProcessFields(context, type);
    }

    private void ProcessStructure(ProcessorContext context, TypeDefinition type)
    {
      context.WeavingTasks.Add(new AddAttributeTask(type, context.References.StructureTypeAttributeConstructor));

      var references = context.References;
      var signatures = new[] {
        new[] {references.Tuple},
        new[] {references.Session, references.Tuple},
        new[] {references.Persistent, references.FieldInfo},
        new[] {references.SerializationInfo, references.StreamingContext},
      };

      foreach (var signature in signatures)
        context.WeavingTasks.Add(new AddFactoryTask(type, signature));

      ProcessFields(context, type);
    }

    private void ProcessFields(ProcessorContext context, TypeDefinition type)
    {
    }
  }
}