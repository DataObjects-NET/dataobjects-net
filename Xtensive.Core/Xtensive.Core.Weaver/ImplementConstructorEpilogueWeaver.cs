// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.05.29

using System;
using PostSharp.CodeModel;
using PostSharp.CodeWeaver;
using PostSharp.Collections;
using PostSharp.Laos.Weaver;

namespace Xtensive.Core.Weaver
{
  internal class ImplementConstructorEpilogueWeaver : MethodLevelAspectWeaver
  {
    private const string GetTypeMethodName = "GetType";

    private readonly ITypeSignature handlerTypeSignature;
    private readonly string handlerMethodName;

    public override void Implement()
    {
      MethodDefDeclaration methodDef = (MethodDefDeclaration) TargetMethod;
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
      writer.EmitInstruction(OpCodeNumber.Ldarg_0);
      IMethod typeGetType = module.FindMethod(
        typeof (Type).GetMethod(GetTypeMethodName, new Type[] {}), BindingOptions.Default);

      MethodSignature methodSignature =
        new MethodSignature(CallingConvention.HasThis, module.Cache.GetIntrinsic(IntrinsicType.Void),
          new [] {typeGetType.ReturnType}, 0);

      writer.EmitInstructionMethod(OpCodeNumber.Call, typeGetType);
      writer.EmitInstructionMethod(OpCodeNumber.Call,
        (IMethod) baseTypeRef.Methods.GetMethod(handlerMethodName,
          methodSignature.Translate(module),
          BindingOptions.Default).Translate(module));
      writer.EmitInstruction(OpCodeNumber.Ret);
      writer.DetachInstructionSequence();
    }

    
    // Constructors

    internal ImplementConstructorEpilogueWeaver(ITypeSignature handlerTypeSignature, string handlerName)
    {
      this.handlerTypeSignature = handlerTypeSignature;
      this.handlerMethodName = handlerName;
    }
  }
}