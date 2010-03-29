// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.05.29

using System;
using PostSharp.AspectInfrastructure;
using PostSharp.AspectWeaver;
using PostSharp.AspectWeaver.AspectWeavers;
using PostSharp.AspectWeaver.Transformations;
using PostSharp.CodeModel;
using PostSharp.CodeModel.TypeSignatures;
using PostSharp.CodeWeaver;
using PostSharp.Collections;
using PostSharp.Extensibility;
using PostSharp.Extensibility.Tasks;
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

      transformation = new AutoPropertyReplacementTransformation(this, "AutoProperty replacement.");
      ApplyEffectWaivers(transformation);
    }

    protected override AspectWeaverInstance CreateAspectWeaverInstance(AspectInstanceInfo aspectInstanceInfo)
    {
      return new ConstructorEpilogueWeaverInstance(this, aspectInstanceInfo);
    }


    // Constructors

    public AutoPropertyReplacementWeaver()
      : base(null, MulticastTargets.Class)
    { }


    // Nested class

    private class ConstructorEpilogueWeaverInstance : MethodLevelAspectWeaverInstance
    {
      private readonly ConstructorEpilogueWeaver parent;

      public override void ProvideAspectTransformations(AspectWeaverTransformationAdder adder)
      {
        adder.Add(TargetElement, parent.transformation.CreateInstance(this));
      }

      public AutoPropertyReplacementWeaverInstance(AutoPropertyReplacementWeaver parent, AspectInstanceInfo aspectInstanceInfo)
        : base(parent, aspectInstanceInfo)
      {
        this.parent = parent;
      }
    }
  }

  internal class ConstructorEpilogueTransformation : MethodBodyTransformation
  {
    private readonly string displayName;

    public override string GetDisplayName(MethodSemantics semantic)
    {
      return displayName;
    }

    public AspectWeaverTransformationInstance CreateInstance(AspectWeaverInstance aspectWeaverInstance)
    {
      var aspect = (ConstructorEpilogueAspect)aspectWeaverInstance.Aspect;
      var module = AspectWeaver.Module;
      var handlerType = module.Cache.GetType(aspect.HandlerType);
      var handlerMethodSuffix = aspect.HandlerMethodSuffix;

      return new ConstructorEpilogueTransformationInstance(
        this, aspectWeaverInstance, handlerType, handlerMethodSuffix);
    }


    // Constructors

    public ConstructorEpilogueTransformation(AspectWeaver aspectWeaver, string displayName)
      : base(aspectWeaver, displayName)
    {
      this.displayName = displayName;
    }

    // Nested class

    private class ConstructorEpilogueTransformationInstance : MethodBodyTransformationInstance
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
        bool isGetter = context.MethodSemantic == MethodSemantics.Getter;
        //        bool isGetter = methodDef.Name.Substring(0, splitterPos + 1) == WellKnown.GetterPrefix;

        var fieldDef = typeDef.Fields.GetByName(fieldName);
        if (fieldDef == null)
          return;

        var module = AspectWeaver.Module;
        var handlerTypeDef = handlerTypeSignature.GetTypeDefinition();

        var methodBody = new MethodBodyDeclaration();
        methodDef.MethodBody = methodBody;
        InstructionBlock instructionBlock = methodBody.CreateInstructionBlock();
        methodBody.RootInstructionBlock = instructionBlock;
        InstructionSequence sequence = methodBody.CreateInstructionSequence();
        instructionBlock.AddInstructionSequence(sequence, NodePosition.After, null);
        using (var writer = new InstructionWriter())
        {
          writer.AttachInstructionSequence(sequence);

          writer.EmitInstruction(OpCodeNumber.Ldarg_0);
          writer.EmitInstructionString(OpCodeNumber.Ldstr, propertyName);
          if (!isGetter)
            writer.EmitInstruction(OpCodeNumber.Ldarg_1);
          var replacementMethodSignature = new MethodSignature(module, CallingConvention.HasThis,
                                isGetter
                                  ? (ITypeSignature)module.Cache.GetGenericParameter(0, GenericParameterKind.Method)
                                  : module.Cache.GetIntrinsic(IntrinsicType.Void),
                                isGetter
                                  ? new ITypeSignature[] { module.Cache.GetIntrinsic(IntrinsicType.String) }
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
                                         replacementMethodRef.FindGenericInstance(new[] { fieldDef.FieldType },
                                                                                  BindingOptions.Default));
          else if (replacementMethodDef != null)
            writer.EmitInstructionMethod(OpCodeNumber.Callvirt,
                                         replacementMethodDef.FindGenericInstance(new[] { fieldDef.FieldType },
                                                                                  BindingOptions.Default));
          else
            throw new InvalidOperationException();
          writer.EmitInstruction(OpCodeNumber.Ret);
          writer.DetachInstructionSequence();
        }

        //        try
        //        {
        var project = Transformation.AspectWeaver.AspectWeaverTask.Project;
        RemoveTask.GetTask(project).MarkForRemoval(fieldDef);
        //        }
        //        catch {
        // Field is already marked for removal
        //        }
      }

      public override MethodBodyTransformationOptions GetOptions(MetadataDeclaration originalTargetElement, MethodSemantics semantic)
      {
        return MethodBodyTransformationOptions.CreateMethodBody;
      }


      // Constructors

      public ConstructorEpilogueTransformationInstance(MethodBodyTransformation parent, AspectWeaverInstance aspectWeaverInstance, ITypeSignature handlerTypeSignature, string handlerMethodSuffix)
        : base(parent, aspectWeaverInstance)
      {
        this.handlerTypeSignature = handlerTypeSignature;
        this.handlerMethodSuffix = handlerMethodSuffix;
      }
    }
  }

