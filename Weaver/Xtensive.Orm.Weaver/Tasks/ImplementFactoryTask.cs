// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.19

using System;
using System.Linq;
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
      var constructor = GetExistingConstructor() ?? DefineConstructor(context);
      if (!targetType.IsAbstract)
        DefineFactoryMethod(context, constructor);
      return ActionResult.Success;
    }

    private MethodDefinition DefineConstructor(ProcessorContext context)
    {
      var baseConstructor = GetBaseConstructor(context);
      var method = new MethodDefinition(WellKnown.Constructor, ConstructorAttributes, voidType) {HasThis = true};
      DefineParameters(method);
      WeavingHelper.MarkAsCompilerGenerated(context, method);
      var il = method.Body.GetILProcessor();
      WeavingHelper.EmitLoadArguments(il, signature.Length + 1);
      il.Emit(OpCodes.Call, baseConstructor);
      il.Emit(OpCodes.Ret);
      targetType.Methods.Add(method);
      return method;
    }

    private void DefineFactoryMethod(ProcessorContext context, MethodReference constructor)
    {
      var returnType = (TypeReference) targetType;
      if (targetType.HasGenericParameters) {
        returnType = WeavingHelper.CreateGenericInstance(targetType);
        constructor = new MethodReference(WellKnown.Constructor, voidType, returnType) {HasThis = true};
        DefineParameters(constructor);
      }
      var method = new MethodDefinition(WellKnown.FactoryMethod, FactoryMethodAttributes, returnType);
      DefineParameters(method);
      WeavingHelper.MarkAsCompilerGenerated(context, method);
      var il = method.Body.GetILProcessor();
      WeavingHelper.EmitLoadArguments(il, signature.Length);
      il.Emit(OpCodes.Newobj, constructor);
      il.Emit(OpCodes.Ret);
      targetType.Methods.Add(method);
    }

    private MethodDefinition GetExistingConstructor()
    {
      var parameterTypes = signature.Select(t => new TypeIdentity(t)).ToList();
      var constructors = targetType.Methods.Where(m => m.IsConstructor && !m.IsStatic);
      var existingConstructor = constructors
        .FirstOrDefault(m => m.Parameters.Select(p => new TypeIdentity(p.ParameterType)).SequenceEqual(parameterTypes));
      return existingConstructor;
    }

    private MethodReference GetBaseConstructor(ProcessorContext context)
    {
      var reference = new MethodReference(WellKnown.Constructor, voidType, targetType.BaseType) {HasThis = true};
      DefineParameters(reference);
      return context.TargetModule.ImportReference(reference);
    }

    private void DefineParameters(MethodDefinition method)
    {
      for (int i = 0; i < signature.Length; i++)
        method.Parameters.Add(new ParameterDefinition("p" + i, ParameterAttributes.In, signature[i]));
    }

    private void DefineParameters(MethodReference reference)
    {
      foreach (var item in signature)
        reference.Parameters.Add(new ParameterDefinition(item));
    }

    public ImplementFactoryTask(TypeDefinition targetType, TypeReference[] signature)
    {
      ArgumentNullException.ThrowIfNull(targetType);
      ArgumentNullException.ThrowIfNull(signature);
      this.targetType = targetType;
      this.signature = signature;
    }
  }
}