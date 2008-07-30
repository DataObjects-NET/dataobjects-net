// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.05.29

using System.Reflection;
using PostSharp.CodeModel;
using PostSharp.Laos.Weaver;
using PostSharp.ModuleWriter;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Weaver
{
  internal class ImplementConstructorWeaver : LaosAspectWeaver
  {
    private const string ParameterNamePrefix = "arg";

    private readonly ITypeSignature[] parameterTypeSignatures;

    public override void Implement()
    {
      TypeDefDeclaration typeDef = (TypeDefDeclaration) TargetElement;
      var baseType = typeDef.BaseType;
      var baseTypeDef = baseType.GetTypeDefinition();
      ModuleDeclaration  module  = Task.Project.Module;

      MethodSignature ctorSignature = new MethodSignature(CallingConvention.HasThis, 
        module.Cache.GetIntrinsic(IntrinsicType.Void), 
        parameterTypeSignatures, 0);
      
      MethodDefDeclaration ctorDef = new MethodDefDeclaration();
      ctorDef.Name = WellKnown.CtorName;
      ctorDef.CallingConvention = CallingConvention.HasThis;
      ctorDef.Attributes = MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
      typeDef.Methods.Add(ctorDef);
      ctorDef.CustomAttributes.Add(Task.WeavingHelper.GetDebuggerNonUserCodeAttribute());
      ctorDef.ReturnParameter = new ParameterDeclaration();
      ctorDef.ReturnParameter.Name = string.Empty;
      ctorDef.ReturnParameter.ParameterType = module.Cache.GetIntrinsic(IntrinsicType.Void);
      ctorDef.ReturnParameter.Attributes = ParameterAttributes.Retval;

      for(int i = 0; i < parameterTypeSignatures.Length; i++) {
        ParameterDeclaration parameter =
          new ParameterDeclaration(i, ParameterNamePrefix+i, parameterTypeSignatures[i]);
        ctorDef.Parameters.Add(parameter);
      }

      MethodBodyDeclaration body = new MethodBodyDeclaration();
      ctorDef.MethodBody = body;
      InstructionBlock instructionBlock = body.CreateInstructionBlock();
      body.RootInstructionBlock = instructionBlock;
      InstructionSequence sequence = body.CreateInstructionSequence();
      instructionBlock.AddInstructionSequence(sequence, PostSharp.Collections.NodePosition.After, null);
      InstructionWriter writer = Task.InstructionWriter;
      writer.AttachInstructionSequence(sequence);


      writer.EmitInstruction(OpCodeNumber.Ldarg_0);
      for(short i = 0; i < parameterTypeSignatures.Length; i++)
        writer.EmitInstructionParameter(OpCodeNumber.Ldarg, ctorDef.Parameters[i]);

      IMethod baseConstructor = null;
      try {
        baseConstructor = baseTypeDef.Methods.GetMethod(WellKnown.CtorName,
          ctorSignature.Translate(module),
          BindingOptions.OnlyExisting);
      } catch {}
      while (baseConstructor == null && baseType != null)
      {
        baseType = baseTypeDef.BaseType;
        baseTypeDef = baseType.GetTypeDefinition();
        try {
          baseConstructor = baseType.Methods.GetMethod(WellKnown.CtorName,
            ctorSignature.Translate(module),
            BindingOptions.OnlyExisting);
        } catch {}
      }
      writer.EmitInstructionMethod(OpCodeNumber.Call,
        (IMethod)baseConstructor.Translate(module));
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

    internal ImplementConstructorWeaver(ITypeSignature[] parameterTypeSignatures)
    {
      this.parameterTypeSignatures = parameterTypeSignatures;
    }
  }
}
