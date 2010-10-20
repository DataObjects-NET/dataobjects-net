// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.07.26

using System;

namespace Xtensive.Core
{
  ///<summary>
  /// Describes an object implementing consumption pattern.
  ///</summary>
  public interface IResource: IDisposable
  {
    /// <summary>
    /// Registers the object that wishes to act like a consumer of this instance.
    /// </summary>
    /// <param name="consumer">Object that wishes to act like a consumer of this instance.</param>
    void AddConsumer(object consumer);

    /// <summary>
    /// Unregisters the object that no longer wishes to act like a consumer of this instance.
    /// </summary>
    /// <param name="consumer">Object that no longer wishes to acts like a consumer of this instance.</param>
    void RemoveConsumer(object consumer);

    /// <summary>
    /// Gets a value indicating whether this instance is active (has one or more consumers).
    /// </summary>
    /// <value>
    ///   <see langword="true"/> if this instance is active (has one or more consumers); otherwise, <see langword="false"/>.
    /// </value>
    bool HasConsumers { get; }
  }
}