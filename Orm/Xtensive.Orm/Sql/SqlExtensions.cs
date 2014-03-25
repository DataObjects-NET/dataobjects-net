// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.30

using System;
using Xtensive.Core;
using Xtensive.Orm;
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
      if (type==SqlType.Boolean)
        return typeof (bool);
      if (type==SqlType.Int8)
        return typeof (sbyte);
      if (type==SqlType.UInt8)
        return typeof (byte);
      if (type==SqlType.Int16)
        return typeof (short);
      if (type==SqlType.UInt16)
        return typeof (ushort);
      if (type==SqlType.Int32)
        return typeof (int);
      if (type==SqlType.UInt32)
        return typeof (uint);
      if (type==SqlType.Int64)
        return typeof (long);
      if (type==SqlType.UInt64)
        return typeof (ulong);
      if (type==SqlType.Decimal)
        return typeof (decimal);
      if (type==SqlType.Float)
        return typeof (float);
      if (type==SqlType.Double)
        return typeof (double);
      if (type==SqlType.DateTime)
        return typeof (DateTime);
      if (type==SqlType.DateTimeOffset)
        return typeof (DateTimeOffset);
      if (type==SqlType.Interval)
        return typeof (TimeSpan);
      if (type==SqlType.Char ||
        type==SqlType.VarChar ||
        type==SqlType.VarCharMax)
        return typeof (string);
      if (type==SqlType.Binary ||
        type==SqlType.VarBinary ||
        type==SqlType.VarBinaryMax)
        return typeof (byte[]);
      if (type==SqlType.Guid)
        return typeof (Guid);
      if (type==SqlType.Geometry)
        return Type.GetType("Microsoft.SqlServer.Types.SqlGeometry, Microsoft.SqlServer.Types, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");
      if (type==SqlType.Geography)
        return Type.GetType("Microsoft.SqlServer.Types.SqlGeography, Microsoft.SqlServer.Types, Version=10.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91");

      throw new ArgumentOutOfRangeException("type");
    }
  }
}