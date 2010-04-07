// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.03.29

using System;
using System.Collections.Generic;
using PostSharp.AspectInfrastructure;
using PostSharp.AspectInfrastructure.Helpers;
using PostSharp.Aspects;
using PostSharp.AspectWeaver;
using PostSharp.AspectWeaver.AspectWeavers;
using PostSharp.AspectWeaver.Transformations;
using PostSharp.CodeModel;
using PostSharp.CodeModel.Helpers;
using PostSharp.CodeModel.TypeSignatures;
using PostSharp.CodeWeaver;
using PostSharp.Collections;
using PostSharp.Extensibility;
using PostSharp.Extensibility.Tasks;
using Xtensive.Core.Aspects;
using Xtensive.Core.Aspects.Helpers;
using Xtensive.Core.Weaver.Resources;

namespace Xtensive.Core.Weaver
{
  internal class ConstructorEpilogueWeaver : MethodLevelAspectWeaver
  {
    private ConstructorEpilogueTransformation transformation;

    protected override void Initialize()
    {
      base.Initialize();

      transformation = new ConstructorEpilogueTransformation(this);
      ApplyEffectWaivers(transformation);
      RequiresRuntimeInstance = false;
      RequiresRuntimeInstanceInitialization = false;
      RequiresRuntimeReflectionObject = false;
    }

    protected override AspectWeaverInstance CreateAspectWeaverInstance(AspectInstanceInfo aspectInstanceInfo)
    {
      return new Instance(this, aspectInstanceInfo);
    }


    // Constructors

    public ConstructorEpilogueWeaver()
      : base(null, MulticastTargets.InstanceConstructor)
    { }


    // Nested class

    private class Instance : MethodLevelAspectWeaverInstance
    {
      private readonly ConstructorEpilogueWeaver parent;

      public override void ProvideAspectTransformations(AspectWeaverTransformationAdder adder)
      {
        adder.Add(TargetElement, parent.transformation.CreateInstance(this));
      }

      public Instance(ConstructorEpilogueWeaver parent, AspectInstanceInfo aspectInstanceInfo)
        : base(parent, aspectInstanceInfo)
      {
        this.parent = parent;
      }
    }
  }

  internal class ConstructorEpilogueTransformation : MethodBodyTransformation
  {
    public override string GetDisplayName(MethodSemantics semantic)
    {
      return "Applying constructor epilogue.";
    }

    public AspectWeaverTransformationInstance CreateInstance(AspectWeaverInstance aspectWeaverInstance)
    {
      var aspect = (ImplementConstructorEpilogue)aspectWeaverInstance.Aspect;
      var module = AspectWeaver.Module;
      var handlerType = module.Cache.GetType(aspect.HandlerType);
      var handlerMethodName = aspect.HandlerMethodName;
      var errorHandlerMethodName = aspect.ErrorHandlerMethodName;

      return new Instance(this, aspectWeaverInstance, handlerType, handlerMethodName, errorHandlerMethodName);
    }


    // Constructors

    public ConstructorEpilogueTransformation(AspectWeaver aspectWeaver)
      : base(aspectWeaver, null)
    {
    }

    // Nested class

    private class Instance : MethodBodyTransformationInstance
    {
      private const string GetTypeFromHandleMethodName = "GetTypeFromHandle";
      private readonly ITypeSignature handlerTypeSignature;
      private readonly string handlerMethodName;
      private readonly string errorHandlerMethodName;
      
      public override void Implement(MethodBodyTransformationContext context)
      {
        new Wrapper(this, context).Implement();
      }

      public override MethodBodyTransformationOptions GetOptions(MetadataDeclaration originalTargetElement, MethodSemantics semantic)
      {
        return MethodBodyTransformationOptions.None;
      }


      // Constructors

      public Instance(MethodBodyTransformation parent, AspectWeaverInstance aspectWeaverInstance, ITypeSignature handlerTypeSignature, string handlerMethodName, string errorHandlerMethodName)
        : base(parent, aspectWeaverInstance)
      {
        this.handlerTypeSignature = handlerTypeSignature;
        this.handlerMethodName = handlerMethodName;
        this.errorHandlerMethodName = errorHandlerMethodName;
      }

      private class Wrapper : MethodBodyWrappingImplementation
      {
        private readonly Instance parent;
        private readonly MethodBodyTransformationContext context;
        private IMethod getTypeFromHandleMethod;
        private IMethod errorHandlerMethod;
        private IMethod handlerMethod;
        private MethodDefDeclaration targetMethodDef;

