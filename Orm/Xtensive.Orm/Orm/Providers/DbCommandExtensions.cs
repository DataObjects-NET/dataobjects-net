// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.10.14

using System;
using System.Data.Common;
using System.Text;

namespace Xtensive.Orm.Providers
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
      var commandText = command.CommandText.Trim();
      if (command.Parameters.Count==0)
        return commandText;
      var result = new StringBuilder(commandText);
      result.Append(" [");
      foreach (DbParameter parameter in command.Parameters) {
        var value = parameter.Value;
        if (value is DateTime)
          value = ((DateTime) value).ToString("yyyy-MM-dd HH:mm:ss.fffffff");
        else if (value is DateTimeOffset)
          value = ((DateTimeOffset) value).ToString("yyyy-MM-dd HH:mm:ss.fffffffK");
        result.AppendFormat("{0}='{1}';", parameter.ParameterName, value);
      }
      result.Remove(result.Length - 1, 1);
      result.Append("]");
      return result.ToString();
    }
  }
}
