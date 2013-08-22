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
  internal sealed class ImplementFieldAccessorTask : WeavingTask
  {
    private readonly TypeDefinition type;
    private readonly PropertyDefinition property;
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
      var body = property.SetMethod.Body;
      body.Instructions.Clear();
      var il = body.GetILProcessor();
      il.Emit(OpCodes.Ldarg_0);
      il.Emit(OpCodes.Ldstr, GetPropertyName());
      il.Emit(OpCodes.Ldarg_1);
      il.Emit(OpCodes.Call, GetSetter(context));
      il.Emit(OpCodes.Ret);
    }

    private void ImplementGetter(ProcessorContext context)
    {
      var body = property.GetMethod.Body;
      body.Instructions.Clear();
      var il = body.GetILProcessor();
      il.Emit(OpCodes.Ldarg_0);
      il.Emit(OpCodes.Ldstr, GetPropertyName());
      il.Emit(OpCodes.Call, GetGetter(context));
      il.Emit(OpCodes.Ret);
    }

    private string GetPropertyName()
    {
      // TODO: handle explicit interface implementations
      return property.Name;
    }

    private GenericInstanceMethod GetGetter(ProcessorContext context)
    {
      var getterRegistry = context.References.PersistentGetters;
      var identity = new TypeIdentity(property.PropertyType);
      GenericInstanceMethod result;
      if (!getterRegistry.TryGetValue(identity, out result)) {
        result = new GenericInstanceMethod(context.References.PersistentGetterDefinition);
        result.GenericArguments.Add(property.PropertyType);
        getterRegistry.Add(identity, result);
      }
      return result;
    }

    private GenericInstanceMethod GetSetter(ProcessorContext context)
    {
      var setterRegistry = context.References.PersistentSetters;
      var identity = new TypeIdentity(property.PropertyType);
      GenericInstanceMethod result;
      if (!setterRegistry.TryGetValue(identity, out result)) {
        result = new GenericInstanceMethod(context.References.PersistentSetterDefinition);
        result.GenericArguments.Add(property.PropertyType);
        setterRegistry.Add(identity, result);
      }
      return result;
    }

    public ImplementFieldAccessorTask(TypeDefinition type, PropertyDefinition property, AccessorKind kind)
    {
      if (type==null)
        throw new ArgumentNullException("type");
      if (property==null)
        throw new ArgumentNullException("property");

      this.type = type;
      this.property = property;
      this.kind = kind;
    }
  }
}