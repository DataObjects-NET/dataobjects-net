// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;
using Xtensive.Core.Helpers;

namespace Xtensive.Sql.Common
{
  /// <summary>
  /// Describes a data type.
  /// </summary>
  public class DataTypeInfo : LockableBase
  {
    private DataTypeFeatures features = DataTypeFeatures.None;
    private SqlDataType sqlType;
    private Type type;
    private string[] nativeTypes;
    public static DataTypeInfo Empty = new DataTypeInfo();

    /// <summary>
    /// Gets or sets the features of this instance.
    /// </summary>
    /// <value>The features.</value>
    public DataTypeFeatures Features
    {
      get { return features; }
      set
      {
        this.EnsureNotLocked();
        features = value;
      }
    }

    /// <summary>
    /// Gets the native database type names.
    /// </summary>
    /// <value>The native types.</value>
    public string[] GetNativeTypes()
    {
      string[] clone = new string[nativeTypes.Length];
      nativeTypes.CopyTo(clone, 0);
      return clone;
    }

    /// <summary>
    /// Gets or sets the Sql type
    /// </summary>
    /// <value>The Sql type.</value>
    public SqlDataType SqlType
    {
      get { return sqlType; }
      set
      {
        this.EnsureNotLocked();
        sqlType = value;
      }
    }

    /// <summary>
    /// Gets or sets the .NET Framework type.
    /// </summary>
    /// <value>The .NET Framework type.</value>
    public Type Type
    {
      get { return type; }
      set
      {
        this.EnsureNotLocked();
        type = value;
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataTypeInfo"/> class.
    /// </summary>
    /// <param name="sqlType">SQL data type.</param>
    /// <param name="type">The type.</param>
    /// <param name="nativeTypes">The native types.</param>
    protected DataTypeInfo(SqlDataType sqlType, Type type, string[] nativeTypes)
    {
      this.sqlType = sqlType;
      this.type = type;
      this.nativeTypes = nativeTypes;
    }

    private DataTypeInfo()
    {
    }
  }
}
