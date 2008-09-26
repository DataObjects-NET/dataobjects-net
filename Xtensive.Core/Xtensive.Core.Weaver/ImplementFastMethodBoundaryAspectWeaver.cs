// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.09.25

using System;
using PostSharp.CodeModel;
using PostSharp.CodeWeaver;
using PostSharp.Collections;
using PostSharp.Laos.Weaver;
using Xtensive.Core.Aspects.Helpers;

namespace Xtensive.Core.Weaver
{
  internal class ImplementFastMethodBoundaryAspectWeaver : MethodLevelAspectWeaver
  {
    private IMethod onEntryMethod;
    private IMethod onExitMethod;
    private IMethod onSuccessMethod;

    public override void Implement()
    {
      var methodDef = (MethodDefDeclaration)TargetMethod;
      var module = Task.Project.Module;
      var methodBody = methodDef.MethodBody;
      var writer = Task.InstructionWriter;
      var restructurer = new MethodBodyRestructurer(methodDef, MethodBodyRestructurerOptions.ChangeReturnInstructions, Task.WeavingHelper);
      restructurer.Restructure(writer);
      var returnBranchTarget = restructurer.ReturnBranchTarget;

      ITypeSignature objectType = module.FindType(typeof(object), BindingOptions.Default);
      var onEntryResult = methodBody.RootInstructionBlock.DefineLocalVariable(objectType, "onEntryResult");
      methodBody.MaxStack += 2;

      var onEntryBlock = restructurer.AfterInitializationBlock;
      if (onEntryBlock == null) {
        onEntryBlock = methodBody.CreateInstructionBlock();
        methodBody.RootInstructionBlock.AddChildBlock(onEntryBlock, NodePosition.Before, null);
      }
      var onEntrySequence = methodBody.CreateInstructionSequence();
      onEntryBlock.AddInstructionSequence(onEntrySequence, NodePosition.Before, null);
      writer.AttachInstructionSequence(onEntrySequence);
      writer.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);
      writer.EmitInstructionField(OpCodeNumber.Ldsfld, AspectRuntimeInstanceField);
      writer.EmitInstruction(OpCodeNumber.Ldarg_0);
      writer.EmitInstructionMethod(OpCodeNumber.Callvirt, onEntryMethod);
      writer.EmitInstructionLocalVariable(OpCodeNumber.Stloc, onEntryResult);
      writer.DetachInstructionSequence();

      var returnBlock = methodBody.CreateInstructionBlock();
      methodBody.RootInstructionBlock.AddChildBlock(returnBlock, NodePosition.After, null);
      if (returnBranchTarget.ParentInstructionBlock==null)
        returnBlock.AddInstructionSequence(returnBranchTarget, NodePosition.After, null);
      writer.AttachInstructionSequence(returnBranchTarget);
      writer.EmitInstructionField(OpCodeNumber.Ldsfld, AspectRuntimeInstanceField);
      writer.EmitInstruction(OpCodeNumber.Ldarg_0);
        writer.EmitInstructionMethod(OpCodeNumber.Callvirt, onSuccessMethod);
      if (restructurer.ReturnValueVariable!=null)
        writer.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, restructurer.ReturnValueVariable);
      writer.EmitInstruction(OpCodeNumber.Ret);
      writer.DetachInstructionSequence();
//      restructurer.

