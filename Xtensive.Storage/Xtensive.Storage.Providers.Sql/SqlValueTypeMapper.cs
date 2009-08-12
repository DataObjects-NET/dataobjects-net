// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.23

using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using Xtensive.Core.Reflection;
using Xtensive.Sql;
using Xtensive.Sql.Info;
using Xtensive.Sql.ValueTypeMapping;
using Xtensive.Sql.ValueTypeMapping;
using Xtensive.Storage.Providers.Sql.Mappings;
using Xtensive.Storage.Providers.Sql.Resources;
using ColumnInfo = Xtensive.Storage.Model.ColumnInfo;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// A SQL specific handler that builds mapping between native .NET types and server specific data types.
  /// </summary>
  public sealed class SqlValueTypeMapper
  {
    private readonly TypeMappingCollection allMappings;
    
    #region TryGetTypeMapping methods

    /// <summary>
    /// Gets the type mapping.
    /// </summary>
    /// <param name="column">The column.</param>
    /// <returns><see cref="TypeMapping"/> instance for the specified <paramref name="column"/>.
    /// If no mapping exists returns null.</returns>
    public TypeMapping TryGetTypeMapping(ColumnInfo column)
    {
      return allMappings.TryGetMapping(column.ValueType);
    }

    /// <summary>
    /// Gets the type mapping.
    /// </summary>
    /// <param name="type">The column type.</param>
    /// <returns><see cref="TypeMapping"/> instance
    /// for the specified <paramref name="type"/>.
    /// If no mapping exists returns null.</returns>
    public TypeMapping TryGetTypeMapping(Type type)
    {
      return allMappings.TryGetMapping(type);
    }

    #endregion

    #region GetTypeMapping methods

    /// <summary>
    /// Gets the type mapping.
    /// </summary>
    /// <param name="column">The column.</param>
    /// <returns><see cref="TypeMapping"/> instance for the specified <paramref name="column"/>.</returns>
    /// <exception cref="InvalidOperationException">Type of column is not supported.</exception>
    public TypeMapping GetTypeMapping(ColumnInfo column)
    {
      return allMappings.GetMapping(column.ValueType);
    }

    /// <summary>
    /// Gets the type mapping.
    /// </summary>
    /// <param name="type">The column type.</param>
    /// <returns><see cref="TypeMapping"/> instance for the specified <paramref name="type"/>.</returns>
    /// <exception cref="NotSupportedException"><paramref name="type"/> is not supported.</exception>
    public TypeMapping GetTypeMapping(Type type)
    {
      return allMappings.GetMapping(type);
    }

    #endregion

    #region BuildSqlValueType methods

    /// <summary>
    /// Builds the type of the SQL value.
    /// </summary>
    /// <param name="columnInfo">The column info.</param>
    /// <returns></returns>
    public SqlValueType BuildSqlValueType(ColumnInfo columnInfo)
    {
      return allMappings.GetMapping(columnInfo.ValueType).BuildSqlType(columnInfo.Length, null, null);
    }

    /// <summary>
    /// Builds the <see cref="SqlValueType"/> from specified <see cref="Type"/> and length.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="length">The length.</param>
    /// <returns></returns>
    public SqlValueType BuildSqlValueType(Type type, int? length)
    {
      return allMappings.GetMapping(type).BuildSqlType(length, null, null);
    }

    /// <summary>
    /// Builds the <see cref="SqlValueType"/> from specified <see cref="Type"/>, length, scale and precision.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="length">The length.</param>
    /// <param name="precision">The precision.</param>
    /// <param name="scale">The scale.</param>
    /// <returns></returns>
    public SqlValueType BuildSqlValueType(Type type, int? length, int? precision, int? scale)
    {
      return allMappings.GetMapping(type).BuildSqlType(length, precision, scale);
    }

    #endregion

    internal SqlValueTypeMapper(SqlDriver driver)
    {
      allMappings = driver.TypeMappings;
    }
  }
}