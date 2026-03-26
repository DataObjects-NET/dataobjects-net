// Copyright (C) 2009-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.08.24

using System;
using System.Text;

namespace Xtensive.Sql.Dml
{
  /// <summary>
  /// Various extension methods related to this namespace.
  /// </summary>
  public static class Extensions
  {
    /// <summary>
    /// Determines whether the specified expression is a null reference.
    /// Use this method instead of comparison with null,
    /// because equality operator is overloaded for <see cref="SqlExpression"/>
    /// to yield equality comparison expression.
    /// </summary>
    /// <param name="expression">The expression to check.</param>
    /// <returns>
    /// <see langword="true"/> if argument is a null reference; otherwise, <see langword="false"/>.
    /// </returns>
    [Obsolete(@"Use 'is null' operator")]
    public static bool IsNullReference(this SqlExpression expression)
    {
      return ReferenceEquals(expression, null);
    }

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
      var result = new StringBuilder();
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
#if NET6_0_OR_GREATER
    internal static SqlDateTimeOffsetPart ToDtoPartFast(this SqlDatePart datePart) => (SqlDateTimeOffsetPart) (int) datePart;
    internal static SqlDateTimeOffsetPart ToDtoPartFast(this SqlTimePart datePart) => (SqlDateTimeOffsetPart) (int) datePart;
#endif
    internal static SqlDateTimeOffsetPart ToDtoPartFast(this SqlDateTimePart datePart) => (SqlDateTimeOffsetPart) (int) datePart;
    internal static SqlDateTimeOffsetPart ToDtoPartFast(this SqlIntervalPart datePart) => (SqlDateTimeOffsetPart) (int) datePart;

#if NET6_0_OR_GREATER
    internal static SqlDatePart ToDatePartFast(this SqlDateTimeOffsetPart dtoPart) => (SqlDatePart)(int) dtoPart;
    internal static SqlTimePart ToTimePartFast(this SqlDateTimeOffsetPart dtoPart) => (SqlTimePart)(int) dtoPart;
#endif
    internal static SqlDateTimePart ToDateTimePartFast(this SqlDateTimeOffsetPart dtoPart) => (SqlDateTimePart) (int) dtoPart;
    internal static SqlIntervalPart ToIntervalPartFast(this SqlDateTimeOffsetPart dtoPart) => (SqlIntervalPart)(int) dtoPart;
  }
}