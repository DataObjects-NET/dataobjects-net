// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.06

using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Building
{
  /// <summary>
  /// Enumerates possible <see cref="Stage"/> values.
  /// <seealso cref="Domain.Build(DomainConfiguration)"/>
  /// <seealso cref="Domain"/>
  /// </summary>
  public enum Stage
  {
    /// <summary>
    /// Initial state.
    /// Value is <see langword="0x0"/>. 
    /// </summary>
    Created = 0,
    /// <summary>
    /// Storage is building (<see cref="Domain.Build(DomainConfiguration)"/> method is executing).
    /// Value is <see langword="0x1"/>. 
    /// </summary>
    Building = 1,
    /// <summary>
    /// Storage is initializing system objects (see
    /// <see cref="Domain.InitializeSystemObjects"/>).
    /// <see cref="Domain.Build(DomainConfiguration)"/> method is still executing.
    /// Value is <see langword="0x2"/>. 
    /// </summary>
    InitializingSystemObjects = 2,
    /// <summary>
    /// Storage is initializing (see 
    /// <see cref="Domain.Initialize"/>).
    /// <see cref="Domain.Build(DomainConfiguration)"/> method is still executing.
    /// Value is <see langword="0x3"/>. 
    /// </summary>
    Initializing = 3,
    /// <summary>
    /// Storage is in upgrading mode.
    /// <see cref="Domain.Build(DomainConfiguration)"/> method is still executing.
    /// Value is <see langword="0x4"/>. 
    /// </summary>
    Upgrading = 4,
    /// <summary>
    /// Storage is ready to use.
    /// Value is <see langword="0x10"/>. 
    /// </summary>
    Ready = 0x10,
    /// <summary>
    /// Storage is dropped.
    /// Value is <see langword="0x20"/>. 
    /// </summary>
    Dropped = 0x20,
    /// <summary>
    /// Error occured during storage building.
    /// Value is <see langword="0x100"/>. 
    /// </summary>
    Error = 0x100,
  } ;
}