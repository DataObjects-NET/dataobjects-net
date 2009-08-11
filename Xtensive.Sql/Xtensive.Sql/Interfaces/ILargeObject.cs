// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.11

using System;
using System.Data.Common;

namespace Xtensive.Sql
{
  /// <summary>
  /// A server-independed wrapper for using native large objects (LOBs) as query parameters.
  /// </summary>
  public interface ILargeObject : IDisposable
  {
    /// <summary>
    /// Gets a value indicating whether this instance is null.
    /// </summary>
    /// <value>
    ///	<see langword="true"/> if this instance is null; otherwise, <see langword="false"/>.
    /// </value>
    bool IsNull { get; }

    /// <summary>
    /// Gets a value indicating whether this instance is empty.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this instance is empty; otherwise, <see langword="false"/>.
    /// </value>
    bool IsEmpty { get; }

    /// <summary>
    /// Nullifies this instance.
    /// </summary>
    void Nullify();

    /// <summary>
    /// Erases this instance.
    /// </summary>
    void Erase();
    
    /// <summary>
    /// Sets the parameter value.
    /// </summary>
    /// <param name="parameter">The parameter to set value.</param>
    void SetParameterValue(DbParameter parameter);
  }
}