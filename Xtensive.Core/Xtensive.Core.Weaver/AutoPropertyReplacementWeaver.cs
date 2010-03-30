// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.03.29

using System;
using PostSharp.AspectInfrastructure;
using PostSharp.AspectWeaver;
using PostSharp.AspectWeaver.AspectWeavers;
using PostSharp.AspectWeaver.Transformations;
using PostSharp.CodeModel;
using PostSharp.CodeModel.TypeSignatures;
using PostSharp.Collections;
using PostSharp.Extensibility;
using PostSharp.Extensibility.Tasks;
using Xtensive.Core.Aspects.Helpers;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Weaver
{
  internal class AutoPropertyReplacementWeaver : MethodLevelAspectWeaver
  {
    private AutoPropertyReplacementTransformation transformation;

    protected override void Initialize()
    {
      base.Initialize();

      transformation = new AutoPropertyReplacementTransformation(this, "AutoProperty replacement.");
      ApplyEffectWaivers(transformation);
    }

    protected override AspectWeaverInstance CreateAspectWeaverInstance(AspectInstanceInfo aspectInstanceInfo)
    {
      return new Instance(this, aspectInstanceInfo);
    }


    // Constructors

    public AutoPropertyReplacementWeaver()
      : base(null, MulticastTargets.Class)
    {}


    // Nested class

    private class Instance : MethodLevelAspectWeaverInstance
    {
      private readonly AutoPropertyReplacementWeaver parent;

      public override void ProvideAspectTransformations(AspectWeaverTransformationAdder adder)
      {
        adder.Add(TargetElement, parent.transformation.CreateInstance(this));
      }

      public Instance(AutoPropertyReplacementWeaver parent, AspectInstanceInfo aspectInstanceInfo)
        : base(parent, aspectInstanceInfo)
      {
        this.parent = parent;
      }
    }
  }

  internal class AutoPropertyReplacementTransformation : MethodBodyTransformation
  {
    private readonly string displayName;

    public override string GetDisplayName(MethodSemantics semantic)
    {
      return displayName;
    }

    public AspectWeaverTransformationInstance CreateInstance(AspectWeaverInstance aspectWeaverInstance)
    {
      var aspect = (AutoPropertyReplacementAspect)aspectWeaverInstance.Aspect;
      var module = AspectWeaver.Module;
      var handlerType = module.Cache.GetType(aspect.HandlerType);
      var handlerMethodSuffix = aspect.HandlerMethodSuffix;
            
      return new Instance(
        this, aspectWeaverInstance, handlerType, handlerMethodSuffix);
    }


    // Constructors

    public AutoPropertyReplacementTransformation(AspectWeaver aspectWeaver, string displayName)
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
      private readonly ITypeSignature handlerTypeSignature;
      private readonly string handlerMethodSuffix;

      public override void Implement(MethodBodyTransformationContext context)
      {
        var methodDef = (MethodDefDeclaration)context.TargetElement;
        var typeDef = methodDef.DeclaringType;
         
        int splitterPos = methodDef.Name.IndexOf('_');
        if (splitterPos <= 0)
          return;

        string propertyName = methodDef.Name.Substring(splitterPos + 1);
        string fieldName = string.Format(AutoPropertyBackingFieldFormat, propertyName);
        bool isGetter = methodDef.Name.Substring(0, splitterPos + 1) == WellKnown.GetterPrefix;

        var fieldDef = typeDef.Fields.GetByName(fieldName);
        if (fieldDef == null)
          return;

        var module = AspectWeaver.Module;
        var handlerTypeDef = handlerTypeSignature.GetTypeDefinition();
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
          var replacementMethodSignature = new MethodSignature(module, CallingConvention.HasThis,
                                isGetter
                                  ? (ITypeSignature) module.Cache.GetGenericParameter(0, GenericParameterKind.Method)
                                  : module.Cache.GetIntrinsic(IntrinsicType.Void),
                                isGetter
                                  ? new ITypeSignature[] {module.Cache.GetIntrinsic(IntrinsicType.String)}
                                  : new ITypeSignature[]
                                      {
                                        module.Cache.GetIntrinsic(IntrinsicType.String),
                                        GenericParameterTypeSignature.GetInstance(module, 0, GenericParameterKind.Method)
                                      }, 1);

          var replacementMethod = handlerTypeDef.Methods.GetMethod(
            (isGetter ? HandlerGetMethodPrefix : HandlerSetMethodPrefix) + handlerMethodSuffix,
            replacementMethodSignature,
            BindingOptions.Default).Translate(module);
          var replacementMethodDef = replacementMethod as MethodDefDeclaration;
          var replacementMethodRef = replacementMethod as MethodRefDeclaration;

          if (replacementMethodRef != null)
            writer.EmitInstructionMethod(OpCodeNumber.Callvirt,
                                         replacementMethodRef.FindGenericInstance(new[] {fieldDef.FieldType},
                                                                                  BindingOptions.Default));
          else if (replacementMethodDef != null)
            writer.EmitInstructionMethod(OpCodeNumber.Callvirt,
                                         replacementMethodDef.FindGenericInstance(new[] {fieldDef.FieldType},
                                                                                  BindingOptions.Default));
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

      public Instance(MethodBodyTransformation parent, AspectWeaverInstance aspectWeaverInstance, ITypeSignature handlerTypeSignature, string handlerMethodSuffix) 
        : base(parent, aspectWeaverInstance)
      {
        this.handlerTypeSignature = handlerTypeSignature;
        this.handlerMethodSuffix = handlerMethodSuffix;
      }
    }
  }
}
