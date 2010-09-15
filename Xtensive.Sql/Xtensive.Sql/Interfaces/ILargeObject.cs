// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.11

using System;
using System.Data.Common;

namespace Xtensive.Sql
{
  /// <summary>
  /// A contract for server-independent native large objects (LOBs) query parameter.
  /// </summary>
  public interface ILargeObject : IDisposable
  {
    /// <summary>
    /// Gets a value indicating whether this instance is null.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if this instance is null; otherwise, <see langword="false"/>.
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
    /// Binds this LOB to the specified parameter.
    /// </summary>
    /// <param name="parameter">The parameter to bind to.</param>
    void BindTo(DbParameter parameter);
  }
}