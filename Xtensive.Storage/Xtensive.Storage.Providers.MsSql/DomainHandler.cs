// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.07.04

using System;
using Xtensive.Sql.Common;
using Xtensive.Storage.Providers.MsSql.Resources;
using Xtensive.Storage.Rse.Compilation;

namespace Xtensive.Storage.Providers.MsSql
{
  public class DomainHandler : Sql.DomainHandler
  {
    protected override ICompiler BuildCompiler()
    {
      return new Compilers.Compiler(Handlers);
    }

    public override SqlDataType GetSqlDataType(Type type, int? length)
    {
      if (type == typeof(Boolean))
        return SqlDataType.Boolean;
      if (type == typeof(SByte))
        return SqlDataType.SByte;
      if (type == typeof(Byte))
        return SqlDataType.Byte;
      if (type == typeof(Int16))
        return SqlDataType.Int16;
      if (type == typeof(UInt16))
        return SqlDataType.UInt16;
      if (type == typeof(Int32))
        return SqlDataType.Int32;
      if (type == typeof(UInt32))
        return SqlDataType.UInt32;
      if (type == typeof(Int64))
        return SqlDataType.Int64;
      if (type == typeof(UInt64))
        return SqlDataType.UInt64;
      if (type == typeof(Decimal))
        return SqlDataType.Decimal;
      if (type == typeof(float))
        return SqlDataType.Float;
      if (type == typeof(double))
        return SqlDataType.Double;
      if (type == typeof(DateTime))
        return SqlDataType.DateTime;
      if (type == typeof(String)) {
        if (length.HasValue && length.Value <= 4000)
          return SqlDataType.VarChar;
        else
          return SqlDataType.VarCharMax;
      }
      if (type == typeof(byte[])) {
        if (length.HasValue && length.Value <= 8000)
          return SqlDataType.VarBinary;
        else
          return SqlDataType.VarBinaryMax;
      }
      if (type == typeof(Guid))
        return SqlDataType.Guid;
      throw new InvalidOperationException(String.Format(Strings.ExUnsupportedColumnType, type.FullName));
      //    ********** not supported types  *************
      //    SmallMoney 
      //    Money
      //    SmallDateTime
      //    AnsiChar
      //    AnsiVarChar
      //    AnsiText
      //    AnsiVarCharMax
      //    Char
      //    VarChar
      //    Text
      //    Binary
      //    VarBinaryMax
      //    Image
      //    Variant
      //    TimeStamp
      //    Interval
      //    Xml
    }
  }
}