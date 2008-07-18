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
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Weaver
{
  internal class ImplementProtectedConstructorAccessorWeaver : LaosAspectWeaver
  {
    private const string CtorName = ".ctor";
    private const string ParameterNamePrefix = "arg";

    private readonly ITypeSignature[] parameterTypeSignatures;
    private readonly ITypeSignature returnTypeSignature;

    public override void Implement()
    {
      TypeDefDeclaration typeDef = (TypeDefDeclaration)TargetElement;
      ModuleDeclaration module = Task.Project.Module;

      ImplementDelegateMethodBody(typeDef, module);
    }

    private void ImplementDelegateMethodBody(TypeDefDeclaration typeDef, ModuleDeclaration module)
    {
      IMethod tupleConstructor = FindConstructor(typeDef, module);
      if (tupleConstructor == null)
        return;

      var callerDef = new MethodDefDeclaration();
      callerDef.Name = DelegateHelper.AspectedProtectedConstructorCallerName;
      callerDef.CallingConvention = CallingConvention.Default;
      callerDef.Attributes = MethodAttributes.Private | MethodAttributes.Static;
      typeDef.Methods.Add(callerDef);

      callerDef.ReturnParameter = new ParameterDeclaration();
      callerDef.ReturnParameter.ParameterType = returnTypeSignature;
      callerDef.ReturnParameter.Attributes = ParameterAttributes.Retval;
      callerDef.CustomAttributes.Add(Task.WeavingHelper.GetDebuggerNonUserCodeAttribute());

      for (int i = 0; i < parameterTypeSignatures.Length; i++) {
        ParameterDeclaration parameter =
          new ParameterDeclaration(i, ParameterNamePrefix+i, parameterTypeSignatures[i]);
        callerDef.Parameters.Add(parameter);
      }

      var methodBody = new MethodBodyDeclaration();
      callerDef.MethodBody = methodBody;
      InstructionBlock instructionBlock = methodBody.CreateInstructionBlock();
      methodBody.RootInstructionBlock = instructionBlock;
      InstructionSequence sequence = methodBody.CreateInstructionSequence();
      instructionBlock.AddInstructionSequence(sequence, PostSharp.Collections.NodePosition.Before, null);
      InstructionWriter writer = Task.InstructionWriter;
      writer.AttachInstructionSequence(sequence);

      for (short i = 0; i < parameterTypeSignatures.Length; i++)
        writer.EmitInstructionParameter(OpCodeNumber.Ldarg, callerDef.Parameters[i]);

      writer.EmitInstructionMethod(OpCodeNumber.Newobj, tupleConstructor);
      writer.EmitInstruction(OpCodeNumber.Ret);
      writer.DetachInstructionSequence();
    }

    private IMethod FindConstructor(TypeDefDeclaration typeDef, ModuleDeclaration module)
    {
      IMethod foundConstructor = null;
      foreach (IMethod constructor in typeDef.Methods.GetByName(CtorName)) {
        if (constructor.ParameterCount == parameterTypeSignatures.Length) {
          int i = 0;
          for (; i < parameterTypeSignatures.Length; i++)
            if (constructor.GetParameterType(i).GetType() != parameterTypeSignatures[i].GetType())
              break;
          if (i == parameterTypeSignatures.Length) {
            foundConstructor = (IMethod) constructor.Translate(module);
            break;
          }
        }
      }
      return foundConstructor;
    }

    public override void EmitCompileTimeInitialization(InstructionEmitter writer)
    {
      return;
    }

    public override bool ValidateSelf()
    {
      return true;
    }


    // Constructors

    internal ImplementProtectedConstructorAccessorWeaver(
      ITypeSignature[] parameterTypeSignatures, 
      ITypeSignature returnTypeSignature)
    {
      this.parameterTypeSignatures = parameterTypeSignatures;
      this.returnTypeSignature = returnTypeSignature;
    }

    internal ImplementProtectedConstructorAccessorWeaver(
      ITypeSignature accessorType)
    {
      // TODO: implement
    }
  }
}
