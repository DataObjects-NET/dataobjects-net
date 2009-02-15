// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.02.15

using System;
using Xtensive.Core.Linq;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom.Dml;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.Sql.Mappings.FunctionMappings
{
  internal class NumericMappings
  {
    private const SqlDataType StringType = SqlDataType.VarChar;

    #region ToString mappings

    [Compiler(typeof(byte), "ToString")]
    public static SqlExpression ByteToString(SqlExpression this_)
    {
      return SqlFactory.Cast(this_, StringType);
    }

    [Compiler(typeof(sbyte), "ToString")]
    public static SqlExpression SByteToString(SqlExpression this_)
    {
      return SqlFactory.Cast(this_, StringType);
    }

    [Compiler(typeof(short), "ToString")]
    public static SqlExpression ShortToString(SqlExpression this_)
    {
      return SqlFactory.Cast(this_, StringType);
    }

    [Compiler(typeof(ushort), "ToString")]
    public static SqlExpression UShortToString(SqlExpression this_)
    {
      return SqlFactory.Cast(this_, StringType);
    }

    [Compiler(typeof(int), "ToString")]
    public static SqlExpression IntToString(SqlExpression this_)
    {
      return SqlFactory.Cast(this_, StringType);
    }

    [Compiler(typeof(uint), "ToString")]
    public static SqlExpression UIntToString(SqlExpression this_)
    {
      return SqlFactory.Cast(this_, StringType);
    }

    [Compiler(typeof(long), "ToString")]
    public static SqlExpression LongToString(SqlExpression this_)
    {
      return SqlFactory.Cast(this_, StringType);
    }

    [Compiler(typeof(ulong), "ToString")]
    public static SqlExpression ULongToString(SqlExpression this_)
    {
      return SqlFactory.Cast(this_, StringType);
    }

    [Compiler(typeof(float), "ToString")]
    public static SqlExpression FloatToString(SqlExpression this_)
    {
      return SqlFactory.Cast(this_, StringType);
    }

    [Compiler(typeof(double), "ToString")]
    public static SqlExpression DoubleToString(SqlExpression this_)
    {
      return SqlFactory.Cast(this_, StringType);
    }

    [Compiler(typeof(decimal), "ToString")]
    public static SqlExpression DecimalToString(SqlExpression this_)
    {
      return SqlFactory.Cast(this_, StringType);
    }

    #endregion

    #region CompareTo mappings

    [Compiler(typeof(byte), "CompareTo")]
    public static SqlExpression ByteCompareTo(SqlExpression this_,
      [ParamType(typeof(byte))] SqlExpression value)
    {
      return SqlFactory.Sign(this_ - value);
    }
    
    [Compiler(typeof(sbyte), "CompareTo")]
    public static SqlExpression SByteCompareTo(SqlExpression this_,
      [ParamType(typeof(sbyte))] SqlExpression value)
    {
      return SqlFactory.Sign(this_ - value);
    }

    [Compiler(typeof(short), "CompareTo")]
    public static SqlExpression ShortCompareTo(SqlExpression this_,
      [ParamType(typeof(short))] SqlExpression value)
    {
      return SqlFactory.Sign(this_ - value);
    }

    [Compiler(typeof(ushort), "CompareTo")]
    public static SqlExpression UShortCompareTo(SqlExpression this_,
      [ParamType(typeof(ushort))] SqlExpression value)
    {
      return SqlFactory.Sign(this_ - value);
    }

    [Compiler(typeof(int), "CompareTo")]
    public static SqlExpression IntCompareTo(SqlExpression this_,
      [ParamType(typeof(int))] SqlExpression value)
    {
      return SqlFactory.Sign(this_ - value);
    }

    [Compiler(typeof(uint), "CompareTo")]
    public static SqlExpression UIntCompareTo(SqlExpression this_,
      [ParamType(typeof(uint))] SqlExpression value)
    {
      return SqlFactory.Sign(this_ - value);
    }

    [Compiler(typeof(long), "CompareTo")]
    public static SqlExpression LongCompareTo(SqlExpression this_,
      [ParamType(typeof(long))] SqlExpression value)
    {
      return SqlFactory.Sign(this_ - value);
    }

    [Compiler(typeof(ulong), "CompareTo")]
    public static SqlExpression ULongCompareTo(SqlExpression this_,
      [ParamType(typeof(ulong))] SqlExpression value)
    {
      return SqlFactory.Sign(this_ - value);
    }

    [Compiler(typeof(float), "CompareTo")]
    public static SqlExpression FloatCompareTo(SqlExpression this_,
      [ParamType(typeof(float))] SqlExpression value)
    {
      return SqlFactory.Sign(this_ - value);
    }

    [Compiler(typeof(double), "CompareTo")]
    public static SqlExpression DoubleCompareTo(SqlExpression this_,
      [ParamType(typeof(double))] SqlExpression value)
    {
      return SqlFactory.Sign(this_ - value);
    }

    [Compiler(typeof(decimal), "CompareTo")]
    public static SqlExpression DecimalCompareTo(SqlExpression this_,
      [ParamType(typeof(decimal))] SqlExpression value)
    {
      return SqlFactory.Sign(this_ - value);
    }

    #endregion

  }
}
