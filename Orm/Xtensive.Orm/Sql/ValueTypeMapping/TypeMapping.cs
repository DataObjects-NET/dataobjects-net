// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.03

using System;
using System.Data.Common;

namespace Xtensive.Sql
{
  /// <summary>
  /// Value (data) type mapping.
  /// </summary>
  public sealed class TypeMapping
  {
    private readonly Func<DbDataReader, int, object> valueReader;
    private readonly Action<DbParameter, object> valueBinder;
    private readonly Func<int?, int?, int?, SqlValueType> mapper;

    public Type Type { get; private set; }
    public bool LiteralCastRequired { get; private set; }
    public bool ParameterCastRequired { get; private set; }

    public object ReadValue(DbDataReader reader, int index)
    {
      return valueReader.Invoke(reader, index);
    }

    public void BindValue(DbParameter parameter, object value)
    {
      valueBinder.Invoke(parameter, value);
    }

    public SqlValueType MapType()
    {
      return mapper.Invoke(null, null, null);
    }

    public SqlValueType MapType(int? length, int? precision, int? scale)
    {
      return mapper.Invoke(length, precision, scale);
    }


    // Constructors

    internal TypeMapping(Type type,
      Func<DbDataReader, int, object> valueReader,
      Action<DbParameter, object> valueBinder,
      Func<int?, int?, int?, SqlValueType> mapper,
      bool parameterCastRequired,
      bool literalCastRequired)
    {
      Type = type;
      ParameterCastRequired = parameterCastRequired;
      LiteralCastRequired = literalCastRequired;
      this.valueReader = valueReader;
      this.valueBinder = valueBinder;
      this.mapper = mapper;
    }
  }
}