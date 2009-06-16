// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.23

using System;
using System.Data;
using System.Data.Common;
using Xtensive.Sql.Common;
using Xtensive.Storage.Providers.Sql.Mappings;

namespace Xtensive.Storage.Providers.MsSql
{
  /// <summary>
  /// A <see cref="Sql.SqlValueTypeMapper"/> descendant specific to MSSQL server.
  /// </summary>
  public sealed class SqlValueTypeMapper : Sql.SqlValueTypeMapper
  {
    private static readonly long TicksPerMillisecond = TimeSpan.FromMilliseconds(1).Ticks;

    protected override DataTypeMapping CreateDateTimeMapping(DataTypeInfo type)
    {
      var dti = DomainHandler.Driver.ServerInfo.DataTypes.DateTime;
      var min = dti.Value.MinValue;
      return new DataTypeMapping(typeof(DateTime), type, DbType.DateTime,
        (reader, index) => reader.GetDateTime(index),
        value => ((DateTime) value < min) ? min : value);
    }

    protected override DataTypeMapping CreateTimeSpanMapping(DataTypeInfo type)
    {
      var @int64 = DomainHandler.Driver.ServerInfo.DataTypes.Int64;
      var timespan = new RangeDataTypeInfo<TimeSpan>(SqlDataType.Int64, null)
        {
          Value = new ValueRange<TimeSpan>(
            TimeSpan.FromTicks(@int64.Value.MinValue),
            TimeSpan.FromTicks(@int64.Value.MaxValue))
        };

      return new DataTypeMapping(typeof (TimeSpan), timespan, DbType.Int64,
        (reader, index) => TimeSpan.FromMilliseconds(reader.GetInt64(index)),
        value => ((TimeSpan) value).Ticks / TicksPerMillisecond);
    }
  }
}