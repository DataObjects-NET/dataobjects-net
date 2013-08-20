// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

using Mono.Cecil;
using Xtensive.Orm.Weaver.Tasks;

namespace Xtensive.Orm.Weaver.Inspections
{
  internal sealed class EntityInspector : Inspector
  {
    public override ActionResult Execute(ProcessorContext context)
    {
      foreach (var type in context.TargetModule.Types) {
        var baseType = type.BaseType;
        if (baseType!=null && baseType.Namespace==WellKnown.OrmNamespace && baseType.Name==WellKnown.EntityTypeName)
          ProcessEntity(context, type);
      }

      return ActionResult.Success;
    }

    private void ProcessEntity(ProcessorContext context, TypeDefinition type)
    {
      var references = context.References;
      var signatures = new[] {
        new[] {references.EntityState},
        new[] {references.Session, references.EntityState},
        new[] {references.SerializationInfo, references.StreamingContext},
      };

      foreach (var signature in signatures)
        context.WeavingTasks.Add(new AddFactoryMethodAndConstructorTask(type, signature));
    }
  }
}