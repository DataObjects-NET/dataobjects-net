// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.02.18

using System;
using System.Data.Common;
using System.Diagnostics;
using Oracle.ManagedDataAccess.Client;

namespace Xtensive.Sql.Drivers.Oracle.v10
{
  internal class TypeMapper : v09.TypeMapper
  {
    public override void BindFloat(DbParameter parameter, object value)
    {
      var nativeParameter = (OracleParameter) parameter;
      nativeParameter.OracleDbType = OracleDbType.BinaryFloat;
      nativeParameter.Value = value ?? DBNull.Value;
    }

    public override void BindDouble(DbParameter parameter, object value)
    {
      var nativeParameter = (OracleParameter) parameter;
      nativeParameter.OracleDbType = OracleDbType.BinaryDouble;
      nativeParameter.Value = value ?? DBNull.Value;
    }


    // Constructor

    public TypeMapper(SqlDriver driver)
      : base(driver)
    {
    }
  }
}