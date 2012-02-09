// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.12.03

using System;
using Xtensive.Reflection;
using Xtensive.Sql.Info;

namespace Xtensive.Sql
{
  /// <summary>
  /// A range validator for primitive types.
  /// </summary>
  public static class ValueRangeValidator
  {
    /// <summary>
    /// Ensures that the specified value is in <paramref name="allowedRange"/>.
    /// </summary>
    /// <typeparam name="T">Type of the value to validate.</typeparam>
    /// <param name="value">The value.</param>
    /// <param name="allowedRange">The allowed range.</param>
    public static void Validate<T>(T value, ValueRange<T> allowedRange)
      where T : struct, IComparable<T>
    {
      if (allowedRange.MinValue.CompareTo(value) > 0)
        throw OutOfRange(Strings.ExThisStorageDoesNotSupportXValuesLessThanYSuppliedValueIsZ,
          typeof (T).GetShortName(), allowedRange.MinValue, value);
      if (allowedRange.MaxValue.CompareTo(value) < 0)
        throw OutOfRange(Strings.ExThisStorageDoesNotSupportXValuesGreatherThanYSuppliedValueIsZ,
          typeof (T).GetShortName(), allowedRange.MaxValue, value);
    }

    /// <summary>
    /// Corrects the specified value to fall into the <paramref name="allowedRange"/>.
    /// </summary>
    /// <typeparam name="T">Type of the value to validate</typeparam>
    /// <param name="value">The value.</param>
    /// <param name="allowedRange">The allowed range.</param>
    /// <returns>Corrected value.</returns>
    public static T Correct<T>(T value, ValueRange<T> allowedRange)
      where T : struct, IComparable<T>
    {
      if (allowedRange.MinValue.CompareTo(value) > 0)
        return allowedRange.MinValue;
      if (allowedRange.MaxValue.CompareTo(value) < 0)
        return allowedRange.MaxValue;
      return value;
    }
    
    private static InvalidOperationException OutOfRange<T>(string format,
      string typeName, T boundaryValue, T suppliedValue)
    {
      return new InvalidOperationException(
        string.Format(
          Strings.ExThisStorageDoesNotSupportX,
          string.Format(format, typeName, boundaryValue, suppliedValue)));
    }
  }
}