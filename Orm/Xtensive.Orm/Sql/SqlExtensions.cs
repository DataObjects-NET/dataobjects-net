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
  }
}