// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.10.14

using System.Data.Common;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// <see cref="DbCommand"/> related extension methods.
  /// </summary>
  public static class DbCommandExtensions
  {
    /// <summary>
    /// Converts the specified <see cref="DbCommand"/> to human readable string.
    /// </summary>
    /// <param name="command">The command to beautify.</param>
    /// <returns>Human readable representation of the specified command.</returns>
    public static string ToHumanReadableString(this DbCommand command)
    {
      // Does almost nothing for now
      return command.CommandText.Trim();
    }
  }
}