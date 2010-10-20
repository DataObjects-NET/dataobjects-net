// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.06

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Xtensive.Core;
using Xtensive.Storage.Model.Resources;

namespace Xtensive.Storage.Model
{
  // TODO: Use Xtensive.Comparers
  /// <summary>
  /// Provides comparers for primitive types and strings.
  /// </summary>
  public static class ComparerProvider
  {
    private static IDictionary<CultureInfo, StringComparer> stringComparers =
      new Dictionary<CultureInfo, StringComparer>();

    /// <summary>
    /// Gets the comparer according to type and culture.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="cultureInfo">The culture info.</param>
    /// <returns></returns>
    public static IComparer GetComparer(Type type, CultureInfo cultureInfo)
    {
      ArgumentValidator.EnsureArgumentNotNull(type, "type");

      switch (Type.GetTypeCode(type)) {
        case TypeCode.Boolean:
          return Comparer<bool>.Default;
        case TypeCode.Byte:
          return Comparer<Byte>.Default;
        case TypeCode.Char:
          return Comparer<char>.Default;
        case TypeCode.DateTime:
          return Comparer<DateTime>.Default;
        case TypeCode.Decimal:
          return Comparer<decimal>.Default;
        case TypeCode.Double:
          return Comparer<double>.Default;
        case TypeCode.Int16:
          return Comparer<short>.Default;
        case TypeCode.Int32:
          return Comparer<int>.Default;
        case TypeCode.Int64:
          return Comparer<long>.Default;
        case TypeCode.SByte:
          return Comparer<sbyte>.Default;
        case TypeCode.Single:
          return Comparer<float>.Default;
        case TypeCode.String:
          return GetStringComparer(cultureInfo);
        case TypeCode.UInt16:
          return Comparer<ushort>.Default;
        case TypeCode.UInt32:
          return Comparer<uint>.Default;
        case TypeCode.UInt64:
          return Comparer<ulong>.Default;
      }
      if (type == typeof (Guid))
        return Comparer<Guid>.Default;
      throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Strings.ExComparerForTypeIsNotAvailable, type));
    }

    private static IComparer GetStringComparer(CultureInfo cultureInfo)
    {
      ArgumentValidator.EnsureArgumentNotNull(cultureInfo, "cultureInfo");

      BuildStringComparer(cultureInfo);
      return stringComparers[cultureInfo];
    }

    private static void BuildStringComparer(CultureInfo cultureInfo)
    {
      if (stringComparers.ContainsKey(cultureInfo))
        return;
      stringComparers[cultureInfo] = StringComparer.Create(cultureInfo, false);
    }
  }
}