//  internal class ConstructorEpilogueWeaver : MethodLevelAspectWeaver,
//    IMethodLevelAdvice
//  {
//    private const string GetTypeFromHandleMethodName = "GetTypeFromHandle";
//
//    private readonly ITypeSignature handlerTypeSignature;
//    private readonly string handlerMethodName;
//    private readonly string errorHandlerMethodName;
//    private IMethod getTypeFromHandleMethod;
//    private IMethod errorHandlerMethod;
//    private IMethod handlerMethod;
//    private MethodDefDeclaration targetMethodDef;
//    private JoinPointKinds joinPointKinds;
//
//    #region IMethodLevelAdvice Members
//
//    public int Priority
//    {
//      get { return ((ILaosWeavableAspect) Aspect).AspectPriority; }
//    }
//
//    public MethodDefDeclaration Method
//    {
//      get { return targetMethodDef; }
//    }
//
//    public MetadataDeclaration Operand
//    {
//      get { return null; }
//    }
//
//    public JoinPointKinds JoinPointKinds {
//      get { return joinPointKinds; }
//    }
//
//    public bool RequiresWeave(WeavingContext context)
//    {
//      return true;
//    }
//
//    public void Weave(WeavingContext context, InstructionBlock block)
//    {
//      var joinPointKind = context.JoinPoint.JoinPointKind;
//      switch (joinPointKind) {
//      case JoinPointKinds.AfterMethodBodySuccess:
//        WeaveOnSuccess(context, block);
//        break;
//      case JoinPointKinds.AfterMethodBodyException:
//        WeaveOnError(context, block);
//        break;
//      default:
//        throw new InvalidOperationException(string.Format(
//          Strings.ExUnexpectedJoinPointKindX, joinPointKind));
//      }
//    }
//
//    #endregion
//
//    protected override void OnTargetAssigned(bool reassigned)
//    {
//      targetMethodDef = (MethodDefDeclaration) TargetMethod;
//
//      var baseTypeRef = handlerTypeSignature.GetTypeDefinition();
//      if (baseTypeRef == null)
//        return;
//
//      var module = Task.Project.Module;
//
//      getTypeFromHandleMethod = module.FindMethod(
//        typeof (Type).GetMethod(GetTypeFromHandleMethodName, 
//          new [] {typeof(RuntimeTypeHandle)}), BindingOptions.Default);
//
//      var handlerSignature =
//        new MethodSignature(CallingConvention.HasThis, module.Cache.GetIntrinsic(IntrinsicType.Void),
//          new[] {getTypeFromHandleMethod.ReturnType}, 0);
//      handlerMethod = (IMethod) baseTypeRef.Methods.GetMethod(
//        handlerMethodName,
//        handlerSignature.Translate(module),
//        BindingOptions.Default).Translate(module);
//
//      if (!errorHandlerMethodName.IsNullOrEmpty()) {
//        var errorHandlerSignature =
//          new MethodSignature(CallingConvention.HasThis, module.Cache.GetIntrinsic(IntrinsicType.Void),
//            new[] {
//              getTypeFromHandleMethod.ReturnType, 
//              module.FindType(typeof (Exception), BindingOptions.Default)
//            }, 0);
//        errorHandlerMethod = (IMethod) baseTypeRef.Methods.GetMethod(
//          errorHandlerMethodName,
//          errorHandlerSignature.Translate(module),
//          BindingOptions.Default).Translate(module);
//      }
//    }
//
//    public override void ValidateInteractions(LaosAspectWeaver[] aspectsOnSameTarget)
//    {
//      // Uncomment this method to dump method-level aspect sequence
//
//      if (processedMethods.Contains(targetMethodDef))
//        return;
//      processedMethods.Add(targetMethodDef);
//
//      string target = string.Format("{0}.{1}", targetMethodDef.DeclaringType.Name, targetMethodDef.Name);
//      var sequence = new StringBuilder();
//      foreach (var a in aspectsOnSameTarget.Select(w => w.Aspect as ILaosMethodLevelAspect))
//        sequence.AppendFormat("{0} ({1})\n", a.GetType().Name, a.AspectPriority);
//      ErrorLog.Write(SeverityType.Warning,
//        "Sequence for {0} ({1}):\n{2}", target, joinPointKinds, sequence);
//    }
//
//    public override void Implement()
//    {
//      base.Implement();
//      if (targetMethodDef.MayHaveBody) {
//        targetMethodDef.MethodBody.InitLocalVariables = true;
//        Task.MethodLevelAdvices.Add(this);
//      }
//    }
//
//    private void WeaveOnSuccess(WeavingContext context, InstructionBlock block)
//    {
//      var module = Task.Project.Module;
//      var writer = context.InstructionWriter;
//      var methodBody = block.MethodBody;
//      var sequence = methodBody.CreateInstructionSequence();
//      block.AddInstructionSequence( sequence, NodePosition.Before, null );
//
//      writer.AttachInstructionSequence( sequence );
//      writer.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);
//      writer.EmitInstruction(OpCodeNumber.Ldarg_0); // Push "this"
//      writer.EmitInstructionType(OpCodeNumber.Ldtoken, targetMethodDef.DeclaringType.Translate(module));
//      writer.EmitInstructionMethod(OpCodeNumber.Call, getTypeFromHandleMethod); // Push "typeof(...)"
//      writer.EmitInstructionMethod(OpCodeNumber.Callvirt, handlerMethod);
//      writer.DetachInstructionSequence();
//    }
//
//    private void WeaveOnError(WeavingContext context, InstructionBlock block)
//    {
//      var writer = context.InstructionWriter;
//      var module = block.Module;
//      var methodBody = block.MethodBody;
//      var exceptionLocal = methodBody.RootInstructionBlock.DefineLocalVariable(module.Cache.GetType(typeof(Exception)), "~exception~{0}");
//      var sequence = methodBody.CreateInstructionSequence();
//      block.AddInstructionSequence( sequence, NodePosition.Before, null );
//      
//      writer.AttachInstructionSequence( sequence );
//      writer.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);
//      writer.EmitInstructionLocalVariable(OpCodeNumber.Stloc, exceptionLocal);
//      writer.EmitInstruction(OpCodeNumber.Ldarg_0); // Push "this"
//      writer.EmitInstructionType(OpCodeNumber.Ldtoken, targetMethodDef.DeclaringType.Translate(module));
//      writer.EmitInstructionMethod(OpCodeNumber.Call, getTypeFromHandleMethod); // Push "typeof(...)"
//      writer.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, exceptionLocal); // Push "exception"
//      writer.EmitInstructionMethod(OpCodeNumber.Callvirt, errorHandlerMethod);
//      writer.EmitInstruction(OpCodeNumber.Rethrow);
//      writer.DetachInstructionSequence();
//    }
//
//
    // Constructors
//
//    internal ConstructorEpilogueWeaver(ITypeSignature handlerTypeSignature, string handlerMethodName, string errorHandlerMethodName)
//    {
//      this.handlerTypeSignature = handlerTypeSignature;
//      this.handlerMethodName = handlerMethodName;
//      this.errorHandlerMethodName = errorHandlerMethodName;
//
//      joinPointKinds = JoinPointKinds.AfterMethodBodySuccess;
//      if (!errorHandlerMethodName.IsNullOrEmpty())
//        joinPointKinds |= JoinPointKinds.AfterMethodBodyException;
//    }
//  }
}