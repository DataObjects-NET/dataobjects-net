// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.12.03

using System;
using Xtensive.Core.Reflection;
using Xtensive.Sql.Info;
using Xtensive.Sql.Resources;

namespace Xtensive.Sql.ValueTypeMapping
{
  /// <summary>
  /// A validator for primitive types.
  /// </summary>
  public static class DataRangeValidator
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
        throw new InvalidOperationException(string.Format(
          Strings.ExCurrentStorageDoesNotSupporXValuesLessThanYSuppliedValueIsZ,
          typeof (T).GetShortName(), allowedRange.MinValue, value));
      if (allowedRange.MaxValue.CompareTo(value) < 0)
        throw new InvalidOperationException(string.Format(
          Strings.ExCurrentStorageDoesNotSupportXValuesGreatherThanYSuppliedValueIsZ,
          typeof (T).GetShortName(), allowedRange.MaxValue, value));
    }

    /// <summary>
    /// Corrects the specified value to fall into the <paramref name="allowedRange"/>.
    /// </summary>
    /// <typeparam name="T">Type of the value to validate</typeparam>
    /// <param name="value">The value.</param>
    /// <param name="allowedRange">The allowed range.</param>
    /// <returns></returns>
    public static T Correct<T>(T value, ValueRange<T> allowedRange)
      where T : struct, IComparable<T>
    {
      if (allowedRange.MinValue.CompareTo(value) > 0)
        return allowedRange.MinValue;
      if (allowedRange.MaxValue.CompareTo(value) < 0)
        return allowedRange.MaxValue;
      return value;
    }
  }
}