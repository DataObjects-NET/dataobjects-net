// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.09.17

using System;
using Xtensive.Tuples;
using Xtensive.Resources;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Tuples
{
  /// <summary>
  /// Extension methods for <see cref="TupleFieldState"/>.
  /// </summary>
  public static class TupleFieldStateExtensions
  {
    /// <summary>
    /// Checks if specified field state has <see cref="TupleFieldState.Available"/> flag.
    /// </summary>
    /// <param name="fieldState"><see cref="TupleFieldState"/> to check.</param>
    /// <returns><see langword="true"/> if field state value has <see cref="TupleFieldState.Available"/> flag; otherwise, <see langword="false"/>.</returns>
    public static bool IsAvailable(this TupleFieldState fieldState)
    {
      return (fieldState & TupleFieldState.Available) != 0;
    }

    /// <summary>
    /// Checks if specified field state has <see cref="TupleFieldState.Null"/> flag.
    /// </summary>
    /// <param name="fieldState"><see cref="TupleFieldState"/> to check.</param>
    /// <returns><see langword="true"/> if field state value has <see cref="TupleFieldState.Null"/> flag; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="InvalidOperationException">Field value is not available.</exception>
    public static bool IsNull(this TupleFieldState fieldState)
    {
      if ((fieldState & TupleFieldState.Available) == 0)
        throw new InvalidOperationException(Strings.ExValueIsNotAvailable);
      return (fieldState & TupleFieldState.Null) == TupleFieldState.Null;
    }

    /// <summary>
    /// Checks if specified field state has both <see cref="TupleFieldState.Null"/> and <see cref="TupleFieldState.Available"/> flags.
    /// </summary>
    /// <param name="fieldState"><see cref="TupleFieldState"/> to check.</param>
    /// <returns><see langword="true"/> if field state value has both <see cref="TupleFieldState.Null"/> and <see cref="TupleFieldState.Available"/> flags; otherwise, <see langword="false"/>.</returns>
    public static bool IsAvailableAndNull(this TupleFieldState fieldState)
    {
      return fieldState == (TupleFieldState.Available | TupleFieldState.Null);
    }

    /// <summary>
    /// Checks if specified field state has <see cref="TupleFieldState.Available"/> flag and has no <see cref="TupleFieldState.Null"/> flag.
    /// </summary>
    /// <param name="fieldState"><see cref="TupleFieldState"/> to check.</param>
    /// <returns><see langword="true"/> if field state value is equal to <see cref="TupleFieldState.Available"/>;
    /// otherwise, <see langword="false"/>.</returns>
    public static bool HasValue(this TupleFieldState fieldState)
    {
      return fieldState == TupleFieldState.Available;
    }
  }
}