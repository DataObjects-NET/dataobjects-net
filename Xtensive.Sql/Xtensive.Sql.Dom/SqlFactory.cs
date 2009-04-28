// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Reflection;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Ddl;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Sql.Dom.Resources;

namespace Xtensive.Sql.Dom
{
  public static class Sql
  {
    public static readonly SqlDefaultValue DefaultValue = new SqlDefaultValue();
    public static readonly SqlNull Null = new SqlNull();
    public static readonly SqlBreak Break = new SqlBreak();
    public static readonly SqlContinue Continue = new SqlContinue();
    public static readonly SqlNative Asterisk = Native("*");

    #region Aggregates

    public static SqlAggregate Count()
    {
      return Count(Asterisk);
    }

    public static SqlAggregate Count(SqlExpression expression)
    {
      return Count(expression, false);
    }

    public static SqlAggregate Count(SqlExpression expression, bool distinct)
    {
      return new SqlAggregate(SqlNodeType.Count, expression, distinct);
    }

    public static SqlAggregate Avg(SqlExpression expression)
    {
      return Avg(expression, false);
    }

    public static SqlAggregate Avg(SqlExpression expression, bool distinct)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      return new SqlAggregate(SqlNodeType.Avg, expression, distinct);
    }

    public static SqlAggregate Sum(SqlExpression expression)
    {
      return Sum(expression, false);
    }

