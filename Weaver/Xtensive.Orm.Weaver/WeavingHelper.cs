// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.20

using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Xtensive.Orm.Weaver
{
  internal static class WeavingHelper
  {
    public static readonly StringComparer AssemblyNameComparer = StringComparer.InvariantCultureIgnoreCase;
    public static readonly StringComparer TypeNameComparer = StringComparer.InvariantCulture;

    public static byte[] ParsePublicKeyToken(string value)
    {
      var result = new List<byte>();
      for (var i = 0; i + 1 < value.Length; i += 2) {
        var itemValue = value.Substring(i, 2);
        result.Add(Convert.ToByte(itemValue, 16));
      }
      return result.ToArray();
    }

    public static void MarkAsCompilerGenerated(ProcessorContext context, ICustomAttributeProvider target)
    {
      if (context==null)
        throw new ArgumentNullException("context");
      if (target==null)
        throw new ArgumentNullException("target");
      target.CustomAttributes.Add(new CustomAttribute(context.References.CompilerGeneratedAttributeConstructor));
    }

    public static void EmitLoadArguments(ILProcessor il, int count)
    {
      if (il==null)
        throw new ArgumentNullException("il");
      if (count > Byte.MaxValue)
        throw new ArgumentOutOfRangeException("count");

      if (count > 0)
        il.Emit(OpCodes.Ldarg_0);
      if (count > 1)
        il.Emit(OpCodes.Ldarg_1);
      if (count > 2)
        il.Emit(OpCodes.Ldarg_2);
      if (count > 3)
        il.Emit(OpCodes.Ldarg_3);
      if (count > 4)
        for (var i = 4; i < count; i++)
          il.Emit(OpCodes.Ldarg_S, (byte) i);
    }
  }
}