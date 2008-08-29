// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.05.29

using System;
using System.Reflection;
using System.Text;
using PostSharp.CodeModel;
using PostSharp.Laos.Weaver;
using PostSharp.ModuleWriter;
using Xtensive.Core.Aspects;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Weaver
{
  internal class ImplementProtectedConstructorAccessorWeaver : TypeLevelAspectWeaver
  {
    private const string ParameterNamePrefix = "arg";

    private readonly ITypeSignature[] parameterTypes;

    public override void Implement()
    {
      var typeDef = (TypeDefDeclaration) TargetElement;
      var module = Task.Project.Module;

      IMethod constructor = FindConstructor(typeDef, module);
      if (constructor == null)
        return;

      var callerDef = new MethodDefDeclaration();
      callerDef.Name = DelegateHelper.AspectedProtectedConstructorCallerName;
      callerDef.CallingConvention = CallingConvention.Default;
      callerDef.Attributes = MethodAttributes.Private | MethodAttributes.Static;
      typeDef.Methods.Add(callerDef);

      callerDef.ReturnParameter = new ParameterDeclaration();
      callerDef.ReturnParameter.ParameterType = typeDef;
      callerDef.ReturnParameter.Attributes = ParameterAttributes.Retval;
      callerDef.CustomAttributes.Add(Task.WeavingHelper.GetDebuggerNonUserCodeAttribute());

      for (int i = 0; i < parameterTypes.Length; i++) {
        ParameterDeclaration parameter =
          new ParameterDeclaration(i, ParameterNamePrefix+i, parameterTypes[i]);
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

      for (short i = 0; i < parameterTypes.Length; i++)
        writer.EmitInstructionParameter(OpCodeNumber.Ldarg, callerDef.Parameters[i]);

      writer.EmitInstructionMethod(OpCodeNumber.Newobj, constructor);
      writer.EmitInstruction(OpCodeNumber.Ret);
      writer.DetachInstructionSequence();
    }

    private IMethod FindConstructor(TypeDefDeclaration typeDef, ModuleDeclaration module)
    {
      IMethod foundConstructor = null;
      foreach (IMethod constructor in typeDef.Methods.GetByName(WellKnown.CtorName)) {
        if (constructor.ParameterCount == parameterTypes.Length) {
          int i = 0;
          for (; i < parameterTypes.Length; i++) {
            var parameterName = GetTypeName(constructor.GetParameterType(i));
            var targetParameterName = GetTypeName(parameterTypes[i]);
            if (parameterName != targetParameterName)
              break;
          }
          if (i == parameterTypes.Length) {
            foundConstructor = (IMethod) constructor.Translate(module);
            break;
          }
        }
      }
      return foundConstructor;
    }

    internal static string GetTypeName(ITypeSignature parareterTypeSignature)
    {
      var nameBuilder = new StringBuilder();
      parareterTypeSignature.WriteReflectionTypeName(nameBuilder, ReflectionNameOptions.None);
      return nameBuilder.ToString();
    }


    // Constructors

    internal ImplementProtectedConstructorAccessorWeaver(ITypeSignature[] parameterTypes)
    {
      this.parameterTypes = parameterTypes;
    }
  }
}
