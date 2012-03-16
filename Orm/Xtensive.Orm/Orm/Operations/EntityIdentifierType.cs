// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.07.28

using System;
using System.Diagnostics;

namespace Xtensive.Orm.Operations
{
  /// <summary>
  /// Possible identifier types for <see cref="IEntity.IdentifyAs"/> method.
  /// </summary>
  public enum EntityIdentifierType
  {
    /// <summary>
    /// Automatically generated indetifier.
    /// </summary>
    Auto,
    /// <summary>
    /// No identifier (i.e. identifier must not be logged).
    /// </summary>
    None,
  }
}