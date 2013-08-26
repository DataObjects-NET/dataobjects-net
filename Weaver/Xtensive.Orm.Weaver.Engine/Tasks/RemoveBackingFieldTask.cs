// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.21

using System;
using Mono.Cecil;

namespace Xtensive.Orm.Weaver.Tasks
{
  internal sealed class RemoveBackingFieldTask : WeavingTask
  {
    private readonly TypeDefinition type;
    private readonly PropertyDefinition property;
    private readonly TypeReference explicitlyImplementedInterface;
    private const string BackingFieldNameFormat = "<{0}>k__BackingField";

    public override ActionResult Execute(ProcessorContext context)
    {
      var fieldName = GetBackingFieldName();
      if (type.Fields.Remove(fieldName))
        return ActionResult.Success;
      context.Logger.Write(MessageCode.ErrorUnableToRemoveBackingField,
        string.Format("type: {0}, property: {1}, field: {2}", type.FullName, property.Name, fieldName));
      return ActionResult.Failure;
    }

    private string GetBackingFieldName()
    {
      return string.Format(BackingFieldNameFormat,
        explicitlyImplementedInterface!=null ? explicitlyImplementedInterface.FullName : property.Name);
    }

    public RemoveBackingFieldTask(TypeDefinition type, PropertyDefinition property, TypeReference explicitlyImplementedInterface)
    {
      if (type==null)
        throw new ArgumentNullException("type");
      if (property==null)
        throw new ArgumentNullException("property");

      this.type = type;
      this.property = property;
      this.explicitlyImplementedInterface = explicitlyImplementedInterface;
    }
  }
}