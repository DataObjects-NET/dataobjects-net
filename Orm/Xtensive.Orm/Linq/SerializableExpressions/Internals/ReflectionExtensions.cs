// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.02.25

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Xtensive.Core;

namespace Xtensive.Linq.SerializableExpressions.Internals
{
  internal static class ReflectionExtensions
  {
    public static string ToSerializableForm(this Type type) 
      => type?.AssemblyQualifiedName;

    public static Type GetTypeFromSerializableForm(this string serializedValue) 
      => serializedValue == null ? null : Type.GetType(serializedValue);
  }
}
