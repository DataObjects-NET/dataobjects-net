// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.03

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Xtensive.Reflection;
using Xtensive.Sql.Resources;

namespace Xtensive.Sql
{
  /// <summary>
  /// A collection of <see cref="TypeMapping"/> objects.
  /// </summary>
  public sealed class TypeMappingCollection : IEnumerable<TypeMapping>
  {
    public TypeMapping Boolean { get; private set; }
    public TypeMapping Char { get; private set; }
    public TypeMapping String { get; private set; }
    public TypeMapping Byte { get; private set; }
    public TypeMapping SByte { get; private set; }
    public TypeMapping Short { get; private set; }
    public TypeMapping UShort { get; private set; }
    public TypeMapping Int { get; private set; }
    public TypeMapping UInt { get; private set; }
    public TypeMapping Long { get; private set; }
    public TypeMapping ULong { get; private set; }
    public TypeMapping Float { get; private set; }
    public TypeMapping Double { get; private set; }
    public TypeMapping Decimal { get; private set; }
    public TypeMapping DateTime { get; private set; }
    public TypeMapping TimeSpan { get; private set; }
    public TypeMapping Guid { get; private set; }
    public TypeMapping ByteArray { get; private set; }

    public TypeMapping this[Type type] { get { return GetMapping(type); } }
    
    public TypeMapping TryGetMapping(Type type)
    {
      switch (Type.GetTypeCode(type)) {
      case TypeCode.Boolean:
        return Boolean;
      case TypeCode.Char:
        return Char;
      case TypeCode.String:
        return String;
      case TypeCode.Byte:
        return Byte;
      case TypeCode.SByte:
        return SByte;
      case TypeCode.Int16:
        return Short;
      case TypeCode.UInt16:
        return UShort;
      case TypeCode.Int32:
        return Int;
      case TypeCode.UInt32:
        return UInt;
      case TypeCode.Int64:
        return Long;
      case TypeCode.UInt64:
        return ULong;
      case TypeCode.Single:
        return Float;
      case TypeCode.Double:
        return Double;
      case TypeCode.Decimal:
        return Decimal;
      case TypeCode.DateTime:
        return DateTime;
      }
      if (type==typeof(TimeSpan))
        return TimeSpan;
      if (type==typeof(Guid))
        return Guid;
      if (type==typeof(byte[]))
        return ByteArray;
      return null;
    }

    public TypeMapping GetMapping(Type type)
    {
      var result = TryGetMapping(type);
      if (result==null)
        throw new NotSupportedException(
          string.Format(Strings.ExTypeXIsNotSupported, type.GetFullName()));
      return result;
    }

    public IEnumerator<TypeMapping> GetEnumerator()
    {
      yield return Boolean;
      yield return Char;
      yield return String;
      yield return Byte;
      yield return SByte;
      yield return Short;
      yield return UShort;
      yield return Int;
      yield return UInt;
      yield return Long;
      yield return ULong;
      yield return Float;
      yield return Double;
      yield return Decimal;
      yield return DateTime;
      yield return TimeSpan;
      yield return Guid;
      yield return ByteArray;

    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
    
    private static TypeMapping BuildMapping(TypeMapper h, Type type,
      Func<DbDataReader, int, object> valueReader,
      Action<DbParameter, object> parameterValueSetter,
      Func<int?, int?, int?, SqlValueType> sqlTypeBuilder)
    {
      return new TypeMapping(type,
        valueReader, parameterValueSetter, sqlTypeBuilder,
        h.IsParameterCastRequired(type),
        h.IsLiteralCastRequired(type));
    }
    
    // Constructors

    public TypeMappingCollection(TypeMapper h)
    {
      Boolean = BuildMapping(h, typeof(bool), h.ReadBoolean, h.SetBooleanParameterValue, h.BuildBooleanSqlType);
      Char = BuildMapping(h, typeof(char), h.ReadChar, h.SetCharParameterValue, h.BuildCharSqlType);
      String = BuildMapping(h, typeof(string), h.ReadString, h.SetStringParameterValue, h.BuildStringSqlType);
      Byte = BuildMapping(h, typeof(byte), h.ReadByte, h.SetByteParameterValue, h.BuildByteSqlType);
      SByte = BuildMapping(h, typeof(sbyte), h.ReadSByte, h.SetSByteParameterValue, h.BuildSByteSqlType);
      Short = BuildMapping(h, typeof(short), h.ReadShort, h.SetShortParameterValue, h.BuildShortSqlType);
      UShort = BuildMapping(h, typeof(ushort), h.ReadUShort, h.SetUShortParameterValue, h.BuildUShortSqlType);
      Int = BuildMapping(h, typeof(int), h.ReadInt, h.SetIntParameterValue, h.BuildIntSqlType);
      UInt = BuildMapping(h, typeof(uint), h.ReadUInt, h.SetUIntParameterValue, h.BuildUIntSqlType);
      Long = BuildMapping(h, typeof(long), h.ReadLong, h.SetLongParameterValue, h.BuildLongSqlType);
      ULong = BuildMapping(h, typeof(ulong), h.ReadULong, h.SetULongParameterValue, h.BuildULongSqlType);
      Float = BuildMapping(h, typeof(float), h.ReadFloat, h.SetFloatParameterValue, h.BuildFloatSqlType);
      Double = BuildMapping(h, typeof(double), h.ReadDouble, h.SetDoubleParameterValue, h.BuildDoubleSqlType);
      Decimal = BuildMapping(h, typeof(decimal), h.ReadDecimal, h.SetDecimalParameterValue, h.BuildDecimalSqlType);
      DateTime = BuildMapping(h, typeof(DateTime), h.ReadDateTime, h.SetDateTimeParameterValue, h.BuildDateTimeSqlType);
      TimeSpan = BuildMapping(h, typeof(TimeSpan), h.ReadTimeSpan, h.SetTimeSpanParameterValue, h.BuildTimeSpanSqlType);
      Guid = BuildMapping(h, typeof(Guid), h.ReadGuid, h.SetGuidParameterValue, h.BuildGuidSqlType);
      ByteArray = BuildMapping(h, typeof(byte[]), h.ReadByteArray, h.SetByteArrayParameterValue, h.BuildByteArraySqlType);
    }
  }
}