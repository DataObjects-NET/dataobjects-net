// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Elena Vakhtina
// Created:    2008.08.26

using System.Collections;

namespace Xtensive.Core
{
  /// <summary>
  /// Named value collection of saved context data.
  /// </summary>
  public interface INamedValueCollection
  {
    /// <summary>
    /// Gets the specified name.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    object Get(string name);

    /// <summary>
    /// Sets the specified name.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="value">The source.</param>
    void Set(string name, object value);
  }
}