// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;
using Xtensive.Core.Helpers;

namespace Xtensive.Sql.Common
{
  /// <summary>
  /// Describes a fraction data type.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class FractionalDataTypeInfo<T> : RangeDataTypeInfo<T> where T : struct, IComparable, IComparable<T>
  {
    private ValueRange<short> scale;
    private ValueRange<short> precision;

    /// <summary>
    /// Gets or sets the scale range.
    /// </summary>
    /// <value>The scale.</value>
    public ValueRange<short> Scale
    {
      get { return scale; }
      set
      {
        this.EnsureNotLocked();
        scale = value;
      }
    }

    /// <summary>
    /// Gets o sets the precision range.
    /// </summary>
    /// <value>The precision.</value>
    public ValueRange<short> Precision
    {
      get { return precision; }
      set
      {
        this.EnsureNotLocked();
        precision = value;
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FractionalDataTypeInfo&lt;T&gt;"/> class.
    /// </summary>
    /// <param name="sqlType">SQL datatype.</param>
    /// <param name="nativeTypes">The native types.</param>
    public FractionalDataTypeInfo(SqlDataType sqlType, string[] nativeTypes) : base(sqlType, nativeTypes)
    {
    }
  }
}
