// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.03.29

using System;
using System.Collections.Generic;
using PostSharp.AspectInfrastructure;
using PostSharp.AspectWeaver;
using PostSharp.AspectWeaver.AspectWeavers;
using PostSharp.AspectWeaver.Transformations;
using PostSharp.CodeModel;
using PostSharp.CodeModel.Helpers;
using PostSharp.CodeModel.TypeSignatures;
using PostSharp.Collections;
using PostSharp.Extensibility;
using PostSharp.Extensibility.Tasks;
using Xtensive.Core.Aspects.Helpers;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Weaver
{
  internal class ReplaceAutoPropertyWeaver : MethodLevelAspectWeaver
  {
    private ReplaceAutoPropertyTransformation transformation;

    protected override void Initialize()
    {
      base.Initialize();

      transformation = new ReplaceAutoPropertyTransformation(this, "AutoProperty replacement.");
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

    public ReplaceAutoPropertyWeaver()
      : base(null, MulticastTargets.Class)
    {}


    // Nested class

    private class Instance : MethodLevelAspectWeaverInstance
    {
      private readonly ReplaceAutoPropertyWeaver parent;

      public override void ProvideAspectTransformations(AspectWeaverTransformationAdder adder)
      {
        adder.Add(TargetElement, parent.transformation.CreateInstance(this));
      }

      public Instance(ReplaceAutoPropertyWeaver parent, AspectInstanceInfo aspectInstanceInfo)
        : base(parent, aspectInstanceInfo)
      {
        this.parent = parent;
      }
    }
  }

  internal class ReplaceAutoPropertyTransformation : MethodBodyTransformation
  {
    private readonly string displayName;

    public override string GetDisplayName(MethodSemantics semantic)
    {
      return displayName;
    }

    public AspectWeaverTransformationInstance CreateInstance(AspectWeaverInstance aspectWeaverInstance)
    {
      var aspect = (ReplaceAutoProperty)aspectWeaverInstance.Aspect;
      var module = AspectWeaver.Module;
      var handlerMethodSuffix = aspect.HandlerMethodSuffix;
            
      return new Instance(this, aspectWeaverInstance, handlerMethodSuffix);
    }


    // Constructors

    public ReplaceAutoPropertyTransformation(AspectWeaver aspectWeaver, string displayName)
      : base(aspectWeaver, displayName)
    {
      this.displayName = displayName;
    }

    // Nested class

    private class Instance : MethodBodyTransformationInstance
    {
      private const string AutoPropertyBackingFieldFormat = "<{0}>k__BackingField";
      private const string HandlerGetMethodPrefix = "Get";
      private const string HandlerSetMethodPrefix = "Set";
      private readonly string handlerMethodSuffix;

      public override void Implement(MethodBodyTransformationContext context)
      {
        var targetMethod = (MethodDefDeclaration)context.TargetElement;
        var targetType = AspectWeaverInstance.TargetType;
         
        int splitterPos = targetMethod.Name.IndexOf('_');
        if (splitterPos <= 0)
          return;

        string propertyName = targetMethod.Name.Substring(splitterPos + 1);
        string fieldName = string.Format(AutoPropertyBackingFieldFormat, propertyName);
        bool isGetter = targetMethod.Name.Substring(0, splitterPos + 1) == WellKnown.GetterPrefix;

        var fieldDef = targetType.Fields.GetByName(fieldName);
        if (fieldDef == null)
          return;

        var module = AspectWeaver.Module;
        var methodBody = context.InstructionBlock.MethodBody;
        var instructionBlock = methodBody.CreateInstructionBlock();
        methodBody.RootInstructionBlock = instructionBlock;
        var sequence = methodBody.CreateInstructionSequence();
        instructionBlock.AddInstructionSequence(sequence, NodePosition.After, null);
        using (var writer = new InstructionWriter()) {
          writer.AttachInstructionSequence(sequence);

          writer.EmitInstruction(OpCodeNumber.Ldarg_0);
          writer.EmitInstructionString(OpCodeNumber.Ldstr, propertyName);
          if (!isGetter)
            writer.EmitInstruction(OpCodeNumber.Ldarg_1);
          var returnType = isGetter
            ? (ITypeSignature) module.Cache.GetGenericParameter(0, GenericParameterKind.Method)
            : module.Cache.GetIntrinsic(IntrinsicType.Void);
          var parameterTypes = isGetter
           ? new ITypeSignature[] { module.Cache.GetIntrinsic(IntrinsicType.String) }
           : new ITypeSignature[] {
                 module.Cache.GetIntrinsic(IntrinsicType.String),
                 GenericParameterTypeSignature.GetInstance(module, 0, GenericParameterKind.Method)
               };
          var methodSignature = new MethodSignature(module, CallingConvention.HasThis, returnType, parameterTypes, 1);
          var methodName = (isGetter ? HandlerGetMethodPrefix : HandlerSetMethodPrefix) + handlerMethodSuffix;
          var genericMethodReference = targetType.FindMethod(methodName, methodSignature);
          if (genericMethodReference != null) {
//            var typeMap = targetType.GetGenericContext(GenericContextOptions.None);
//            var methodMap = new GenericMap(typeMap, new[] {fieldDef.FieldType});
//            var genericMap = new GenericMap(new List<ITypeSignature>(), new List<ITypeSignature>() {fieldDef.FieldType});
//            var genericContext = targetMethod.GetGenericContext(GenericContextOptions.ResolveGenericParameterDefinitions);
//            var method = genericMethodReference.GetInstance(module, typeMap).TranslateMethod(module);
            var method = genericMethodReference.Method.FindGenericInstance(
              new[] {fieldDef.FieldType},
              BindingOptions.Default);
            writer.EmitInstructionMethod(OpCodeNumber.Callvirt, method);
          }
          else
            throw new InvalidOperationException();
          writer.EmitInstruction(OpCodeNumber.Ret);
          writer.DetachInstructionSequence();
        }

        try
        {
          var project = Transformation.AspectWeaver.AspectWeaverTask.Project;
          RemoveTask.GetTask(project).MarkForRemoval(fieldDef);
        }
        catch {
          // Field is already marked for removal
        }
      }

      public override MethodBodyTransformationOptions GetOptions(MetadataDeclaration originalTargetElement, MethodSemantics semantic)
      {
        return MethodBodyTransformationOptions.CreateMethodBody;
      }


      // Constructors

      public Instance(MethodBodyTransformation parent, AspectWeaverInstance aspectWeaverInstance, string handlerMethodSuffix) 
        : base(parent, aspectWeaverInstance)
      {
        this.handlerMethodSuffix = handlerMethodSuffix;
      }
    }
  }
}
