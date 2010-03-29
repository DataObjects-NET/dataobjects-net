// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.18

using System;
using PostSharp.AspectWeaver.AspectWeavers;
using PostSharp.CodeModel;
using PostSharp.CodeModel.Helpers;
using PostSharp.Extensibility;
using Xtensive.Core.Aspects;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Weaver
{
//  internal class ImplementProtectedConstructorBodyWeaver : TypeLevelAspectWeaver
//  {
//    private readonly ITypeSignature[] argumentTypes;
//    private readonly Type targetType;
//
//    public override void Implement()
//    {
//      var type = targetType;
//      var typeDef = Task.Project.Module.Domain.FindTypeDefinition(type);
//      var baseType = typeDef.BaseType;
//      var module = Task.Project.Module;
//
//      var ctorSignature =
//        new MethodSignature(
//          CallingConvention.HasThis,
//          module.Cache.GetIntrinsic(IntrinsicType.Void),
//          argumentTypes,
//          0);
//
//      var ctorDef = (MethodDefDeclaration)typeDef.Methods.GetMethod(WellKnown.CtorName,
//          ctorSignature.Translate(module),
//          BindingOptions.Default);
//      var body = ctorDef.MethodBody;
//
//      IMethod baseConstructor = null;
//      try {
//        baseConstructor = baseType.Methods.GetMethod(WellKnown.CtorName,
//          ctorSignature.Translate(module),
//          BindingOptions.Default);
//      } catch (Exception e) {
//        ErrorLog.Debug("Error: {0}", e);
//        return;
//      }
//
//      var sequence = body.CreateInstructionSequence();
//      body.RootInstructionBlock.AddInstructionSequence(sequence, PostSharp.Collections.NodePosition.After, null);
//      var writer = Task.InstructionWriter;
//      writer.AttachInstructionSequence(sequence);
//      writer.EmitInstruction(OpCodeNumber.Ldarg_0);
//      for (short i = 0; i < argumentTypes.Length; i++)
//        writer.EmitInstructionParameter(OpCodeNumber.Ldarg_S, ctorDef.Parameters[i]);
//      
//      writer.EmitInstructionMethod(OpCodeNumber.Call,
//        (IMethod)baseConstructor.Translate(module));
//      writer.EmitInstruction(OpCodeNumber.Ret);
//      writer.DetachInstructionSequence();
//
//      ErrorLog.Write(SeverityType.Warning,
//        "Implementing .ctor body for {0}, module: {1}.", type, module);
//    }
//
//    public ImplementProtectedConstructorBodyWeaver(Type targetType, ITypeSignature[] argumentTypes)
//    {
//      this.argumentTypes = argumentTypes;
//      this.targetType = targetType;
//    }
//  }
}