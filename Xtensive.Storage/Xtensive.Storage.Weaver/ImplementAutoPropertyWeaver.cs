// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.05.29

using System;
using System.Collections.Generic;
using System.Reflection;
using PostSharp.CodeModel;
using PostSharp.CodeModel.TypeSignatures;
using PostSharp.Extensibility;
using PostSharp.Extensibility.Tasks;
using PostSharp.Laos.Weaver;
using PostSharp.ModuleWriter;

namespace Xtensive.Storage.Weaver
{
  internal class ImplementAutoPropertyWeaver :
    BaseTypeRelatedWeaver
  {
    public override void Implement()
    {
      MethodDefDeclaration methodDef = (MethodDefDeclaration) TargetElement;
      TypeDefDeclaration typeDef = methodDef.DeclaringType;

      int splitterPos = methodDef.Name.IndexOf('_');
      if (splitterPos <= 0)
        return;

      string propertyName = methodDef.Name.Substring(splitterPos + 1);
      string autoFieldName = "<" + propertyName + ">k__BackingField";
      bool isGetter = methodDef.Name.Substring(0, splitterPos) == "get";
      FieldDefDeclaration autoField = typeDef.Fields.GetByName(autoFieldName);
      if (autoField == null)
        return;

      ModuleDeclaration module = Task.Project.Module;
      TypeDefDeclaration typeRef = BaseTypeSignature.GetTypeDefinition();

      MethodBodyDeclaration methodBody = new MethodBodyDeclaration();
      methodDef.MethodBody = methodBody;
      InstructionBlock instructionBlock = methodBody.CreateInstructionBlock();
      methodBody.RootInstructionBlock = instructionBlock;
      InstructionSequence sequence = methodBody.CreateInstructionSequence();
      instructionBlock.AddInstructionSequence(sequence, PostSharp.Collections.NodePosition.After, null);
      InstructionWriter writer = Task.InstructionWriter;
      writer.AttachInstructionSequence(sequence);

      writer.EmitInstruction(OpCodeNumber.Ldarg_0);
      writer.EmitInstructionString(OpCodeNumber.Ldstr, propertyName);
      if (!isGetter)
        writer.EmitInstruction(OpCodeNumber.Ldarg_1);

      MethodSignature methodSignature =
        new MethodSignature(CallingConvention.HasThis,
          isGetter
            ? (ITypeSignature) module.Cache.GetGenericParameter(0, GenericParameterKind.Method)
            : module.Cache.GetIntrinsic(IntrinsicType.Void),
          isGetter
            ? new ITypeSignature[] {module.Cache.GetIntrinsic(IntrinsicType.String)}
            : new ITypeSignature[]
                {
                  module.Cache.GetIntrinsic(IntrinsicType.String),
                  GenericParameterTypeSignature.GetInstance(module, 0, GenericParameterKind.Method)
                }, 1);

      MethodRefDeclaration method = (MethodRefDeclaration) typeRef.Methods.GetMethod((isGetter ? "Get" : "Set") + "Value",
        methodSignature,
        BindingOptions.Default).Translate(module);

      writer.EmitInstructionMethod(OpCodeNumber.Callvirt,
        method.FindGenericInstance(new ITypeSignature[] {autoField.FieldType},
        BindingOptions.Default));
      writer.EmitInstruction(OpCodeNumber.Ret);
      writer.DetachInstructionSequence();

      try {
        RemoveTask.GetTask(Task.Project).MarkForRemoval(autoField);
      }
      catch {
        // Field is already marked for removal
      }
    }

    public override void EmitCompileTimeInitialization(InstructionEmitter writer)
    {
    }

    public override bool ValidateSelf()
    {
      return true;
    }


    // Constructors

    internal ImplementAutoPropertyWeaver(ITypeSignature baseTypeSignature) :
      base(baseTypeSignature)
    {
    }
  }
}