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

namespace Xtensive.Storage.Providers.VistaDb
{
  public sealed class SqlValueTypeMapper : Sql.SqlValueTypeMapper
  {
    protected override void BuildTypeSubstitutes()
    {
      base.BuildTypeSubstitutes();
      var @int64 = DomainHandler.Driver.ServerInfo.DataTypes.Int64;
      var @timespan = new RangeDataTypeInfo<TimeSpan>(SqlDataType.Int64, null);
      @timespan.Value = new ValueRange<TimeSpan>(TimeSpan.FromTicks(@int64.Value.MinValue), TimeSpan.FromTicks(@int64.Value.MaxValue));
      BuildDataTypeMapping(@timespan);
    }

    protected override DataTypeMapping CreateDataTypeMapping(DataTypeInfo dataTypeInfo)
    {
      if (dataTypeInfo.Type==typeof (TimeSpan))
        return new DataTypeMapping(dataTypeInfo, BuildDataReaderAccessor(dataTypeInfo), DbType.Int64, value => ((TimeSpan) value).Ticks, value => TimeSpan.FromTicks((long) value));
      return base.CreateDataTypeMapping(dataTypeInfo);
    }

    protected override Func<DbDataReader, int, object> BuildDataReaderAccessor(DataTypeInfo dataTypeInfo)
    {
      if (dataTypeInfo.Type == typeof(TimeSpan))
        return (reader, fieldIndex) => reader.GetInt64(fieldIndex);
      return base.BuildDataReaderAccessor(dataTypeInfo);
    }
  }
}