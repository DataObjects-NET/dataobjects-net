// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;
using Xtensive.Core.Helpers;

namespace Xtensive.Sql.Common
{
  /// <summary>
  /// Describes a range data type.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class RangeDataTypeInfo<T> : DataTypeInfo where T : struct, IComparable, IComparable<T>
  {
    private ValueRange<T> value;

    /// <summary>
    /// Gets or sets the value range for data type.
    /// </summary>
    /// <value>The value range.</value>
    public ValueRange<T> Value
    {
      get { return value; }
      set
      {
        this.EnsureNotLocked();
        this.value = value;
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RangeDataTypeInfo&lt;T&gt;"/> class.
    /// </summary>
    /// <param name="sqlType">SQL datatype.</param>
    /// <param name="nativeTypes">The native types.</param>
    public RangeDataTypeInfo(SqlDataType sqlType, string[] nativeTypes) : base(sqlType, typeof(T), nativeTypes)
    {
    }
  }
}
