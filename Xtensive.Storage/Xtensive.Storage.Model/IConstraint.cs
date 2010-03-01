// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.06.24

using System;

namespace Xtensive.Storage.Model
{
  /// <summary>
  /// Value constraint.
  /// </summary>
  public interface IConstraint
  {
    /// <summary>
    /// Gets the error message.
    /// </summary>
    string Message { get; }

    /// <summary>
    /// Determines whether the specified type is supported.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>
    /// <see langword="true"/> if the specified type is supported; otherwise, <see langword="false"/>.
    /// </returns>
    bool SupportsValueType(Type type);

    /// <summary>
    /// Determines whether the specified value is value correct.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>
    /// <see langword="true"/> if the specified value is value correct; otherwise, <see langword="false"/>.
    /// </returns>
    bool Check(object value);
  }
}