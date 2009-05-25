// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.05.22

using System;
using System.Linq;

namespace Xtensive.Storage.Model.Stored
{
  internal static class NamingHelper
  {
    private const string GenericTypeNameFormat = "{0}{{{1}}}";
    private const string GenericArgumentsDelimiter = ",";

    private const string FieldNameFormat = "{0}::{1}";
    private const string FieldNameDelimiter = "::";

    public static string GetFullTypeName(Type type)
    {
      if (!type.IsGenericType)
        return type.FullName;

      return string.Format(
        GenericTypeNameFormat,
        type.GetGenericTypeDefinition().FullName,
        string.Join(
          GenericArgumentsDelimiter,
          type.GetGenericArguments().Select(t => GetFullTypeName(t)).ToArray()));
    }

    public static string GetFullTypeName(TypeInfo type)
    {
      return GetFullTypeName(type.UnderlyingType);
    }

    public static string GetFullFieldName(string fullTypeName, string fieldName)
    {
      return string.Format(FieldNameFormat, fullTypeName, fieldName);
    }

    public static string GetFullFieldName(FieldInfo field)
    {
      return GetFullFieldName(GetFullTypeName(field.DeclaringType), field.Name);
    }

    public static string ChangeFieldName(StoredFieldInfo field, string newName)
    {
      int index = field.Name.LastIndexOf(FieldNameDelimiter);
      return GetFullFieldName(field.Name.Substring(0, index), newName);
    }
  }
}