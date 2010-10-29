// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.26

namespace Xtensive.Collections
{
  /// <summary>
  /// Named value collection contract.
  /// </summary>
  public interface INamedValueCollection
  {
    /// <summary>
    /// Gets the specified value by its name.
    /// </summary>
    /// <param name="name">The name of the value.</param>
    /// <returns>
    /// Specified value, if found;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    object Get(string name);

    /// <summary>
    /// Sets the value of the specified name.
    /// </summary>
    /// <param name="name">The name to set the value of.</param>
    /// <param name="value">The value to set.</param>
    void Set(string name, object value);
  }
}