// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.28

using System;

namespace Xtensive.Storage
{
  /// <summary>
  /// Provides persistent type names.
  /// </summary>
  public interface ITypeNameProvider
  {
    /// <summary>
    /// Gets the name that identifies specified <see cref="Type"/> within the <see cref="Domain"/>.
    /// </summary>
    /// <param name="type">Type to get the name for.</param>
    /// <returns>Name of the specified type.</returns>
    string GetTypeName(Type type);
  }
}