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
  internal sealed class ImplementFactoryTask : WeavingTask
  {
    private const MethodAttributes ConstructorAttributes =
      MethodAttributes.Family
        | MethodAttributes.HideBySig
        | MethodAttributes.SpecialName
        | MethodAttributes.RTSpecialName;

    private const MethodAttributes FactoryMethodAttributes =
      MethodAttributes.Private
        | MethodAttributes.Static;

    private readonly TypeDefinition targetType;
    private readonly TypeReference[] signature;
    private TypeReference voidType;

    public override ActionResult Execute(ProcessorContext context)
    {
      voidType = context.TargetModule.TypeSystem.Void;
      var constructor = AddConstructor(context);
      AddFactoryMethod(context, constructor);
      return ActionResult.Success;
    }

    private MethodDefinition AddConstructor(ProcessorContext context)
    {
      var baseConstructor = GetBaseConstructor(context);
      var method = new MethodDefinition(WellKnown.Constructor, ConstructorAttributes, voidType) {HasThis = true};
      AddParameters(method);
      WeavingHelper.MarkAsCompilerGenerated(context, method);
      var il = method.Body.GetILProcessor();
      WeavingHelper.EmitLoadArguments(il, signature.Length + 1);
      il.Emit(OpCodes.Call, baseConstructor);
      il.Emit(OpCodes.Ret);
      targetType.Methods.Add(method);
      return method;
    }

    private void AddParameters(MethodDefinition method)
    {
      for (int i = 0; i < signature.Length; i++)
        method.Parameters.Add(new ParameterDefinition("p" + i, ParameterAttributes.In, signature[i]));
    }

    private MethodReference GetBaseConstructor(ProcessorContext context)
    {
      var reference = new MethodReference(WellKnown.Constructor, voidType, targetType.BaseType) {HasThis = true};
      foreach (var item in signature)
        reference.Parameters.Add(new ParameterDefinition(item));
      return context.TargetModule.Import(reference);
    }

    private void AddFactoryMethod(ProcessorContext context, MethodDefinition constructor)
    {
      var method = new MethodDefinition(WellKnown.FactoryMethod, FactoryMethodAttributes, targetType);
      AddParameters(method);
      WeavingHelper.MarkAsCompilerGenerated(context, method);
      var il = method.Body.GetILProcessor();
      WeavingHelper.EmitLoadArguments(il, signature.Length);
      il.Emit(OpCodes.Newobj, constructor);
      il.Emit(OpCodes.Ret);
      targetType.Methods.Add(method);
    }

    public ImplementFactoryTask(TypeDefinition targetType, TypeReference[] signature)
    {
      if (targetType==null)
        throw new ArgumentNullException("targetType");
      if (signature==null)
        throw new ArgumentNullException("signature");

      this.targetType = targetType;
      this.signature = signature;
    }
  }
}