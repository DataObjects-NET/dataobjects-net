// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.02.25

using System;
using System.Linq;
using System.Reflection;

namespace Xtensive.Linq.SerializableExpressions.Internals
{
  internal static class ReflectionExtensions
  {
    public static string ToSerializableForm(this Type type)
    {
      if (type == null)
        return null;

      return type.AssemblyQualifiedName;
    }

    public static Type GetTypeFromSerializableForm(this string serializedValue)
    {
      if (serializedValue == null)
        return null;

      return Type.GetType(serializedValue);
    }

    public static string ToSerializableForm(this MethodInfo method)
    {
      if (method == null)
        return null;

      var serializableName = method.DeclaringType.AssemblyQualifiedName + Environment.NewLine;
      if (!method.IsGenericMethod)
        serializableName += method.ToString();
      else
        serializableName += method.GetGenericMethodDefinition() + Environment.NewLine +
                            String.Join(Environment.NewLine,
                                        method.GetGenericArguments().Select(ty => ty.ToSerializableForm()).ToArray());
      return serializableName;
    }

    public static MethodInfo GetMethodFromSerializableForm(this string serializedValue)
    {
      if (serializedValue == null) 
        return null;

      var fullName = SplitString(serializedValue);
      var name = fullName[1];
      var method = Type.GetType(fullName[0]).GetMethods().First(m => m.ToString() == name);

      if (method.IsGenericMethod)
        method = method.MakeGenericMethod(fullName.Skip(2).Select(s => GetTypeFromSerializableForm(s)).ToArray());
      return method;
    }

    public static string ToSerializableForm(this MemberInfo member)
    {
      if (member == null)
        return null;

      return member.DeclaringType.AssemblyQualifiedName + Environment.NewLine + member;
    }

    public static MemberInfo GetMemberFromSerializableForm(this string serializedValue)
    {
      if (serializedValue == null)
        return null;

      var fullName = SplitString(serializedValue);
      var name = fullName[1];
      var member = Type.GetType(fullName[0]).GetMembers().First(m => m.ToString() == name);
      return member;
    }

    public static string ToSerializableForm(this ConstructorInfo obj)
    {
      if (obj == null)
        return null;
      return obj.DeclaringType.AssemblyQualifiedName + Environment.NewLine + obj;
    }

    public static ConstructorInfo GetConstructorFromSerializableForm(this string serializedValue)
    {
      if (serializedValue == null)
        return null;
      var fullName = SplitString(serializedValue);
      var name = fullName[1];
      var newObj = Type.GetType(fullName[0]).GetConstructors().First(m => m.ToString() == name);
      return newObj;
    }

    private static string[] SplitString(string str)
    {
      return str.Split(str.Contains(Environment.NewLine) 
        ? new[] {Environment.NewLine} 
        : new[] {"\n"}, StringSplitOptions.None);
    }
  }
}