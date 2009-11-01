// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.05.29

using System;
using System.Collections.Generic;
using System.Reflection;
using PostSharp.CodeModel;
using PostSharp.CodeWeaver;
using PostSharp.Collections;
using PostSharp.Extensibility;
using PostSharp.Laos.Weaver;

namespace Xtensive.Storage.Weaver
{
  internal class ImplementConstructorEpilogueWeaver : 
    MethodLevelAspectWeaver
  {
    private ITypeSignature baseTypeSignature;

    public override void Implement()
    {
      MethodDefDeclaration methodDef = (MethodDefDeclaration)TargetMethod;

      TypeDefDeclaration typeRef = baseTypeSignature.GetTypeDefinition();
      if (typeRef == null)
        return;
      ModuleDeclaration module = Task.Project.Module;
      MethodBodyDeclaration methodBody = methodDef.MethodBody;
      InstructionWriter writer = Task.InstructionWriter;

      RestructureMethodBodyResult result = Task.WeavingHelper.RestructureMethodBody(
        methodDef,
        RestructureMethodBodyOptions.ChangeReturnInstructions, 
        writer);

      methodBody.RootInstructionBlock.AddChildBlock(methodBody.CreateInstructionBlock(), NodePosition.After, null);
      methodBody.RootInstructionBlock.LastChildBlock.AddInstructionSequence(
        result.ReturnBranchTarget, 
        NodePosition.After, 
        null);
      
      writer.AttachInstructionSequence(result.ReturnBranchTarget);
      writer.EmitInstruction(OpCodeNumber.Ldarg_0);
      writer.EmitInstruction(OpCodeNumber.Ldarg_0);
      IMethod typeGetType = module.FindMethod(
        typeof (Type).GetMethod("GetType", new Type[] {}), BindingOptions.Default);

      MethodSignature methodSignature =
        new MethodSignature(CallingConvention.HasThis, module.Cache.GetIntrinsic(IntrinsicType.Void),
          new ITypeSignature[] {typeGetType.ReturnType}, 0);

      writer.EmitInstructionMethod(OpCodeNumber.Call, typeGetType);
      writer.EmitInstructionMethod(OpCodeNumber.Call,
        (IMethod) typeRef.Methods.GetMethod("OnCreated",
          methodSignature.Translate(module),
          BindingOptions.Default).Translate(module));
      writer.EmitInstruction(OpCodeNumber.Ret);
      writer.DetachInstructionSequence();
    }

    
    // Constructors

    internal ImplementConstructorEpilogueWeaver(ITypeSignature baseTypeSignature)
    {
      this.baseTypeSignature = baseTypeSignature;
    }
  }
}