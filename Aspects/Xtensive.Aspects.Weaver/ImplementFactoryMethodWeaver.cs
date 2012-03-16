// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.04.07

using System;
using System.Linq;
using System.Reflection;
using PostSharp;
using PostSharp.Extensibility;
using PostSharp.Sdk.AspectInfrastructure;
using PostSharp.Sdk.AspectWeaver;
using PostSharp.Sdk.AspectWeaver.AspectWeavers;
using PostSharp.Sdk.AspectWeaver.Transformations;
using PostSharp.Sdk.CodeModel;
using PostSharp.Sdk.CodeWeaver;
using PostSharp.Sdk.Collections;
using Xtensive.Aspects;
using Xtensive.Aspects.Helpers;

namespace Xtensive.Aspects.Weaver
{
  internal class ImplementFactoryMethodWeaver : TypeLevelAspectWeaver
  {
    private ImplementFactoryMethodTransformation transformation;

    protected override void Initialize()
    {
      base.Initialize();

      transformation = new ImplementFactoryMethodTransformation(this);
      ApplyWaivedEffects(transformation);
      RequiresRuntimeInstance = false;
      RequiresRuntimeInstanceInitialization = false;
      RequiresRuntimeReflectionObject = false;
    }

    protected override AspectWeaverInstance CreateAspectWeaverInstance(AspectInstanceInfo aspectInstanceInfo)
    {
      return new Instance(this, aspectInstanceInfo);
    }


    // Constructors

    public ImplementFactoryMethodWeaver()
      : base(null, MulticastTargets.Class)
    { }


    // Nested class

    private class Instance : TypeLevelAspectWeaverInstance
    {
      private readonly ImplementFactoryMethodWeaver parent;

      public override void ProvideAspectTransformations(AspectWeaverTransformationAdder adder)
      {
        adder.Add(TargetElement, parent.transformation.CreateInstance(this));
      }

      public Instance(ImplementFactoryMethodWeaver parent, AspectInstanceInfo aspectInstanceInfo)
        : base(parent, aspectInstanceInfo)
      {
        this.parent = parent;
      }
    }
  }

  internal class ImplementFactoryMethodTransformation : StructuralTransformation
  {
    public override string GetDisplayName(MethodSemantics semantic)
    {
      return "Implementing factory method";
    }

    public AspectWeaverTransformationInstance CreateInstance(AspectWeaverInstance aspectWeaverInstance)
    {
      var module = AspectWeaver.Module;
      var aspect = (ImplementFactoryMethod)aspectWeaverInstance.Aspect;
      var argumentTypes = aspect.ParameterTypes.Select(t => module.Cache.GetType(t)).ToArray();
      return new Instance(this, aspectWeaverInstance, argumentTypes);
    }


    // Constructors

    public ImplementFactoryMethodTransformation(AspectWeaver aspectWeaver)
      : base(aspectWeaver)
    {
    }

    // Nested class

    private class Instance : StructuralTransformationInstance
    {
      private const string parameterNamePrefix = "arg";

      private readonly ITypeSignature[] argumentTypes;

      public override void Implement(TransformationContext context)
      {
        var typeDef = (TypeDefDeclaration)context.TargetElement;
        var genericType = typeDef.GetCanonicalGenericInstance();
        var module = AspectWeaver.Module;
        var helper = new WeavingHelper(module);

        var ctorSignature = new MethodSignature(
          module,
          CallingConvention.HasThis,
          module.Cache.GetIntrinsic(IntrinsicType.Void),
          argumentTypes,
          0);

        IMethod constructor;
        try {
          constructor = genericType.Methods
           .GetMethod(AspectHelper.CtorName, ctorSignature.Translate(module), BindingOptions.Default);
        }
        catch (Exception e) {
          ErrorLog.Write(MessageLocation.Of(typeDef), SeverityType.Warning, "{0}", e);
          return;
        }
  
        var factoryMathodDef = new MethodDefDeclaration();
        factoryMathodDef.Name = AspectHelper.AspectedFactoryMethodName;
        factoryMathodDef.CallingConvention = CallingConvention.Default;
        factoryMathodDef.Attributes = MethodAttributes.Private | MethodAttributes.Static;
        typeDef.Methods.Add(factoryMathodDef);
  
        factoryMathodDef.ReturnParameter = new ParameterDeclaration();
        factoryMathodDef.ReturnParameter.ParameterType = genericType;
        factoryMathodDef.ReturnParameter.Attributes = ParameterAttributes.Retval;
        factoryMathodDef.CustomAttributes.Add(helper.GetDebuggerNonUserCodeAttribute());
        factoryMathodDef.CustomAttributes.Add(helper.GetCompilerGeneratedAttribute());
  
        for (int i = 0; i < argumentTypes.Length; i++) {
          var parameter = new ParameterDeclaration(i, parameterNamePrefix+i, argumentTypes[i]);
          factoryMathodDef.Parameters.Add(parameter);
        }
  
        var body = new MethodBodyDeclaration();
        factoryMathodDef.MethodBody = body;
        var instructionBlock = body.CreateInstructionBlock();
        body.RootInstructionBlock = instructionBlock;
        var sequence = body.CreateInstructionSequence();
        instructionBlock.AddInstructionSequence(sequence, NodePosition.Before, null);
        using (var writer = new InstructionWriter()) {
          writer.AttachInstructionSequence(sequence);

          for (short i = 0; i < argumentTypes.Length; i++)
            writer.EmitInstructionParameter(OpCodeNumber.Ldarg, factoryMathodDef.Parameters[i]);

          writer.EmitInstructionMethod(OpCodeNumber.Newobj, constructor);
          writer.EmitInstruction(OpCodeNumber.Ret);
          writer.DetachInstructionSequence();
        }
      }


      // Constructors

      public Instance(ImplementFactoryMethodTransformation parent, AspectWeaverInstance aspectWeaverInstance, ITypeSignature[] argumentTypes)
        : base(parent, aspectWeaverInstance)
      {
        this.argumentTypes = argumentTypes;
      }
    }
  }
}