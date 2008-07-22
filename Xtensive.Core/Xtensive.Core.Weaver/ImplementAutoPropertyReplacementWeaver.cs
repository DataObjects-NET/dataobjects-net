// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.05.29

using PostSharp.CodeModel;
using PostSharp.CodeModel.TypeSignatures;
using PostSharp.Extensibility.Tasks;
using PostSharp.Laos.Weaver;
using PostSharp.ModuleWriter;

namespace Xtensive.Core.Weaver
{
  internal class ImplementAutoPropertyReplacementWeaver : MethodLevelAspectWeaver
  {
    private const string AutoPropertyBackingFieldFormat = "<{0}>k__BackingField";
    private const string GetterNamePrefix = "get";
    private const string HandlerGetMethodPrefix = "Get";
    private const string HandlerSetMethodPrefix = "Set";

    private readonly ITypeSignature handlerTypeSignature;
    private readonly string handlerMethodSuffix;

    public override void Implement()
    {
      MethodDefDeclaration methodDef = (MethodDefDeclaration) TargetElement;
      TypeDefDeclaration   typeDef   = methodDef.DeclaringType;

      int splitterPos = methodDef.Name.IndexOf('_');
      if (splitterPos <= 0)
        return;

      string propertyName = methodDef.Name.Substring(splitterPos + 1);
      string fieldName    = string.Format(AutoPropertyBackingFieldFormat, propertyName);
      bool   isGetter     = methodDef.Name.Substring(0, splitterPos) == GetterNamePrefix;
      FieldDefDeclaration fieldDef = typeDef.Fields.GetByName(fieldName);
      if (fieldDef == null)
        return;

      ModuleDeclaration module = Task.Project.Module;
      TypeDefDeclaration handlerTypeDef = handlerTypeSignature.GetTypeDefinition();

      MethodBodyDeclaration methodBody = new MethodBodyDeclaration();
      methodDef.MethodBody = methodBody;
      InstructionBlock instructionBlock = methodBody.CreateInstructionBlock();
      methodBody.RootInstructionBlock = instructionBlock;
      InstructionSequence sequence = methodBody.CreateInstructionSequence();
      instructionBlock.AddInstructionSequence(sequence, PostSharp.Collections.NodePosition.After, null);
      InstructionWriter writer = Task.InstructionWriter;
      writer.AttachInstructionSequence(sequence);

      writer.EmitInstruction(OpCodeNumber.Ldarg_0);
      writer.EmitInstructionString(OpCodeNumber.Ldstr, propertyName);
      if (!isGetter)
        writer.EmitInstruction(OpCodeNumber.Ldarg_1);

      MethodSignature replacementMethodSignature =
        new MethodSignature(CallingConvention.HasThis,
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

      if (replacementMethodRef!=null)
        writer.EmitInstructionMethod(OpCodeNumber.Callvirt,
          replacementMethodRef.FindGenericInstance(new [] {fieldDef.FieldType},
          BindingOptions.Default));
      else
        writer.EmitInstructionMethod(OpCodeNumber.Callvirt,
          replacementMethodDef.FindGenericInstance(new [] {fieldDef.FieldType},
          BindingOptions.Default));
      writer.EmitInstruction(OpCodeNumber.Ret);
      writer.DetachInstructionSequence();

      try {
        RemoveTask.GetTask(Task.Project).MarkForRemoval(fieldDef);
      }
      catch {
        // Field is already marked for removal
      }
    }

    public override void EmitCompileTimeInitialization(InstructionEmitter writer)
    {
    }

    public override bool ValidateSelf()
    {
      return true;
    }


    // Constructors

    internal ImplementAutoPropertyReplacementWeaver(ITypeSignature handlerTypeSignature, string handlerMethodSuffix)
    {
      this.handlerTypeSignature = handlerTypeSignature;
      this.handlerMethodSuffix  = handlerMethodSuffix;
    }
  }
}
