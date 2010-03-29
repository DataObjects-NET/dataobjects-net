// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.18

using System;
using System.Net.NetworkInformation;
using System.Reflection;
using PostSharp.AspectInfrastructure;
using PostSharp.AspectWeaver;
using PostSharp.AspectWeaver.AspectWeavers;
using PostSharp.AspectWeaver.Transformations;
using PostSharp.CodeModel;
using PostSharp.Extensibility;
using Xtensive.Core.Aspects;
using Xtensive.Core.Aspects.Helpers;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Weaver
{
  internal class DeclareProtectedConstructorWeaver : TypeLevelAspectWeaver
  {
    private DeclareProtectedConstructorTransformation transformation;

    protected override void Initialize()
    {
      base.Initialize();

      transformation = new DeclareProtectedConstructorTransformation(this);
      ApplyEffectWaivers(transformation);
    }

    protected override AspectWeaverInstance CreateAspectWeaverInstance(AspectInstanceInfo aspectInstanceInfo)
    {
      return new Instance(this, aspectInstanceInfo);
    }


    // Constructors

    public DeclareProtectedConstructorWeaver()
      : base(null, MulticastTargets.Class)
    { }


    // Nested class

    private class Instance : TypeLevelAspectWeaverInstance
    {
      private readonly DeclareProtectedConstructorWeaver parent;

      public override void ProvideAspectTransformations(AspectWeaverTransformationAdder adder)
      {
        adder.Add(TargetElement, parent.transformation.CreateInstance(this));
      }

      public Instance(DeclareProtectedConstructorWeaver parent, AspectInstanceInfo aspectInstanceInfo)
        : base(parent, aspectInstanceInfo)
      {
        this.parent = parent;
      }
    }
  }

  internal class DeclareProtectedConstructorTransformation : StructuralTransformation
  {
    public override string GetDisplayName(MethodSemantics semantic)
    {
      return "sd";
    }

    public AspectWeaverTransformationInstance CreateInstance(AspectWeaverInstance aspectWeaverInstance)
    {
//      var aspect = (ProtectedConstructorAspect)aspectWeaverInstance.Aspect;
//      var module = AspectWeaver.Module;
//      var handlerType = module.Cache.GetType(aspect.HandlerType);
//      var handlerMethodName = aspect.HandlerMethodName;
//      var errorHandlerMethodName = aspect.ErrorHandlerMethodName;

//      return new Instance(this, aspectWeaverInstance, handlerType, handlerMethodName, errorHandlerMethodName);
      throw new NetworkInformationException();
    }


    // Constructors

    public DeclareProtectedConstructorTransformation(AspectWeaver aspectWeaver)
      : base(aspectWeaver)
    {
    }

    // Nested class

    private class Instance : StructuralTransformationInstance
    {
      private const string GetTypeFromHandleMethodName = "GetTypeFromHandle";
      private readonly ITypeSignature handlerTypeSignature;
      private readonly string handlerMethodName;
      private readonly string errorHandlerMethodName;
      private IMethod getTypeFromHandleMethod;
      private IMethod errorHandlerMethod;
      private IMethod handlerMethod;
      private MethodDefDeclaration targetMethodDef;

      public override void Implement(StructuralTransformationContext context)
      {
      }


      // Constructors

      public Instance(DeclareProtectedConstructorTransformation parent, AspectWeaverInstance aspectWeaverInstance, ITypeSignature handlerTypeSignature, string handlerMethodName, string errorHandlerMethodName)
        : base(parent, aspectWeaverInstance)
      {
        this.handlerTypeSignature = handlerTypeSignature;
        this.handlerMethodName = handlerMethodName;
        this.errorHandlerMethodName = errorHandlerMethodName;
      }
    }
  }
}

//  internal class DeclareProtectedConstructorAspectWeaver : TypeLevelAspectWeaver
//  {
//    private const string ParameterNamePrefix = "arg";
//    private readonly ITypeSignature[] argumentTypes;
//    private readonly Type targetType;
//
//    public override void Implement()
//    {
//      var type = targetType;
//      var typeDef = Task.Project.Module.Domain.FindTypeDefinition(type);
//      var module = Task.Project.Module;
//
//      var ctorDef = new MethodDefDeclaration();
//      ctorDef.Name = WellKnown.CtorName;
//      ctorDef.CallingConvention = CallingConvention.HasThis;
//      ctorDef.Attributes = MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
//      typeDef.Methods.Add(ctorDef);
//      ctorDef.CustomAttributes.Add(Task.WeavingHelper.GetDebuggerNonUserCodeAttribute());
//      ctorDef.ReturnParameter = new ParameterDeclaration();
//      ctorDef.ReturnParameter.Name = string.Empty;
//      ctorDef.ReturnParameter.ParameterType = module.Cache.GetIntrinsic(IntrinsicType.Void);
//      ctorDef.ReturnParameter.Attributes = ParameterAttributes.Retval;
//
//      for (int i = 0; i < argumentTypes.Length; i++)
//        ctorDef.Parameters.Add(new ParameterDeclaration(i, ParameterNamePrefix + i, argumentTypes[i]));
//      ctorDef.MethodBody = new MethodBodyDeclaration();
//      ctorDef.MethodBody.RootInstructionBlock = ctorDef.MethodBody.CreateInstructionBlock();
//
//      ErrorLog.Write(SeverityType.Warning, 
//        "Declaring .ctor for {0}, module: {1}.", type, module);
//    }
//
//
    // Constructors
//
//    public DeclareProtectedConstructorAspectWeaver(Type targetType, ITypeSignature[] argumentTypes)
//    {
//      this.argumentTypes = argumentTypes;
//      this.targetType = targetType;
//    }
// }