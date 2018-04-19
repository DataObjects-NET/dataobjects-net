// Copyright (C) 2013 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2013.08.20

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Xtensive.Orm.Weaver
{
  public static class WeavingHelper
  {
    private const string GetterPrefix = "get_";
    private const string SetterPrefix = "set_";

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

    public static string GetPropertyName(string accessorName)
    {
      if (accessorName.StartsWith(GetterPrefix))
        return accessorName.Substring(GetterPrefix.Length);
      if (accessorName.StartsWith(SetterPrefix))
        return accessorName.Substring(SetterPrefix.Length);
      throw new InvalidOperationException(String.Format("Invalid or unsupported accessor name '{0}'", accessorName));
    }

    public static string GetPropertySignatureName(PropertyDefinition definition)
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append(definition.PropertyType.FullName);
      stringBuilder.Append(' ');
      stringBuilder.Append(definition.Name);
      stringBuilder.Append('(');
      if (definition.HasParameters) {
        var parameters = definition.Parameters;
        for (int index = 0; index < parameters.Count; ++index) {
          if (index > 0)
            stringBuilder.Append(',');
          stringBuilder.Append(parameters[index].ParameterType.FullName);
        }
      }
      stringBuilder.Append(')');
      return stringBuilder.ToString();
    }

    public static string GetPropertySignatureName(MethodReference accessor)
    {
      if (accessor == null)
        throw new ArgumentNullException("accessor");

      if (accessor.Name.StartsWith(GetterPrefix))
      {
        var stringBuilder = new StringBuilder();
        var propertyType = accessor.ReturnType;
        var propertyName = accessor.Name.Substring(GetterPrefix.Length);

        stringBuilder.Append(propertyType.FullName);
        stringBuilder.Append(" ");
        stringBuilder.Append(propertyName);
        stringBuilder.Append("(");
        bool insertDelimiter = false;
        foreach (var parameterDefinition in accessor.Parameters) {
          if (insertDelimiter)
            stringBuilder.Append(',');
          else
            insertDelimiter = true;
          stringBuilder.Append(parameterDefinition.ParameterType.FullName);
        }
        stringBuilder.Append(")");
        return stringBuilder.ToString();
      }
      if (accessor.Name.StartsWith(SetterPrefix)) {
        var stringBuilder = new StringBuilder();
        var parametersCount = accessor.Parameters.Count;
        var propertyType = accessor.Parameters[parametersCount - 1];
        var propertyName = accessor.Name.Substring(SetterPrefix.Length);

        stringBuilder.Append(propertyType.ParameterType.FullName);
        stringBuilder.Append(" ");
        stringBuilder.Append(propertyName);
        stringBuilder.Append("(");
        bool insertDelimiter = false;
        foreach (var parameterDefinition in accessor.Parameters.Take(parametersCount - 1)) {
          if (insertDelimiter)
            stringBuilder.Append(',');
          else
            insertDelimiter = true;
          stringBuilder.Append(parameterDefinition.ParameterType.FullName);
        }
        stringBuilder.Append(")");
        return stringBuilder.ToString();
      }
      throw new InvalidOperationException(String.Format("Invalid or unsupported accessor name '{0}'", accessor.Name));
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

    public static TypeReference CreateGenericInstance(TypeDefinition type)
    {
      var typeInstance = new GenericInstanceType(type);
      foreach (var parameter in type.GenericParameters)
        typeInstance.GenericArguments.Add(parameter);
      return typeInstance;
    }

    public static string BuildComplexPersistentName(TypeInfo type, PropertyInfo property)
    {
      return String.Format("{0}.{1}", type.Name, property.Name);
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