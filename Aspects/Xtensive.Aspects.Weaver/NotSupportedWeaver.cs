// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.10.30

using System;
using PostSharp.Extensibility;
using PostSharp.Reflection;
using PostSharp.Sdk.AspectInfrastructure;
using PostSharp.Sdk.AspectWeaver;
using PostSharp.Sdk.AspectWeaver.AspectWeavers;
using PostSharp.Sdk.AspectWeaver.Transformations;
using PostSharp.Sdk.CodeModel;
using PostSharp.Sdk.Collections;
using Xtensive.Aspects;

namespace Xtensive.Aspects.Weaver
{
  internal class NotSupportedWeaver : MethodLevelAspectWeaver
  {
    private NotSupportedTransformation transformation;

    protected override AspectWeaverInstance CreateAspectWeaverInstance(AspectInstanceInfo aspectInstanceInfo)
    {
      return new Instance(this, aspectInstanceInfo);
    }

    protected override void Initialize()
    {
      base.Initialize();
      transformation = new NotSupportedTransformation(this);
      ApplyWaivedEffects(transformation);
      RequiresRuntimeInstance = false;
      RequiresRuntimeInstanceInitialization = false;
      RequiresRuntimeReflectionObject = false;
    }

    public NotSupportedWeaver()
      : base(null, MulticastTargets.Method | MulticastTargets.InstanceConstructor | MulticastTargets.StaticConstructor)
    {}

    private class Instance : MethodLevelAspectWeaverInstance
    {
      private readonly NotSupportedWeaver parent;

      public override void ProvideAspectTransformations(AspectWeaverTransformationAdder adder)
      {
        adder.Add(TargetElement, parent.transformation.CreateInstance(this));
      }

      public Instance(NotSupportedWeaver parent, AspectInstanceInfo aspectInstanceInfo)
        : base(parent, aspectInstanceInfo)
      {
        this.parent = parent;
      }
    }
  }

  internal class NotSupportedTransformation : MethodBodyTransformation
  {
    const string DisplayName = "NotSupported.";

    public override string GetDisplayName(MethodSemantics semantic)
    {
      return DisplayName;
    }

    public AspectWeaverTransformationInstance CreateInstance(AspectWeaverInstance aspectWeaverInstance)
    {
      return new Instance(this, aspectWeaverInstance);
    }

    public NotSupportedTransformation(AspectWeaver aspectWeaver)
      : base(aspectWeaver)
    {}

    private class Instance : MethodBodyTransformationInstance
    {
      private string text;

      public override void Implement(MethodBodyTransformationContext context)
      {
        var methodDef = (MethodDefDeclaration)context.TargetElement;
        var body = methodDef.MethodBody;
        using (var writer = new InstructionWriter()) {
          body.RootInstructionBlock = body.CreateInstructionBlock();
          var sequence = body.CreateInstructionSequence();
          body.RootInstructionBlock.AddInstructionSequence(sequence, NodePosition.Before, null);
          writer.AttachInstructionSequence(sequence);
          writer.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);

          var hasDescription = !string.IsNullOrEmpty(text);
          if (hasDescription)
            writer.EmitInstructionString(OpCodeNumber.Ldstr, new LiteralString(text));

          var module = AspectWeaver.Module;
          var ctorString =
            module.Cache.GetItem(() => 
              module.FindMethod(typeof (InvalidOperationException).GetConstructor(new[] {typeof (string)}), BindingOptions.RequireGenericDefinition));
          var ctorEmpty =
            module.Cache.GetItem(() =>
              module.FindMethod(typeof (InvalidOperationException).GetConstructor(Type.EmptyTypes), BindingOptions.RequireGenericDefinition));

          writer.EmitInstructionMethod(OpCodeNumber.Newobj, hasDescription ? ctorString : ctorEmpty);
          writer.EmitInstruction(OpCodeNumber.Throw);
          writer.DetachInstructionSequence();
        }

      }

      public override MethodBodyTransformationOptions GetOptions(MetadataDeclaration originalTargetElement, MethodSemantics semantic)
      {
        return MethodBodyTransformationOptions.CreateMethodBody;
      }

      public Instance(MethodBodyTransformation parent, AspectWeaverInstance aspectWeaverInstance)
        : base(parent, aspectWeaverInstance)
      {
        var aspect = (NotSupportedAttribute)aspectWeaverInstance.Aspect;
        text = aspect.Text;
      }
    }
  }
}