      /*InstructionWriter instructionWriter = context.InstructionWriter;
        InstructionSequence newSequence = block.MethodBody.CreateInstructionSequence();
        block.AddInstructionSequence(newSequence, NodePosition.Before, null);
        instructionWriter.AttachInstructionSequence(newSequence);
        instructionWriter.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);
        base.Task.EventArgsBuilders.BuildMethodExecutionEventArgs(this.targetMethodDef, instructionWriter, out this.eventArgsLocal, out this.argumentsLocal);
        base.Task.InstanceTagManager.EmitLoadInstanceTag(this.eventArgsLocal, this.instanceTagField, instructionWriter);
        instructionWriter.EmitInstructionField(OpCodeNumber.Ldsfld, base.AspectRuntimeInstanceField);
        instructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, this.eventArgsLocal);
        instructionWriter.EmitInstructionMethod(OpCodeNumber.Callvirt, this.onEntryMethod);
        if (this.isStruct)
        {
          instructionWriter.EmitInstruction(OpCodeNumber.Ldarg_0);
          instructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, this.eventArgsLocal);
          instructionWriter.EmitInstructionMethod(OpCodeNumber.Call, this.getInstanceMethod);
          instructionWriter.EmitInstructionType(OpCodeNumber.Unbox, this.Method.DeclaringType);
          instructionWriter.EmitInstructionType(OpCodeNumber.Cpobj, this.Method.DeclaringType);
        }
        base.Task.InstanceTagManager.EmitStoreInstanceTag(this.eventArgsLocal, this.instanceTagField, instructionWriter);
        InstructionSequence sequence2 = methodBody.CreateInstructionSequence();
        block.AddInstructionSequence(sequence2, NodePosition.After, null);
        InstructionSequence sequence3 = methodBody.CreateInstructionSequence();
        block.AddInstructionSequence(sequence3, NodePosition.After, null);
        instructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, this.eventArgsLocal);
        instructionWriter.EmitInstructionMethod(OpCodeNumber.Call, this.getFlowBehaviorMethod);
        instructionWriter.EmitInstructionInt32(OpCodeNumber.Ldc_I4, 3);
        instructionWriter.EmitBranchingInstruction(OpCodeNumber.Bne_Un, sequence3);
        instructionWriter.DetachInstructionSequence();
        instructionWriter.AttachInstructionSequence(sequence2);
        ITypeSignature parameterType = this.targetMethodDef.ReturnParameter.ParameterType;
        if (!IntrinsicTypeSignature.Is(parameterType, IntrinsicType.Void))
        {
          instructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, this.eventArgsLocal);
          instructionWriter.EmitInstructionMethod(OpCodeNumber.Call, this.getReturnValueMethod);
          context.WeavingHelper.FromObject(parameterType, instructionWriter);
          instructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Stloc, context.ReturnValueVariable);
        }
        if (this.hasOutParameter)
        {
          context.WeavingHelper.CopyArgumentsFromArray(this.argumentsLocal, this.targetMethodDef, instructionWriter);
        }
        instructionWriter.EmitBranchingInstruction(OpCodeNumber.Leave, context.LeaveBranchTarget);
        instructionWriter.DetachInstructionSequence();
      */


      /*MethodDefDeclaration methodDef = (MethodDefDeclaration) TargetMethod;
            TypeDefDeclaration baseTypeRef = handlerTypeSignature.GetTypeDefinition();
            if (baseTypeRef == null)
              return;

            ModuleDeclaration module = Task.Project.Module;
            MethodBodyDeclaration methodBody = methodDef.MethodBody;
            InstructionWriter writer = Task.InstructionWriter;
            
            MethodBodyRestructurer restructurer = 
              new MethodBodyRestructurer(methodDef, MethodBodyRestructurerOptions.ChangeReturnInstructions, Task.WeavingHelper);

            restructurer.Restructure(writer);      

            methodBody.RootInstructionBlock.AddChildBlock(methodBody.CreateInstructionBlock(), NodePosition.After, null);
            methodBody.RootInstructionBlock.LastChildBlock.AddInstructionSequence(
              restructurer.ReturnBranchTarget, 
              NodePosition.After, 
              null);
            writer.AttachInstructionSequence(restructurer.ReturnBranchTarget);

            writer.EmitInstruction(OpCodeNumber.Ldarg_0);

            writer.EmitInstructionType(OpCodeNumber.Ldtoken, methodDef.DeclaringType);
            IMethod getTypeFromHandleMethod = module.FindMethod(
              typeof (Type).GetMethod(GetTypeFromHandleMethodName, new [] {typeof(RuntimeTypeHandle)}), BindingOptions.Default);
            writer.EmitInstructionMethod(OpCodeNumber.Call, getTypeFromHandleMethod);

            MethodSignature handlerSignature =
              new MethodSignature(CallingConvention.HasThis, module.Cache.GetIntrinsic(IntrinsicType.Void),
                new [] {getTypeFromHandleMethod.ReturnType}, 0);
            writer.EmitInstructionMethod(OpCodeNumber.Call,
              (IMethod) baseTypeRef.Methods.GetMethod(handlerMethodName,
                handlerSignature.Translate(module),
                BindingOptions.Default).Translate(module));

            writer.EmitInstruction(OpCodeNumber.Ret);
            writer.DetachInstructionSequence();*/
    }

    public override void Initialize()
    {
      base.Initialize();
      ModuleDeclaration module = Task.Project.Module;
      onEntryMethod = (IMethod)module.Cache.GetItem(theModule => theModule.FindMethod(typeof (ImplementFastMethodBoundaryAspect).GetMethod("OnEntry"), BindingOptions.RequireGenericDefinition));
      onExitMethod = (IMethod)module.Cache.GetItem(theModule => theModule.FindMethod(typeof (ImplementFastMethodBoundaryAspect).GetMethod("OnExit"), BindingOptions.RequireGenericDefinition));
      onSuccessMethod = (IMethod)module.Cache.GetItem(theModule => theModule.FindMethod(typeof (ImplementFastMethodBoundaryAspect).GetMethod("OnSuccess"), BindingOptions.RequireGenericDefinition));
    }
  }
}