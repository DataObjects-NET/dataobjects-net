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
    private readonly Action<DbParameter, object> parameterValueSetter;
    private readonly Func<int?, int?, int?, SqlValueType> sqlTypeBuilder;
    public Type Type { get; private set; }
    public bool LiteralCastRequired { get; private set; }
    public bool ParameterCastRequired { get; private set; }

    public object ReadValue(DbDataReader reader, int index)
    {
      return valueReader.Invoke(reader, index);
    }
    
    public void SetParameterValue(DbParameter parameter, object value)
    {
      parameterValueSetter.Invoke(parameter, value);
    }

    public SqlValueType BuildSqlType()
    {
      return sqlTypeBuilder.Invoke(null, null, null);
    }

    public SqlValueType BuildSqlType(int length)
    {
      return sqlTypeBuilder.Invoke(length, null, null);
    }

    public SqlValueType BuildSqlType(int precision, int scale)
    {
      return sqlTypeBuilder.Invoke(null, precision, scale);
    }

    public SqlValueType BuildSqlType(int? length, int? precision, int? scale)
    {
      return sqlTypeBuilder.Invoke(length, precision, scale);
    }


    // Constructors

    internal TypeMapping(Type type,
      Func<DbDataReader, int, object> valueReader,
      Action<DbParameter, object> parameterValueSetter,
      Func<int?, int?, int?, SqlValueType> sqlTypeBuilder,
      bool parameterCastRequired,
      bool literalCastRequired)
    {
      Type = type;
      ParameterCastRequired = parameterCastRequired;
      LiteralCastRequired = literalCastRequired;
      this.valueReader = valueReader;
      this.parameterValueSetter = parameterValueSetter;
      this.sqlTypeBuilder = sqlTypeBuilder;
    }
  }
}