// Copyright (C) 2009-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.08.24

using System;
using System.Text;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Various extension methods related to this namespace.
  /// </summary>
  public static class Extensions
  {
    /// <summary>
    /// Checks whether <paramref name="available"/> contains all flags of given <paramref name="required"/>.
    /// </summary>
    /// <param name="available">All flags.</param>
    /// <param name="required">Flags to check existance.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="available"/> contains all flags of <paramref name="required"/>,
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool Supports(this SqlLockType available, SqlLockType required)
    {
      return (available & required)==required;
    }

    /// <summary>
    /// Checks whether <paramref name="available"/> contains any flag of given <paramref name="required"/>.
    /// </summary>
    /// <param name="available">All flags.</param>
    /// <param name="required">Flags to check existance.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="available"/> contains any flag of <paramref name="required"/>,
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool SupportsAny(this SqlLockType available, SqlLockType required)
    {
      return (available | required) == required;
    }

    public static string ToString(this SqlLockType lockType, bool humanReadable)
    {
      if (!humanReadable)
        return lockType.ToString();
      if (lockType==SqlLockType.Empty)
        return "No Lock";
      var result = new ValueStringBuilder(stackalloc char[128]);
      if (lockType.Supports(SqlLockType.Shared))
        result.Append("Shared");
      if (lockType.Supports(SqlLockType.Update))
        result.Append("Update");
      if (lockType.Supports(SqlLockType.Exclusive))
        result.Append("Exclusive");
      if (lockType.Supports(SqlLockType.SkipLocked))
        result.Append("|Skip locked rows");
      if (lockType.Supports(SqlLockType.ThrowIfLocked))
        result.Append("|Throw exception if locked");
      return result.ToString();
    }

    // See the description in the Parts file. Be careful.
    internal static SqlDateTimeOffsetPart ToDtoPartFast(this SqlDatePart datePart) => (SqlDateTimeOffsetPart) (int) datePart;
    internal static SqlDateTimeOffsetPart ToDtoPartFast(this SqlTimePart datePart) => (SqlDateTimeOffsetPart) (int) datePart;
    internal static SqlDateTimeOffsetPart ToDtoPartFast(this SqlDateTimePart datePart) => (SqlDateTimeOffsetPart) (int) datePart;
    internal static SqlDateTimeOffsetPart ToDtoPartFast(this SqlIntervalPart datePart) => (SqlDateTimeOffsetPart) (int) datePart;

    internal static SqlDatePart ToDatePartFast(this SqlDateTimeOffsetPart dtoPart) => (SqlDatePart)(int) dtoPart;
    internal static SqlTimePart ToTimePartFast(this SqlDateTimeOffsetPart dtoPart) => (SqlTimePart)(int) dtoPart;
    internal static SqlDateTimePart ToDateTimePartFast(this SqlDateTimeOffsetPart dtoPart) => (SqlDateTimePart) (int) dtoPart;
    internal static SqlIntervalPart ToIntervalPartFast(this SqlDateTimeOffsetPart dtoPart) => (SqlIntervalPart)(int) dtoPart;
  }
}