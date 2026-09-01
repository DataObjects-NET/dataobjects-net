// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.02.25

using System;

namespace Xtensive.Orm.SerializableExpressions.Internals
{
  internal static class ReflectionExtensions
  {
    public static string ToSerializableForm(this Type type)
      => type?.AssemblyQualifiedName;

    public static Type GetTypeFromSerializableForm(this string serializedValue)
      => serializedValue is null ? null : Type.GetType(serializedValue);
  }
}
