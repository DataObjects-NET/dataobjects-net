// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.06.07

using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace Xtensive.Sql.Drivers.SqlServer
{
  internal abstract class SqlServerTypeMapper : CustomTypeMapper
  {
    private readonly Type type;
    private readonly string udtType;
    private readonly SqlType sqlType;

    public override bool Enabled { get { return type!=null; } }

    public override Type Type { get { return type; } }

    public override void BindValue(DbParameter parameter, object value)
    {
      if (value==null) {
        parameter.Value = DBNull.Value;
        return;
      }

      var sqlParameter = (SqlParameter) parameter;
      sqlParameter.Value = value;
    }

    public override object ReadValue(DbDataReader reader, int index)
    {
      return reader.GetValue(index);
    }

    public override SqlValueType MapType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(sqlType);
    }

    // Constructors

    protected SqlServerTypeMapper(string frameworkType, string udtType, SqlType sqlType)
    {
      type = Type.GetType(frameworkType);

      this.udtType = udtType;
      this.sqlType = sqlType;
    }
  }
}