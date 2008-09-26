// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.23

using System;
using System.Data;
using Xtensive.Sql.Common;
using Xtensive.Storage.Providers.Sql;

namespace Xtensive.Storage.Providers.MsSql
{
  public sealed class SqlValueTypeMapper : Sql.SqlValueTypeMapper
  {
    protected override void BuildTypeSubstituteMappings()
    {
      base.BuildTypeSubstituteMappings();

      SubstituteTimeSpan();
    }

    private void SubstituteTimeSpan()
    {
      var substitute = DomainHandler.Driver.ServerInfo.DataTypes.Int64;
      RangeDataTypeInfo<TimeSpan> target = new RangeDataTypeInfo<TimeSpan>(SqlDataType.Int64, null);
      target.Value = new ValueRange<TimeSpan>(TimeSpan.FromTicks(substitute.Value.MinValue), TimeSpan.FromTicks(substitute.Value.MaxValue));
      Register(target);
    }

    public override void ConfigureParameter(SqlParameterBinding binding)
    {
      Type type = binding.Column.ValueType;
      TypeCode typeCode = Type.GetTypeCode(type);
      switch (typeCode) {
      case TypeCode.DateTime:
        RangeDataTypeInfo<DateTime> dti = DomainHandler.Driver.ServerInfo.DataTypes.DateTime;
        binding.ValueConverter = () => {
          DateTime dt = (DateTime) binding.Parameter.Value;
          if (dt < dti.Value.MinValue)
            binding.Parameter.Value = dti.Value.MinValue;
        };
          break;
      case TypeCode.Object:
        if (type == typeof(TimeSpan)) {
          binding.Parameter.DbType = DbType.Int64;
          binding.ValueConverter = () => {
            binding.Parameter.Value = ((TimeSpan) binding.Parameter.Value).Ticks;
          };
        }
        else if (type == typeof(byte[])) {
          binding.Parameter.DbType = DbType.Binary;
        }
        break;
      case TypeCode.SByte:
        binding.Parameter.DbType = DbType.Int16;
        break;
      case TypeCode.UInt16:
        binding.Parameter.DbType = DbType.Int32;
        break;
      case TypeCode.UInt32:
        binding.Parameter.DbType = DbType.Int64;
        break;
      case TypeCode.UInt64:
        binding.Parameter.DbType = DbType.Decimal;
        break;
      default:
        base.ConfigureParameter(binding);
        break;
      }
    }
  }
}