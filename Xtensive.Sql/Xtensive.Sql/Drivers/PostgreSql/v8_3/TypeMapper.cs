// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.23

using System;
using System.Data;
using System.Data.Common;

namespace Xtensive.Sql.PostgreSql.v8_3
{
  internal class TypeMapper : v8_0.TypeMapper
  {
    public override bool IsLiteralCastRequired(Type type)
    {
      if (type==typeof(Guid))
        return true;

      return base.IsLiteralCastRequired(type);
    }

    public override void SetGuidParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Guid;
      parameter.Value = value ?? DBNull.Value;
    }

    public override object ReadGuid(DbDataReader reader, int index)
    {
      return reader.GetGuid(index);
    }

    public override SqlValueType BuildGuidSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Guid);
    }

    // Constructors

    public TypeMapper(SqlDriver driver)
      : base(driver)
    {
    }
  }
}