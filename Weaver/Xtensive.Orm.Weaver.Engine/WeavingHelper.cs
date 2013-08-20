// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.20

using System;
using Mono.Cecil.Cil;

namespace Xtensive.Orm.Weaver
{
  internal static class WeavingHelper
  {
    public static void EmitLoadArguments(ILProcessor il, int count)
    {
      if (il==null)
        throw new ArgumentNullException("il");
      if (count > byte.MaxValue)
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