// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.07.16

namespace Xtensive.Messaging
{
  /// <summary>
  /// Describes messaging processors.
  /// </summary>
  public interface IMessageProcessor
  {
    /// <summary>
    /// Processes message and send reply to <paramref name="replySender"/>
    /// </summary>
    /// <param name="message">Message to process.</param>
    /// <param name="replySender"><see cref="Sender"/> to send reply to.</param>
    void ProcessMessage(object message, Sender replySender);

    /// <summary>
    /// Sets execution context.
    /// </summary>
    void SetContext(object value);
  }
}