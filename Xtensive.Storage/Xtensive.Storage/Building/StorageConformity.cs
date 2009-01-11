// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2009.01.11

namespace Xtensive.Storage.Building
{
  /// <summary>
  /// Storage conformity with model.
  /// </summary>
  public enum StorageConformity
  {
    /// <summary>
    /// Storage structure match with model.
    /// </summary>
    Match = 0x00,

    /// <summary>
    /// Storage does not contain system types or system types incorrect.
    /// </summary>
    SystemTypesMissing = 0x01,

    /// <summary>
    /// Storage contains system types, but user types structure mistmatch with model.
    /// </summary>
    UserTypesMismatch = 0x02,
  }
}