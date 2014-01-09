// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Kryuchkov
// Created:    2009.07.07

using System;
using Xtensive.Sql.Info;
using SqlServerConnection = System.Data.SqlClient.SqlConnection;

namespace Xtensive.Sql.Drivers.SqlServer.v10
{
  internal class ServerInfoProvider : v09.ServerInfoProvider
  {
    public override IndexInfo GetIndexInfo()
    {
      var result = base.GetIndexInfo();
      result.Features |= IndexFeatures.Filtered;
      return result;
    }

    public override DataTypeCollection GetDataTypesInfo()
    {
      var types = base.GetDataTypesInfo();

      var common = DataTypeFeatures.Default | DataTypeFeatures.Nullable | DataTypeFeatures.NonKeyIndexing |
        DataTypeFeatures.Grouping | DataTypeFeatures.Ordering | DataTypeFeatures.Multiple;

      var index = DataTypeFeatures.Indexing | DataTypeFeatures.Clustering |
        DataTypeFeatures.FillFactor | DataTypeFeatures.KeyConstraint;

      types.DateTime = DataTypeInfo.Range(SqlType.DateTime, common | index,
        new ValueRange<DateTime>(new DateTime(1, 1, 1), new DateTime(9999, 12,31)),
        "datetime2", "datetime", "date", "time", "smalldatetime");

      types.DateTimeOffset = DataTypeInfo.Range(SqlType.DateTimeOffset, common | index,
        new ValueRange<DateTimeOffset>(new DateTimeOffset(1, 1, 1, 0, 0, 0, 0, new TimeSpan(0)),
          new DateTimeOffset(9999, 12, 31, 0, 0, 0, 0, new TimeSpan(0))),
          "datetimeoffset");

      types.VarBinaryMax = DataTypeInfo.Regular(SqlType.VarBinaryMax, common, "varbinary(max)", "image");

      var geo = DataTypeFeatures.Default | DataTypeFeatures.Nullable | DataTypeFeatures.Multiple;
      types.Geometry = DataTypeInfo.Regular(SqlType.Geometry, geo, "geometry");
      types.Geography = DataTypeInfo.Regular(SqlType.Geography, geo, "geography");
      return types;
    }

    // Constructors

    public ServerInfoProvider(SqlDriver driver)
      : base(driver)
    {
    }
  }
}