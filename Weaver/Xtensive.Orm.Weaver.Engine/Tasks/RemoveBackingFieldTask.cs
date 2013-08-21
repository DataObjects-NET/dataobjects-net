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
    private const string BackingFieldNameFormat = "<{0}>k__BackingField";

    public override ActionResult Execute(ProcessorContext context)
    {
      var fieldName = string.Format(BackingFieldNameFormat, property.Name);
      type.Fields.Remove(fieldName);
      return ActionResult.Success;
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