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
  internal sealed class ImplementInitializablePatternTask : WeavingTask
  {
    private readonly TypeDefinition type;
    private readonly MethodDefinition constructor;

    public override ActionResult Execute(ProcessorContext context)
    {
      var body = constructor.Body;
      var il = body.GetILProcessor();
      var leavePlaceholder = il.Create(OpCodes.Nop);

      var initializeCall = EmitInitializeCall(context, il);
      il.Append(leavePlaceholder);

      var exceptionType = context.References.Exception;
      var handlerBlock = EmitExceptionHandler(context, il, exceptionType);
      var handlerStart = handlerBlock.Item1;
      var handlerEnd = handlerBlock.Item2;
      var tryStart = GetStartInstruction(il);
      var tryEnd = handlerStart;

      ReplaceRetWithBr(il, tryStart, initializeCall, initializeCall);
      var ret = il.Create(OpCodes.Ret);
      il.Append(ret);
      il.Replace(leavePlaceholder, il.Create(OpCodes.Leave, ret));

      body.InitLocals = true;
      var handler = new ExceptionHandler(ExceptionHandlerType.Catch) {
        TryStart = tryStart,
        TryEnd = tryEnd,
        HandlerStart = handlerStart,
        HandlerEnd = handlerEnd,
        CatchType = exceptionType,
      };
      body.ExceptionHandlers.Add(handler);

      return ActionResult.Success;
    }

    private void ReplaceRetWithBr(ILProcessor il, Instruction start, Instruction end, Instruction brTarget)
    {
      var current = start;
      while (current!=end) {
        var next = current.Next;
        if (current.OpCode.Code==Code.Ret)
          il.Replace(current, il.Create(OpCodes.Br, brTarget));
        current = next;
      }
    }

    private Instruction GetStartInstruction(ILProcessor il)
    {
      var instructions = constructor.Body.Instructions;
      var baseConstructorCall = instructions.FirstOrDefault(IsBaseConstructorCall);
      var startInstruction = baseConstructorCall!=null ? baseConstructorCall.Next : instructions.FirstOrDefault();
      if (startInstruction==null) {
        il.Emit(OpCodes.Nop);
        startInstruction = instructions.FirstOrDefault();
      }
      return startInstruction;
    }

    private bool IsBaseConstructorCall(Instruction instruction)
    {
      if (instruction.OpCode.Code!=Code.Call)
        return false;
      var method = instruction.Operand as MethodReference;
      if (method==null)
        return false;
      if (method.Name!=WellKnown.Constructor)
        return false;
      return true;
    }

    private Instruction EmitInitializeCall(ProcessorContext context, ILProcessor il)
    {
      var initializedType = type.HasGenericParameters ? WeavingHelper.CreateGenericInstance(type) : type;
      var start = il.Create(OpCodes.Ldarg_0);
      il.Append(start);
      il.Emit(OpCodes.Ldtoken, initializedType);
      il.Emit(OpCodes.Call, context.References.TypeGetTypeFromHandle);
      il.Emit(OpCodes.Call, context.References.PersistentInitialize);
      return start;
    }

    private Tuple<Instruction, Instruction> EmitExceptionHandler(ProcessorContext context, ILProcessor il, TypeReference exceptionType)
    {
      var initializedType = type.HasGenericParameters ? WeavingHelper.CreateGenericInstance(type) : type;
      var variables = constructor.Body.Variables;
      var instructions = constructor.Body.Instructions;
      var variableIndex = variables.Count;
      variables.Add(new VariableDefinition("~Xtensive.Orm.InitError", exceptionType));
      il.Emit(OpCodes.Stloc, variableIndex);
      var start = instructions[instructions.Count - 1];
      il.Emit(OpCodes.Ldarg_0);
      il.Emit(OpCodes.Ldtoken, initializedType);
      il.Emit(OpCodes.Call, context.References.TypeGetTypeFromHandle);
      il.Emit(OpCodes.Ldloc, variableIndex);
      il.Emit(OpCodes.Call, context.References.PersistentInitializationError);
      il.Emit(OpCodes.Rethrow);
      il.Emit(OpCodes.Nop);
      var end = instructions[instructions.Count - 1];
      return Tuple.Create(start, end);
    }

    public ImplementInitializablePatternTask(TypeDefinition type, MethodDefinition constructor)
    {
      if (type==null)
        throw new ArgumentNullException("type");
      if (constructor==null)
        throw new ArgumentNullException("constructor");

      this.type = type;
      this.constructor = constructor;
    }
  }
}