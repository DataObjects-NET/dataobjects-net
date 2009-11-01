// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.05.29

using System;
using System.Collections.Generic;
using System.Reflection;
using PostSharp.CodeModel;
using PostSharp.Extensibility;
using PostSharp.Laos.Weaver;
using PostSharp.ModuleWriter;

namespace Xtensive.Storage.Weaver
{
  internal class ImplementConstructorWeaver :
    BaseTypeRelatedWeaver
  {
    private ITypeSignature[] parameterTypeSignatures;

    public override void Implement()
    {
      TypeDefDeclaration typeDef = TargetElement as TypeDefDeclaration;
      if (typeDef == null) {
        WeaverDebug.WriteLine("Type '{0}' is not declared in current assembly.", TargetElement);
        return;
      }

      ModuleDeclaration module = Task.Project.Module;
      TypeDefDeclaration typeRef = BaseTypeSignature.GetTypeDefinition();

      if (typeRef == null)
        return;

      MethodSignature methodSignature =
        new MethodSignature(CallingConvention.HasThis, module.Cache.GetIntrinsic(IntrinsicType.Void),
          parameterTypeSignatures, 0);
      
      try {
        if (null != typeDef.Methods.GetMethod(".ctor", methodSignature, BindingOptions.Default)) {
          string arguments = "";
          foreach (TypeSignature signature in methodSignature.ParameterTypes)
            arguments += signature.Name + ", ";
          WeaverMessageSource.Instance.Write(SeverityType.Error, "TypeMustNotHaveConstructor",
            new object[] { typeDef.Name, arguments });
        }
      } catch {
        // Everything is OK, there is no .ctor(EntityData).
      }

      MethodDefDeclaration methodDef = new MethodDefDeclaration();
      methodDef.Name = ".ctor";
      methodDef.CallingConvention = CallingConvention.HasThis;
      methodDef.Attributes = MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
      typeDef.Methods.Add(methodDef);
      methodDef.CustomAttributes.Add(Task.WeavingHelper.GetDebuggerNonUserCodeAttribute());
      methodDef.ReturnParameter = new ParameterDeclaration();
      methodDef.ReturnParameter.Name = "";
      methodDef.ReturnParameter.ParameterType = module.Cache.GetIntrinsic(IntrinsicType.Void);
      methodDef.ReturnParameter.Attributes = ParameterAttributes.Retval;

      for(int i = 0; i < parameterTypeSignatures.Length; i++) {
        ParameterDeclaration dataParameter =
          new ParameterDeclaration(i, "data", parameterTypeSignatures[i]);
        methodDef.Parameters.Add(dataParameter);
      }


      MethodBodyDeclaration methodBody = new MethodBodyDeclaration();
      methodDef.MethodBody = methodBody;
      InstructionBlock instructionBlock = methodBody.CreateInstructionBlock();
      methodBody.RootInstructionBlock = instructionBlock;
      InstructionSequence sequence = methodBody.CreateInstructionSequence();
      instructionBlock.AddInstructionSequence(sequence, PostSharp.Collections.NodePosition.After, null);
      InstructionWriter writer = Task.InstructionWriter;
      writer.AttachInstructionSequence(sequence);


      writer.EmitInstruction(OpCodeNumber.Ldarg_0);
      for(short i = 0; i < parameterTypeSignatures.Length; i++)
        writer.EmitInstructionParameter(OpCodeNumber.Ldarg, methodDef.Parameters[i]);

      writer.EmitInstructionMethod(OpCodeNumber.Call,
        (IMethod) typeRef.Methods.GetMethod(".ctor",
          methodSignature.Translate(module),
          BindingOptions.Default).Translate(module));
      writer.EmitInstruction(OpCodeNumber.Ret);
      writer.DetachInstructionSequence();
    }

    public override void EmitCompileTimeInitialization(InstructionEmitter writer)
    {
    }

    public override bool ValidateSelf()
    {
      return true;
    }


    // Constructors

    internal ImplementConstructorWeaver(ITypeSignature baseTypeSignature, ITypeSignature[] parameterTypeSignatures) :
      base(baseTypeSignature)
    {
      this.parameterTypeSignatures = parameterTypeSignatures;
    }
  }
}
