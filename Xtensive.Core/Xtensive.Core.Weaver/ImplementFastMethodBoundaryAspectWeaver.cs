// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.09.25

using System;
using PostSharp.CodeModel;
using PostSharp.CodeWeaver;
using PostSharp.Collections;
using PostSharp.Laos;
using PostSharp.Laos.Weaver;
using Xtensive.Core.Aspects.Helpers;

namespace Xtensive.Core.Weaver
{
  internal class ImplementFastMethodBoundaryAspectWeaver : MethodLevelAspectWeaver, IMethodLevelAdvice
  {
    private IMethod onEntryMethod;
    private IMethod onErrorMethod;
    private IMethod onExitMethod;
    private IMethod onSuccessMethod;
    private MethodDefDeclaration targetMethodDef;
    private LocalVariableSymbol onEntryResult;
    private JoinPointKinds joinPoints;

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
        return joinPoints;
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
      default:
        throw new InvalidOperationException(string.Format("UnexpectedJoinPoint: {0}", joinPointKind));
      }
    }

    #endregion

    protected override void OnTargetAssigned(bool reassigned)
    {
      targetMethodDef = (MethodDefDeclaration) TargetMethod;
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
      instructionWriter.DetachInstructionSequence();
    }

    private void WeaveOnExit(WeavingContext context, InstructionBlock block)
    {
      InstructionWriter instructionWriter = context.InstructionWriter;
      InstructionSequence newSequence = block.MethodBody.CreateInstructionSequence();
      block.AddInstructionSequence(newSequence, NodePosition.Before, null);
      instructionWriter.AttachInstructionSequence(newSequence);
      instructionWriter.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);
      instructionWriter.EmitInstructionField(OpCodeNumber.Ldsfld, AspectRuntimeInstanceField);
      instructionWriter.EmitInstruction(OpCodeNumber.Ldarg_0);
      instructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, onEntryResult);
      instructionWriter.EmitInstructionMethod(OpCodeNumber.Callvirt, onExitMethod);
      instructionWriter.DetachInstructionSequence();

    }

    private void WeaveOnSuccess(WeavingContext context, InstructionBlock block)
    {
      InstructionWriter instructionWriter = context.InstructionWriter;
      InstructionSequence newSequence = block.MethodBody.CreateInstructionSequence();
      block.AddInstructionSequence(newSequence, NodePosition.Before, null);
      instructionWriter.AttachInstructionSequence(newSequence);
      instructionWriter.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);
      instructionWriter.EmitInstructionField(OpCodeNumber.Ldsfld, AspectRuntimeInstanceField);
      instructionWriter.EmitInstruction(OpCodeNumber.Ldarg_0);
      instructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, onEntryResult);
      instructionWriter.EmitInstructionMethod(OpCodeNumber.Callvirt, onSuccessMethod);
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
      instructionWriter.EmitBranchingInstruction(OpCodeNumber.Brfalse, leaveSequence);
      instructionWriter.DetachInstructionSequence();
      instructionWriter.AttachInstructionSequence(rethrowSequence);
      instructionWriter.EmitInstruction(OpCodeNumber.Rethrow);
      instructionWriter.DetachInstructionSequence();
    }

    public override void Initialize()
    {
      base.Initialize();
      ModuleDeclaration module = Task.Project.Module;
      onEntryMethod = (IMethod) module.Cache.GetItem(theModule => theModule.FindMethod(typeof (ImplementFastMethodBoundaryAspect).GetMethod("OnEntry"), BindingOptions.RequireGenericDefinition));
      onExitMethod = (IMethod) module.Cache.GetItem(theModule => theModule.FindMethod(typeof (ImplementFastMethodBoundaryAspect).GetMethod("OnExit"), BindingOptions.RequireGenericDefinition));
      onSuccessMethod = (IMethod) module.Cache.GetItem(theModule => theModule.FindMethod(typeof (ImplementFastMethodBoundaryAspect).GetMethod("OnSuccess"), BindingOptions.RequireGenericDefinition));
      onErrorMethod = (IMethod) module.Cache.GetItem(theModule => theModule.FindMethod(typeof (ImplementFastMethodBoundaryAspect).GetMethod("OnError"), BindingOptions.RequireGenericDefinition));
      var aspectType = MethodLevelAspect.GetType();
      var baseType = typeof (ImplementFastMethodBoundaryAspect);
      var onExit = aspectType.GetMethod("OnExit");
      var onSuccess = aspectType.GetMethod("OnSuccess");
      var onError = aspectType.GetMethod("OnError");
      joinPoints = JoinPointKinds.BeforeMethodBody;
      if (onExit.DeclaringType != baseType)
        joinPoints |= JoinPointKinds.AfterMethodBodyAlways;
      if (onSuccess.DeclaringType != baseType)
        joinPoints |= JoinPointKinds.AfterMethodBodySuccess;
      if (onError.DeclaringType != baseType)
        joinPoints |= JoinPointKinds.AfterMethodBodyException;
    }
  }
}