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
      var fieldName = GetBackingFieldName();
      if (fieldName!=null && type.Fields.Remove(fieldName))
        return ActionResult.Success;
      context.Logger.Write(MessageCode.ErrorUnableToRemoveBackingField,
        $"type: {type.FullName}, property: {property.Name}, field: {fieldName}");
      return ActionResult.Failure;
    }

    private string GetBackingFieldName()
    {
      foreach (var instruction in property.GetMethod.Body.Instructions) {
        if (instruction.OpCode.Code==Code.Ldfld) {
          var field = (FieldReference) instruction.Operand;
          return field.Name;
        }
      }
      return null;
    }

    public RemoveBackingFieldTask(TypeDefinition type, PropertyDefinition property)
    {
      if (type==null)
        throw new ArgumentNullException("type");
      if (property==null)
        throw new ArgumentNullException("property");

      this.type = type;
      this.property = property;
    }
  }
}