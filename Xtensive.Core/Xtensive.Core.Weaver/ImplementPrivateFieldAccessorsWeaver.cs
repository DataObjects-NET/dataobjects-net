// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.10.23

using System.Collections.Generic;
using System.Reflection;
using PostSharp.CodeModel;
using PostSharp.Laos.Weaver;
using Xtensive.Core.Reflection;

namespace Xtensive.Core.Weaver
{
  internal class ImplementPrivateFieldAccessorsWeaver : TypeLevelAspectWeaver
  {
    private readonly HashSet<string> targetFields;

    public override void Implement()
    {
      TypeDefDeclaration typeDef = (TypeDefDeclaration)TargetType;

      foreach (FieldDefDeclaration fieldDef in typeDef.Fields) {
        if (!targetFields.Contains(fieldDef.Name))
          continue;
        ImplementGetter(fieldDef);
        ImplementSetter(fieldDef);
      }
    }

    private void ImplementGetter(FieldDefDeclaration fieldDef)
    {
      TypeDefDeclaration typeDef = fieldDef.DeclaringType;

      // Declare get method.
      MethodDefDeclaration methodDef = new MethodDefDeclaration();
      methodDef.Name = DelegateHelper.AspectedPrivateFieldGetterPrefix + fieldDef.Name;
      methodDef.Attributes = MethodAttributes.Assembly;
      typeDef.Methods.Add(methodDef);
      methodDef.CustomAttributes.Add(Task.WeavingHelper.GetDebuggerNonUserCodeAttribute());

      // Define parameter.
      methodDef.ReturnParameter = new ParameterDeclaration();
      methodDef.ReturnParameter.ParameterType = fieldDef.FieldType;
      methodDef.ReturnParameter.Attributes = ParameterAttributes.Retval;

      // Define the body
      MethodBodyDeclaration methodBody = new MethodBodyDeclaration();
      methodDef.MethodBody = methodBody;
      InstructionBlock instructionBlock = methodBody.CreateInstructionBlock ();
      methodBody.RootInstructionBlock = instructionBlock;
      InstructionSequence sequence = methodBody.CreateInstructionSequence();
      instructionBlock.AddInstructionSequence(sequence, PostSharp.Collections.NodePosition.After, null);
      InstructionWriter writer = Task.InstructionWriter;
      writer.AttachInstructionSequence(sequence);

      writer.EmitInstruction(OpCodeNumber.Ldarg_0);
      writer.EmitInstructionField(OpCodeNumber.Ldfld, fieldDef);
      writer.EmitInstruction(OpCodeNumber.Ret);
      writer.DetachInstructionSequence();
    }

    private void ImplementSetter(FieldDefDeclaration fieldDef)
    {
      TypeDefDeclaration typeDef = fieldDef.DeclaringType;
      ModuleDeclaration module = this.Task.Project.Module;

      // Declare get method.
      MethodDefDeclaration methodDef = new MethodDefDeclaration();
      methodDef.Name = DelegateHelper.AspectedPrivateFieldSetterPrefix + fieldDef.Name;
      methodDef.Attributes = MethodAttributes.Assembly;
      typeDef.Methods.Add(methodDef);
      methodDef.CustomAttributes.Add(Task.WeavingHelper.GetDebuggerNonUserCodeAttribute());

      // Define parameter.
      methodDef.ReturnParameter = new ParameterDeclaration();
      methodDef.ReturnParameter.ParameterType = module.Cache.GetIntrinsic(IntrinsicType.Void);
      methodDef.ReturnParameter.Attributes = ParameterAttributes.Retval;

      methodDef.Parameters.Add(new ParameterDeclaration(0, "value", fieldDef.FieldType));

      // Define the body
      MethodBodyDeclaration methodBody = new MethodBodyDeclaration();
      methodDef.MethodBody = methodBody;
      InstructionBlock instructionBlock = methodBody.CreateInstructionBlock ();
      methodBody.RootInstructionBlock = instructionBlock;
      InstructionSequence sequence = methodBody.CreateInstructionSequence();
      instructionBlock.AddInstructionSequence(sequence, PostSharp.Collections.NodePosition.After, null);
      InstructionWriter writer = Task.InstructionWriter;
      writer.AttachInstructionSequence(sequence);

      writer.EmitInstruction(OpCodeNumber.Ldarg_0);
      writer.EmitInstruction(OpCodeNumber.Ldarg_1);
      writer.EmitInstructionField(OpCodeNumber.Stfld, fieldDef);
      writer.EmitInstruction(OpCodeNumber.Ret);
      writer.DetachInstructionSequence();
    }

    
    // Constructors

    internal ImplementPrivateFieldAccessorsWeaver(string[] targetFields)
    {
      this.targetFields = new HashSet<string>(targetFields);
    }
  }
}