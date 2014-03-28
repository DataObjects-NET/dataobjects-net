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
  public sealed class TypeMappingRegistryBuilder : List<TypeMapping>
  {
    public TypeMapper Mapper { get; private set; }

    private readonly Dictionary<SqlType, Type> registeredSqlTypes = new Dictionary<SqlType, Type>(); 

    public void Add(Type type, Func<DbDataReader, int, object> valueReader,
      Action<DbParameter, object> valueBinder, Func<int?, int?, int?, SqlValueType> mapper)
    {
      var mapping = new TypeMapping(
        type, valueReader, valueBinder,
        mapper, Mapper.IsParameterCastRequired(type));

      Add(mapping);
    }

    public void Add(CustomTypeMapper customMapper)
    {
      // Allow custom mapper to dynamically disable itself
      if (!customMapper.Enabled)
        return;

      var mapping = new TypeMapping(
        customMapper.Type, customMapper.ReadValue, customMapper.BindValue,
        customMapper.MapType, customMapper.ParameterCastRequired);

      Add(mapping);
    }

    public void RegisterType(SqlType sqlType, Type type)
    {
      registeredSqlTypes.Add(sqlType, type);
    }

    public TypeMappingRegistry Build()
    {
      return new TypeMappingRegistry(this, registeredSqlTypes);
    }

    // Constructors

    public TypeMappingRegistryBuilder(TypeMapper mapper)
    {
      ArgumentValidator.EnsureArgumentNotNull(mapper, "mapper");
      Mapper = mapper;
    }
  }
}