// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.02

using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace Xtensive.Sql.SqlServer.v10
{
  public class TypeMapper : v09.TypeMapper
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
        return new DateTime(time.Ticks/100);
      }
      return base.ReadDateTime(reader, index);
    }

    public object ReadGeometry(DbDataReader reader, int index)
    {
      return reader.GetValue(index);
    }

    public void SetGeometryParameterValue(DbParameter parameter, object value)
    {
      var sqlParameter = (SqlParameter)parameter;
      sqlParameter.UdtTypeName = "geometry";
      sqlParameter.Value = value;
    }

    public SqlValueType BuildGeographySqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Geography);
    }

    public object ReadGeography(DbDataReader reader, int index)
    {
      return reader.GetValue(index);
    }

    public void SetGeographyParameterValue(DbParameter parameter, object value)
    {
      var sqlParameter = (SqlParameter)parameter;
      sqlParameter.UdtTypeName = "geography";
      sqlParameter.Value = value;
    }

    public SqlValueType BuildGeometrySqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Geometry);
    }


    // Constructors

    public TypeMapper(SqlDriver driver)
      : base(driver)
    {
    }
  }
}