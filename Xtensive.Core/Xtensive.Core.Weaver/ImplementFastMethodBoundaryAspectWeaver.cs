// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.09.25

using System;
using PostSharp.CodeModel;
using PostSharp.CodeModel.TypeSignatures;
using PostSharp.CodeWeaver;
using PostSharp.Collections;
using PostSharp.Laos;
using PostSharp.Laos.Weaver;
using Xtensive.Core.Aspects.Helpers;

namespace Xtensive.Core.Weaver
{
  internal class ImplementFastMethodBoundaryAspectWeaver : MethodLevelAspectWeaver, IMethodLevelAdvice
  {
    private bool hasOutParameter;
    private bool isStruct;
    private IMethod onEntryMethod;
    private IMethod onErrorMethod;
    private IMethod onExitMethod;
    private IMethod onSuccessMethod;
    private MethodDefDeclaration targetMethodDef;
    private LocalVariableSymbol onEntryResult;

    #region IMethodLevelAdvice Members

    public int Priority
    {
      get { return ((LaosMethodLevelAspect) Aspect).AspectPriority; }
    }

    public MethodDefDeclaration Method
    {
      get { return targetMethodDef; }
    }

    public MetadataDeclaration Operand
    {
      get { return null; }
    }

    public JoinPointKinds JoinPointKinds
    {
      get
      {
        return
          JoinPointKinds.AfterMethodBodySuccess |
          JoinPointKinds.AfterMethodBodyException |
          JoinPointKinds.AfterMethodBodyAlways |
          JoinPointKinds.BeforeMethodBody;
      }
    }

    public bool RequiresWeave(WeavingContext context)
    {
      return true;
    }

    public void Weave(WeavingContext context, InstructionBlock block)
    {
      JoinPointKinds joinPointKind = context.JoinPoint.JoinPointKind;
      switch (joinPointKind) {
      case JoinPointKinds.BeforeMethodBody:
        WeaveOnEntry(context, block);
        break;
      case JoinPointKinds.AfterMethodBodyAlways:
        WeaveOnExit(context, block);
        break;
      case JoinPointKinds.AfterMethodBodySuccess:
        WeaveOnSuccess(context, block);
        break;
      case JoinPointKinds.AfterMethodBodyException:
        WeaveOnError(context, block);
        break;
      case JoinPointKinds.AfterInstanceInitialization:
        WeaveOnEntry(context, block);
        break;
      default:
        throw new InvalidOperationException(string.Format("UnexpectedJoinPoint: {0}", joinPointKind));
      }
    }

    #endregion

    protected override void OnTargetAssigned(bool reassigned)
    {
      targetMethodDef = (MethodDefDeclaration) TargetMethod;
      isStruct = targetMethodDef.DeclaringType.BelongsToClassification(TypeClassifications.ValueType);
      if (!reassigned) {
        for (int i = 0; i < targetMethodDef.Parameters.Count; i++) {
          ParameterDeclaration declaration2 = targetMethodDef.Parameters[i];
          if (declaration2.ParameterType.BelongsToClassification(TypeClassifications.Pointer)!=false) {
            hasOutParameter = true;
            break;
          }
        }
        isStruct = targetMethodDef.DeclaringType.BelongsToClassification(TypeClassifications.ValueType);
      }
    }

    public override void Implement()
    {
      base.Implement();
      if (targetMethodDef.MayHaveBody) {
        targetMethodDef.MethodBody.InitLocalVariables = true;
        Task.MethodLevelAdvices.Add(this);
      }
    }

    private void WeaveOnEntry(WeavingContext context, InstructionBlock block)
    {
      ModuleDeclaration module = block.Module;
      MethodBodyDeclaration methodBody = targetMethodDef.MethodBody;
      ITypeSignature objectType = module.FindType(typeof(object), BindingOptions.Default);
      onEntryResult = methodBody.RootInstructionBlock.DefineLocalVariable(objectType, "onEntryResult");
      InstructionWriter instructionWriter = context.InstructionWriter;
      InstructionSequence newSequence = block.MethodBody.CreateInstructionSequence();
      block.AddInstructionSequence(newSequence, NodePosition.Before, null);
      instructionWriter.AttachInstructionSequence(newSequence);
      instructionWriter.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);
      instructionWriter.EmitInstructionField(OpCodeNumber.Ldsfld, AspectRuntimeInstanceField);
      instructionWriter.EmitInstruction(OpCodeNumber.Ldarg_0);
      instructionWriter.EmitInstructionMethod(OpCodeNumber.Callvirt, onEntryMethod);
      instructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Stloc, onEntryResult);
      if (this.isStruct)
      {
//        instructionWriter.EmitInstruction(OpCodeNumber.Ldarg_0);
//        instructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, this.eventArgsLocal);
//        instructionWriter.EmitInstructionMethod(OpCodeNumber.Call, this.getInstanceMethod);
//        instructionWriter.EmitInstructionType(OpCodeNumber.Unbox, this.Method.DeclaringType);
//        instructionWriter.EmitInstructionType(OpCodeNumber.Cpobj, this.Method.DeclaringType);
      }
      InstructionSequence sequence2 = methodBody.CreateInstructionSequence();
      block.AddInstructionSequence(sequence2, NodePosition.After, null);
      InstructionSequence sequence3 = methodBody.CreateInstructionSequence();
      block.AddInstructionSequence(sequence3, NodePosition.After, null);
//      instructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, this.eventArgsLocal);
//      instructionWriter.EmitInstructionMethod(OpCodeNumber.Call, this.getFlowBehaviorMethod);
//      instructionWriter.EmitInstructionInt32(OpCodeNumber.Ldc_I4, 3);
//      instructionWriter.EmitBranchingInstruction(OpCodeNumber.Bne_Un, sequence3);
      instructionWriter.EmitBranchingInstruction(OpCodeNumber.Br, sequence3);

