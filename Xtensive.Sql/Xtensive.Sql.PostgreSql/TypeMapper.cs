// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.23

using System;
using System.Data;
using System.Data.Common;
using Xtensive.Core.Helpers;

#if !OLD_NPGSQL
using Npgsql;
using NpgsqlTypes;
#endif

namespace Xtensive.Sql.PostgreSql
{
  internal class TypeMapper : ValueTypeMapping.TypeMapper
  {
    public override bool IsLiteralCastRequired(Type type)
    {
      switch (Type.GetTypeCode(type)) {
      case TypeCode.Byte:
      case TypeCode.SByte:
      case TypeCode.Int16:
      case TypeCode.UInt16:
      case TypeCode.Int64:
      case TypeCode.UInt64:
        return true;
      }
      return false;
    }

    public override bool IsParameterCastRequired(Type type)
    {
      switch (Type.GetTypeCode(type)) {
      case TypeCode.Byte:
      case TypeCode.SByte:
      case TypeCode.Int16:
      case TypeCode.UInt16:
      case TypeCode.Single:
      case TypeCode.Double:
      case TypeCode.DateTime:
        return true;
      }
      if (type==typeof(Guid))
        return true;
      if (type==typeof(TimeSpan))
        return true;
      if (type==typeof(byte[]))
        return true;
      return false;
    }

    public override void SetSByteParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Int16;
      parameter.Value = value ?? DBNull.Value;
    }

    public override void SetUShortParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Int32;
      parameter.Value = value ?? DBNull.Value;
    }
    
    public override void SetUIntParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Int64;
      parameter.Value = value ?? DBNull.Value;
    }

    public override void SetULongParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Decimal;
      parameter.Value = value ?? DBNull.Value;
    }

    public override void SetTimeSpanParameterValue(DbParameter parameter, object value)
    {
#if OLD_NPGSQL
      parameter.DbType = DbType.String;
      if (value==null) {
        parameter.Value = DBNull.Value;
        return;
      }
      parameter.Value = TimeSpanToString((TimeSpan) value);
#else
      var nativeParameter = (NpgsqlParameter) parameter;
      nativeParameter.NpgsqlDbType = NpgsqlDbType.Interval;
      nativeParameter.Value = value!=null
        ? (object) new NpgsqlInterval((TimeSpan) value)
        : DBNull.Value;
#endif
    }

    public override void SetGuidParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.String;
      parameter.Value = value==null ? (object) DBNull.Value : SqlHelper.GuidToString((Guid) value);
    }

    public override SqlValueType BuildByteSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Int16);
    }
 
    public override SqlValueType BuildSByteSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Int16);
    }

    public override SqlValueType BuildUShortSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Int32);
    }

    public override SqlValueType BuildUIntSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Int64);
    }

    public override SqlValueType BuildULongSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Decimal, 20, 0);
    }

    public override SqlValueType BuildGuidSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.VarChar, 32);
    }

    public override object ReadByte(DbDataReader reader, int index)
    {
      return Convert.ToByte(reader[index]);
    }

    public override object ReadGuid(DbDataReader reader, int index)
    {
      return SqlHelper.GuidFromString(reader.GetString(index));
    }

    public override object ReadTimeSpan(DbDataReader reader, int index)
    {
#if OLD_NPGSQL
      object value = reader.GetValue(index);

      switch (Type.GetTypeCode(value.GetType())) {
      case TypeCode.String:
        return StringToTimeSpan((string) value);
      case TypeCode.Byte:
      case TypeCode.SByte:
      case TypeCode.Int16:
      case TypeCode.Int32:
      case TypeCode.Int64:
      case TypeCode.UInt16:
      case TypeCode.UInt32:
      case TypeCode.UInt64:
        return new TimeSpan(Convert.ToInt64(value));
      }

      if (value is TimeSpan)
        return value;
      throw new NotSupportedException();
#else
      var nativeReader = (NpgsqlDataReader) reader;
      return (TimeSpan) nativeReader.GetInterval(index);
#endif
    }

#if OLD_NPGSQL
    public static TimeSpan StringToTimeSpan(string input)
    {
      int days = 0;
      int hours = 0;
      int minutes = 0;
      int seconds = 0;
      int milliseconds = 0;

      //pattern: [[-]DD* day[s]] [[-]H[H]:M[M]:S[S][.ff*]]
      bool mainPartNegative;
      string[] parts = input.Split(new[]{' '},StringSplitOptions.RemoveEmptyEntries);
      switch (parts.Length) {
        case 1:
          // no day part
          mainPartNegative = ParseMainIntervalPart(parts[0], out hours, out minutes, out seconds, out milliseconds);
          break;
        case 2:
          // only day part: "x days"
          mainPartNegative = false;
          days = int.Parse(parts[0]);
          break;
        case 3:
          // both day and HMS parts: "x days y:z:v.ww"
          days = int.Parse(parts[0]);
          mainPartNegative = ParseMainIntervalPart(parts[2], out hours, out minutes, out seconds, out milliseconds);
          break;
        default:
          throw new InvalidOperationException();
      }

      if (mainPartNegative || days < 0) {
        hours = -hours;
        minutes = -minutes;
        seconds = -seconds;
        milliseconds = -milliseconds;
        if (days >= 0)
          days = -days;
      }

      return new TimeSpan(days, hours, minutes, seconds, milliseconds);
    }

    private static bool ParseMainIntervalPart(string value,
      out int hours,
      out int minutes,
      out int seconds,
      out int millisececonds)
    {
      hours = minutes = seconds = millisececonds = 0;
      bool negativeResult;
      value = value.TryCutPrefix("-", out negativeResult);
      string[] parts = value.Split(':', '.');
      if (parts.Length == 0)
        return false;

      hours = int.Parse(parts[0]);
      if (parts.Length > 1) {
        minutes = int.Parse(parts[1]);
        if (parts.Length > 2) {
          seconds = int.Parse(parts[2]);
          if (parts.Length > 3)
            if (parts[3].Length > 4) {
              millisececonds = int.Parse(parts[3].Substring(0, 4));
              if (millisececonds % 10 > 4)
                millisececonds += 10;
              millisececonds /= 10;
            }
            else
              millisececonds = int.Parse(parts[3].PadRight(3, '0'));
        }
      }
      
      return negativeResult;
    }

    public static string TimeSpanToString(TimeSpan value)
    {
      return value.ToString("{0}{1} days {0}{2}:{3}:{4}.{5:000}");
    }
#endif

    // Constructors

    public TypeMapper(SqlDriver driver)
      : base(driver)
    {
    }
  }
}