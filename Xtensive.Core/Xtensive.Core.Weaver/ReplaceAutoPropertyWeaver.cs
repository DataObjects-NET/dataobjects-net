// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.03.29

using System;
using System.Collections.Generic;
using System.Reflection;
using PostSharp.AspectInfrastructure;
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
using Xtensive.Core.Reflection;
using System.Linq;

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
        var targetType = targetMethod.DeclaringType;
         
        int splitterPos = targetMethod.Name.IndexOf('_');
        if (splitterPos <= 0)
          return;

        var module = AspectWeaver.Module;
        var isGetter = targetMethod.ReturnParameter.ParameterType != module.Cache.GetIntrinsic(IntrinsicType.Void);
        var isExplicit = targetMethod.InterfaceImplementations.Count > 0 &&
                         targetMethod.IsNew &&
                         targetMethod.IsVirtual &&
                         targetMethod.Visibility == Visibility.Private;
        var propertyFilter = isGetter
          ? (Func<MethodDefDeclaration, PropertyDeclaration, bool>)((m, p) => p.Getter == m)
          : (m, p) => p.Setter == m;
        var condition = propertyFilter.Bind(targetMethod);
        var targetProperty = targetType.Properties.Single(condition);
        var propertyName = targetProperty.Name;
        var isNew = false;
        if (!isExplicit) {
          var baseType = targetType.BaseType.GetTypeDefinition();
          var baseProperty = baseType.FindProperty(propertyName);
          if (baseProperty != null)
            if (!targetMethod.IsVirtual)
              isNew = true;
        }
        var fieldName = string.Format(AutoPropertyBackingFieldFormat, propertyName);
        if (isExplicit) {
          var interfaceMethod = (MethodDefDeclaration)targetMethod.InterfaceImplementations.Single().ImplementedMethod;
          var interfaceType = interfaceMethod.DeclaringType;
          var interfaceCondition = propertyFilter.Bind(interfaceMethod);
          var interfaceProperty = interfaceType.Properties.Single(interfaceCondition);
          propertyName = string.Format("{0}.{1}", interfaceType.ShortName, interfaceProperty.Name);
        }
        else if (isNew)
          propertyName = string.Format("{0}.{1}", targetType.ShortName, propertyName);

        var fieldDef = targetType.Fields.GetByName(fieldName);
        if (fieldDef == null)
          return;

        var methodBody = targetMethod.MethodBody;
        methodBody.MaxStack = 8;
        var block = methodBody.CreateInstructionBlock();
        var sequence = methodBody.CreateInstructionSequence();
        block.AddInstructionSequence(sequence, NodePosition.After, null);
        var principalBlock = (InstructionBlock) null;
        var childrenEnumerator = methodBody.RootInstructionBlock.GetChildrenEnumerator(true);
        while(childrenEnumerator.MoveNext()) {
          var current = childrenEnumerator.Current;
          if (current.Comment == "Old Root Block")
            principalBlock = current;
        }
        if (principalBlock == null)
          principalBlock = methodBody.RootInstructionBlock;
        principalBlock.AddChildBlock(block, NodePosition.After, null);

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
          var reference = targetType.FindMethod(methodName, methodSignature);
          if (reference != null) {
            var redirection = IntroduceMemberHelper.GetRedirection(reference.Method, AspectWeaverInstance);
            if (redirection != null)
              reference = new GenericMethodReference(redirection, reference.GenericMap);
            var method = reference.Method.MethodSpecs.GetGenericInstance(new[] {fieldDef.FieldType}, true).Translate(module);
            writer.EmitInstructionMethod(OpCodeNumber.Call, method);
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
//          Field is already marked for removal
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
