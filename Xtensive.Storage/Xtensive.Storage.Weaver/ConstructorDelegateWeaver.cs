// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.05.29

using System;
using System.Collections.Generic;
using System.Reflection;
using PostSharp.CodeModel;
using PostSharp.CodeModel.TypeSignatures;
using PostSharp.Extensibility;
using PostSharp.Laos.Weaver;
using PostSharp.ModuleWriter;

namespace Xtensive.Storage.Weaver
{
  internal class ConstructorDelegateWeaver : 
    BaseTypeRelatedWeaver
  {
    private const string StaticMethodName = "<.cctor>m__CreateInstance";
    private const string DelegateFieldName = "<.cctor>d__CreateInstance";
    private ITypeSignature[] parameterTypeSignatures;
    private MethodDefDeclaration delegateMethodDef;
    private ITypeSignature delegateTypeSignature;
    private FieldDefDeclaration delegateField;

    public override void Implement()
    {
      TypeDefDeclaration typeDef = (TypeDefDeclaration)TargetElement;
      ModuleDeclaration module = Task.Project.Module;

      if (BaseTypeSignature == null)
        return;

      ImplementDelegateMethodBody(typeDef, module);

      delegateField = new FieldDefDeclaration();
      delegateField.FieldType = delegateTypeSignature;
      delegateField.Attributes = FieldAttributes.Static | FieldAttributes.Private;
      delegateField.Name = DelegateFieldName;
      typeDef.Fields.Add(delegateField);
    }

    private void ImplementDelegateMethodBody(TypeDefDeclaration typeDef, ModuleDeclaration module)
    {
      IMethod tupleConstructor = FindConstructor(typeDef, module);
      if (tupleConstructor == null)
        return;

      delegateMethodDef = new MethodDefDeclaration();
      delegateMethodDef.Name = StaticMethodName;
      delegateMethodDef.CallingConvention = CallingConvention.Default;
      delegateMethodDef.Attributes = MethodAttributes.Private | MethodAttributes.Static;
      typeDef.Methods.Add(delegateMethodDef);

      delegateMethodDef.ReturnParameter = new ParameterDeclaration();
      delegateMethodDef.ReturnParameter.ParameterType = BaseTypeSignature;
      delegateMethodDef.ReturnParameter.Attributes = ParameterAttributes.Retval;
      delegateMethodDef.CustomAttributes.Add(Task.WeavingHelper.GetDebuggerNonUserCodeAttribute());

      for (int i = 0; i < parameterTypeSignatures.Length; i++) {
        ParameterDeclaration dataParameter =
          new ParameterDeclaration(i, "data", parameterTypeSignatures[i]);
        delegateMethodDef.Parameters.Add(dataParameter);
      }


      var methodBody = new MethodBodyDeclaration();
      delegateMethodDef.MethodBody = methodBody;
      InstructionBlock instructionBlock = methodBody.CreateInstructionBlock();
      methodBody.RootInstructionBlock = instructionBlock;
      InstructionSequence sequence = methodBody.CreateInstructionSequence();
      instructionBlock.AddInstructionSequence(sequence, PostSharp.Collections.NodePosition.Before, null);
      InstructionWriter writer = Task.InstructionWriter;
      writer.AttachInstructionSequence(sequence);

      for (short i = 0; i < parameterTypeSignatures.Length; i++)
        writer.EmitInstructionParameter(OpCodeNumber.Ldarg, delegateMethodDef.Parameters[i]);

      writer.EmitInstructionMethod(OpCodeNumber.Newobj, tupleConstructor);
      writer.EmitInstruction(OpCodeNumber.Ret);
      writer.DetachInstructionSequence();
    }

    private IMethod FindConstructor(TypeDefDeclaration typeDef, ModuleDeclaration module)
    {
      IMethod foundConstructor = null;
      foreach (IMethod constructor in typeDef.Methods.GetByName(".ctor")) {
        if (constructor.ParameterCount == parameterTypeSignatures.Length) {
          int i = 0;
          for (; i < parameterTypeSignatures.Length; i++)
            if (constructor.GetParameterType(i).GetType() != parameterTypeSignatures[i].GetType())
              break;
          if (i == parameterTypeSignatures.Length) {
            foundConstructor = (IMethod) constructor.Translate(module);
            break;
          }
        }
      }
      return foundConstructor;
    }

    public override void EmitCompileTimeInitialization(InstructionEmitter writer)
    {
      if (delegateMethodDef == null)
        return;
      ModuleDeclaration module = Task.Project.Module;
      TypeDefDeclaration typeDef = (TypeDefDeclaration)TargetElement;

      MethodSignature methodSignature =
        (MethodSignature)(new MethodSignature(CallingConvention.HasThis,
          module.Cache.GetIntrinsic(IntrinsicType.Void),
          new[] { 
            module.Cache.GetIntrinsic(IntrinsicType.Object), 
            module.Cache.GetIntrinsic(IntrinsicType.IntPtr)}, 0))
            .Translate(module);

      writer.EmitInstruction(OpCodeNumber.Ldnull);
      writer.EmitInstructionMethod(OpCodeNumber.Ldftn, delegateMethodDef);

      IMethod method = (IMethod)(((TypeSpecDeclaration)delegateTypeSignature).MethodRefs
        .GetMethod(".ctor", methodSignature,
        BindingOptions.Default)).Translate(module);

      writer.EmitInstructionMethod(OpCodeNumber.Newobj, method);
      writer.EmitInstructionField(OpCodeNumber.Stsfld, delegateField);
      writer.EmitInstructionType(OpCodeNumber.Ldtoken, typeDef);

      TypeRefDeclaration typeTypeRef = (TypeRefDeclaration)module.Cache.GetType(typeof(Type));
      MethodSignature getTypeMethodSignature =
        (MethodSignature)(new MethodSignature(
          CallingConvention.Default,
          typeTypeRef,
          new[] { module.Cache.GetType(typeof(RuntimeTypeHandle))}, 0))
            .Translate(module);

      IMethod getTypeFromHandle = (IMethod)typeTypeRef.MethodRefs
        .GetMethod("GetTypeFromHandle", getTypeMethodSignature, BindingOptions.Default)
        .Translate(module);

      writer.EmitInstructionMethod(OpCodeNumber.Call, getTypeFromHandle);
      writer.EmitInstructionField(OpCodeNumber.Ldsfld, delegateField);

      MethodSignature registerTypeMethodSignature =
        (MethodSignature)(new MethodSignature(
          CallingConvention.Default,
          module.Cache.GetIntrinsic(IntrinsicType.Void),
          new[] { typeTypeRef, delegateTypeSignature }, 0))
            .Translate(module);

      IMethod registerTypeActivator = (IMethod)((TypeRefDeclaration)BaseTypeSignature)
        .MethodRefs
        .GetMethod(
          "RegisterActivator", 
          registerTypeMethodSignature, 
          BindingOptions.Default)
            .Translate(module);

      writer.EmitInstructionMethod(OpCodeNumber.Call, registerTypeActivator);
    }

    public override bool ValidateSelf()
    {
      return true;
    }


    // Constructors

    internal ConstructorDelegateWeaver(
      ITypeSignature baseTypeSignature,
      ITypeSignature[] parameterTypeSignatures,
      ITypeSignature delegateTypeSignature) :
      base(baseTypeSignature)
    {
      this.parameterTypeSignatures = parameterTypeSignatures;
      this.delegateTypeSignature = delegateTypeSignature;
    }
  }
}
