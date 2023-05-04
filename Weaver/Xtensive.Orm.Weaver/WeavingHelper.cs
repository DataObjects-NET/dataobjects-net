// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.20

using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Xtensive.Orm.Weaver
{
  public static class WeavingHelper
  {
    public static readonly StringComparer AssemblyNameComparer = StringComparer.OrdinalIgnoreCase;
    public static readonly StringComparer TypeNameComparer = StringComparer.Ordinal;

    public static byte[] ParsePublicKeyToken(string value)
    {
      var result = new List<byte>();
      for (var i = 0; i + 1 < value.Length; i += 2) {
        var itemValue = value.Substring(i, 2);
        result.Add(Convert.ToByte(itemValue, 16));
      }
      return result.ToArray();
    }

    public static string FormatPublicKeyToken(byte[] value)
    {
      var result = new StringBuilder();
      for (var i = 0; i < value.Length; i++)
        result.Append(value[i].ToString("x2"));
      return result.ToString();
    }

    public static string GetPropertyName(string accessorName)
    {
      const string getterPrefix = "get_";
      const string setterPrefix = "set_";

      if (accessorName.StartsWith(getterPrefix, StringComparison.Ordinal))
        return accessorName.Substring(getterPrefix.Length);
      if (accessorName.StartsWith(setterPrefix, StringComparison.Ordinal))
        return accessorName.Substring(setterPrefix.Length);
      throw new InvalidOperationException($"Invalid or unsupported accessor name '{accessorName}'");
    }

    public static void MarkAsCompilerGenerated(ProcessorContext context, ICustomAttributeProvider target)
    {
      ArgumentNullException.ThrowIfNull(context);
      ArgumentNullException.ThrowIfNull(target);
      target.CustomAttributes.Add(new CustomAttribute(context.References.CompilerGeneratedAttributeConstructor));
    }

    public static void EmitLoadArguments(ILProcessor il, int count)
    {
      ArgumentNullException.ThrowIfNull(il);
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

    public static TypeReference CreateGenericInstance(TypeDefinition type)
    {
      var typeInstance = new GenericInstanceType(type);
      foreach (var parameter in type.GenericParameters)
        typeInstance.GenericArguments.Add(parameter);
      return typeInstance;
    }

    public static string BuildComplexPersistentName(TypeInfo type, PropertyInfo property)
    {
      return $"{type.Name}.{property.Name}";
    }

    public static SourceLanguage ParseLanguage(string projectType)
    {
      if (string.IsNullOrEmpty(projectType))
        return SourceLanguage.Unknown;
      switch (projectType.ToLowerInvariant()) {
      case ".csproj":
        return SourceLanguage.CSharp;
      case ".vbproj":
        return SourceLanguage.VbNet;
      case ".fsproj":
        return SourceLanguage.FSharp;
      default:
        return SourceLanguage.Unknown;
      }
    }
  }
}
