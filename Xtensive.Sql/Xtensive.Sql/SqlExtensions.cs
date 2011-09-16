// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.30

using System;
using Xtensive.Core;
using Xtensive.Core.Helpers;
using Xtensive.Sql.Dml;

namespace Xtensive.Sql
{
  /// <summary>
  /// Various extension methods related to this namespace.
  /// </summary>
  public static class SqlExtensions
  {
    private const char SchemaSeparator = '/';
    private const string SchemaSeparatorString = "/";

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
        return url.Resource.TryCutSuffix(SchemaSeparatorString);
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
      var result = resource.Substring(position + 1).TryCutSuffix(SchemaSeparatorString);
      return string.IsNullOrEmpty(result) ? defaultValue : result;
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
      case SqlType.Geometry:
        return Type.GetType("Microsoft.SqlServer.Types.SqlGeometry, Microsoft.SqlServer.Types, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
      case SqlType.Geography:
        return Type.GetType("Microsoft.SqlServer.Types.SqlGeography, Microsoft.SqlServer.Types, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
      default:
        throw new ArgumentOutOfRangeException("type");
      }
    }
  }
}