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

    public TypeMappingRegistry Build()
    {
      return new TypeMappingRegistry(this);
    }

    public void Add(
      Type type, Func<DbDataReader, int, object> valueReader,
      Action<DbParameter, object> parameterValueSetter,
      Func<int?, int?, int?, SqlValueType> sqlTypeBuilder)
    {
      var mapping = new TypeMapping(
        type, valueReader, parameterValueSetter, sqlTypeBuilder,
        Mapper.IsParameterCastRequired(type),
        Mapper.IsLiteralCastRequired(type));
      Add(mapping);
    }

    // Constructors

    public TypeMappingRegistryBuilder(TypeMapper mapper)
    {
      ArgumentValidator.EnsureArgumentNotNull(mapper, "mapper");
      Mapper = mapper;
    }
  }
}