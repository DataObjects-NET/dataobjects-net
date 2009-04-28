// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.28

using System;

namespace Xtensive.Storage
{
  /// <summary>
  /// Default implementation of <see cref="ITypeNameResolver"/>
  /// </summary>
  [Serializable]
  public class DefaultTypeNameResolver : ITypeNameResolver
  {
    /// <inheritdoc/>
    public string GetTypeName(Type type)
    {
      return type.FullName;
    }
  }
}