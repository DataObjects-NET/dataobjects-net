// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.06.07

using System;
using System.Collections.Generic;
using System.Data.Common;
using Xtensive.Core;

namespace Xtensive.Sql
{
  public sealed class TypeMappingRegistryBuilder
  {
    public TypeMapper Mapper { get; private set; }

    private readonly List<TypeMapping> mappings = new List<TypeMapping>();
    private readonly Dictionary<SqlType, Type> reverseMappings = new Dictionary<SqlType, Type>(); 

    public void Add(Type type, Func<DbDataReader, int, object> valueReader,
      Action<DbParameter, object> valueBinder, Func<int?, int?, int?, SqlValueType> mapper)
    {
      var mapping = new TypeMapping(
        type, valueReader, valueBinder,
        mapper, Mapper.IsParameterCastRequired(type));

      mappings.Add(mapping);
    }

    public void Add(CustomTypeMapper customMapper)
    {
      // Allow custom mapper to dynamically disable itself
      if (!customMapper.Enabled)
        return;

      var mapping = new TypeMapping(
        customMapper.Type, customMapper.ReadValue, customMapper.BindValue,
        customMapper.MapType, customMapper.ParameterCastRequired);

      mappings.Add(mapping);
    }
    
    public void AddReverseMapping(SqlType sqlType, Type type)
    {
      reverseMappings.Add(sqlType, type);
    }

    public TypeMappingRegistry Build()
    {
      return new TypeMappingRegistry(mappings, reverseMappings);
    }

    // Constructors

    public TypeMappingRegistryBuilder(TypeMapper mapper)
    {
      ArgumentValidator.EnsureArgumentNotNull(mapper, "mapper");
      Mapper = mapper;
    }
  }
}