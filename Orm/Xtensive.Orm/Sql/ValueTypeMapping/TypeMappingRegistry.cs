// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.03

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Reflection;

namespace Xtensive.Sql
{
  /// <summary>
  /// A collection of <see cref="TypeMapping"/> objects.
  /// </summary>
  public sealed class TypeMappingRegistry : IEnumerable<TypeMapping>
  {
    private readonly Dictionary<Type, TypeMapping> mappings;

    public TypeMapping this[Type type] { get { return GetMapping(type); } }
    
    public TypeMapping TryGetMapping(Type type)
    {
      if (type.IsEnum)
        type = Enum.GetUnderlyingType(type);

      TypeMapping result;
      mappings.TryGetValue(type, out result);
      return result;
    }

    public TypeMapping GetMapping(Type type)
    {
      var result = TryGetMapping(type);
      if (result==null)
        throw new NotSupportedException(string.Format(
          Strings.ExTypeXIsNotSupported, type.GetFullName()));
      return result;
    }

    public Type GetMapping(SqlType sqlType)
    {
      Type result;
      SqlType.RegisteredTypes.TryGetValue(sqlType, out result);
      if (result==null)
        throw new ArgumentOutOfRangeException("sqlType");
      return result;
    }

    public IEnumerator<TypeMapping> GetEnumerator()
    {
      return mappings.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    // Constructors

    public TypeMappingRegistry(IEnumerable<TypeMapping> mappings)
    {
      this.mappings = mappings.ToDictionary(m => m.Type);
    }
  }
}