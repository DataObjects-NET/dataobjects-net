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
  public sealed class SqlValueTypeMapper : Sql.SqlValueTypeMapper
  {
    protected override void BuildTypeSubstitutes()
    {
      base.BuildTypeSubstitutes();
      var int64 = DomainHandler.SqlDriver.ServerInfo.DataTypes.Int64;
      var ts = new RangeDataTypeInfo<TimeSpan>(SqlDataType.Int64, null);
      ts.Value = new ValueRange<TimeSpan>(TimeSpan.FromTicks(int64.Value.MinValue), TimeSpan.FromTicks(int64.Value.MaxValue));
      BuildDataTypeMapping(ts);
    }

    protected override DataTypeMapping CreateDataTypeMapping(DataTypeInfo dataTypeInfo)
    {
      DataTypeMapping result = null;
      Type type = dataTypeInfo.Type;
      TypeCode typeCode = Type.GetTypeCode(type);
      var dataReaderAccessor = BuildDataReaderAccessor(dataTypeInfo);
      switch (typeCode) {
      case TypeCode.DateTime: {
        result = new DataTypeMapping(dataTypeInfo, dataReaderAccessor, DbType.DateTime2);
        break;
      }
      case TypeCode.Object: {
        if (type==typeof (TimeSpan))
          result = new DataTypeMapping(dataTypeInfo, dataReaderAccessor, DbType.Int64, value => ((TimeSpan) value).Ticks, value => TimeSpan.FromTicks((long) value));
        else if (type==typeof (byte[]))
          result = new DataTypeMapping(dataTypeInfo, dataReaderAccessor, DbType.Binary);
        else
          result = base.CreateDataTypeMapping(dataTypeInfo);
        break;
      }
      case TypeCode.SByte:
        result = new DataTypeMapping(dataTypeInfo, dataReaderAccessor, DbType.Int16);
        break;
      case TypeCode.UInt16:
        result = new DataTypeMapping(dataTypeInfo, dataReaderAccessor, DbType.Int32);
        break;
      case TypeCode.UInt32:
        result = new DataTypeMapping(dataTypeInfo, dataReaderAccessor, DbType.Int64);
        break;
      case TypeCode.UInt64:
        result = new DataTypeMapping(dataTypeInfo, dataReaderAccessor, DbType.Decimal);
        break;
      default:
        result = base.CreateDataTypeMapping(dataTypeInfo);
        break;
      }
      return result;
    }

    protected override Func<DbDataReader, int, object> BuildDataReaderAccessor(DataTypeInfo dataTypeInfo)
    {
      if (dataTypeInfo.Type == typeof(TimeSpan))
        return (reader, fieldIndex) => reader.GetInt64(fieldIndex);
      return base.BuildDataReaderAccessor(dataTypeInfo);
    }
  }
}