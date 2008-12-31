// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.12.31

using System;

namespace Xtensive.Storage.Building
{
  /// <summary>
  /// Enumerates possible foreign key build modes for <see cref="Domain"/>.
  /// <seealso cref="Domain"/>
  /// <seealso cref="Domain.Build"/>
  /// </summary>
  [Flags]
  public enum ForeignKeyMode
  {
    /// <summary>
    /// Default foreign key mode. Equlas to <see cref="None"/>.
    /// </summary>
    Default = None,

    /// <summary>
    /// No foreign keys will be builded for storage.
    /// </summary>
    None = 0x00,

    /// <summary>
    /// Foreign keys for hierarchy inheritance will be builded.
    /// <see cref="Hierarchy"/>
    /// </summary>
    Hierarchy = 0x01,

    /// <summary>
    /// Foreign keys for <see cref="Entity"/> references will be builded. 
    /// <seealso cref="EntitySet{TItem}"/>.
    /// </summary>
    Reference = 0x02,

    /// <summary>
    /// All foreign keys (<see cref="Hierarchy"/> and <see cref="Reference"/>) will be builded for storage.
    /// </summary>
    All = Hierarchy + Reference,
  }
}