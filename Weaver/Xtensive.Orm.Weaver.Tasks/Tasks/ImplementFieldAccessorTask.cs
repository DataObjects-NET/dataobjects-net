// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Xtensive.Orm.Weaver.Tasks
{
  internal sealed class ImplementFieldAccessorTask : WeavingTask
  {
    private readonly TypeDefinition type;
    private readonly PropertyDefinition property;
    private readonly string persistentName;
    private readonly AccessorKind kind;

    public override ActionResult Execute(ProcessorContext context)
    {
      switch (kind) {
      case AccessorKind.Getter:
        ImplementGetter(context);
        break;
      case AccessorKind.Setter:
        ImplementSetter(context);
        break;
      default:
        throw new ArgumentOutOfRangeException();
      }

      return ActionResult.Success;
    }

    private void ImplementSetter(ProcessorContext context)
    {
      var accessor = GetAccessor(context,
        context.References.PersistentSetters, context.References.PersistentSetterDefinition);
      var body = property.SetMethod.Body;
      body.Instructions.Clear();
      var il = body.GetILProcessor();
      il.Emit(OpCodes.Ldarg_0);
      il.Emit(OpCodes.Ldstr, persistentName);
      il.Emit(OpCodes.Ldarg_1);
      il.Emit(OpCodes.Call, accessor);
      il.Emit(OpCodes.Ret);
    }

    private void ImplementGetter(ProcessorContext context)
    {
      var accessor = GetAccessor(context,
        context.References.PersistentGetters, context.References.PersistentGetterDefinition);
      var body = property.GetMethod.Body;
      body.Instructions.Clear();
      var il = body.GetILProcessor();
      il.Emit(OpCodes.Ldarg_0);
      il.Emit(OpCodes.Ldstr, persistentName);
      il.Emit(OpCodes.Call, accessor);
      il.Emit(OpCodes.Ret);
    }

    private MethodReference GetAccessor(ProcessorContext context,
      IDictionary<TypeIdentity, MethodReference> registry, MethodReference definition)
    {
      var identity = new TypeIdentity(property.PropertyType);
      MethodReference result;
      if (!registry.TryGetValue(identity, out result)) {
        var accessor = new GenericInstanceMethod(definition);
        accessor.GenericArguments.Add(property.PropertyType);
        result = context.TargetModule.Import(accessor);
        registry.Add(identity, result);
      }
      return result;
    }

    public ImplementFieldAccessorTask(AccessorKind kind, TypeDefinition type, PropertyDefinition property, string persistentName)
    {
      if (type==null)
        throw new ArgumentNullException("type");
      if (property==null)
        throw new ArgumentNullException("property");

      this.kind = kind;
      this.type = type;
      this.property = property;
      this.persistentName = persistentName;
    }
  }
}