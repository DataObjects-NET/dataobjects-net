// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.21

using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Xtensive.Orm.Weaver.Tasks
{
  internal sealed class RemoveBackingFieldTask : WeavingTask
  {
    private readonly TypeDefinition type;
    private readonly PropertyDefinition property;

    public override ActionResult Execute(ProcessorContext context)
    {
      var fieldName =  property?.GetBackingField()?.Name;
      if (fieldName is not null && type.Fields.Remove(fieldName))
        return ActionResult.Success;
      context.Logger.Write(MessageCode.ErrorUnableToRemoveBackingField,
        $"type: {type.FullName}, property: {property.Name}, field: {fieldName}");
      return ActionResult.Failure;
    }

    public RemoveBackingFieldTask(TypeDefinition type, PropertyDefinition property)
    {
      this.type = type ?? throw new ArgumentNullException(nameof(type));
      this.property = property ?? throw new ArgumentNullException(nameof(property));
    }
  }
}