    public static SqlAggregate Sum(SqlExpression expression, bool distinct)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      return new SqlAggregate(SqlNodeType.Sum, expression, distinct);
    }

    public static SqlAggregate Min(SqlExpression expression)
    {
      return Min(expression, false);
    }

    public static SqlAggregate Min(SqlExpression expression, bool distinct)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      return new SqlAggregate(SqlNodeType.Min, expression, distinct);
    }

    public static SqlAggregate Max(SqlExpression expression)
    {
      return Max(expression, false);
    }

    public static SqlAggregate Max(SqlExpression expression, bool distinct)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      return new SqlAggregate(SqlNodeType.Max, expression, distinct);
    }

    #endregion

    #region Arithmetic

    public static SqlBinary Add(SqlExpression left, SqlExpression right)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      SqlValidator.EnsureIsArithmeticExpression(left);
      SqlValidator.EnsureIsArithmeticExpression(right);
      return Binary(SqlNodeType.Add, left, right);
    }

    public static SqlBinary Subtract(SqlExpression left, SqlExpression right)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      SqlValidator.EnsureIsArithmeticExpression(left);
      SqlValidator.EnsureIsArithmeticExpression(right);
      return Binary(SqlNodeType.Subtract, left, right);
    }

    public static SqlBinary Multiply(SqlExpression left, SqlExpression right)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      SqlValidator.EnsureIsArithmeticExpression(left);
      SqlValidator.EnsureIsArithmeticExpression(right);
      return Binary(SqlNodeType.Multiply, left, right);
    }

    public static SqlBinary Divide(SqlExpression left, SqlExpression right)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      SqlValidator.EnsureIsArithmeticExpression(left);
      SqlValidator.EnsureIsArithmeticExpression(right);
      return Binary(SqlNodeType.Divide, left, right);
    }

    public static SqlBinary Modulo(SqlExpression left, SqlExpression right)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      SqlValidator.EnsureIsArithmeticExpression(left);
      SqlValidator.EnsureIsArithmeticExpression(right);
      return Binary(SqlNodeType.Modulo, left, right);
    }

    #endregion

    #region Array

    public static SqlArray<bool> Array(params bool[] value)
    {
      return new SqlArray<bool>(value);
    }

    public static SqlArray<char> Array(params char[] value)
    {
      return new SqlArray<char>(value);
    }

    public static SqlArray<sbyte> Array(params sbyte[] value)
    {
      return new SqlArray<sbyte>(value);
    }

    public static SqlArray<byte> Array(params byte[] value)
    {
      return new SqlArray<byte>(value);
    }

    public static SqlArray<short> Array(params short[] value)
    {
      return new SqlArray<short>(value);
    }

    public static SqlArray<ushort> Array(params ushort[] value)
    {
      return new SqlArray<ushort>(value);
    }

    public static SqlArray<int> Array(params int[] value)
    {
      return new SqlArray<int>(value);
    }

    public static SqlArray<uint> Array(params uint[] value)
    {
      return new SqlArray<uint>(value);
    }

    public static SqlArray<long> Array(params long[] value)
    {
      return new SqlArray<long>(value);
    }

    public static SqlArray<ulong> Array(params ulong[] value)
    {
      return new SqlArray<ulong>(value);
    }

    public static SqlArray<float> Array(params float[] value)
    {
      return new SqlArray<float>(value);
    }

    public static SqlArray<double> Array(params double[] value)
    {
      return new SqlArray<double>(value);
    }

    public static SqlArray<decimal> Array(params decimal[] value)
    {
      return new SqlArray<decimal>(value);
    }

    public static SqlArray<string> Array(params string[] value)
    {
      return new SqlArray<string>(value);
    }

    public static SqlArray<DateTime> Array(params DateTime[] value)
    {
      return new SqlArray<DateTime>(value);
    }

    public static SqlArray<TimeSpan> Array(params TimeSpan[] value)
    {
      return new SqlArray<TimeSpan>(value);
    }

    #endregion

    #region Binary

    public static SqlBinary BitAnd(SqlExpression left, SqlExpression right)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      SqlValidator.EnsureIsArithmeticExpression(left);
      SqlValidator.EnsureIsArithmeticExpression(right);
      return Binary(SqlNodeType.BitAnd, left, right);
    }

    public static SqlBinary BitOr(SqlExpression left, SqlExpression right)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      SqlValidator.EnsureIsArithmeticExpression(left);
      SqlValidator.EnsureIsArithmeticExpression(right);
      return Binary(SqlNodeType.BitOr, left, right);
    }

    public static SqlBinary BitXor(SqlExpression left, SqlExpression right)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      SqlValidator.EnsureIsArithmeticExpression(left);
      SqlValidator.EnsureIsArithmeticExpression(right);
      return Binary(SqlNodeType.BitXor, left, right);
    }

    public static SqlBinary And(SqlExpression left, SqlExpression right)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      SqlValidator.EnsureIsBooleanExpression(left);
      SqlValidator.EnsureIsBooleanExpression(right);
      return Binary(SqlNodeType.And, left, right);
    }

    public static SqlBinary Or(SqlExpression left, SqlExpression right)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      SqlValidator.EnsureIsBooleanExpression(left);
      SqlValidator.EnsureIsBooleanExpression(right);
      return Binary(SqlNodeType.Or, left, right);
    }

    public static SqlQueryExpression Except(ISqlQueryExpression left, ISqlQueryExpression right)
    {
      return Except(left, right, false);
    }

    public static SqlQueryExpression ExceptAll(ISqlQueryExpression left, ISqlQueryExpression right)
    {
      return Except(left, right, true);
    }

    private static SqlQueryExpression Except(ISqlQueryExpression left, ISqlQueryExpression right, bool all)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      return new SqlQueryExpression(SqlNodeType.Except, left, right, all);
    }

    public static SqlQueryExpression Intersect(ISqlQueryExpression left, ISqlQueryExpression right)
    {
      return Intersect(left, right, false);
    }

    public static SqlQueryExpression IntersectAll(ISqlQueryExpression left, ISqlQueryExpression right)
    {
      return Intersect(left, right, true);
    }

    private static SqlQueryExpression Intersect(ISqlQueryExpression left, ISqlQueryExpression right, bool all)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      return new SqlQueryExpression(SqlNodeType.Intersect, left, right, all);
    }

    public static SqlQueryExpression Union(ISqlQueryExpression left, ISqlQueryExpression right)
    {
      return Union(left, right, false);
    }

    public static SqlQueryExpression UnionAll(ISqlQueryExpression left, ISqlQueryExpression right)
    {
      return Union(left, right, true);
    }

    private static SqlQueryExpression Union(ISqlQueryExpression left, ISqlQueryExpression right, bool all)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      return new SqlQueryExpression(SqlNodeType.Union, left, right, all);
    }

    public static SqlBinary In(SqlExpression left, ISqlQueryExpression right)
    {
      SqlValidator.EnsureIsRowValueConstructor(left);
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      return Binary(SqlNodeType.In, left, SubQuery(right));
    }

    public static SqlBinary In(SqlExpression left, SqlRow right)
    {
      SqlValidator.EnsureIsRowValueConstructor(left);
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      return Binary(SqlNodeType.In, left, right);
    }

    public static SqlBinary In<T>(SqlExpression left, SqlArray<T> right) where T : IConvertible
    {
      SqlValidator.EnsureIsRowValueConstructor(left);
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      return Binary(SqlNodeType.In, left, right);
    }

    public static SqlBinary NotIn(SqlExpression left, ISqlQueryExpression right)
    {
      SqlValidator.EnsureIsRowValueConstructor(left);
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      return Binary(SqlNodeType.NotIn, left, SubQuery(right));
    }

    public static SqlBinary NotIn(SqlExpression left, SqlRow right)
    {
      SqlValidator.EnsureIsRowValueConstructor(left);
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      return Binary(SqlNodeType.NotIn, left, right);
    }

    public static SqlBinary NotIn<T>(SqlExpression left, SqlArray<T> right) where T : IConvertible
    {
      SqlValidator.EnsureIsRowValueConstructor(left);
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      return Binary(SqlNodeType.NotIn, left, right);
    }

    public static SqlBetween Between(
      SqlExpression expression, SqlExpression leftBoundary, SqlExpression rightBoundary)
    {
      SqlValidator.EnsureIsRowValueConstructor(expression);
      SqlValidator.EnsureIsRowValueConstructor(leftBoundary);
      SqlValidator.EnsureIsRowValueConstructor(rightBoundary);
      return
        new SqlBetween(
          SqlNodeType.Between, expression, leftBoundary, rightBoundary);
    }

    public static SqlBetween NotBetween(
      SqlExpression expression, SqlExpression leftBoundary, SqlExpression rightBoundary)
    {
      SqlValidator.EnsureIsRowValueConstructor(expression);
      SqlValidator.EnsureIsRowValueConstructor(leftBoundary);
      SqlValidator.EnsureIsRowValueConstructor(rightBoundary);
      return
        new SqlBetween(
          SqlNodeType.NotBetween, expression, leftBoundary, rightBoundary);
    }

    internal static SqlBinary Binary(SqlNodeType nodeType, SqlExpression left, SqlExpression right)
    {
      return new SqlBinary(nodeType, left, right);
    }

    #endregion

    #region Cast

    public static SqlCast Cast(SqlExpression operand, SqlValueType type)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      SqlValidator.EnsureIsArithmeticExpression(operand);
      return new SqlCast(operand, type);
    }

    public static SqlCast Cast(SqlExpression operand, SqlDataType dataType)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      SqlValidator.EnsureIsArithmeticExpression(operand);
      return new SqlCast(operand, new SqlValueType(dataType));
    }

    public static SqlCast Cast(SqlExpression operand, SqlDataType dataType, int size)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      SqlValidator.EnsureIsArithmeticExpression(operand);
      return new SqlCast(operand, new SqlValueType(dataType, size));
    }

    public static SqlCast Cast(SqlExpression operand, SqlDataType dataType, short precision, short scale)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      SqlValidator.EnsureIsArithmeticExpression(operand);
      return new SqlCast(operand, new SqlValueType(dataType, precision, scale));
    }

    public static SqlCast Cast(SqlExpression operand, SqlDataType dataType, short precision)
    {
      return Cast(operand, dataType, precision, 0);
    }

    #endregion

    #region Comparison

    public static SqlBinary Equals(SqlExpression left, SqlExpression right)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      return Binary(SqlNodeType.Equals, left, right);
    }

    public static SqlBinary NotEquals(SqlExpression left, SqlExpression right)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      return Binary(SqlNodeType.NotEquals, left, right);
    }

    public static SqlBinary GreaterThan(SqlExpression left, SqlExpression right)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      return Binary(SqlNodeType.GreaterThan, left, right);
    }

    public static SqlBinary LessThan(SqlExpression left, SqlExpression right)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      return Binary(SqlNodeType.LessThan, left, right);
    }

    public static SqlBinary GreaterThanOrEquals(SqlExpression left, SqlExpression right)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      return Binary(SqlNodeType.GreaterThanOrEquals, left, right);
    }

    public static SqlBinary LessThanOrEquals(SqlExpression left, SqlExpression right)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      return Binary(SqlNodeType.LessThanOrEquals, left, right);
    }

    #endregion

    # region DateTime functions

    public static SqlFunctionCall CurrentDate()
    {
      return new SqlFunctionCall(SqlFunctionType.CurrentDate);
    }
    
    /*
    public static SqlFunctionCall CurrentTime()
    {
      return new SqlFunctionCall(SqlFunctionType.CurrentTime);
    }

    public static SqlFunctionCall CurrentTime(uint precision)
    {
      return new SqlFunctionCall(SqlFunctionType.CurrentTime, Literal(precision));
    }

    public static SqlFunctionCall CurrentTime(SqlExpression precision)
    {
      SqlValidator.EnsureIsArithmeticExpression(precision);
      return new SqlFunctionCall(SqlFunctionType.CurrentTime, precision);
    }
    */

    public static SqlFunctionCall CurrentTimeStamp()
    {
      return new SqlFunctionCall(SqlFunctionType.CurrentTimeStamp);
    }

    public static SqlFunctionCall CurrentTimeStamp(uint precision)
    {
      return new SqlFunctionCall(SqlFunctionType.CurrentTimeStamp, Literal(precision));
    }

    public static SqlFunctionCall CurrentTimeStamp(SqlExpression precision)
    {
      SqlValidator.EnsureIsArithmeticExpression(precision);
      return new SqlFunctionCall(SqlFunctionType.CurrentTimeStamp, precision);
    }

    public static SqlFunctionCall Extract(SqlDateTimePart part, SqlExpression source)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      SqlValidator.EnsureIsArithmeticExpression(source);
      return new SqlFunctionCall(SqlFunctionType.Extract, new SqlLiteral<SqlDateTimePart>(part), source);
    }
    
    public static SqlFunctionCall DateTimeConstruct(SqlExpression year, SqlExpression month, SqlExpression day)
    {
      ArgumentValidator.EnsureArgumentNotNull(year, "year");
      ArgumentValidator.EnsureArgumentNotNull(month, "month");
      ArgumentValidator.EnsureArgumentNotNull(day, "day");
      SqlValidator.EnsureIsArithmeticExpression(year);
      SqlValidator.EnsureIsArithmeticExpression(month);
      SqlValidator.EnsureIsArithmeticExpression(day);
      return new SqlFunctionCall(SqlFunctionType.DateTimeConstruct, year, month, day);
    }

    public static SqlFunctionCall DateTimeAddInterval(SqlExpression left, SqlExpression right)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      SqlValidator.EnsureIsArithmeticExpression(left);
      SqlValidator.EnsureIsArithmeticExpression(right);
      return new SqlFunctionCall(SqlFunctionType.DateTimeAddInterval, left, right);
    }

    public static SqlFunctionCall DateTimeSubtractInterval(SqlExpression left, SqlExpression right)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      SqlValidator.EnsureIsArithmeticExpression(left);
      SqlValidator.EnsureIsArithmeticExpression(right);
      return new SqlFunctionCall(SqlFunctionType.DateTimeSubtractInterval, left, right);
    }

    public static SqlFunctionCall DateTimeSubtractDateTime(SqlExpression left, SqlExpression right)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      SqlValidator.EnsureIsArithmeticExpression(left);
      SqlValidator.EnsureIsArithmeticExpression(right);
      return new SqlFunctionCall(SqlFunctionType.DateTimeSubtractDateTime, left, right);
    }

    public static SqlFunctionCall DateTimeAddYears(SqlExpression source, SqlExpression years)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(years, "years");
      SqlValidator.EnsureIsArithmeticExpression(source);
      SqlValidator.EnsureIsArithmeticExpression(years);
      return new SqlFunctionCall(SqlFunctionType.DateTimeAddYears, source, years);
    }

    public static SqlFunctionCall DateTimeAddMonths(SqlExpression source, SqlExpression months)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      ArgumentValidator.EnsureArgumentNotNull(months, "months");
      SqlValidator.EnsureIsArithmeticExpression(source);
      SqlValidator.EnsureIsArithmeticExpression(months);
      return new SqlFunctionCall(SqlFunctionType.DateTimeAddMonths, source, months);
    }

    public static SqlFunctionCall DateTimeTruncate(SqlExpression source)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      SqlValidator.EnsureIsArithmeticExpression(source);
      return new SqlFunctionCall(SqlFunctionType.DateTimeTruncate, source);
    }

    public static SqlFunctionCall IntervalConstruct(SqlExpression milliseconds)
    {
      ArgumentValidator.EnsureArgumentNotNull(milliseconds, "milliseconds");
      SqlValidator.EnsureIsArithmeticExpression(milliseconds);
      return new SqlFunctionCall(SqlFunctionType.IntervalConstruct, milliseconds);
    }

    public static SqlFunctionCall IntervalToMilliseconds(SqlExpression source)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      SqlValidator.EnsureIsArithmeticExpression(source);
      return new SqlFunctionCall(SqlFunctionType.IntervalToMilliseconds, source); 
    }

    public static SqlFunctionCall IntervalExtract(SqlIntervalPart part, SqlExpression source)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      SqlValidator.EnsureIsArithmeticExpression(source);
      return new SqlFunctionCall(SqlFunctionType.IntervalExtract, new SqlLiteral<SqlIntervalPart>(part), source);
    }

    public static SqlFunctionCall IntervalDuration(SqlExpression source)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      SqlValidator.EnsureIsArithmeticExpression(source);
      return new SqlFunctionCall(SqlFunctionType.IntervalDuration, source);
    }

    #endregion
    
    #region DDL

    public static SqlCreateAssertion Create(Assertion assertion)
    {
      ArgumentValidator.EnsureArgumentNotNull(assertion, "assertion");
      return new SqlCreateAssertion(assertion);
    }

    public static SqlCreateCharacterSet Create(CharacterSet characterSet)
    {
      ArgumentValidator.EnsureArgumentNotNull(characterSet, "characterSet");
      return new SqlCreateCharacterSet(characterSet);
    }

    public static SqlCreateCollation Create(Collation collation)
    {
      ArgumentValidator.EnsureArgumentNotNull(collation, "collation");
      return new SqlCreateCollation(collation);
    }

    public static SqlCreateDomain Create(Domain domain)
    {
      ArgumentValidator.EnsureArgumentNotNull(domain, "domain");
      return new SqlCreateDomain(domain);
    }

    public static SqlCreateIndex Create(Index index)
    {
      ArgumentValidator.EnsureArgumentNotNull(index, "index");
      return new SqlCreateIndex(index);
    }

    public static SqlCreatePartitionFunction Create(PartitionFunction partitionFunction)
    {
      ArgumentValidator.EnsureArgumentNotNull(partitionFunction, "partitionFunction");
      return new SqlCreatePartitionFunction(partitionFunction);
    }

    public static SqlCreatePartitionScheme Create(PartitionSchema partitionSchema)
    {
      ArgumentValidator.EnsureArgumentNotNull(partitionSchema, "partitionSchema");
      return new SqlCreatePartitionScheme(partitionSchema);
    }

    public static SqlCreateSchema Create(Schema schema)
    {
      ArgumentValidator.EnsureArgumentNotNull(schema, "schema");
      return new SqlCreateSchema(schema);
    }

    public static SqlCreateSequence Create(Sequence sequence)
    {
      ArgumentValidator.EnsureArgumentNotNull(sequence, "sequence");
      return new SqlCreateSequence(sequence);
    }

    public static SqlCreateTable Create(Table table)
    {
      ArgumentValidator.EnsureArgumentNotNull(table, "table");
      return new SqlCreateTable(table);
    }

    public static SqlCreateTranslation Create(Translation translation)
    {
      ArgumentValidator.EnsureArgumentNotNull(translation, "translation");
      return new SqlCreateTranslation(translation);
    }

    public static SqlCreateView Create(View view)
    {
      ArgumentValidator.EnsureArgumentNotNull(view, "view");
      return new SqlCreateView(view);
    }

    public static SqlDropAssertion Drop(Assertion assertion)
    {
      ArgumentValidator.EnsureArgumentNotNull(assertion, "assertion");
      return new SqlDropAssertion(assertion);
    }

    public static SqlDropCharacterSet Drop(CharacterSet characterSet)
    {
      ArgumentValidator.EnsureArgumentNotNull(characterSet, "characterSet");
      return new SqlDropCharacterSet(characterSet);
    }

    public static SqlDropCollation Drop(Collation collation)
    {
      ArgumentValidator.EnsureArgumentNotNull(collation, "collation");
      return new SqlDropCollation(collation);
    }

    public static SqlDropDomain Drop(Domain domain)
    {
      ArgumentValidator.EnsureArgumentNotNull(domain, "domain");
      return new SqlDropDomain(domain);
    }

    public static SqlDropIndex Drop(Index index)
    {
      ArgumentValidator.EnsureArgumentNotNull(index, "index");
      return new SqlDropIndex(index);
    }

    //public static SqlDropIndex Drop(IIndex index, bool? online)
    //{
    //  ArgumentValidator.EnsureArgumentNotNull(index, "index");
    //  return new SqlDropIndex(index, online, null);
    //}

    //public static SqlDropIndex Drop(IIndex index, byte? maxDegreeOfParallelism)
    //{
    //  ArgumentValidator.EnsureArgumentNotNull(index, "index");
    //  return new SqlDropIndex(index, null, maxDegreeOfParallelism);
    //}

    //public static SqlDropIndex Drop(IIndex index, bool? online, byte? maxDegreeOfParallelism)
    //{
    //  ArgumentValidator.EnsureArgumentNotNull(index, "index");
    //  return new SqlDropIndex(index, online, maxDegreeOfParallelism);
    //}

    //public static SqlDropIndex Drop(IIndex index, bool? online, byte? maxDegreeOfParallelism, IPartitionDescriptor partitioningDescriptor)
    //{
    //  ArgumentValidator.EnsureArgumentNotNull(index, "index");
    //  return new SqlDropIndex(index, online, maxDegreeOfParallelism, partitioningDescriptor);
    //}

    //public static SqlDropIndex Drop(IIndex index, bool? online, byte? maxDegreeOfParallelism, string tableSpace)
    //{
    //  ArgumentValidator.EnsureArgumentNotNull(index, "index");
    //  return new SqlDropIndex(index, online, maxDegreeOfParallelism, tableSpace);
    //}

    //public static SqlDropIndex Drop(IIndex index, byte? maxDegreeOfParallelism, IPartitionDescriptor partitioningDescriptor)
    //{
    //  ArgumentValidator.EnsureArgumentNotNull(index, "index");
    //  return new SqlDropIndex(index, null, maxDegreeOfParallelism, partitioningDescriptor);
    //}

    //public static SqlDropIndex Drop(IIndex index, byte? maxDegreeOfParallelism, string tableSpace)
    //{
    //  ArgumentValidator.EnsureArgumentNotNull(index, "index");
    //  return new SqlDropIndex(index, null, maxDegreeOfParallelism, tableSpace);
    //}

    //public static SqlDropIndex Drop(IIndex index, bool? online, IPartitionDescriptor partitioningDescriptor)
    //{
    //  ArgumentValidator.EnsureArgumentNotNull(index, "index");
    //  return new SqlDropIndex(index, online, null, partitioningDescriptor);
    //}

    //public static SqlDropIndex Drop(IIndex index, bool? online, string tableSpace)
    //{
    //  ArgumentValidator.EnsureArgumentNotNull(index, "index");
    //  return new SqlDropIndex(index, online, null, tableSpace);
    //}

    //public static SqlDropIndex Drop(IIndex index, IPartitionDescriptor partitioningDescriptor)
    //{
    //  ArgumentValidator.EnsureArgumentNotNull(index, "index");
    //  return new SqlDropIndex(index, null, null, partitioningDescriptor);
    //}

    //public static SqlDropIndex Drop(IIndex index, string tableSpace)
    //{
    //  ArgumentValidator.EnsureArgumentNotNull(index, "index");
    //  return new SqlDropIndex(index, null, null, tableSpace);
    //}

    public static SqlDropPartitionFunction Drop(PartitionFunction partitionFunction)
    {
      ArgumentValidator.EnsureArgumentNotNull(partitionFunction, "partitionFunction");
      return new SqlDropPartitionFunction(partitionFunction);
    }

    public static SqlDropPartitionScheme Drop(PartitionSchema partitionSchema)
    {
      ArgumentValidator.EnsureArgumentNotNull(partitionSchema, "partitionSchema");
      return new SqlDropPartitionScheme(partitionSchema);
    }

    public static SqlDropDomain Drop(Domain domain, bool cascade)
    {
      ArgumentValidator.EnsureArgumentNotNull(domain, "domain");
      return new SqlDropDomain(domain, cascade);
    }

    public static SqlDropSchema Drop(Schema schema)
    {
      ArgumentValidator.EnsureArgumentNotNull(schema, "schema");
      return new SqlDropSchema(schema);
    }

    public static SqlDropSchema Drop(Schema schema, bool cascade)
    {
      ArgumentValidator.EnsureArgumentNotNull(schema, "schema");
      return new SqlDropSchema(schema, cascade);
    }

    public static SqlDropSequence Drop(Sequence sequence)
    {
      ArgumentValidator.EnsureArgumentNotNull(sequence, "sequence");
      return new SqlDropSequence(sequence);
    }

    public static SqlDropSequence Drop(Sequence sequence, bool cascade)
    {
      ArgumentValidator.EnsureArgumentNotNull(sequence, "sequence");
      return new SqlDropSequence(sequence, cascade);
    }

    public static SqlDropTable Drop(Table table)
    {
      ArgumentValidator.EnsureArgumentNotNull(table, "table");
      return new SqlDropTable(table);
    }

    public static SqlDropTable Drop(Table table, bool cascade)
    {
      ArgumentValidator.EnsureArgumentNotNull(table, "table");
      return new SqlDropTable(table, cascade);
    }

    public static SqlDropTranslation Drop(Translation translation)
    {
      ArgumentValidator.EnsureArgumentNotNull(translation, "translation");
      return new SqlDropTranslation(translation);
    }

    public static SqlDropView Drop(View view)
    {
      ArgumentValidator.EnsureArgumentNotNull(view, "view");
      return new SqlDropView(view);
    }

    public static SqlDropView Drop(View view, bool cascade)
    {
      ArgumentValidator.EnsureArgumentNotNull(view, "view");
      return new SqlDropView(view, cascade);
    }

    public static SqlAlterTable Alter(Table table, SqlAction action)
    {
      ArgumentValidator.EnsureArgumentNotNull(table, "table");
      ArgumentValidator.EnsureArgumentNotNull(action, "action");
      if (action is SqlSetDefault && ((SqlSetDefault)action).Column==null ||
          action is SqlDropDefault && ((SqlDropDefault)action).Column==null)
        throw new ArgumentException("Invalid action type.", "action");
      if (action is SqlAddColumn && ((SqlAddColumn)action).Column.DataTable!=null &&
          ((SqlAddColumn)action).Column.DataTable!=table ||
          action is SqlSetDefault && ((SqlSetDefault)action).Column.DataTable!=null &&
          ((SqlSetDefault)action).Column.DataTable!=table ||
          action is SqlDropDefault && ((SqlDropDefault)action).Column.DataTable!=null &&
          ((SqlDropDefault)action).Column.DataTable!=table ||
          action is SqlDropColumn && ((SqlDropColumn)action).Column.DataTable!=null &&
          ((SqlDropColumn)action).Column.DataTable!=table ||
          action is SqlAlterIdentityInfo && ((SqlAlterIdentityInfo)action).Column.DataTable!=null &&
          ((SqlAlterIdentityInfo)action).Column.DataTable!=table)
        throw new ArgumentException("Column belongs to other table.", "action");
      else if (action is SqlAddConstraint) {
        TableConstraint constraint = ((SqlAddConstraint)action).Constraint as TableConstraint;
        if (constraint==null)
          throw new ArgumentException("Invalid constraint type.", "action");
        else if (constraint.Table!=null && constraint.Table!=table)
          throw new ArgumentException("Constraint belongs to other table.", "action");
      }
      else if (action is SqlDropConstraint) {
        TableConstraint constraint = ((SqlDropConstraint)action).Constraint as TableConstraint;
        if (constraint==null)
          throw new ArgumentException("Invalid constraint type.", "action");
        else if (constraint.Table!=null && constraint.Table!=table)
          throw new ArgumentException("Constraint belongs to other table.", "action");
      }
      return new SqlAlterTable(table, action);
    }

    public static SqlAlterTable Rename(Table table, string name)
    {
      ArgumentValidator.EnsureArgumentNotNull(table, "table");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      if (table.Name == name)
        throw new ArgumentException("Table already has specified name.");

      return Alter(table, new SqlRenameAction(table, name));
    }

    public static SqlAlterTable Rename(TableColumn column, string name)
    {
      ArgumentValidator.EnsureArgumentNotNull(column, "table");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      if (column.Name == name)
        throw new ArgumentException("Column already has specified name.");

      return Alter(column.Table, new SqlRenameAction(column, name));
    }

    public static SqlAlterSequence Alter(Sequence sequence, SequenceDescriptor descriptor, SqlAlterIdentityInfoOptions infoOption)
    {
      ArgumentValidator.EnsureArgumentNotNull(sequence, "sequence");
      ArgumentValidator.EnsureArgumentNotNull(descriptor, "info");
      return new SqlAlterSequence(sequence, descriptor, infoOption);
    }

    public static SqlAlterSequence Alter(Sequence sequence, SequenceDescriptor descriptor)
    {
      ArgumentValidator.EnsureArgumentNotNull(sequence, "sequence");
      ArgumentValidator.EnsureArgumentNotNull(descriptor, "info");
      return new SqlAlterSequence(sequence, descriptor, SqlAlterIdentityInfoOptions.All);
    }

    public static SqlAlterDomain Alter(Domain domain, SqlAction action)
    {
      ArgumentValidator.EnsureArgumentNotNull(domain, "domain");
      ArgumentValidator.EnsureArgumentNotNull(action, "action");
      if (action is SqlAddConstraint) {
        DomainConstraint constraint = ((SqlAddConstraint)action).Constraint as DomainConstraint;
        if (constraint==null)
          throw new ArgumentException("Invalid constraint type.", "action");
        else if (constraint.Domain!=null && constraint.Domain!=domain)
          throw new ArgumentException("Constraint belongs to other domain.", "action");
      }
      else if (action is SqlDropConstraint) {
        DomainConstraint constraint = ((SqlDropConstraint)action).Constraint as DomainConstraint;
        if (constraint==null)
          throw new ArgumentException("Invalid constraint type.", "action");
        else if (constraint.Domain!=null && constraint.Domain!=domain)
          throw new ArgumentException("Constraint belongs to other domain.", "action");
      }
      else if (action is SqlSetDefault && ((SqlSetDefault)action).Column!=null ||
               action is SqlDropDefault && ((SqlDropDefault)action).Column!=null)
        throw new ArgumentException("Invalid action type.", "action");
      else if (action is SqlAddColumn || action is SqlDropColumn || action is SqlAlterIdentityInfo)
        throw new ArgumentException("Invalid action type.", "action");
      return new SqlAlterDomain(domain, action);
    }

    public static SqlAlterPartitionFunction Alter(
      PartitionFunction partitionFunction, string booundary, SqlAlterPartitionFunctionOption option)
    {
      ArgumentValidator.EnsureArgumentNotNull(partitionFunction, "partitionFunction");
      ArgumentValidator.EnsureArgumentNotNull(booundary, "booundary");
      return new SqlAlterPartitionFunction(partitionFunction, booundary, option);
    }

    public static SqlAlterPartitionScheme Alter(PartitionSchema partitionSchema)
    {
      ArgumentValidator.EnsureArgumentNotNull(partitionSchema, "partitionSchema");
      return new SqlAlterPartitionScheme(partitionSchema, null);
    }

    public static SqlAlterPartitionScheme Alter(PartitionSchema partitionSchema, string filegroup)
    {
      ArgumentValidator.EnsureArgumentNotNull(partitionSchema, "partitionSchema");
      return new SqlAlterPartitionScheme(partitionSchema, filegroup);
    }

    public static SqlAlterIdentityInfo Alter(TableColumn column, SequenceDescriptor descriptor)
    {
      ArgumentValidator.EnsureArgumentNotNull(column, "column");
      ArgumentValidator.EnsureArgumentNotNull(descriptor, "info");
      return new SqlAlterIdentityInfo(column, descriptor, SqlAlterIdentityInfoOptions.All);
    }

    public static SqlAlterIdentityInfo Alter(
      TableColumn column, SequenceDescriptor descriptor, SqlAlterIdentityInfoOptions infoOption)
    {
      ArgumentValidator.EnsureArgumentNotNull(column, "column");
      ArgumentValidator.EnsureArgumentNotNull(descriptor, "info");
      return new SqlAlterIdentityInfo(column, descriptor, infoOption);
    }

    public static SqlAddColumn AddColumn(TableColumn column)
    {
      ArgumentValidator.EnsureArgumentNotNull(column, "column");
      return new SqlAddColumn(column);
    }

    public static SqlAddConstraint AddConstraint(Constraint constraint)
    {
      ArgumentValidator.EnsureArgumentNotNull(constraint, "constraint");
      return new SqlAddConstraint(constraint);
    }

    public static SqlDropColumn DropColumn(TableColumn column)
    {
      ArgumentValidator.EnsureArgumentNotNull(column, "column");
      return new SqlDropColumn(column);
    }

    public static SqlDropColumn DropColumn(TableColumn column, bool cascade)
    {
      ArgumentValidator.EnsureArgumentNotNull(column, "column");
      return new SqlDropColumn(column, cascade);
    }

    public static SqlDropConstraint DropConstraint(Constraint constraint)
    {
      ArgumentValidator.EnsureArgumentNotNull(constraint, "constraint");
      return new SqlDropConstraint(constraint);
    }

    public static SqlDropConstraint DropConstraint(Constraint constraint, bool cascade)
    {
      ArgumentValidator.EnsureArgumentNotNull(constraint, "constraint");
      return new SqlDropConstraint(constraint, cascade);
    }

    public static SqlSetDefault SetDefault(SqlExpression defaulValue, TableColumn column)
    {
      ArgumentValidator.EnsureArgumentNotNull(defaulValue, "defaulValue");
      return new SqlSetDefault(defaulValue, column);
    }

    public static SqlSetDefault SetDefault(SqlExpression defaulValue)
    {
      return SetDefault(defaulValue, null);
    }

    public static SqlDropDefault DropDefault(TableColumn column)
    {
      return new SqlDropDefault(column);
    }

    public static SqlDropDefault DropDefault()
    {
      return DropDefault(null);
    }

    #endregion

    #region FunctionCall

    public static SqlUserFunctionCall FunctionCall(string name, IEnumerable<SqlExpression> expressions)
    {
      SqlValidator.EnsureAreSqlRowArguments(expressions);
      return new SqlUserFunctionCall(name, expressions);
    }

    public static SqlUserFunctionCall FunctionCall(string name, params SqlExpression[] expressions)
    {
      Collection<SqlExpression> collection = new Collection<SqlExpression>();
      for (int i = 0, l = expressions.Length; i<l; i++)
        collection.Add(expressions[i]);
      return FunctionCall(name, collection);
    }

    public static SqlUserFunctionCall FunctionCall(string name)
    {
      return FunctionCall(name, new Collection<SqlExpression>());
    }

    public static SqlFunctionCall CurrentUser()
    {
      return new SqlFunctionCall(SqlFunctionType.CurrentUser);
    }

    public static SqlFunctionCall SessionUser()
    {
      return new SqlFunctionCall(SqlFunctionType.SessionUser);
    }

    public static SqlFunctionCall SystemUser()
    {
      return new SqlFunctionCall(SqlFunctionType.SystemUser);
    }

    public static SqlFunctionCall User()
    {
      return new SqlFunctionCall(SqlFunctionType.User);
    }

    public static SqlFunctionCall NullIf(SqlExpression left, SqlExpression right)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      SqlValidator.EnsureIsArithmeticExpression(left);
      SqlValidator.EnsureIsArithmeticExpression(right);
      //SqlCase c = new SqlCase(null);
      //c[left==right] = Null;
      //c.Else = left;
      //return c;
      return new SqlFunctionCall(SqlFunctionType.NullIf, left, right);
    }

    public static SqlFunctionCall Coalesce(SqlExpression left, SqlExpression right)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      SqlValidator.EnsureIsArithmeticExpression(left);
      SqlValidator.EnsureIsArithmeticExpression(right);
      //SqlCase c = new SqlCase(null);
      //c[IsNotNull(left)] = left;
      //c.Else = right;
      //return c;
      return new SqlFunctionCall(SqlFunctionType.Coalesce, left, right);
    }

    public static SqlFunctionCall Coalesce(
      SqlExpression left, SqlExpression right, params SqlExpression[] values)
    {
      //if (values!=null && values.Length>0) {
      //  SqlExpression[] v = null;
      //  if (values.Length>1) {
      //    v = new SqlExpression[values.Length-1];
      //    for (int i = 1, l = values.Length; i<l; i++)
      //      v[i-1] = values[i];
      //  }
      //  ArgumentValidator.EnsureArgumentNotNull(left, "left");
      //  SqlValidator.VerifyArithmeticalOperatorsArgs(left);
      //  SqlCase c = new SqlCase(null);
      //  c[IsNotNull(left)] = left;
      //  c.Else = Coalesce(right, values[0], v);
      //  return c;
      //}
      //else {
      //  return Coalesce(left, right);
      //}
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      SqlValidator.EnsureIsArithmeticExpression(left);
      SqlValidator.EnsureIsArithmeticExpression(right);
      SqlExpression[] expressions;
      if (values!=null) {
        int l = values.Length;
        expressions = new SqlExpression[l+2];
        expressions[0] = left;
        expressions[1] = right;
        for (int i = 0; i<l; i++) {
          ArgumentValidator.EnsureArgumentNotNull(values[i], "values");
          SqlValidator.EnsureIsArithmeticExpression(values[i]);
          expressions[i+2] = values[i];
        }
      }
      else
        expressions = new SqlExpression[2] {left, right};
      return new SqlFunctionCall(SqlFunctionType.Coalesce, expressions);
    }

    #endregion

    #region Join

    public static SqlJoinedTable Join(SqlJoinType joinType, SqlTable left, SqlTable right)
    {
      return Join(joinType, left, right, null);
    }

    public static SqlJoinedTable Join(
      SqlJoinType joinType, SqlTable left, SqlTable right, SqlExpression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      if (expression != null && (joinType == SqlJoinType.CrossApply || joinType == SqlJoinType.LeftOuterApply))
        throw new ArgumentException(Strings.ExJoinExpressionShouldBeNullForCrossApplyAndLeftOuterApply, "expression");
      return new SqlJoinedTable(new SqlJoinExpression(joinType, left, right, expression));
    }

    public static SqlJoinedTable Join(SqlTable left, SqlTable right, params SqlColumn[] columns)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      return new SqlJoinedTable(new SqlJoinExpression(SqlJoinType.UsingJoin, left, right, Row(columns)));
    }

    #endregion

    #region Literal

    public static SqlLiteral<T> Literal<T>(T value)
    {
      if (ReferenceEquals(value, null))
        throw new ArgumentNullException("value");
      if (typeof (T)==typeof (object))
        SqlValidator.EnsureLiteralTypeIsSupported(value.GetType());
      else
        SqlValidator.EnsureLiteralTypeIsSupported(typeof (T));
      return new SqlLiteral<T>(value);
    }

    public static SqlLiteral<bool> Literal(bool value)
    {
      return new SqlLiteral<bool>(value);
    }

    public static SqlLiteral<char> Literal(char value)
    {
      return new SqlLiteral<char>(value);
    }

    public static SqlLiteral<sbyte> Literal(sbyte value)
    {
      return new SqlLiteral<sbyte>(value);
    }

    public static SqlLiteral<byte> Literal(byte value)
    {
      return new SqlLiteral<byte>(value);
    }

    public static SqlLiteral<short> Literal(short value)
    {
      return new SqlLiteral<short>(value);
    }

    public static SqlLiteral<ushort> Literal(ushort value)
    {
      return new SqlLiteral<ushort>(value);
    }

    public static SqlLiteral<int> Literal(int value)
    {
      return new SqlLiteral<int>(value);
    }

    public static SqlLiteral<uint> Literal(uint value)
    {
      return new SqlLiteral<uint>(value);
    }

    public static SqlLiteral<long> Literal(long value)
    {
      return new SqlLiteral<long>(value);
    }

    public static SqlLiteral<ulong> Literal(ulong value)
    {
      return new SqlLiteral<ulong>(value);
    }

    public static SqlLiteral<float> Literal(float value)
    {
      return new SqlLiteral<float>(value);
    }

    public static SqlLiteral<double> Literal(double value)
    {
      return new SqlLiteral<double>(value);
    }

    public static SqlLiteral<decimal> Literal(decimal value)
    {
      return new SqlLiteral<decimal>(value);
    }

    public static SqlLiteral<string> Literal(string value)
    {
      return new SqlLiteral<string>(value);
    }

    public static SqlLiteral<DateTime> Literal(DateTime value)
    {
      return new SqlLiteral<DateTime>(value);
    }

    public static SqlLiteral<TimeSpan> Literal(TimeSpan value)
    {
      return new SqlLiteral<TimeSpan>(value);
    }

    public static SqlLiteral<byte[]> Literal(byte[] value)
    {
      return new SqlLiteral<byte[]>(value);
    }

    public static SqlLiteral<Guid> Literal(Guid value)
    {
      return new SqlLiteral<Guid>(value);
    }

    public static SqlExpression Literal(object value, Type valueType)
    {
      var type = typeof (SqlLiteral<>).MakeGenericType(valueType);
      var method = type.GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
      var result = (SqlExpression)method.Invoke(null, new[] {value});
      return result;
    }



    #endregion

    # region Match

    public static SqlMatch Match(SqlRow value, ISqlQueryExpression query, bool unique, SqlMatchType matchType)
    {
      ArgumentValidator.EnsureArgumentNotNull(value, "value");
      ArgumentValidator.EnsureArgumentNotNull(query, "query");
      return new SqlMatch(value, SubQuery(query), unique, matchType);
    }

    public static SqlMatch Match(SqlRow value, ISqlQueryExpression query, bool unique)
    {
      return Match(value, query, unique, SqlMatchType.None);
    }

    public static SqlMatch Match(SqlRow value, ISqlQueryExpression query, SqlMatchType matchType)
    {
      return Match(value, query, false, matchType);
    }

    public static SqlMatch Match(SqlRow value, ISqlQueryExpression query)
    {
      return Match(value, query, false, SqlMatchType.None);
    }

    public static SqlMatch Match(ISqlQueryExpression value, ISqlQueryExpression query, bool unique, SqlMatchType matchType)
    {
      ArgumentValidator.EnsureArgumentNotNull(value, "value");
      ArgumentValidator.EnsureArgumentNotNull(query, "query");
      return new SqlMatch(SubQuery(value), SubQuery(query), unique, matchType);
    }

    public static SqlMatch Match(ISqlQueryExpression value, ISqlQueryExpression query, bool unique)
    {
      return Match(value, query, unique, SqlMatchType.None);
    }

    public static SqlMatch Match(ISqlQueryExpression value, ISqlQueryExpression query, SqlMatchType matchType)
    {
      return Match(value, query, false, matchType);
    }

    public static SqlMatch Match(ISqlQueryExpression value, ISqlQueryExpression query)
    {
      return Match(value, query, false, SqlMatchType.None);
    }

    # endregion

    # region Math functions

    public static SqlFunctionCall Abs(SqlExpression argument)
    {
      ArgumentValidator.EnsureArgumentNotNull(argument, "argument");
      SqlValidator.EnsureIsArithmeticExpression(argument);
      return new SqlFunctionCall(SqlFunctionType.Abs, argument);
    }

    public static SqlFunctionCall Acos(SqlExpression argument)
    {
      ArgumentValidator.EnsureArgumentNotNull(argument, "argument");
      SqlValidator.EnsureIsArithmeticExpression(argument);
      return new SqlFunctionCall(SqlFunctionType.Acos, argument);
    }

    public static SqlFunctionCall Asin(SqlExpression argument)
    {
      ArgumentValidator.EnsureArgumentNotNull(argument, "argument");
      SqlValidator.EnsureIsArithmeticExpression(argument);
      return new SqlFunctionCall(SqlFunctionType.Asin, argument);
    }

    public static SqlFunctionCall Atan(SqlExpression argument)
    {
      ArgumentValidator.EnsureArgumentNotNull(argument, "argument");
      SqlValidator.EnsureIsArithmeticExpression(argument);
      return new SqlFunctionCall(SqlFunctionType.Atan, argument);
    }

    public static SqlFunctionCall Atan2(SqlExpression argument1, SqlExpression argument2)
    {
      ArgumentValidator.EnsureArgumentNotNull(argument1, "argument1");
      ArgumentValidator.EnsureArgumentNotNull(argument2, "argument2");
      SqlValidator.EnsureIsArithmeticExpression(argument1);
      SqlValidator.EnsureIsArithmeticExpression(argument2);
      return new SqlFunctionCall(SqlFunctionType.Atan2, argument1, argument2);
    }

    public static SqlFunctionCall Ceiling(SqlExpression argument)
    {
      ArgumentValidator.EnsureArgumentNotNull(argument, "argument");
      SqlValidator.EnsureIsArithmeticExpression(argument);
      return new SqlFunctionCall(SqlFunctionType.Ceiling, argument);
    }

    public static SqlFunctionCall Cos(SqlExpression argument)
    {
      ArgumentValidator.EnsureArgumentNotNull(argument, "argument");
      SqlValidator.EnsureIsArithmeticExpression(argument);
      return new SqlFunctionCall(SqlFunctionType.Cos, argument);
    }

    public static SqlFunctionCall Cot(SqlExpression argument)
    {
      ArgumentValidator.EnsureArgumentNotNull(argument, "argument");
      SqlValidator.EnsureIsArithmeticExpression(argument);
      return new SqlFunctionCall(SqlFunctionType.Cot, argument);
    }

    public static SqlFunctionCall Degrees(SqlExpression argument)
    {
      ArgumentValidator.EnsureArgumentNotNull(argument, "argument");
      SqlValidator.EnsureIsArithmeticExpression(argument);
      return new SqlFunctionCall(SqlFunctionType.Degrees, argument);
    }

    public static SqlFunctionCall Exp(SqlExpression argument)
    {
      ArgumentValidator.EnsureArgumentNotNull(argument, "argument");
      SqlValidator.EnsureIsArithmeticExpression(argument);
      return new SqlFunctionCall(SqlFunctionType.Exp, argument);
    }

    public static SqlFunctionCall Floor(SqlExpression argument)
    {
      ArgumentValidator.EnsureArgumentNotNull(argument, "argument");
      SqlValidator.EnsureIsArithmeticExpression(argument);
      return new SqlFunctionCall(SqlFunctionType.Floor, argument);
    }

    public static SqlFunctionCall Log(SqlExpression argument)
    {
      ArgumentValidator.EnsureArgumentNotNull(argument, "argument");
      SqlValidator.EnsureIsArithmeticExpression(argument);
      return new SqlFunctionCall(SqlFunctionType.Log, argument);
    }

    public static SqlFunctionCall Log10(SqlExpression argument)
    {
      ArgumentValidator.EnsureArgumentNotNull(argument, "argument");
      SqlValidator.EnsureIsArithmeticExpression(argument);
      return new SqlFunctionCall(SqlFunctionType.Log10, argument);
    }

    public static SqlFunctionCall Pi()
    {
      return new SqlFunctionCall(SqlFunctionType.Pi);
    }

    public static SqlFunctionCall Power(SqlExpression argument, SqlExpression power)
    {
      ArgumentValidator.EnsureArgumentNotNull(argument, "argument");
      SqlValidator.EnsureIsArithmeticExpression(argument);
      ArgumentValidator.EnsureArgumentNotNull(power, "power");
      SqlValidator.EnsureIsArithmeticExpression(power);
      return new SqlFunctionCall(SqlFunctionType.Power, argument, power);
    }

    public static SqlFunctionCall Radians(SqlExpression argument)
    {
      ArgumentValidator.EnsureArgumentNotNull(argument, "argument");
      SqlValidator.EnsureIsArithmeticExpression(argument);
      return new SqlFunctionCall(SqlFunctionType.Radians, argument);
    }

    public static SqlFunctionCall Rand(SqlExpression seed)
    {
      if (!SqlExpression.IsNull(seed)) {
        SqlValidator.EnsureIsArithmeticExpression(seed);
        return new SqlFunctionCall(SqlFunctionType.Rand, seed);
      }
      else
        return new SqlFunctionCall(SqlFunctionType.Rand, seed);
    }

    public static SqlFunctionCall Rand()
    {
      return Rand(null);
    }

    public static SqlFunctionCall Round(SqlExpression argument, SqlExpression length, SqlExpression function)
    {
      ArgumentValidator.EnsureArgumentNotNull(argument, "argument");
      SqlValidator.EnsureIsArithmeticExpression(argument);
      ArgumentValidator.EnsureArgumentNotNull(length, "length");
      SqlValidator.EnsureIsArithmeticExpression(length);
      if (!SqlExpression.IsNull(function)) {
        SqlValidator.EnsureIsArithmeticExpression(function);
        return new SqlFunctionCall(SqlFunctionType.Round, argument, length, function);
      }
      else
        return new SqlFunctionCall(SqlFunctionType.Round, argument, length);
    }

    public static SqlFunctionCall Round(SqlExpression argument, SqlExpression length)
    {
      return Round(argument, length, null);
    }

    public static SqlFunctionCall Sign(SqlExpression argument)
    {
      ArgumentValidator.EnsureArgumentNotNull(argument, "argument");
      SqlValidator.EnsureIsArithmeticExpression(argument);
      return new SqlFunctionCall(SqlFunctionType.Sign, argument);
    }

    public static SqlFunctionCall Sin(SqlExpression argument)
    {
      ArgumentValidator.EnsureArgumentNotNull(argument, "argument");
      SqlValidator.EnsureIsArithmeticExpression(argument);
      return new SqlFunctionCall(SqlFunctionType.Sin, argument);
    }

    public static SqlFunctionCall Sqrt(SqlExpression argument)
    {
      ArgumentValidator.EnsureArgumentNotNull(argument, "argument");
      SqlValidator.EnsureIsArithmeticExpression(argument);
      return new SqlFunctionCall(SqlFunctionType.Sqrt, argument);
    }

    public static SqlFunctionCall Square(SqlExpression argument)
    {
      ArgumentValidator.EnsureArgumentNotNull(argument, "argument");
      SqlValidator.EnsureIsArithmeticExpression(argument);
      return new SqlFunctionCall(SqlFunctionType.Square, argument);
    }

    public static SqlFunctionCall Tan(SqlExpression argument)
    {
      ArgumentValidator.EnsureArgumentNotNull(argument, "argument");
      SqlValidator.EnsureIsArithmeticExpression(argument);
      return new SqlFunctionCall(SqlFunctionType.Tan, argument);
    }

    # endregion

    #region Misc

    public static SqlNative Native(string value)
    {
      ArgumentValidator.EnsureArgumentNotNull(value, "value");
      return new SqlNative(value);
    }

    public static SqlSubQuery SubQuery(ISqlQueryExpression operand)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      return new SqlSubQuery(operand);
    }

    public static SqlCursor Cursor(string name)
    {
      return Cursor(name, null);
    }

    public static SqlCursor Cursor(string name, ISqlQueryExpression query)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      return new SqlCursor(name, query);
    }

    public static SqlParameterRef ParameterRef(object parameter)
    {
      ArgumentValidator.EnsureArgumentNotNull(parameter, "parameter");
      return new SqlParameterRef(parameter);
    }

    public static SqlParameterRef ParameterRef(string name)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      return new SqlParameterRef(name);
    }

    public static SqlCase Case(SqlExpression value)
    {
      return new SqlCase(value);
    }

    public static SqlCase Case()
    {
      return Case(null);
    }

    public static SqlNextValue NextValue(Sequence sequence)
    {
      ArgumentValidator.EnsureArgumentNotNull(sequence, "sequence");
      return new SqlNextValue(sequence);
    }

    public static SqlStatementBlock StatementBlock()
    {
      return new SqlStatementBlock();
    }

    public static SqlVariant Variant(SqlExpression main, SqlExpression alternative, object key)
    {
      ArgumentValidator.EnsureArgumentNotNull(main, "main");
      ArgumentValidator.EnsureArgumentNotNull(alternative, "alternative");
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
      return new SqlVariant(main, alternative, key);
    }

    #endregion

    #region OrderBy

    public static SqlOrder Order(SqlExpression expression)
    {
      return Order(expression, true);
    }

    public static SqlOrder Order(SqlExpression expression, bool ascending)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      return new SqlOrder(expression, ascending);
    }

    public static SqlOrder Order(int position)
    {
      return Order(position, true);
    }

    public static SqlOrder Order(int position, bool ascending)
    {
      if (position<=0)
        throw new ArgumentOutOfRangeException(
          "position", position, Strings.ExPositionValueShouldBeGreaterThanZero);
      return new SqlOrder(position, ascending);
    }

    #endregion

    #region Row

    public static SqlRow Row(IList<SqlExpression> expressions)
    {
      SqlValidator.EnsureAreSqlRowArguments(expressions);
      return new SqlRow(expressions);
    }

    public static SqlRow Row(params SqlExpression[] expressions)
    {
      Collection<SqlExpression> collection = new Collection<SqlExpression>();
      for (int i = 0, l = expressions.Length; i<l; i++)
        collection.Add(expressions[i]);
      return Row(collection);
    }

    public static SqlRow Row()
    {
      return Row(new Collection<SqlExpression>());
    }

    #endregion

    #region Statement

    public static SqlWhile While(SqlExpression condition)
    {
      ArgumentValidator.EnsureArgumentNotNull(condition, "condition");
      SqlValidator.EnsureIsBooleanExpression(condition);
      return new SqlWhile(condition);
    }

    public static SqlBatch Batch()
    {
      return new SqlBatch();
    }

    public static SqlAssignment Assign(SqlVariable left, SqlExpression right)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      SqlValidator.EnsureIsArithmeticExpression(right);
      return new SqlAssignment(left, right);
    }

    public static SqlAssignment Assign(SqlParameterRef left, SqlExpression right)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      SqlValidator.EnsureIsArithmeticExpression(right);
      return new SqlAssignment(left, right);
    }

    public static SqlIf If(SqlExpression condition, SqlStatement ifTrue, SqlStatement ifFalse)
    {
      ArgumentValidator.EnsureArgumentNotNull(condition, "condition");
      SqlValidator.EnsureIsBooleanExpression(condition);
      ArgumentValidator.EnsureArgumentNotNull(ifTrue, "ifTrue");
      return new SqlIf(condition, ifTrue, ifFalse);
    }

    public static SqlIf If(SqlExpression condition, SqlStatement ifTrue)
    {
      return If(condition, ifTrue, null);
    }

    public static SqlDelete Delete()
    {
      return new SqlDelete();
    }

    public static SqlDelete Delete(SqlTableRef tableRef)
    {
      ArgumentValidator.EnsureArgumentNotNull(tableRef, "table");
      return new SqlDelete(tableRef);
    }

    public static SqlUpdate Update()
    {
      return new SqlUpdate();
    }

    public static SqlUpdate Update(SqlTableRef tableRef)
    {
      ArgumentValidator.EnsureArgumentNotNull(tableRef, "table");
      return new SqlUpdate(tableRef);
    }

    public static SqlInsert Insert()
    {
      return new SqlInsert();
    }

    public static SqlInsert Insert(SqlTableRef tableRef)
    {
      ArgumentValidator.EnsureArgumentNotNull(tableRef, "table");
      return new SqlInsert(tableRef);
    }

    public static SqlSelect Select()
    {
      return new SqlSelect();
    }

    public static SqlSelect Select(SqlTable table)
    {
      ArgumentValidator.EnsureArgumentNotNull(table, "table");
      return new SqlSelect(table);
    }

    #endregion

    # region String functions

    public static SqlBinary Concat(SqlExpression left, SqlExpression right)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      SqlValidator.EnsureIsCharacterExpression(left);
      SqlValidator.EnsureIsCharacterExpression(right);
      return new SqlBinary(SqlNodeType.Concat, left, right);
    }
    
    /// <summary>
    /// Concates underlying expression without any sign between.
    /// </summary>
    /// <param name="left">Left expression</param>
    /// <param name="right">Right expression</param>
    /// <returns>New <see cref="SqlBinary"/> expression.</returns>
    public static SqlBinary RawConcat(SqlExpression left, SqlExpression right)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      SqlValidator.EnsureIsCharacterExpression(left);
      SqlValidator.EnsureIsCharacterExpression(right);
      return new SqlBinary(SqlNodeType.RawConcat, left, right);
    }

    public static SqlFunctionCall Substring(SqlExpression operand, int start)
    {
      return Substring(operand, start, null);
    }

    public static SqlFunctionCall Substring(SqlExpression operand, int start, int? length)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      SqlValidator.EnsureIsCharacterExpression(operand);
      if (length<0)
        throw new ArgumentException(Strings.ExLengthShouldBeNotNegativeValue, "length");
      Collection<SqlExpression> arguments = new Collection<SqlExpression>();
      arguments.Add(operand);
      arguments.Add(new SqlLiteral<int>(start));
      if (length.HasValue)
        arguments.Add(new SqlLiteral<int>(length.Value));
      return new SqlFunctionCall(SqlFunctionType.Substring, arguments);
    }

    public static SqlFunctionCall Substring(
      SqlExpression operand, SqlExpression start)
    {
      return Substring(operand, start, null);
    }

    public static SqlFunctionCall Substring(
      SqlExpression operand, SqlExpression start, SqlExpression length)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      ArgumentValidator.EnsureArgumentNotNull(start, "start");
      SqlValidator.EnsureIsCharacterExpression(operand);
      SqlValidator.EnsureIsArithmeticExpression(start);
      if (length != null)
        SqlValidator.EnsureIsArithmeticExpression(length);
      Collection<SqlExpression> arguments = new Collection<SqlExpression>();
      arguments.Add(operand);
      arguments.Add(start);
      if (length != null)
        arguments.Add(length);
      return new SqlFunctionCall(SqlFunctionType.Substring, arguments);
    }

    public static SqlFunctionCall Upper(SqlExpression operand)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      SqlValidator.EnsureIsCharacterExpression(operand);
      Collection<SqlExpression> arguments = new Collection<SqlExpression>();
      arguments.Add(operand);
      return new SqlFunctionCall(SqlFunctionType.Upper, arguments);
    }

    public static SqlFunctionCall Lower(SqlExpression operand)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      SqlValidator.EnsureIsCharacterExpression(operand);
      Collection<SqlExpression> arguments = new Collection<SqlExpression>();
      arguments.Add(operand);
      return new SqlFunctionCall(SqlFunctionType.Lower, arguments);
    }

    public static SqlTrim Trim(SqlExpression operand, SqlTrimType trimType, char character)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      SqlValidator.EnsureIsCharacterExpression(operand);
      return new SqlTrim(operand, new SqlLiteral<char>(character), trimType);
    }

    public static SqlTrim Trim(SqlExpression operand, SqlTrimType trimType)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      SqlValidator.EnsureIsCharacterExpression(operand);
      return new SqlTrim(operand, null, trimType);
    }

    public static SqlTrim Trim(SqlExpression operand)
    {
      return Trim(operand, SqlTrimType.Both);
    }

    public static SqlLike Like(
      SqlExpression expression, SqlExpression pattern, SqlExpression escape)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      ArgumentValidator.EnsureArgumentNotNull(pattern, "pattern");
      SqlValidator.EnsureIsCharacterExpression(expression);
      SqlValidator.EnsureIsCharacterExpression(pattern);
      SqlValidator.EnsureIsCharacterExpression(escape);
      return new SqlLike(expression, pattern, escape, false);
    }

    public static SqlLike Like(SqlExpression expression, SqlExpression pattern)
    {
      return Like(expression, pattern, null);
    }

    public static SqlLike Like(SqlExpression expression, string pattern)
    {
      ArgumentValidator.EnsureArgumentNotNull(pattern, "pattern");
      return Like(expression, new SqlLiteral<string>(pattern), null);
    }

    public static SqlLike Like(SqlExpression expression, string pattern, char escape)
    {
      SqlValidator.EnsureIsCharacterExpression(expression);
      ArgumentValidator.EnsureArgumentNotNull(pattern, "pattern");
      return Like(expression, new SqlLiteral<string>(pattern), new SqlLiteral<char>(escape));
    }

    public static SqlLike NotLike(
      SqlExpression expression, SqlExpression pattern, SqlExpression escape)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      ArgumentValidator.EnsureArgumentNotNull(pattern, "pattern");
      SqlValidator.EnsureIsCharacterExpression(expression);
      SqlValidator.EnsureIsCharacterExpression(pattern);
      SqlValidator.EnsureIsCharacterExpression(escape);
      return new SqlLike(expression, pattern, escape, true);
    }

    public static SqlLike NotLike(SqlExpression expression, SqlExpression pattern)
    {
      return NotLike(expression, pattern, null);
    }

    public static SqlLike NotLike(SqlExpression expression, string pattern)
    {
      ArgumentValidator.EnsureArgumentNotNull(pattern, "pattern");
      return NotLike(expression, new SqlLiteral<string>(pattern), null);
    }

    public static SqlLike NotLike(SqlExpression expression, string pattern, char escape)
    {
      SqlValidator.EnsureIsCharacterExpression(expression);
      ArgumentValidator.EnsureArgumentNotNull(pattern, "pattern");
      return NotLike(expression, new SqlLiteral<string>(pattern), new SqlLiteral<char>(escape));
    }

    public static SqlBinary Overlaps(DateTime from1, TimeSpan span1, DateTime from2, TimeSpan span2)
    {
      return new SqlBinary(SqlNodeType.Overlaps, Row(from1, span1), Row(from2, span2));
    }

    public static SqlBinary Overlaps(DateTime from1, DateTime to1, DateTime from2, DateTime to2)
    {
      return new SqlBinary(SqlNodeType.Overlaps, Row(from1, to1), Row(from2, to2));
    }

    public static SqlBinary Overlaps(SqlExpression from1, SqlExpression toOrSpan1, SqlExpression from2, SqlExpression toOrSpan2)
    {
      ArgumentValidator.EnsureArgumentNotNull(from1, "from1");
      ArgumentValidator.EnsureArgumentNotNull(toOrSpan1, "toOrSpan1");
      ArgumentValidator.EnsureArgumentNotNull(from2, "from2");
      ArgumentValidator.EnsureArgumentNotNull(toOrSpan2, "toOrSpan2");
      return new SqlBinary(SqlNodeType.Overlaps, Row(from1, toOrSpan1), Row(from2, toOrSpan2));
    }

    public static SqlBinary Overlaps(SqlRow left, SqlRow right)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      return new SqlBinary(SqlNodeType.Overlaps, left, right);
    }

    public static SqlBinary Overlaps(SqlSelect left, SqlRow right)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      return new SqlBinary(SqlNodeType.Overlaps, SubQuery(left), right);
    }

    public static SqlBinary Overlaps(SqlRow left, SqlSelect right)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      return new SqlBinary(SqlNodeType.Overlaps, left, SubQuery(right));
    }

    public static SqlBinary Overlaps(SqlSelect left, SqlSelect right)
    {
      ArgumentValidator.EnsureArgumentNotNull(left, "left");
      ArgumentValidator.EnsureArgumentNotNull(right, "right");
      return new SqlBinary(SqlNodeType.Overlaps, left, right);
    }

    public static SqlFunctionCall Length(SqlExpression operand)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      SqlValidator.EnsureIsCharacterExpression(operand);
      Collection<SqlExpression> arguments = new Collection<SqlExpression>();
      arguments.Add(operand);
      return new SqlFunctionCall(SqlFunctionType.Length, arguments);
    }

    public static SqlFunctionCall CharLength(SqlExpression operand)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      SqlValidator.EnsureIsCharacterExpression(operand);
      return new SqlFunctionCall(SqlFunctionType.CharLength, operand);
    }

    public static SqlFunctionCall OctetLength(SqlExpression operand)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      SqlValidator.EnsureIsCharacterExpression(operand);
      return new SqlFunctionCall(SqlFunctionType.OctetLength, operand);
    }

    public static SqlFunctionCall BitLength(SqlExpression operand)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      SqlValidator.EnsureIsCharacterExpression(operand);
      return new SqlFunctionCall(SqlFunctionType.BitLength, operand);
    }

    public static SqlFunctionCall Position(SqlExpression pattern, SqlExpression source)
    {
      ArgumentValidator.EnsureArgumentNotNull(pattern, "pattern");
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      SqlValidator.EnsureIsCharacterExpression(pattern);
      SqlValidator.EnsureIsCharacterExpression(source);
      Collection<SqlExpression> arguments = new Collection<SqlExpression>();
      arguments.Add(pattern);
      arguments.Add(source);
      return new SqlFunctionCall(SqlFunctionType.Position, arguments);
    }

    public static SqlFunctionCall Replace(SqlExpression text, SqlExpression from, SqlExpression to)
    {
      ArgumentValidator.EnsureArgumentNotNull(text, "text");
      ArgumentValidator.EnsureArgumentNotNull(from, "from");
      ArgumentValidator.EnsureArgumentNotNull(to, "to");
      SqlValidator.EnsureIsCharacterExpression(text);
      SqlValidator.EnsureIsCharacterExpression(from);
      SqlValidator.EnsureIsCharacterExpression(to);
      Collection<SqlExpression> arguments = new Collection<SqlExpression>();
      arguments.Add(text);
      arguments.Add(from);
      arguments.Add(to);
      return new SqlFunctionCall(SqlFunctionType.Replace, arguments);
    }

    //
    //    public static SqlFunctionCall Convert(SqlExpression operand, string conversionName)
    //    {
    //      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
    //      SqlValidator.VerifyCharacterArgs(operand);
    //      ArgumentValidator.EnsureArgumentNotNullOrEmpty(conversionName, "conversionName");
    //      return new SqlFunctionCall(SqlFunctionType.Convert, operand, Literal(conversionName));
    //    }
    //
    //    public static SqlFunctionCall Translate(SqlExpression operand, ITranslation translation)
    //    {
    //      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
    //      SqlValidator.VerifyCharacterArgs(operand);
    //      ArgumentValidator.EnsureArgumentNotNull(translation, "translation");
    //      return new SqlFunctionCall(SqlFunctionType.Translate, operand, Native(translation.Name));
    //    }

    public static SqlCollate Collate(SqlExpression operand, Collation collation)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      SqlValidator.EnsureIsCharacterExpression(operand);
      ArgumentValidator.EnsureArgumentNotNull(collation, "collation");
      return new SqlCollate(operand, collation);
    }

    # endregion

    #region DataTable & Column

    internal static SqlTableColumn TableColumn(SqlTable sqlTable)
    {
      ArgumentValidator.EnsureArgumentNotNull(sqlTable, "table");
      return new SqlTableColumn(sqlTable, string.Empty);
    }

    internal static SqlTableColumn TableColumn(SqlTable sqlTable, string name)
    {
      ArgumentValidator.EnsureArgumentNotNull(sqlTable, "table");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      return new SqlTableColumn(sqlTable, name);
    }

    public static SqlUserColumn Column(SqlExpression expression)
    {
      ArgumentValidator.EnsureArgumentNotNull(expression, "expression");
      return new SqlUserColumn(expression);
    }

    internal static SqlColumnRef ColumnRef(SqlColumn column)
    {
      ArgumentValidator.EnsureArgumentNotNull(column, "column");
      return new SqlColumnRef(column);
    }

    public static SqlColumnRef ColumnRef(SqlColumn column, string alias)
    {
      ArgumentValidator.EnsureArgumentNotNull(column, "column");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(alias, "alias");
      return new SqlColumnRef(column, alias);
    }

    public static SqlTableRef TableRef(DataTable dataTable)
    {
      ArgumentValidator.EnsureArgumentNotNull(dataTable, "dataTable");
      return new SqlTableRef(dataTable);
    }

    public static SqlTableRef TableRef(DataTable dataTable, string name)
    {
      ArgumentValidator.EnsureArgumentNotNull(dataTable, "dataTable");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "alias");
      return new SqlTableRef(dataTable, name);
    }

    public static SqlQueryRef QueryRef(ISqlQueryExpression query)
    {
      ArgumentValidator.EnsureArgumentNotNull(query, "query");
      return new SqlQueryRef(query);
    }

    public static SqlQueryRef QueryRef(ISqlQueryExpression query, string alias)
    {
      ArgumentValidator.EnsureArgumentNotNull(query, "query");
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(alias, "alias");
      return new SqlQueryRef(query, alias);
    }

    #endregion

    #region Unary

    public static SqlUnary BitNot(SqlExpression operand)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      SqlValidator.EnsureIsArithmeticExpression(operand);
      return Unary(SqlNodeType.BitNot, operand);
    }

    public static SqlUnary Negate(SqlExpression operand)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      SqlValidator.EnsureIsArithmeticExpression(operand);
      return Unary(SqlNodeType.Negate, operand);
    }

    public static SqlUnary Not(SqlExpression operand)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      SqlValidator.EnsureIsBooleanExpression(operand);
      return Unary(SqlNodeType.Not, operand);
    }

    public static SqlUnary IsNull(SqlExpression operand)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      SqlValidator.EnsureIsArithmeticExpression(operand);
      return Unary(SqlNodeType.IsNull, operand);
    }

    public static SqlUnary IsNotNull(SqlExpression operand)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      SqlValidator.EnsureIsArithmeticExpression(operand);
      return Unary(SqlNodeType.Not, Unary(SqlNodeType.IsNull, operand));
    }

    public static SqlUnary Unique(ISqlQueryExpression operand)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      return Unary(SqlNodeType.Unique, SubQuery(operand));
    }

    public static SqlUnary Exists(ISqlQueryExpression operand)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      return new SqlUnary(SqlNodeType.Exists, SubQuery(operand));
    }

    public static SqlUnary All(ISqlQueryExpression operand)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      return Unary(SqlNodeType.All, SubQuery(operand));
    }

    public static SqlUnary Any(ISqlQueryExpression operand)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      return Unary(SqlNodeType.Any, SubQuery(operand));
    }

    public static SqlUnary Some(ISqlQueryExpression operand)
    {
      ArgumentValidator.EnsureArgumentNotNull(operand, "operand");
      return Unary(SqlNodeType.Some, SubQuery(operand));
    }

    internal static SqlUnary Unary(SqlNodeType nodeType, SqlExpression operand)
    {
      return new SqlUnary(nodeType, operand);
    }

    #endregion

    #region Variable

    public static SqlVariable Variable(string name, SqlValueType type)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      return new SqlVariable(name, type);
    }

    public static SqlVariable Variable(string name, SqlDataType dataType)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      return new SqlVariable(name, new SqlValueType(dataType));
    }

    public static SqlVariable Variable(string name, SqlDataType dataType, int size)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      return new SqlVariable(name, new SqlValueType(dataType, size));
    }

    public static SqlVariable Variable(string name, SqlDataType dataType, short precision, short scale)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(name, "name");
      return new SqlVariable(name, new SqlValueType(dataType, precision, scale));
    }

    public static SqlVariable Variable(string name, SqlDataType dataType, short precision)
    {
      return Variable(name, dataType, precision, 0);
    }

    #endregion

    #region Hints

    public static SqlJoinHint JoinHint(SqlJoinMethod method, SqlJoinedTable joinedTable)
    {
      ArgumentValidator.EnsureArgumentNotNull(joinedTable, "joinedTable");
      ArgumentValidator.EnsureArgumentIs<SqlJoinExpression>(joinedTable.JoinExpression, "join.JoinExpression");
      return new SqlJoinHint(method, joinedTable.JoinExpression as SqlJoinExpression);
    }

    public static SqlJoinHint JoinHint(SqlJoinMethod method, SqlTable sqlTable, params SqlTable[] sqlTables)
    {
      ArgumentValidator.EnsureArgumentNotNull(sqlTable, "sqlTable");
      SqlTable[] allSqlTables;
      if (sqlTables!=null && sqlTables.Length>0) {
        allSqlTables = new SqlTable[sqlTables.Length+1];
        sqlTables.CopyTo(allSqlTables, 1);
        allSqlTables[0] = sqlTable;
      }
      else
        allSqlTables = new SqlTable[] {sqlTable};
      return new SqlJoinHint(method, allSqlTables);
    }

    public static SqlJoinHint JoinHint(SqlJoinMethod method)
    {
      return new SqlJoinHint(method);
    }

    public static SqlTableLockHint TableLockHint(
      SqlTable sqlTable, SqlTableIsolationLevel isolationLevel, SqlTableLockType lockType)
    {
      ArgumentValidator.EnsureArgumentNotNull(sqlTable, "sqlTable");
      return new SqlTableLockHint(sqlTable, isolationLevel, lockType);
    }

    public static SqlTableLockHint TableLockHint(SqlTable sqlTable, SqlTableIsolationLevel isolationLevel)
    {
      return TableLockHint(sqlTable, isolationLevel, SqlTableLockType.Default);
    }

    public static SqlTableLockHint TableLockHint(SqlTable sqlTable, SqlTableLockType lockType)
    {
      return TableLockHint(sqlTable, SqlTableIsolationLevel.Default, lockType);
    }

    public static SqlTableScanHint TableScanHint(SqlTable sqlTable, SqlTableScanMethod scanMethod)
    {
      ArgumentValidator.EnsureArgumentNotNull(sqlTable, "sqlTable");
      return new SqlTableScanHint(sqlTable, scanMethod);
    }

    public static SqlTableScanHint TableScanHint(
      SqlTable sqlTable, SqlTableScanMethod scanMethod, Index index, params Index[] indexes)
    {
      ArgumentValidator.EnsureArgumentNotNull(sqlTable, "sqlTable");
      ArgumentValidator.EnsureArgumentNotNull(index, "index");
      Index[] allIndexes;
      if (indexes!=null && indexes.Length>0) {
        allIndexes = new Index[indexes.Length+1];
        indexes.CopyTo(allIndexes, 1);
        allIndexes[0] = index;
      }
      else
        allIndexes = new Index[] {index};
      return new SqlTableScanHint(sqlTable, scanMethod, allIndexes);
    }

    public static SqlTableScanHint TableScanHint(
      SqlTable sqlTable, SqlTableScanMethod scanMethod, string indexName, params string[] indexNames)
    {
      ArgumentValidator.EnsureArgumentNotNull(sqlTable, "sqlTable");
      ArgumentValidator.EnsureArgumentNotNull(indexName, "indexName");
      string[] allIndexes;
      if (indexNames!=null && indexNames.Length>0) {
        allIndexes = new string[indexNames.Length+1];
        indexNames.CopyTo(allIndexes, 1);
        allIndexes[0] = indexName;
      }
      else
        allIndexes = new string[] {indexName};
      return new SqlTableScanHint(sqlTable, scanMethod, allIndexes);
    }

    public static SqlForceJoinOrderHint ForceJoinOrderHint(SqlTable sqlTable, params SqlTable[] sqlTables)
    {
      ArgumentValidator.EnsureArgumentNotNull(sqlTable, "sqlTable");
      SqlTable[] allSqlTables;
      if (sqlTables!=null && sqlTables.Length>0) {
        allSqlTables = new SqlTable[sqlTables.Length+1];
        sqlTables.CopyTo(allSqlTables, 1);
        allSqlTables[0] = sqlTable;
      }
      else
        allSqlTables = new SqlTable[] {sqlTable};
      return new SqlForceJoinOrderHint(allSqlTables);
    }

    public static SqlForceJoinOrderHint ForceJoinOrderHint()
    {
      return new SqlForceJoinOrderHint();
    }
    
    public static SqlFastFirstRowsHint FastFirstRowsHint(int amount)
    {
      if (amount<=0)
        throw new ArgumentOutOfRangeException("amount", Strings.ExRowAmountShouldBePositiveNumber);
      return new SqlFastFirstRowsHint(amount);
    }

    public static SqlUsePlanHint UsePlanHint(string plan)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(plan, "plan");
      return new SqlUsePlanHint(plan);
    }

    public static SqlNativeHint NativeHint(string hintText)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(hintText, "hintText");
      return new SqlNativeHint(hintText);
    }

    #endregion
  }
}