      instructionWriter.DetachInstructionSequence();
      instructionWriter.AttachInstructionSequence(sequence2);
      ITypeSignature parameterType = targetMethodDef.ReturnParameter.ParameterType;
      if (!IntrinsicTypeSignature.Is(parameterType, IntrinsicType.Void)) {
//        instructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, this.eventArgsLocal);
//        instructionWriter.EmitInstructionMethod(OpCodeNumber.Call, this.getReturnValueMethod);
//        context.WeavingHelper.FromObject(parameterType, instructionWriter);
//        instructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Stloc, context.ReturnValueVariable);
      }
      instructionWriter.EmitBranchingInstruction(OpCodeNumber.Leave, context.LeaveBranchTarget);
      instructionWriter.DetachInstructionSequence();
    }

    private void WeaveOnExit(WeavingContext context, InstructionBlock block)
    {
      InstructionWriter instructionWriter = context.InstructionWriter;
      InstructionSequence newSequence = block.MethodBody.CreateInstructionSequence();
      block.AddInstructionSequence(newSequence, NodePosition.Before, null);
      instructionWriter.AttachInstructionSequence(newSequence);
      instructionWriter.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);
      
      ITypeSignature parameterType = targetMethodDef.ReturnParameter.ParameterType;
      if (!IntrinsicTypeSignature.Is(parameterType, IntrinsicType.Void)) {
//        instructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, this.eventArgsLocal);
//        instructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, context.ReturnValueVariable);
//        context.WeavingHelper.ToObject(parameterType, instructionWriter);
//        instructionWriter.EmitInstructionMethod(OpCodeNumber.Call, this.setReturnValueMethod);
      }
      instructionWriter.EmitInstructionField(OpCodeNumber.Ldsfld, AspectRuntimeInstanceField);
      instructionWriter.EmitInstruction(OpCodeNumber.Ldarg_0);
      instructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, onEntryResult);
      instructionWriter.EmitInstructionMethod(OpCodeNumber.Callvirt, onExitMethod);
      if (!IntrinsicTypeSignature.Is(parameterType, IntrinsicType.Void)) {
//        instructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, this.eventArgsLocal);
//        instructionWriter.EmitInstructionMethod(OpCodeNumber.Call, this.getReturnValueMethod);
//        context.WeavingHelper.FromObject(parameterType, instructionWriter);
//        instructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Stloc, context.ReturnValueVariable);
      }
      instructionWriter.DetachInstructionSequence();

    }

    private void WeaveOnSuccess(WeavingContext context, InstructionBlock block)
    {
      InstructionWriter instructionWriter = context.InstructionWriter;
      InstructionSequence newSequence = block.MethodBody.CreateInstructionSequence();
      block.AddInstructionSequence(newSequence, NodePosition.Before, null);
      instructionWriter.AttachInstructionSequence(newSequence);
      instructionWriter.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);
      
      ITypeSignature parameterType = targetMethodDef.ReturnParameter.ParameterType;
      if (!IntrinsicTypeSignature.Is(parameterType, IntrinsicType.Void)) {
//        instructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, this.eventArgsLocal);
//        instructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, context.ReturnValueVariable);
//        context.WeavingHelper.ToObject(parameterType, instructionWriter);
//        instructionWriter.EmitInstructionMethod(OpCodeNumber.Call, this.setReturnValueMethod);
      }
      instructionWriter.EmitInstructionField(OpCodeNumber.Ldsfld, AspectRuntimeInstanceField);
      instructionWriter.EmitInstruction(OpCodeNumber.Ldarg_0);
      instructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, onEntryResult);
      instructionWriter.EmitInstructionMethod(OpCodeNumber.Callvirt, onSuccessMethod);
      if (!IntrinsicTypeSignature.Is(parameterType, IntrinsicType.Void)) {
//        instructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, this.eventArgsLocal);
//        instructionWriter.EmitInstructionMethod(OpCodeNumber.Call, this.getReturnValueMethod);
//        context.WeavingHelper.FromObject(parameterType, instructionWriter);
//        instructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Stloc, context.ReturnValueVariable);
      }
      instructionWriter.DetachInstructionSequence();

    }

    private void WeaveOnError(WeavingContext context, InstructionBlock block)
    {
      InstructionWriter instructionWriter = context.InstructionWriter;
      ModuleDeclaration module = block.Module;
      MethodBodyDeclaration methodBody = block.MethodBody;
      LocalVariableSymbol exceptionLocal = methodBody.RootInstructionBlock.DefineLocalVariable(module.Cache.GetType(typeof(Exception)), "~exception~{0}");
      InstructionSequence newSequence = methodBody.CreateInstructionSequence();
      block.AddInstructionSequence(newSequence, NodePosition.Before, null);
      instructionWriter.AttachInstructionSequence(newSequence);
      instructionWriter.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);
      instructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Stloc, exceptionLocal);
      
      instructionWriter.EmitInstructionField(OpCodeNumber.Ldsfld, AspectRuntimeInstanceField);
      instructionWriter.EmitInstruction(OpCodeNumber.Ldarg_0);
      instructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, exceptionLocal);
      instructionWriter.EmitInstructionMethod(OpCodeNumber.Callvirt, onErrorMethod);
      
      InstructionSequence rethrowSequence = methodBody.CreateInstructionSequence();
      block.AddInstructionSequence(rethrowSequence, NodePosition.After, null);
      InstructionSequence leaveSequence = methodBody.CreateInstructionSequence();
      block.AddInstructionSequence(leaveSequence, NodePosition.After, null);
      InstructionSequence sequence4 = methodBody.CreateInstructionSequence();
      block.AddInstructionSequence(sequence4, NodePosition.After, null);
      instructionWriter.EmitBranchingInstruction(OpCodeNumber.Brfalse, leaveSequence);
      instructionWriter.DetachInstructionSequence();
      instructionWriter.AttachInstructionSequence(rethrowSequence);
      instructionWriter.EmitInstruction(OpCodeNumber.Rethrow);
      instructionWriter.DetachInstructionSequence();
      instructionWriter.AttachInstructionSequence(leaveSequence);
      ITypeSignature parameterType = targetMethodDef.ReturnParameter.ParameterType;
      if (!IntrinsicTypeSignature.Is(parameterType, IntrinsicType.Void)) {
//        instructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, this.eventArgsLocal);
//        instructionWriter.EmitInstructionMethod(OpCodeNumber.Call, this.getReturnValueMethod);
//        context.WeavingHelper.FromObject(parameterType, instructionWriter);
//        instructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Stloc, context.ReturnValueVariable);
      }
      instructionWriter.EmitBranchingInstruction(OpCodeNumber.Leave, context.LeaveBranchTarget);
      instructionWriter.DetachInstructionSequence();

    }




    /*public override void Implement()
    {
      var methodDef = (MethodDefDeclaration)TargetMethod;
      var module = Task.Project.Module;
      var methodBody = methodDef.MethodBody;
      var writer = Task.InstructionWriter;
      methodBody.MaxStack += 3;
      methodBody.InitLocalVariables = true;
      var restructurer = new MethodBodyRestructurer(methodDef, MethodBodyRestructurerOptions.ChangeReturnInstructions, Task.WeavingHelper);
      restructurer.Restructure(writer);
      var returnBranchTarget = restructurer.ReturnBranchTarget;
      var rootBlock = methodBody.RootInstructionBlock;

      var onEntryBlock = restructurer.AfterInitializationBlock ?? restructurer.EntryBlock;
      ITypeSignature objectType = module.FindType(typeof(object), BindingOptions.Default);
      var onEntryResult = onEntryBlock.DefineLocalVariable(objectType, "onEntryResult");
      var onEntrySequence = methodBody.CreateInstructionSequence();
      onEntryBlock.AddInstructionSequence(onEntrySequence, NodePosition.After, null);
      writer.AttachInstructionSequence(onEntrySequence);
      writer.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);
      writer.EmitInstructionField(OpCodeNumber.Ldsfld, AspectRuntimeInstanceField);
      writer.EmitInstruction(OpCodeNumber.Ldarg_0);
      writer.EmitInstructionMethod(OpCodeNumber.Callvirt, onEntryMethod);
      writer.EmitInstructionLocalVariable(OpCodeNumber.Stloc, onEntryResult);
      writer.DetachInstructionSequence();


      var outerBlock = methodBody.CreateInstructionBlock();
      var protectedBlock = methodBody.CreateInstructionBlock();
      var lastChildBlock = rootBlock.LastChildBlock;
      lastChildBlock.Detach();
      rootBlock.AddChildBlock(outerBlock, NodePosition.After, null);
      outerBlock.AddChildBlock(protectedBlock, NodePosition.After, null);
      protectedBlock.AddChildBlock(lastChildBlock, NodePosition.After, null);
      var onExitBlock = methodBody.CreateInstructionBlock();
      protectedBlock.AddExceptionHandlerFinally(onExitBlock, NodePosition.After, null);
      var onExitSequence = methodBody.CreateInstructionSequence();
      onExitBlock.AddInstructionSequence(onExitSequence, NodePosition.After, null);
      writer.AttachInstructionSequence(onExitSequence);
      writer.EmitInstructionField(OpCodeNumber.Ldsfld, AspectRuntimeInstanceField);
      writer.EmitInstruction(OpCodeNumber.Ldarg_0);
      writer.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, onEntryResult);
      writer.EmitInstructionMethod(OpCodeNumber.Callvirt, onExitMethod);
      writer.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);
      writer.EmitInstruction(OpCodeNumber.Endfinally);
      writer.DetachInstructionSequence();

      var catchBlock = methodBody.CreateInstructionBlock();
      ITypeSignature exceptionType = module.FindType(typeof(Exception), BindingOptions.Default);
      var exception = onEntryBlock.DefineLocalVariable(exceptionType, "e");
      lastChildBlock.AddExceptionHandlerCatch(exceptionType, catchBlock, NodePosition.Before, null);
      var onErrorSequence = methodBody.CreateInstructionSequence();
      var rethrowSequence = methodBody.CreateInstructionSequence();
      catchBlock.AddInstructionSequence(onErrorSequence, NodePosition.After, null);
      writer.AttachInstructionSequence(onErrorSequence);
      writer.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);
      writer.EmitInstructionLocalVariable(OpCodeNumber.Stloc, exception);
      writer.EmitInstructionField(OpCodeNumber.Ldsfld, AspectRuntimeInstanceField);
      writer.EmitInstruction(OpCodeNumber.Ldarg_0);
      writer.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, exception);
      writer.EmitInstructionMethod(OpCodeNumber.Callvirt, onErrorMethod);
      writer.EmitBranchingInstruction(OpCodeNumber.Brfalse, rethrowSequence);
      writer.EmitInstruction(OpCodeNumber.Rethrow);
      writer.DetachInstructionSequence();
      catchBlock.AddInstructionSequence(rethrowSequence, NodePosition.After, null);
      writer.AttachInstructionSequence(rethrowSequence);
      writer.EmitBranchingInstruction(OpCodeNumber.Leave, returnBranchTarget);
      writer.DetachInstructionSequence();

      var returnBlock = methodBody.CreateInstructionBlock();
      rootBlock.AddChildBlock(returnBlock, NodePosition.After, null);
      if (returnBranchTarget.ParentInstructionBlock==null)
        returnBlock.AddInstructionSequence(returnBranchTarget, NodePosition.After, null);
      writer.AttachInstructionSequence(returnBranchTarget);
      writer.EmitInstructionField(OpCodeNumber.Ldsfld, AspectRuntimeInstanceField);
      writer.EmitInstruction(OpCodeNumber.Ldarg_0);
      writer.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, onEntryResult);
      writer.EmitInstructionMethod(OpCodeNumber.Callvirt, onSuccessMethod);
      if (restructurer.ReturnValueVariable!=null)
        writer.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, restructurer.ReturnValueVariable);
      writer.EmitInstruction(OpCodeNumber.Ret);
      writer.DetachInstructionSequence();
    }*/

    public override void Initialize()
    {
      base.Initialize();
      ModuleDeclaration module = Task.Project.Module;
      onEntryMethod = (IMethod) module.Cache.GetItem(theModule => theModule.FindMethod(typeof (ImplementFastMethodBoundaryAspect).GetMethod("OnEntry"), BindingOptions.RequireGenericDefinition));
      onExitMethod = (IMethod) module.Cache.GetItem(theModule => theModule.FindMethod(typeof (ImplementFastMethodBoundaryAspect).GetMethod("OnExit"), BindingOptions.RequireGenericDefinition));
      onSuccessMethod = (IMethod) module.Cache.GetItem(theModule => theModule.FindMethod(typeof (ImplementFastMethodBoundaryAspect).GetMethod("OnSuccess"), BindingOptions.RequireGenericDefinition));
      onErrorMethod = (IMethod) module.Cache.GetItem(theModule => theModule.FindMethod(typeof (ImplementFastMethodBoundaryAspect).GetMethod("OnError"), BindingOptions.RequireGenericDefinition));
    }
  }
}