// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Core;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Describes a data type.
  /// </summary>
  public sealed class DataTypeInfo
  {
    public static DataTypeInfo Empty = new DataTypeInfo
      {
        Type = SqlType.Unknown,
        Features = DataTypeFeatures.None,
        NativeTypes = Array.Empty<string>()
      };
    
    /// <summary>
    /// Gets the SQL type
    /// </summary>
    public SqlType Type { get; private set; }

    /// <summary>
    /// Gets the features of this instance.
    /// </summary>
    public DataTypeFeatures Features { get; set; }

    /// <summary>
    /// Gets the native database type names.
    /// </summary>
    public IEnumerable<string> NativeTypes { get; private set; }

    /// <summary>
    /// Gets the maximum length of this data type.
    /// </summary>
    public int? MaxLength { get; private set; }

    /// <summary>
    /// Gets the maximum precision of this data type.
    /// </summary>
    public int? MaxPrecision { get; private set; }

    /// <summary>
    /// Gets the range.
    /// </summary>
    public ValueRange ValueRange { get; private set; }

    /// <summary>
    /// Creates a fractional <see cref="DataTypeInfo"/>.
    /// </summary>
    public static DataTypeInfo Fractional(
      SqlType sqlType,
      DataTypeFeatures features,
      ValueRange valueRange,
      int maxPrecision,
      params string[] nativeTypes)
    {
      ArgumentValidator.EnsureArgumentNotNull(valueRange, "valueRange");
      ArgumentValidator.EnsureArgumentIsGreaterThan(maxPrecision, 0, "maxPrecision");
      return new DataTypeInfo
        {
          Type = sqlType,
          Features = features,
          ValueRange = valueRange,
          MaxPrecision = maxPrecision,
          NativeTypes = nativeTypes.ToArraySafely(),
        };
    }

    /// <summary>
    /// Ranges a range <see cref="DataTypeInfo"/>.
    /// </summary>
    public static DataTypeInfo Range(
      SqlType sqlType,
      DataTypeFeatures features,
      ValueRange valueRange,
      params string[] nativeTypes)
    {
      ArgumentValidator.EnsureArgumentNotNull(valueRange, "valueRange");
      return new DataTypeInfo
        {
          Type = sqlType,
          Features = features,
          ValueRange = valueRange,
          NativeTypes = nativeTypes.ToArraySafely(),
        };
    }

    /// <summary>
    /// Creates a stream <see cref="DataTypeInfo"/>.
    /// </summary>
    public static DataTypeInfo Stream(
      SqlType sqlType,
      DataTypeFeatures features,
      int maxLength,
      params string[] nativeTypes)
    {
      ArgumentValidator.EnsureArgumentIsGreaterThan(maxLength, 0, "maxLength");
      return new DataTypeInfo
        {
          Type = sqlType,
          Features = features,
          MaxLength = maxLength,
          NativeTypes = nativeTypes.ToArraySafely(),
        };
    }

    /// <summary>
    /// Creates a regular <see cref="DataTypeInfo"/>
    /// </summary>
    public static DataTypeInfo Regular(
      SqlType sqlType,
      DataTypeFeatures features,
      params string[] nativeTypes)
    {
      return new DataTypeInfo
        {
          Type = sqlType,
          Features = features,
          NativeTypes = nativeTypes.ToArraySafely(),
        };
    }
    
    // Constructors

    private DataTypeInfo()
    {
    }
  }
}
