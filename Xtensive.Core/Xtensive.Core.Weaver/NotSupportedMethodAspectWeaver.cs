// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.10.30

using System;
using PostSharp.CodeModel;
using PostSharp.CodeWeaver;
using PostSharp.Collections;
using PostSharp.Laos;
using PostSharp.Laos.Weaver;
using Xtensive.Core.Aspects.Helpers;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;

namespace Xtensive.Core.Weaver
{
  public class NotSupportedMethodAspectWeaver : MethodLevelAspectWeaver
  {
    public override void Implement()
    {
      base.Implement();
      var methodDef = (MethodDefDeclaration)TargetElement;
      var methodBody = methodDef.MethodBody;
      var writer = Task.InstructionWriter;
      methodBody.RootInstructionBlock = methodBody.CreateInstructionBlock();
      var sequence = methodBody.CreateInstructionSequence();
      methodBody.RootInstructionBlock.AddInstructionSequence(sequence, NodePosition.Before, null);
      writer.AttachInstructionSequence(sequence);
      writer.EmitSymbolSequencePoint(SymbolSequencePoint.Hidden);
      var aspect = (NotSupportedMethodAspect)Aspect;
      var hasDescription = !aspect.Text.IsNullOrEmpty();
      if (hasDescription)
        writer.EmitInstructionString(OpCodeNumber.Ldstr, new LiteralString(aspect.Text));

      ModuleDeclaration module = Task.Project.Module;
      var ctorString = (IMethod)module.Cache.GetItem(theModule => theModule.FindMethod(typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) }), BindingOptions.RequireGenericDefinition));
      var ctorEmpty = (IMethod)module.Cache.GetItem(theModule => theModule.FindMethod(typeof(InvalidOperationException).GetConstructor(ArrayUtils<Type>.EmptyArray), BindingOptions.RequireGenericDefinition));
      if (hasDescription)
        writer.EmitInstructionMethod(OpCodeNumber.Newobj, ctorString);
      else
        writer.EmitInstructionMethod(OpCodeNumber.Newobj, ctorEmpty);
      writer.EmitInstruction(OpCodeNumber.Throw);
      writer.DetachInstructionSequence();
    }
  }
}