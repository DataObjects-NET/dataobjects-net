// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.25

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Xtensive.Reflection
{
  /// <summary>
  /// Code emission (via <see cref="System.Reflection.Emit"/>) helper.
  /// </summary>
  public static class EmitHelper
  {
    /// <summary>
    /// Helps to emit "if-else" or "if" block. 
    /// Implies all the necessary values <paramref name="brElse"/> operation needs
    /// are already on the stack.
    /// </summary>
    /// <param name="il">IL generator.</param>
    /// <param name="brElse">An opcode transferring </param>
    /// <param name="onTrue">Action to execute for "if" part.</param>
    /// <param name="onFalse">Action to execute for "else" part. Could be <see langword="null"/> for simple "if" statement.</param>
    public static void EmitIfElse(this ILGenerator il, OpCode brElse, Action onTrue, Action onFalse)
    {
      Label lElse = il.DefineLabel();
      Label lEnd  = il.DefineLabel();
      il.Emit(brElse, lElse);
      onTrue();
      if (onFalse != null)
        il.Emit(OpCodes.Br, lEnd);
      il.MarkLabel(lElse);
      if (onFalse != null) {
        onFalse();
        il.MarkLabel(lEnd);
      }
    }

    /// <summary>
    /// Helps to emit "switch" block for all the integer values 
    /// in 0...<paramref name="valueCount"/> range. 
    /// Implies that switch argument is already on the stack.
    /// </summary>
    /// <param name="il">IL generator.</param>
    /// <param name="valueCount">Switch integer values count.</param>
    /// <param name="breakAnyCase">Break any case.</param>
    /// <param name="onLabel">Action to execute per any switch label.</param>
    public static void EmitSwitch(this ILGenerator il, int valueCount, bool breakAnyCase, Action<int, bool> onLabel)
    {
      Label[] labels = new Label[valueCount];
      for (int i = 0; i < valueCount; i++)
        labels[i] = il.DefineLabel();
      Label lEnd = il.DefineLabel();
      il.Emit(OpCodes.Switch, labels);
      onLabel(default(int), true);
      il.Emit(OpCodes.Br, lEnd);
      for (int i = 0; i < valueCount; i++) {
        il.MarkLabel(labels[i]);
        onLabel(i, false);
        if (breakAnyCase)
          il.Emit(OpCodes.Br, lEnd);
      }
      il.MarkLabel(lEnd);
    }

    /// <summary>
    /// Helps to emit "switch" block for provided <paramref name="labels"/> using case values
    /// varying in <c>0</c>...<c>labels.Length-1</c> range. 
    /// Implies that switch argument is already on the stack.
    /// </summary>
    /// <param name="il">IL generator.</param>
    /// <param name="labels">An array of not marked yet labels to pass the control to.</param>
    /// <param name="breakAnyCase">Break any case.</param>
    /// <param name="onLabel">Action to execute per any switch label.</param>
    public static void EmitSwitch(this ILGenerator il, Label[] labels, bool breakAnyCase, Action<int, bool> onLabel)
    {
      Label lEnd = il.DefineLabel();
      il.Emit(OpCodes.Switch, labels);
      onLabel(default(int), true);
      il.Emit(OpCodes.Br, lEnd);
      for (int i = 0; i < labels.Length; i++) {
        onLabel(i, false);
        if (breakAnyCase)
          il.Emit(OpCodes.Br, lEnd);
      }
      il.MarkLabel(lEnd);
    }
  }
}