        public void Implement()
        {
          Initialize();
          var module = parent.AspectWeaver.Module;
          var hasOnError = !parent.errorHandlerMethodName.IsNullOrEmpty();
          var exceptionType = hasOnError 
            ? module.Cache.GetType(typeof (Exception)) 
            : null;
          Implement(false, true, false, hasOnError ? new [] {exceptionType} : null);
          Context.AddRedirection(Redirection);
        }

        private void Initialize()
        {
          targetMethodDef = (MethodDefDeclaration) context.TargetElement;

          var baseTypeRef = parent.handlerTypeSignature.GetTypeDefinition();
          if (baseTypeRef == null)
            return;

          var genericType = GenericHelper.GetTypeCanonicalGenericInstance(baseTypeRef);
          var module = parent.AspectWeaver.Module;

          getTypeFromHandleMethod = module.FindMethod(
            typeof (Type).GetMethod(GetTypeFromHandleMethodName,
                                    new[] {typeof (RuntimeTypeHandle)}), BindingOptions.Default);

          var handlerSignature = new MethodSignature(
            module, 
            CallingConvention.HasThis, 
            module.Cache.GetIntrinsic(IntrinsicType.Void),
            new[] {getTypeFromHandleMethod.ReturnType}, 0);
          handlerMethod = (IMethod) genericType.Methods.GetMethod(
            parent.handlerMethodName,
            handlerSignature.Translate(module),
            BindingOptions.Default).Translate(module);

          if (parent.errorHandlerMethodName.IsNullOrEmpty()) return;
          var errorHandlerSignature = new MethodSignature(
            module, 
            CallingConvention.HasThis, 
            module.Cache.GetIntrinsic(IntrinsicType.Void),
            new[] { getTypeFromHandleMethod.ReturnType, module.FindType(typeof (Exception), BindingOptions.Default)}, 0);
          errorHandlerMethod = (IMethod) genericType.Methods.GetMethod(
            parent.errorHandlerMethodName,
            errorHandlerSignature.Translate(module),
            BindingOptions.Default).Translate(module);
        }

        protected override void WeaveOnException(InstructionBlock block, ITypeSignature exceptionType, InstructionWriter writer)
        {
          var module = parent.AspectWeaver.Module;
          var methodBody = block.MethodBody;
          var exceptionLocal = methodBody.RootInstructionBlock.DefineLocalVariable(module.Cache.GetType(typeof(Exception)), "~exception~{0}");
          var sequence = methodBody.CreateInstructionSequence();
          block.AddInstructionSequence( sequence, NodePosition.Before, null );
          
          writer.AttachInstructionSequence( sequence );
          writer.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);
          writer.EmitInstructionLocalVariable(OpCodeNumber.Stloc, exceptionLocal);
          writer.EmitInstruction(OpCodeNumber.Ldarg_0); // Push "this"
          writer.EmitInstructionType(OpCodeNumber.Ldtoken, targetMethodDef.DeclaringType.Translate(module));
          writer.EmitInstructionMethod(OpCodeNumber.Call, getTypeFromHandleMethod); // Push "typeof(...)"
          writer.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, exceptionLocal); // Push "exception"
          writer.EmitInstructionMethod(OpCodeNumber.Callvirt, errorHandlerMethod);
          writer.EmitInstruction(OpCodeNumber.Rethrow);
          writer.DetachInstructionSequence();
        }

        protected override void WeaveOnExit(InstructionBlock block, InstructionWriter writer)
        {
        }

        protected override void WeaveOnSuccess(InstructionBlock block, InstructionWriter writer)
        {
          var module = parent.AspectWeaver.Module;
          var methodBody = block.MethodBody;
          var sequence = methodBody.CreateInstructionSequence();
          block.AddInstructionSequence( sequence, NodePosition.Before, null );
    
          writer.AttachInstructionSequence( sequence );
          writer.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);
          writer.EmitInstruction(OpCodeNumber.Ldarg_0); // Push "this"
          writer.EmitInstructionType(OpCodeNumber.Ldtoken, targetMethodDef.DeclaringType.Translate(module));
          writer.EmitInstructionMethod(OpCodeNumber.Call, getTypeFromHandleMethod); // Push "typeof(...)"
          writer.EmitInstructionMethod(OpCodeNumber.Callvirt, handlerMethod);
          writer.DetachInstructionSequence();
        }

        protected override void WeaveOnEntry(InstructionBlock block, InstructionWriter writer)
        {
        }


        // Constructors

        public Wrapper(Instance parent, MethodBodyTransformationContext context)
          : base(parent.Transformation.AspectInfrastructureTask, context)
        {
          this.parent = parent;
          this.context = context;
        }
      }
    }
  }
}