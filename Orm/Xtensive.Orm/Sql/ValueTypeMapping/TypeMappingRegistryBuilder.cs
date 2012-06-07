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

    public void Add(
      Type type,
      Func<DbDataReader, int, object> valueReader,
      Action<DbParameter, object> valueBinder,
      Func<int?, int?, int?, SqlValueType> mapper)
    {
      var mapping = new TypeMapping(
        type, valueReader, valueBinder,
        mapper, Mapper.IsParameterCastRequired(type));
      Add(mapping);
    }

    public TypeMappingRegistry Build()
    {
      return new TypeMappingRegistry(this);
    }

    // Constructors

    public TypeMappingRegistryBuilder(TypeMapper mapper)
    {
      ArgumentValidator.EnsureArgumentNotNull(mapper, "mapper");
      Mapper = mapper;
    }
  }
}