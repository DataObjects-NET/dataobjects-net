// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.05.28

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Reflection;
using Xtensive.Orm.Upgrade.Model;

namespace Xtensive.Orm.Upgrade
{
  /// <summary>
  /// Verifies whether the conversion between types is allowed or not.
  /// </summary>
  public static class TypeConversionVerifier
  {
    private static readonly Dictionary<Type, List<Type>> supportedConversions;

    /// <summary>
    /// Verifies whether the source type can be converted to the target type. 
    /// Loss of data is allowed.
    /// </summary>
    /// <param name="from">The source type.</param>
    /// <param name="to">The target type.</param>
    /// <returns>
    /// <see langword="true"/> if the source type can be converted to the 
    /// target type; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool CanConvert(StorageTypeInfo from, StorageTypeInfo to)
    {
      ArgumentValidator.EnsureArgumentNotNull(from, "from");
      ArgumentValidator.EnsureArgumentNotNull(to, "to");

      // Truncation and precision loss is ALLOWED by this method

      if (from.IsTypeUndefined || to.IsTypeUndefined)
        return false;

      var fromType = from.Type.StripNullable();
      var toType = to.Type.StripNullable();

      if (fromType==toType) // Comparing just types
        return true;

      // Types are different
      if (toType==typeof(string))
        // Checking target string length
        return !to.Length.HasValue || CanConvertToString(from, to.Length.Value);

      return supportedConversions.ContainsKey(fromType) && supportedConversions[fromType].Contains(toType);
    }

    /// <summary>
    /// Verifies whether the source type can be converted to the target 
    /// type without loss of data.
    /// </summary>
    /// <param name="from">The source type.</param>
    /// <param name="to">The target type.</param>
    /// <returns>
    /// <see langword="true"/> if the source type can be converted to the 
    /// target type without loss of data; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool CanConvertSafely(StorageTypeInfo from, StorageTypeInfo to)
    {
      ArgumentValidator.EnsureArgumentNotNull(from, "from");
      ArgumentValidator.EnsureArgumentNotNull(to, "to");

      // Truncation and precision loss is NOT ALLOWED by this method

      if (!CanConvert(from, to))
        return false;
      if (from.IsNullable && !to.IsNullable)
        return false; // Can't convert NULL

      var toType = to.Type.StripNullable();
      var fromType = from.Type.StripNullable();

      if (toType==typeof(decimal) && fromType==typeof(decimal))
        return CheckScaleAndPrecision(from, to);
      else if (toType==typeof(string) && fromType==typeof(string))
        return CheckLength(from, to);
      else if (toType==typeof (byte[]) && fromType==typeof (byte[]))
        return CheckLength(from, to);
      return true;
    }

    private static bool CanConvertToString(StorageTypeInfo from, int length)
    {
      switch (Type.GetTypeCode(from.Type.StripNullable())) {
      case TypeCode.Char:
      case TypeCode.String:
        return true;
      case TypeCode.Decimal:
        return length >= from.Precision + 2;
      case TypeCode.Byte:
        return length >= 3;
      case TypeCode.SByte:
        return length >= 4;
      case TypeCode.Int16:
        return length >= 6;
      case TypeCode.Int32:
        return length >= 11;
      case TypeCode.Int64:
        return length >= 20;
      case TypeCode.UInt16:
        return length >= 5;
      case TypeCode.UInt32:
        return length >= 10;
      case TypeCode.UInt64:
        return length >= 20;
      default:
        return false;
      }
    }

    private static bool CheckLength(StorageTypeInfo from, StorageTypeInfo to)
    {
      if (!to.Length.HasValue)
        return true; // Conversion to Var*(max) is always possible, or both types have no length
      if (!from.Length.HasValue)
        return false; // Conversion from Var*(max) is possible only to Var*(max)
      // Otherwise it's possible to convert only when new type has higher length
      return from.Length.Value <= to.Length.Value; 
    }

    private static bool CheckScaleAndPrecision(StorageTypeInfo from, StorageTypeInfo to)
    {
      return LessOrEqual(from.Scale, to.Scale) && LessOrEqual(from.Precision,to.Precision);
    }

    private static bool LessOrEqual(int? a, int? b)
    {
      // Can't use 'a <= b' because it evalutes to false when both are null
      return a==b || a < b;
    }

    // Constructors

    static TypeConversionVerifier()
    {
      supportedConversions = new Dictionary<Type, List<Type>>();
      AddConverter<Boolean>(
        typeof (Int16), typeof (UInt16), typeof (Int32), typeof (UInt32),
        typeof (Int64), typeof (UInt64), typeof (Double), typeof (Single), typeof (Decimal));
      AddConverter<Byte>(
        typeof (Int16), typeof (UInt16), typeof (Char), typeof (Int32), typeof (UInt32),
        typeof (Int64), typeof (UInt64), typeof (Double), typeof (Single),
        typeof (Decimal));
      AddConverter<SByte>(
        typeof (Int16), typeof (UInt16), typeof (Char), typeof (Int32),
        typeof (UInt32), typeof (Int64), typeof (UInt64), typeof (Double), typeof (Single),
        typeof (Decimal));
      AddConverter<Int16>(
        typeof (Int32), typeof (UInt32), typeof (Int64), typeof (UInt64),
        typeof (Double), typeof (Single), typeof (Decimal));
      AddConverter<UInt16>(
        typeof (Char), typeof (Int32), typeof (UInt32), typeof (Int64),
        typeof (UInt64), typeof (Double), typeof (Single), typeof (Decimal));
      AddConverter<Int32>(
        typeof (Int64), typeof (UInt64), typeof (Double), typeof (Single), typeof (Decimal));
      AddConverter<UInt32>(
        typeof (Int64), typeof (UInt64), typeof (Double), typeof (Single), typeof (Decimal));
      AddConverter<Int64>(
        typeof (Int64), typeof (UInt64), typeof (Double), typeof (Single), typeof (Decimal));
      AddConverter<UInt64>(
        typeof (Int64), typeof (UInt64), typeof (Double), typeof (Single), typeof (Decimal));
      AddConverter<Char>(
        typeof (UInt16), typeof (Int32), typeof (UInt32), typeof (Int64),
        typeof (UInt64), typeof (Double), typeof (Single), typeof (Decimal));
      AddConverter<Decimal>(typeof (Double), typeof (Single), typeof(Decimal));
      AddConverter<Single>(typeof (Double));
    }

    private static void AddConverter<T>(params Type[] types)
    {
      ArgumentValidator.EnsureArgumentIsGreaterThan(types.Length, -1, "types.Length");
      supportedConversions.Add(typeof(T), new List<Type>(types));
    }
  }
}