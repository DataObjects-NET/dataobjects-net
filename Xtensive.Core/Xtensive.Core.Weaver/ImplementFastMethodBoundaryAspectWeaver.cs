// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.09.25

using PostSharp.CodeModel;
using PostSharp.CodeWeaver;
using PostSharp.Laos.Weaver;

namespace Xtensive.Core.Weaver
{
  internal class ImplementFastMethodBoundaryAspectWeaver : MethodLevelAspectWeaver
  {
    public override void Implement()
    {
      MethodDefDeclaration methodDef = (MethodDefDeclaration)TargetMethod;
      ModuleDeclaration module = Task.Project.Module;
      MethodBodyDeclaration methodBody = methodDef.MethodBody;
      InstructionWriter writer = Task.InstructionWriter;
      MethodBodyRestructurer restructurer =
                    new MethodBodyRestructurer(methodDef, MethodBodyRestructurerOptions.ChangeReturnInstructions, Task.WeavingHelper);
//      restructurer.

      /*InstructionWriter instructionWriter = context.InstructionWriter;
        InstructionSequence newSequence = block.MethodBody.CreateInstructionSequence();
        block.AddInstructionSequence(newSequence, NodePosition.Before, null);
        instructionWriter.AttachInstructionSequence(newSequence);
        instructionWriter.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);
        base.Task.EventArgsBuilders.BuildMethodExecutionEventArgs(this.targetMethodDef, instructionWriter, out this.eventArgsLocal, out this.argumentsLocal);
        base.Task.InstanceTagManager.EmitLoadInstanceTag(this.eventArgsLocal, this.instanceTagField, instructionWriter);
        instructionWriter.EmitInstructionField(OpCodeNumber.Ldsfld, base.AspectRuntimeInstanceField);
        instructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, this.eventArgsLocal);
        instructionWriter.EmitInstructionMethod(OpCodeNumber.Callvirt, this.onEntryMethod);
        if (this.isStruct)
        {
          instructionWriter.EmitInstruction(OpCodeNumber.Ldarg_0);
          instructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, this.eventArgsLocal);
          instructionWriter.EmitInstructionMethod(OpCodeNumber.Call, this.getInstanceMethod);
          instructionWriter.EmitInstructionType(OpCodeNumber.Unbox, this.Method.DeclaringType);
          instructionWriter.EmitInstructionType(OpCodeNumber.Cpobj, this.Method.DeclaringType);
        }
        base.Task.InstanceTagManager.EmitStoreInstanceTag(this.eventArgsLocal, this.instanceTagField, instructionWriter);
        InstructionSequence sequence2 = methodBody.CreateInstructionSequence();
        block.AddInstructionSequence(sequence2, NodePosition.After, null);
        InstructionSequence sequence3 = methodBody.CreateInstructionSequence();
        block.AddInstructionSequence(sequence3, NodePosition.After, null);
        instructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, this.eventArgsLocal);
        instructionWriter.EmitInstructionMethod(OpCodeNumber.Call, this.getFlowBehaviorMethod);
        instructionWriter.EmitInstructionInt32(OpCodeNumber.Ldc_I4, 3);
        instructionWriter.EmitBranchingInstruction(OpCodeNumber.Bne_Un, sequence3);
        instructionWriter.DetachInstructionSequence();
        instructionWriter.AttachInstructionSequence(sequence2);
        ITypeSignature parameterType = this.targetMethodDef.ReturnParameter.ParameterType;
        if (!IntrinsicTypeSignature.Is(parameterType, IntrinsicType.Void))
        {
          instructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Ldloc, this.eventArgsLocal);
          instructionWriter.EmitInstructionMethod(OpCodeNumber.Call, this.getReturnValueMethod);
          context.WeavingHelper.FromObject(parameterType, instructionWriter);
          instructionWriter.EmitInstructionLocalVariable(OpCodeNumber.Stloc, context.ReturnValueVariable);
        }
        if (this.hasOutParameter)
        {
          context.WeavingHelper.CopyArgumentsFromArray(this.argumentsLocal, this.targetMethodDef, instructionWriter);
        }
        instructionWriter.EmitBranchingInstruction(OpCodeNumber.Leave, context.LeaveBranchTarget);
        instructionWriter.DetachInstructionSequence();
      */


      /*MethodDefDeclaration methodDef = (MethodDefDeclaration) TargetMethod;
            TypeDefDeclaration baseTypeRef = handlerTypeSignature.GetTypeDefinition();
            if (baseTypeRef == null)
              return;

            ModuleDeclaration module = Task.Project.Module;
            MethodBodyDeclaration methodBody = methodDef.MethodBody;
            InstructionWriter writer = Task.InstructionWriter;
            
            MethodBodyRestructurer restructurer = 
              new MethodBodyRestructurer(methodDef, MethodBodyRestructurerOptions.ChangeReturnInstructions, Task.WeavingHelper);

            restructurer.Restructure(writer);      

            methodBody.RootInstructionBlock.AddChildBlock(methodBody.CreateInstructionBlock(), NodePosition.After, null);
            methodBody.RootInstructionBlock.LastChildBlock.AddInstructionSequence(
              restructurer.ReturnBranchTarget, 
              NodePosition.After, 
              null);
            writer.AttachInstructionSequence(restructurer.ReturnBranchTarget);

            writer.EmitInstruction(OpCodeNumber.Ldarg_0);

            writer.EmitInstructionType(OpCodeNumber.Ldtoken, methodDef.DeclaringType);
            IMethod getTypeFromHandleMethod = module.FindMethod(
              typeof (Type).GetMethod(GetTypeFromHandleMethodName, new [] {typeof(RuntimeTypeHandle)}), BindingOptions.Default);
            writer.EmitInstructionMethod(OpCodeNumber.Call, getTypeFromHandleMethod);

            MethodSignature handlerSignature =
              new MethodSignature(CallingConvention.HasThis, module.Cache.GetIntrinsic(IntrinsicType.Void),
                new [] {getTypeFromHandleMethod.ReturnType}, 0);
            writer.EmitInstructionMethod(OpCodeNumber.Call,
              (IMethod) baseTypeRef.Methods.GetMethod(handlerMethodName,
                handlerSignature.Translate(module),
                BindingOptions.Default).Translate(module));

            writer.EmitInstruction(OpCodeNumber.Ret);
            writer.DetachInstructionSequence();*/
      throw new System.NotImplementedException();
    }
  }
}