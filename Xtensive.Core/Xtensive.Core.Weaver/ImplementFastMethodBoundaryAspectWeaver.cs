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
      methodBody.MaxStack += 2;
      var restructurer = new MethodBodyRestructurer(methodDef, MethodBodyRestructurerOptions.ChangeReturnInstructions, Task.WeavingHelper);
      restructurer.Restructure(writer);
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

      var returnBlock = methodBody.CreateInstructionBlock();
      rootBlock.AddChildBlock(returnBlock, NodePosition.After, null);
      var returnBranchTarget = restructurer.ReturnBranchTarget;
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