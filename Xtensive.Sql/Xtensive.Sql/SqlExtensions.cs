// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.30

using System;
using System.Data.Common;
using Xtensive.Core;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql
{
  /// <summary>
  /// Various extension methods related to this namespace.
  /// </summary>
  public static class SqlExtensions
  {
    private const char SchemaSeparator = '/';

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

    /// <summary>
    /// Extracts the database component from the specified <see cref="UrlInfo"/>.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <returns>Database name.</returns>
    public static string GetDatabase(this UrlInfo url)
    {
      var resource = url.Resource;
      int position = resource.IndexOf(SchemaSeparator);
      if (position < 0)
        return url.Resource;
      return resource.Substring(0, position);
    }

    /// <summary>
    /// Extracts the schema component from the specified <see cref="UrlInfo"/>.
    /// If schema is not specified returns <paramref name="defaultValue"/>.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <param name="defaultValue">The default schema name.</param>
    /// <returns>Schema name.</returns>
    public static string GetSchema(this UrlInfo url, string defaultValue)
    {
      var resource = url.Resource;
      int position = resource.IndexOf(SchemaSeparator);
      if (position < 0)
        return defaultValue;
      return resource.Substring(position);
    }

    /// <summary>
    /// Converts the specified <see cref="SqlType"/> to corresponding .NET type.
    /// </summary>
    /// <param name="type">The type to convert.</param>
    /// <returns>Converter type.</returns>
    public static Type ToClrType(this SqlType type)
    {
      switch (type) {
      case SqlType.Boolean:
        return typeof (bool);
      case SqlType.Int8:
        return typeof (sbyte);
      case SqlType.UInt8:
        return typeof (byte);
      case SqlType.Int16:
        return typeof (short);
      case SqlType.UInt16:
        return typeof (ushort);
      case SqlType.Int32:
        return typeof (int);
      case SqlType.UInt32:
        return typeof (uint);
      case SqlType.Int64:
        return typeof (long);
      case SqlType.UInt64:
        return typeof (ulong);
      case SqlType.Decimal:
        return typeof (decimal);
      case SqlType.Float:
        return typeof (float);
      case SqlType.Double:
        return typeof (double);
      case SqlType.DateTime:
        return typeof (DateTime);
      case SqlType.Interval:
        return typeof (TimeSpan);
      case SqlType.Char:
      case SqlType.VarChar:
      case SqlType.VarCharMax:
        return typeof (string);
      case SqlType.Binary:
      case SqlType.VarBinary:
      case SqlType.VarBinaryMax:
        return typeof (byte[]);
      case SqlType.Guid:
        return typeof (Guid);
      default:
        throw new ArgumentOutOfRangeException("type");
      }
    }

    public static int? GetNullableInt32(this DbDataReader reader, string column)
    {
      return Convert.IsDBNull(reader[column]) ? null : (int?) Convert.ToInt32(reader[column]);
    }

    public static int? GetNullableInt32(this DbDataReader reader, int column)
    {
      if (reader.IsDBNull(column))
        return null;
      return reader.GetInt32(column);
    }
  }
}