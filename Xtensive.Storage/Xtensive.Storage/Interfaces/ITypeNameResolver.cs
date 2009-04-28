// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.28

using System;

namespace Xtensive.Storage
{
  /// <summary>
  /// Resolves persistent type name.
  /// </summary>
  public interface ITypeNameResolver
  {
    /// <summary>
    /// Gets the name that identifies specified <see cref="Type"/> within the <see cref="Domain"/>.
    /// </summary>
    /// <param name="type">The type to get name for.</param>
    /// <returns>Name of the type.</returns>
    string GetTypeName(Type type);
  }
}