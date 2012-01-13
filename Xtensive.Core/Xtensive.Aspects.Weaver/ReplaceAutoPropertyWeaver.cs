// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.03.29

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using PostSharp.Extensibility;
using PostSharp.Reflection;
using PostSharp.Sdk.AspectInfrastructure;
using PostSharp.Sdk.AspectWeaver;
using PostSharp.Sdk.AspectWeaver.AspectWeavers;
using PostSharp.Sdk.AspectWeaver.Transformations;
using PostSharp.Sdk.CodeModel;
using PostSharp.Sdk.CodeModel.SerializationTypes;
using PostSharp.Sdk.CodeModel.TypeSignatures;
using PostSharp.Sdk.Collections;
using PostSharp.Sdk.Extensibility.Tasks;
using Xtensive.Aspects;
using System.Linq;
using Xtensive.Core;

namespace Xtensive.Aspects.Weaver
{
  internal class ReplaceAutoPropertyWeaver : MethodLevelAspectWeaver
  {
    private ReplaceAutoPropertyTransformation transformation;

    protected override void Initialize()
    {
      base.Initialize();

      transformation = new ReplaceAutoPropertyTransformation(this);
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
    private const string AutoPropertyReplacement = "AutoProperty replacement.";
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

    public ReplaceAutoPropertyTransformation(AspectWeaver aspectWeaver)
      : base(aspectWeaver)
    {
      displayName = AutoPropertyReplacement;
    }

    // Nested class

    private class Instance : MethodBodyTransformationInstance
    {
      private const string AutoPropertyBackingFieldFormat = "<{0}>k__BackingField";
      private const string VBAutoPropertyBackingFieldFormat = "_{0}";
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
        var condition = MakePropertyFilter(targetMethod, isGetter);
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
        var originalPropertyName = propertyName;
        if (isExplicit) {
          var interfaceMethod = (MethodDefDeclaration)targetMethod.InterfaceImplementations.Single().ImplementedMethod;
          var interfaceType = interfaceMethod.DeclaringType;
          var interfaceCondition = MakePropertyFilter(interfaceMethod, isGetter);
          var interfaceProperty = interfaceType.Properties.Single(interfaceCondition);
          propertyName = string.Format("{0}.{1}", interfaceType.ShortName, interfaceProperty.Name);
        }
        else if (isNew)
          propertyName = string.Format("{0}.{1}", targetType.ShortName, propertyName);

        if (isExplicit || isNew) {
          if (isGetter) {
            var overrideAttributeType = (IType)module.FindType(typeof(OverrideFieldNameAttribute));
            var overrideDeclaration = new CustomAttributeDeclaration(module.FindMethod(overrideAttributeType, ".ctor", 1));
            overrideDeclaration.ConstructorArguments.Add(new MemberValuePair(MemberKind.Parameter, 0, "name", IntrinsicSerializationType.CreateValue(module, propertyName)));
            targetProperty.CustomAttributes.Add(overrideDeclaration);
          }
        }

        var fieldName = string.Format(AutoPropertyBackingFieldFormat, originalPropertyName);
        var fieldDef = targetType.Fields.GetByName(fieldName);
        if (fieldDef == null) {
          fieldName = string.Format(VBAutoPropertyBackingFieldFormat, originalPropertyName);
          fieldDef = targetType.Fields.GetByName(fieldName);
          if (fieldDef == null)
            return;
        }

        var compilerGeneratedAttribute = module.FindType(typeof(CompilerGeneratedAttribute), BindingOptions.Default);
        var compilerGeneratedAttributeDeclaration = new CustomAttributeDeclaration(module.FindMethod(compilerGeneratedAttribute, ".ctor", 0));
        targetMethod.CustomAttributes.Add(compilerGeneratedAttributeDeclaration);

        var debuggerNonUserCodeAttribute = module.FindType(typeof(DebuggerNonUserCodeAttribute), BindingOptions.Default);
        var debuggerNonUserCodeAttributeDeclaration = new CustomAttributeDeclaration(module.FindMethod(debuggerNonUserCodeAttribute, ".ctor", 0));
        targetMethod.CustomAttributes.Add(debuggerNonUserCodeAttributeDeclaration);

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
            var redirection = (MethodDefDeclaration) AspectInfrastructureTask.GetRedirectedDeclaration(reference.Method, true);
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

      private static Func<PropertyDeclaration, bool> MakePropertyFilter(MethodDefDeclaration accessor, bool isGetter)
      {
        if (isGetter)
          return property => property.Getter==accessor;
        return property => property.Setter==accessor;
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
