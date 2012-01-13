// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.07.07

using System;
using System.Diagnostics;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// Represents a contiguous range of values.
  /// </summary>
  public abstract partial class ValueRange
  {
    /// <summary>
    /// Gets the min value.
    /// </summary>
    public abstract object GetMinValue();

    /// <summary>
    /// Gets the max value.
    /// </summary>
    public abstract object GetMaxValue();
    
    /// <summary>
    /// Gets the default value.
    /// </summary>
    public abstract object GetDefaultValue();

    /// <summary>
    /// Determines whether default value has been specified.
    /// </summary>
    public abstract bool HasDefaultValue();
  }
}