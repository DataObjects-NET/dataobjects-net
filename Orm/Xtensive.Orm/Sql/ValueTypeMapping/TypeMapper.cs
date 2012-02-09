// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.06.19

using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace Xtensive.Sql
{
  /// <summary>
  /// Abstract base class for any value (data) type mapper.
  /// </summary>
  public abstract class TypeMapper
  {
    private const int DecimalPrecisionLimit = 60;

    public SqlDriver Driver { get; private set; }

    protected int? MaxDecimalPrecision { get; private set; }
    protected int? VarCharMaxLength { get; private set; }
    protected int? VarBinaryMaxLength { get; private set; }
    protected static BinaryFormatter Formatter = new BinaryFormatter();

    public virtual bool IsLiteralCastRequired(Type type)
    {
      return false;
    }

    public virtual bool IsParameterCastRequired(Type type)
    {
      return false;
    }

    #region SetXxxParameterValue methods

    public virtual void SetBooleanParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Boolean;
      parameter.Value = value ?? DBNull.Value;
    }

    public virtual void SetCharParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.String;
      if (value==null) {
        parameter.Value = DBNull.Value;
        return;
      }
      var _char = (char) value;
      parameter.Value = _char==default(char) ? string.Empty : _char.ToString();
    }

    public virtual void SetStringParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.String;
      parameter.Value = value ?? DBNull.Value;
    }

    public virtual void SetByteParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Byte;
      parameter.Value = value ?? DBNull.Value;
    }

    public virtual void SetSByteParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.SByte;
      parameter.Value = value ?? DBNull.Value;
    }

    public virtual void SetShortParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Int16;
      parameter.Value = value ?? DBNull.Value;
    }

    public virtual void SetUShortParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.UInt16;
      parameter.Value = value ?? DBNull.Value;
    }

    public virtual void SetIntParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Int32;
      parameter.Value = value ?? DBNull.Value;
    }

    public virtual void SetUIntParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.UInt32;
      parameter.Value = value ?? DBNull.Value;
    }

    public virtual void SetLongParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Int64;
      parameter.Value = value ?? DBNull.Value;
    }

    public virtual void SetULongParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.UInt64;
      parameter.Value = value ?? DBNull.Value;
    }

    public virtual void SetFloatParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Single;
      parameter.Value = value ?? DBNull.Value;
    }

    public virtual void SetDoubleParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Double;
      parameter.Value = value ?? DBNull.Value;
    }

    public virtual void SetDecimalParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Decimal;
      parameter.Value = value ?? DBNull.Value;
    }

    public virtual void SetDateTimeParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.DateTime;
      parameter.Value = value ?? DBNull.Value;
    }

    public virtual void SetTimeSpanParameterValue(DbParameter parameter, object value)
    {
        parameter.DbType = DbType.Int64;
        if (value != null)
        {
            var timeSpan = (TimeSpan)value;
            parameter.Value = timeSpan.Ticks * 100;
        }
        else
            parameter.Value = DBNull.Value;
    }

    public virtual void SetGuidParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Guid;
      parameter.Value = value ?? DBNull.Value;
    }

    public virtual void SetByteArrayParameterValue(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.Binary;
      parameter.Value = value ?? DBNull.Value;
    }

    #endregion

    #region ReadXxx methods

    public virtual object ReadBoolean(DbDataReader reader, int index)
    {
      return reader.GetBoolean(index);
    }

    public virtual object ReadChar(DbDataReader reader, int index)
    {
      return reader.GetString(index).SingleOrDefault();
    }

    public virtual object ReadString(DbDataReader reader, int index)
    {
      return reader.GetString(index);
    }

    public virtual object ReadByte(DbDataReader reader, int index)
    {
      return reader.GetByte(index);
    }

    public virtual object ReadSByte(DbDataReader reader, int index)
    {
      return Convert.ToSByte(reader[index]);
    }

    public virtual object ReadShort(DbDataReader reader, int index)
    {
      return reader.GetInt16(index);
    }

    public virtual object ReadUShort(DbDataReader reader, int index)
    {
      return Convert.ToUInt16(reader[index]);
    }

    public virtual object ReadInt(DbDataReader reader, int index)
    {
      return reader.GetInt32(index);
    }

    public virtual object ReadUInt(DbDataReader reader, int index)
    {
      return Convert.ToUInt32(reader[index]);
    }

    public virtual object ReadLong(DbDataReader reader, int index)
    {
      return reader.GetInt64(index);
    }

    public virtual object ReadULong(DbDataReader reader, int index)
    {
      return Convert.ToUInt64(reader[index]);
    }

    public virtual object ReadFloat(DbDataReader reader, int index)
    {
      return reader.GetFloat(index);
    }

    public virtual object ReadDouble(DbDataReader reader, int index)
    {
      return reader.GetDouble(index);
    }

    public virtual object ReadDecimal(DbDataReader reader, int index)
    {
      return reader.GetDecimal(index);
    }

    public virtual object ReadDateTime(DbDataReader reader, int index)
    {
      return reader.GetDateTime(index);
    }

    public virtual object ReadTimeSpan(DbDataReader reader, int index)
    {
      throw new NotSupportedException();
    }

    public virtual object ReadGuid(DbDataReader reader, int index)
    {
      return reader.GetGuid(index);
    }

    public virtual object ReadByteArray(DbDataReader reader, int index)
    {
      var value = reader[index];
      if (value == null || value is byte[])
        return value;

      try {
        var ms = new MemoryStream();
        Formatter.Serialize(ms, value);
        return ms.ToArray();
      }
      catch (Exception e) {
        // Log this
        throw;
      }
    }

    #endregion

    #region BuildXxxSqlType methods

    public virtual SqlValueType BuildBooleanSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Boolean);
    }

    public virtual SqlValueType BuildCharSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.VarChar, 1);
    }

    public virtual SqlValueType BuildStringSqlType(int? length, int? precision, int? scale)
    {
      return ChooseStreamType(SqlType.VarChar, SqlType.VarCharMax, length, VarCharMaxLength);
    }
    
    public virtual SqlValueType BuildByteSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.UInt8);
    }

    public virtual SqlValueType BuildSByteSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Int8);
    }

    public virtual SqlValueType BuildShortSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Int16);
    }

    public virtual SqlValueType BuildUShortSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.UInt16);
    }

    public virtual SqlValueType BuildIntSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Int32);
    }

    public virtual SqlValueType BuildUIntSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.UInt32);
    }

    public virtual SqlValueType BuildLongSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Int64);
    }

    public virtual SqlValueType BuildULongSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.UInt64);
    }

    public virtual SqlValueType BuildFloatSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Float);
    }

    public virtual SqlValueType BuildDoubleSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Double);
    }

    public virtual SqlValueType BuildDecimalSqlType(int? length, int? precision, int? scale)
    {
      if (MaxDecimalPrecision==null)
        return new SqlValueType(SqlType.Decimal);
      if (precision==null) {
        var resultPrecision = Math.Min(DecimalPrecisionLimit, MaxDecimalPrecision.Value);
        var resultScale = resultPrecision / 2;
        return new SqlValueType(SqlType.Decimal, resultPrecision, resultScale);
      }
      if (precision.Value > MaxDecimalPrecision.Value)
        throw new InvalidOperationException(string.Format(
          Strings.ExSpecifiedPrecisionXIsGreaterThanMaximumSupportedByStorageY,
          precision.Value, MaxDecimalPrecision.Value));
      return new SqlValueType(SqlType.Decimal, null, null, precision, scale);
    }

    public virtual SqlValueType BuildDateTimeSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.DateTime);
    }

    public virtual SqlValueType BuildTimeSpanSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Interval);
    }

    public virtual SqlValueType BuildGuidSqlType(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Guid);
    }

    public virtual SqlValueType BuildByteArraySqlType(int? length, int? precision, int? scale)
    {
      return ChooseStreamType(SqlType.VarBinary, SqlType.VarBinaryMax, length, VarBinaryMaxLength);
    }

    #endregion

    protected static SqlValueType ChooseStreamType(SqlType varType, SqlType varMaxType, int? length, int? varTypeMaxLength)
    {
      if (varTypeMaxLength==null)
        return new SqlValueType(varMaxType);
      if (length==null)
        return new SqlValueType(varType, varTypeMaxLength.Value);
      if (length.Value > varTypeMaxLength.Value)
        return new SqlValueType(varMaxType);
      return new SqlValueType(varType, length.Value);
    }

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public virtual void Initialize()
    {
      var varchar = Driver.ServerInfo.DataTypes.VarChar;
      if (varchar!=null)
        VarCharMaxLength = varchar.MaxLength;
      var varbinary = Driver.ServerInfo.DataTypes.VarBinary;
      if (varbinary!=null)
        VarBinaryMaxLength = varbinary.MaxLength;
      var _decimal = Driver.ServerInfo.DataTypes.Decimal;
      if (_decimal!=null)
        MaxDecimalPrecision = _decimal.MaxPrecision;
    }

    // Constructors

    protected TypeMapper(SqlDriver driver)
    {
      Driver = driver;
    }
  }
}