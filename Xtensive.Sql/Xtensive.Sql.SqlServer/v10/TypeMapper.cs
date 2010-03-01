// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.02

using System;
using System.Data;
using System.Data.Common;

namespace Xtensive.Sql.SqlServer.v10
{
  internal class TypeMapper : v09.TypeMapper
  {
    public override void SetDateTimeParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.DateTime2;
      parameter.Value = value ?? DBNull.Value;
    }

    public override object ReadDateTime(DbDataReader reader, int index)
    {
      string type = reader.GetDataTypeName(index);
      if (type=="time") {
        var time = (TimeSpan) reader.GetValue(index);
        return new DateTime(time.Ticks);
      }
      return base.ReadDateTime(reader, index);
    }


    // Constructors

    public TypeMapper(SqlDriver driver)
      : base(driver)
    {
    }
  }
}