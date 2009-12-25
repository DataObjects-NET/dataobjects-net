// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.07.17

using System;
using Xtensive.Messaging;

namespace Xtensive.Messaging.Tests
{
  /// <summary>
  /// Default processor. Returns <see cref="ErrorResponseMessage"/> 
  /// </summary>
  [MessageProcessor]
  public class DefaultProcessor : BaseProcessor
  {
    /// <summary>
    /// Processes message and send reply to <paramref name="replySender"/>
    /// </summary>
    /// <param name="message">Message to process.</param>
    /// <param name="replySender"><see cref="Sender"/> to send reply to.</param>
    public override void ProcessMessage(object message, Sender replySender)
    {
      throw new InvalidOperationException("Unknown message");
    }

  }
}