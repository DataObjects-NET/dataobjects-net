// Copyright (C) 2011-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Csaba Beer
// Created:    2011.01.10

using System;
using System.Data;
using System.Data.Common;
using System.Numerics;

namespace Xtensive.Sql.Drivers.Firebird.v3_0
{
  internal class TypeMapper : Sql.TypeMapper
  {
    private static readonly Type BigIntegerType = typeof(BigInteger);

    private static readonly BigInteger MaxIntAsBigInteger = new(int.MaxValue);
    private static readonly BigInteger MinIntAsBigInteger = new(int.MinValue);

    private static readonly BigInteger MaxUIntAsBigInteger = new(uint.MaxValue);
    private static readonly BigInteger MinUIntAsBigInteger = new(uint.MinValue);

    private static readonly BigInteger MaxLongAsBigInteger = new(long.MaxValue);
    private static readonly BigInteger MinLongAsBigInteger = new(long.MinValue);

    private static readonly BigInteger MaxULongAsBigInteger = new(ulong.MaxValue);
    private static readonly BigInteger MinULongAsBigInteger = new(ulong.MinValue);

    private static readonly BigInteger MaxDoubleAsBigInteger = new(double.MaxValue);
    private static readonly BigInteger MinDoubleAsBigInteger = new(double.MinValue);

    public override bool IsParameterCastRequired(Type type)
    {
      return true;
    }

    public override SqlValueType MapBoolean(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Int16);
    }

    public override SqlValueType MapUShort(int? length, int? precision, int? scale)
    {
      return base.MapInt(length, precision, scale);
    }

    public override SqlValueType MapUInt(int? length, int? precision, int? scale)
    {
      return base.MapLong(length, precision, scale);
    }

    public override SqlValueType MapULong(int? length, int? precision, int? scale)
    {
      return base.MapString(30, null, null);
      ;
    }

    public override SqlValueType MapChar(int? length, int? precision, int? scale)
    {
      return new SqlValueType(SqlType.Char, 1);
    }

    public override SqlValueType MapGuid(int? length, int? precision, int? scale)
    {
      return base.MapString(36, null, null);
      ;
    }

    public override SqlValueType MapByte(int? length, int? precision, int? scale)
    {
      return base.MapShort(length, precision, scale);
    }

    public override SqlValueType MapSByte(int? length, int? precision, int? scale)
    {
      return base.MapShort(length, precision, scale);
    }

    public override object ReadGuid(DbDataReader reader, int index)
    {
      var guidAsString = reader.GetString(index);
      return string.IsNullOrEmpty(guidAsString)
        ? null
        : SqlHelper.GuidFromString(guidAsString);
    }

    public override object ReadChar(DbDataReader reader, int index)
    {
      char c = (char) base.ReadChar(reader, index);
      if (char.IsControl(c) || char.IsPunctuation(c))
        return c;
      if (char.IsWhiteSpace(c))
        return null;
      return c;
    }

    public override object ReadInt(DbDataReader reader, int index)
    {
      var typeOfValue = reader.GetFieldType(index);
      if (typeOfValue == BigIntegerType) {
        var value = reader.GetFieldValue<BigInteger>(index);
        if (value > MaxIntAsBigInteger || value < MinIntAsBigInteger)
          throw new ArgumentOutOfRangeException();
        return (int) value;
      }
      return base.ReadInt(reader, index);
    }

    public override object ReadUInt(DbDataReader reader, int index)
    {
      var typeOfValue = reader.GetFieldType(index);
      if (typeOfValue == BigIntegerType) {
        var value = reader.GetFieldValue<BigInteger>(index);
        if (value > MaxUIntAsBigInteger || value < MinUIntAsBigInteger)
          throw new ArgumentOutOfRangeException();
        return (uint) value;
      }
      return base.ReadUInt(reader, index);
    }

    public override object ReadLong(DbDataReader reader, int index)
    {
      var typeOfValue = reader.GetFieldType(index);
      if (typeOfValue == BigIntegerType) {
        var value = reader.GetFieldValue<BigInteger>(index);
        if (value > MaxLongAsBigInteger || value < MinLongAsBigInteger)
          throw new ArgumentOutOfRangeException();
        return (long) value;
      }
      return base.ReadLong(reader, index);
    }

    public override object ReadULong(DbDataReader reader, int index)
    {
      var typeOfValue = reader.GetFieldType(index);
      if (typeOfValue == BigIntegerType) {
        var value = reader.GetFieldValue<BigInteger>(index);
        if (value > MaxULongAsBigInteger || value < MinULongAsBigInteger)
          throw new ArgumentOutOfRangeException();
        return (ulong) value;
      }
      return base.ReadULong(reader, index);
    }

    public override object ReadDouble(DbDataReader reader, int index)
    {
      var typeOfValue = reader.GetFieldType(index);
      if (typeOfValue == BigIntegerType) {
        var value = reader.GetFieldValue<BigInteger>(index);
        if (value > MaxDoubleAsBigInteger || value < MinDoubleAsBigInteger)
          throw new ArgumentOutOfRangeException();
        return (double) value;
      }
      return base.ReadDouble(reader, index);
    }

    public override object ReadString(DbDataReader reader, int index)
    {
      var s = reader.GetString(index);
      return s?.Trim();
    }

    public override void BindChar(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.String;
      if (value is null || (default(char).Equals(value))) {
        parameter.Value = DBNull.Value;
        return;
      }
      var _char = (char) value;
      parameter.Value = _char == default(char) ? string.Empty : _char.ToString();
    }

    public override void BindULong(DbParameter parameter, object value)
    {
      parameter.DbType = DbType.String;
      parameter.Value = value ?? DBNull.Value;
    }

    public override void BindBoolean(DbParameter parameter, object value)
    {
      BindShort(parameter,
        (value is null) ? value : (short) (((bool) value) ? 1 : 0));
    }

    public override void BindGuid(DbParameter parameter, object value)
    {
      BindString(parameter, (value is null) ? value : SqlHelper.GuidToString((Guid) value));
    }

    public TypeMapper(SqlDriver driver)
      : base(driver)
    {
    }
  }
}
