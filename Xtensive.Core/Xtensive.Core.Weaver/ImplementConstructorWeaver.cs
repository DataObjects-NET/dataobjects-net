// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.05.29

using System;
using System.Reflection;
using PostSharp.CodeModel;
using PostSharp.Laos.Weaver;
using PostSharp.ModuleWriter;
using Xtensive.Core.Aspects;
using Xtensive.Core.Collections;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Weaver
{
  internal class ImplementConstructorWeaver : TypeLevelAspectWeaver
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
      ErrorLog.Debug("Finding base for: {0}", typeDef.GetReflectionWrapper(
        ArrayUtils<Type>.EmptyArray, ArrayUtils<Type>.EmptyArray).FullName);
      try {
        baseConstructor = baseTypeDef.Methods.GetMethod(WellKnown.CtorName,
          ctorSignature.Translate(module),
          BindingOptions.Default);
      } catch (Exception e) {
        ErrorLog.Debug("..Error: {0}", e);
      }
      while (baseConstructor == null)
      {
        baseType = baseTypeDef.BaseType;
        baseTypeDef = baseType.GetTypeDefinition();
        try {
          baseConstructor = baseType.Methods.GetMethod(WellKnown.CtorName,
            ctorSignature.Translate(module),
            BindingOptions.Default);
        } catch (Exception e) {
          ErrorLog.Debug("..Error: {0}", e);
        }
      }
      writer.EmitInstructionMethod(OpCodeNumber.Call,
        (IMethod)baseConstructor.Translate(module));
      writer.EmitInstruction(OpCodeNumber.Ret);
      writer.DetachInstructionSequence();
    }


    // Constructors

    internal ImplementConstructorWeaver(ITypeSignature[] parameterTypeSignatures)
    {
      this.parameterTypeSignatures = parameterTypeSignatures;
    }
  }
}
