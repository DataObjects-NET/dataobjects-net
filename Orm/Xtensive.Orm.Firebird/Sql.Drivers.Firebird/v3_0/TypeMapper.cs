// Copyright (C) 2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Data.Common;
using System.Numerics;

namespace Xtensive.Sql.Drivers.Firebird.v3_0
{
  internal class TypeMapper : v2_5.TypeMapper
  {
    private static readonly Type BigIntegerType = typeof(BigInteger);

    private static readonly BigInteger MaxIntAsBigInteger = new BigInteger(int.MaxValue);
    private static readonly BigInteger MinIntAsBigInteger = new BigInteger(int.MinValue);

    private static readonly BigInteger MaxUIntAsBigInteger = new BigInteger(uint.MaxValue);
    private static readonly BigInteger MinUIntAsBigInteger = new BigInteger(uint.MinValue);

    private static readonly BigInteger MaxLongAsBigInteger = new BigInteger(long.MaxValue);
    private static readonly BigInteger MinLongAsBigInteger = new BigInteger(long.MinValue);

    private static readonly BigInteger MaxULongAsBigInteger = new BigInteger(ulong.MaxValue);
    private static readonly BigInteger MinULongAsBigInteger = new BigInteger(ulong.MinValue);

    private static readonly BigInteger MaxDoubleAsBigInteger = new BigInteger(double.MaxValue);
    private static readonly BigInteger MinDoubleAsBigInteger = new BigInteger(double.MinValue);

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

    public TypeMapper(SqlDriver driver)
      : base(driver)
    {
    }
  }
}
