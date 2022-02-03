// Copyright (C) 2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using System.Data.Common;
using System.Numerics;
using System.Text;
using FirebirdSql.Data.FirebirdClient;

namespace Xtensive.Sql.Drivers.Firebird.v4_0
{
  internal class TypeMapper : v2_5.TypeMapper
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
      if(typeOfValue == BigIntegerType) {
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
