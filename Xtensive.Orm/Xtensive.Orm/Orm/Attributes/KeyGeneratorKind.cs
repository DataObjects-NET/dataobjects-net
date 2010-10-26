// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.09.22

namespace Xtensive.Orm
{
  /// <summary>
  /// Specifies key generator type to use for a particular hierarchy.
  /// </summary>
  public enum KeyGeneratorKind
  {
    /// <summary>
    /// No key generator must be provided.
    /// </summary>
    None = 0,

    /// <summary>
    /// Default key generator must be provided.
    /// </summary>
    Default = 1,

    /// <summary>
    /// Custom key generator type.
    /// </summary>
    Custom = 2
  }
}