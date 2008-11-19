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
  internal class PrivateFieldAccessorsWeaver : TypeLevelAspectWeaver
  {
    private readonly HashSet<string> targetFields;

    public override void Implement()
    {
      var typeDef = (TypeDefDeclaration)TargetType;
      foreach (var fieldDef in typeDef.Fields) {
        if (!targetFields.Contains(fieldDef.Name))
          continue;
        ImplementGetter(fieldDef);
        ImplementSetter(fieldDef);
      }
    }

    private void ImplementGetter(FieldDefDeclaration fieldDef)
    {
      var typeDef = fieldDef.DeclaringType;

      // Declare get method.
      var methodDef = new MethodDefDeclaration();
      methodDef.Name = DelegateHelper.AspectedPrivateFieldGetterPrefix + fieldDef.Name;
      methodDef.Attributes = MethodAttributes.Assembly;
      typeDef.Methods.Add(methodDef);
      methodDef.CustomAttributes.Add(Task.WeavingHelper.GetDebuggerNonUserCodeAttribute());

      // Define parameter.
      methodDef.ReturnParameter = new ParameterDeclaration();
      methodDef.ReturnParameter.ParameterType = fieldDef.FieldType;
      methodDef.ReturnParameter.Attributes = ParameterAttributes.Retval;

      // Define the body
      var body = new MethodBodyDeclaration();
      methodDef.MethodBody = body;
      InstructionBlock instructionBlock = body.CreateInstructionBlock();
      body.RootInstructionBlock = instructionBlock;
      InstructionSequence sequence = body.CreateInstructionSequence();
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
      var typeDef = fieldDef.DeclaringType;
      var module  = Task.Project.Module;

      // Declare get method.
      var methodDef = new MethodDefDeclaration();
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
      var body = new MethodBodyDeclaration();
      methodDef.MethodBody = body;
      var instructionBlock = body.CreateInstructionBlock();
      body.RootInstructionBlock = instructionBlock;
      InstructionSequence sequence = body.CreateInstructionSequence();
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

    internal PrivateFieldAccessorsWeaver(string[] targetFields)
    {
      this.targetFields = new HashSet<string>(targetFields);
    }
  }
}