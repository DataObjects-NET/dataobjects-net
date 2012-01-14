// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.24

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
    public static bool IsNullReference(this SqlExpression expression)
    {
      return ReferenceEquals(expression, null);
    }

    public static bool Supports(this SqlLockType available, SqlLockType required)
    {
      return (available & required)==required;
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
  }
}