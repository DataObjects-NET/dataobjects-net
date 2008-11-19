// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.05.29

using System;
using System.Reflection;
using System.Text;
using PostSharp.CodeModel;
using PostSharp.CodeModel.Helpers;
using PostSharp.Extensibility;
using PostSharp.Laos.Weaver;
using Xtensive.Core.Aspects;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Weaver
{
  internal class ProtectedConstructorAccessorWeaver : TypeLevelAspectWeaver
  {
    private const string ParameterNamePrefix = "arg";

    private readonly ITypeSignature[] parameterTypes;

    public override void Implement()
    {
      var typeDef = (TypeDefDeclaration) TargetElement;
      var genericType = GenericHelper.GetTypeCanonicalGenericInstance(typeDef);
      var module = Task.Project.Module;

      var ctorSignature = new MethodSignature(
        CallingConvention.HasThis,
        module.Cache.GetIntrinsic(IntrinsicType.Void),
        parameterTypes,
        0);

      IMethod ctor;
      try {
        ctor = genericType.Methods.GetMethod(WellKnown.CtorName,
          ctorSignature.Translate(module),
          BindingOptions.Default);
      }
      catch (Exception e) {
        ErrorLog.Debug("..Error: {0}", e);
        return;
      }

      var callerDef = new MethodDefDeclaration();
      callerDef.Name = DelegateHelper.AspectedProtectedConstructorCallerName;
      callerDef.CallingConvention = CallingConvention.Default;
      callerDef.Attributes = MethodAttributes.Private | MethodAttributes.Static;
      typeDef.Methods.Add(callerDef);

      callerDef.ReturnParameter = new ParameterDeclaration();
      callerDef.ReturnParameter.ParameterType = genericType;
      callerDef.ReturnParameter.Attributes = ParameterAttributes.Retval;
      callerDef.CustomAttributes.Add(Task.WeavingHelper.GetDebuggerNonUserCodeAttribute());

      for (int i = 0; i < parameterTypes.Length; i++) {
        var parameter = new ParameterDeclaration(i, ParameterNamePrefix+i, parameterTypes[i]);
        callerDef.Parameters.Add(parameter);
      }

      var body = new MethodBodyDeclaration();
      callerDef.MethodBody = body;
      var instructionBlock = body.CreateInstructionBlock();
      body.RootInstructionBlock = instructionBlock;
      var sequence = body.CreateInstructionSequence();
      instructionBlock.AddInstructionSequence(sequence, PostSharp.Collections.NodePosition.Before, null);
      InstructionWriter writer = Task.InstructionWriter;
      writer.AttachInstructionSequence(sequence);

      for (short i = 0; i < parameterTypes.Length; i++)
        writer.EmitInstructionParameter(OpCodeNumber.Ldarg, callerDef.Parameters[i]);

      writer.EmitInstructionMethod(OpCodeNumber.Newobj, ctor);
      writer.EmitInstruction(OpCodeNumber.Ret);
      writer.DetachInstructionSequence();

      ErrorLog.Write(SeverityType.Warning, 
        "Declaring .ctor accessor for {0}, module: {1}.", typeDef, module);
    }

    private IMethod FindConstructor(IType typeDef, ModuleDeclaration module)
    {
      IMethod foundConstructor = null;
      foreach (var ctor in typeDef.Methods.GetByName(WellKnown.CtorName)) {
        if (ctor.ParameterCount == parameterTypes.Length) {
          int i = 0;
          for (; i < parameterTypes.Length; i++) {
            var parameterName = GetTypeName(ctor.GetParameterType(i));
            var targetParameterName = GetTypeName(parameterTypes[i]);
            if (parameterName != targetParameterName)
              break;
          }
          if (i == parameterTypes.Length) {
            foundConstructor = (IMethod) ctor.Translate(module);
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

    internal ProtectedConstructorAccessorWeaver(ITypeSignature[] parameterTypes)
    {
      this.parameterTypes = parameterTypes;
    }
  }
}
