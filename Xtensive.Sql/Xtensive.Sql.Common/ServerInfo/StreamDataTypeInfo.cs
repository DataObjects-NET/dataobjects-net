// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;
using Xtensive.Core.Helpers;

namespace Xtensive.Sql.Common
{
  /// <summary>
  /// Describes a stream data type.
  /// </summary>
  public class StreamDataTypeInfo : DataTypeInfo
  {
    private ValueRange<int> length;

    /// <summary>
    /// Gets or sets the length.
    /// </summary>
    /// <value>The length.</value>
    public ValueRange<int> Length
    {
      get { return length; }
      set
      {
        this.EnsureNotLocked();
        length = value;
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamDataTypeInfo"/> class.
    /// </summary>
    /// <param name="sqlType">SQL data type.</param>
    /// <param name="type">The type.</param>
    /// <param name="nativeTypes">The native types.</param>
    public StreamDataTypeInfo(SqlDataType sqlType, Type type, string[] nativeTypes) : base(sqlType, type, nativeTypes)
    {
    }
  }
}
