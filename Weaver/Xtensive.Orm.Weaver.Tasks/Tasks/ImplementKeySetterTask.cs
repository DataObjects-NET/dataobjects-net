// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Xtensive.Orm.Weaver.Tasks
{
  internal sealed class ImplementKeySetterTask : WeavingTask
  {
    private readonly TypeDefinition type;
    private readonly PropertyDefinition property;

    public override ActionResult Execute(ProcessorContext context)
    {
      var body = property.SetMethod.Body;
      body.Instructions.Clear();
      var il = body.GetILProcessor();
      il.Emit(OpCodes.Ldstr, type.Name);
      il.Emit(OpCodes.Ldstr, property.Name);
      il.Emit(OpCodes.Call, context.References.PersistenceImplementationHandleKeySet);
      il.Emit(OpCodes.Ret);
      return ActionResult.Success;
    }

    public ImplementKeySetterTask(TypeDefinition type, PropertyDefinition property)
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