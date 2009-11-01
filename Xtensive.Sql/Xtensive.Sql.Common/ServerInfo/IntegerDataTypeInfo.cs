// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Common
{
  /// <summary>
  /// Describes an integer data type.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class IntegerDataTypeInfo<T> : RangeDataTypeInfo<T> where T : struct, IComparable, IComparable<T>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="IntegerDataTypeInfo&lt;T&gt;"/> class.
    /// </summary>
    /// <param name="sqlType">SQL data type.</param>
    /// <param name="nativeTypes">The native types.</param>
    public IntegerDataTypeInfo(SqlDataType sqlType, string[] nativeTypes) : base(sqlType, nativeTypes)
    {
    }
  }
}