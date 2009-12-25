// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.07.18

namespace Xtensive.Messaging
{
  /// <summary>
  /// Describes live type of <see cref="IMessageProcessor"/>.
  /// </summary>
  public enum ProcessorActivationMode
  {
    /// <summary>
    /// Create new instance of <see cref="IMessageProcessor"/> for every corresponding incoming message.
    /// </summary>
    SingleCall,
    /// <summary>
    /// Get <see cref="IMessageProcessor"/> from pool and release it back to pool after use.
    /// </summary>
    PooledInstances,
    /// <summary>
    /// Use singletone pattern for <see cref="IMessageProcessor"/>.
    /// </summary>
    Singleton
  }
}