// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.03.18

namespace Xtensive.Orm.Serialization
{
  /// <summary>
  /// Serialization kind (serialization by reference or by value).
  /// </summary>
  public enum SerializationKind
  {
    /// <summary>
    /// Serialization by reference.
    /// </summary>
    ByReference = 0,

    /// <summary>
    /// Serialization by value 
    /// </summary>
    ByValue = 1,
  }
}