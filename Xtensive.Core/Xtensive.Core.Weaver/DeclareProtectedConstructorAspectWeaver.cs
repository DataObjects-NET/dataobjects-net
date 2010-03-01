// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.18

using System;
using System.Reflection;
using PostSharp.CodeModel;
using PostSharp.Extensibility;
using PostSharp.Laos.Weaver;
using Xtensive.Core.Aspects;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Weaver
{
  internal class DeclareProtectedConstructorAspectWeaver : TypeLevelAspectWeaver
  {
    private const string ParameterNamePrefix = "arg";
    private readonly ITypeSignature[] argumentTypes;
    private readonly Type targetType;

    public override void Implement()
    {
      var type = targetType;
      var typeDef = Task.Project.Module.Domain.FindTypeDefinition(type);
      var module = Task.Project.Module;

      var ctorDef = new MethodDefDeclaration();
      ctorDef.Name = WellKnown.CtorName;
      ctorDef.CallingConvention = CallingConvention.HasThis;
      ctorDef.Attributes = MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
      typeDef.Methods.Add(ctorDef);
      ctorDef.CustomAttributes.Add(Task.WeavingHelper.GetDebuggerNonUserCodeAttribute());
      ctorDef.ReturnParameter = new ParameterDeclaration();
      ctorDef.ReturnParameter.Name = string.Empty;
      ctorDef.ReturnParameter.ParameterType = module.Cache.GetIntrinsic(IntrinsicType.Void);
      ctorDef.ReturnParameter.Attributes = ParameterAttributes.Retval;

      for (int i = 0; i < argumentTypes.Length; i++)
        ctorDef.Parameters.Add(new ParameterDeclaration(i, ParameterNamePrefix + i, argumentTypes[i]));
      ctorDef.MethodBody = new MethodBodyDeclaration();
      ctorDef.MethodBody.RootInstructionBlock = ctorDef.MethodBody.CreateInstructionBlock();

//      ErrorLog.Write(SeverityType.Warning, 
//        "Declaring .ctor for {0}, module: {1}.", type, module);
    }


    // Constructors

    public DeclareProtectedConstructorAspectWeaver(Type targetType, ITypeSignature[] argumentTypes)
    {
      this.argumentTypes = argumentTypes;
      this.targetType = targetType;
    }
  }
}