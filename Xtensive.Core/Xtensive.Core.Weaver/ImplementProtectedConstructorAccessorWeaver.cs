// Copyright (C) 2003-2010 Xtensive LLC.
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
  internal class ImplementProtectedConstructorAccessorWeaver : TypeLevelAspectWeaver
  {
    private const string ParameterNamePrefix = "arg";

    private readonly Type targetType;
    private readonly ITypeSignature[] argumentTypes;

    public override void Implement()
    {
      var type = targetType;
      var typeDef = Task.Project.Module.Domain.FindTypeDefinition(type);
      var genericType = GenericHelper.GetTypeCanonicalGenericInstance(typeDef);
      var module = Task.Project.Module;

      var ctorSignature = new MethodSignature(
        CallingConvention.HasThis,
        module.Cache.GetIntrinsic(IntrinsicType.Void),
        argumentTypes,
        0);

      IMethod ctor = genericType.Methods.GetMethod(WellKnown.CtorName,
        ctorSignature.Translate(module),
        BindingOptions.Default);

      var callerDef = new MethodDefDeclaration();
      callerDef.Name = DelegateHelper.AspectedProtectedConstructorCallerName;
      callerDef.CallingConvention = CallingConvention.Default;
      callerDef.Attributes = MethodAttributes.Private | MethodAttributes.Static;
      typeDef.Methods.Add(callerDef);

      callerDef.ReturnParameter = new ParameterDeclaration();
      callerDef.ReturnParameter.ParameterType = genericType;
      callerDef.ReturnParameter.Attributes = ParameterAttributes.Retval;
      callerDef.CustomAttributes.Add(Task.WeavingHelper.GetDebuggerNonUserCodeAttribute());

      for (int i = 0; i < argumentTypes.Length; i++) {
        var parameter = new ParameterDeclaration(i, ParameterNamePrefix+i, argumentTypes[i]);
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

      for (short i = 0; i < argumentTypes.Length; i++)
        writer.EmitInstructionParameter(OpCodeNumber.Ldarg, callerDef.Parameters[i]);

      writer.EmitInstructionMethod(OpCodeNumber.Newobj, ctor);
      writer.EmitInstruction(OpCodeNumber.Ret);
      writer.DetachInstructionSequence();

//      ErrorLog.Write(SeverityType.Warning, 
//        "Implementing .ctor accessor for {0}, module: {1}.", typeDef, module);
    }


    // Constructors

    public ImplementProtectedConstructorAccessorWeaver(Type targetType, ITypeSignature[] argumentTypes)
    {
      this.targetType = targetType;
      this.argumentTypes = argumentTypes;
    }
  }
}
