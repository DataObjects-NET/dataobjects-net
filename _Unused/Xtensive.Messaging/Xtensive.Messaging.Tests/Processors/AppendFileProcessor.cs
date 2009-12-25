// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.07.17

using System;
using Xtensive.Messaging;
using Xtensive.Messaging.Tests;

namespace Xtensive.Messaging.Tests
{
  /// <summary>
  /// Processes <see cref="AppendFileQueryMessage"/>.
  /// </summary>
  [MessageProcessor(typeof (AppendFileQueryMessage))]
  public class AppendFileProcessor: BaseProcessor
  {
    /// <summary>
    /// Processes message and send reply to <paramref name="replySender"/>
    /// </summary>
    /// <param name="message">Message to process.</param>
    /// <param name="replySender"><see cref="Sender"/> to send reply to.</param>
    public override void ProcessMessage(object message, Sender replySender)
    {
      replySender.Send(new ConfirmResponseMessage());
    }

  }
}