// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.06.08

using System;

namespace Xtensive.Messaging
{
  /// <summary>
  /// Describes message receive event arguments.
  /// </summary>
  public class MessageReceivedEventArgs: EventArgs
  {
    private readonly object message;
    private readonly Sender sender;
    private readonly long? responseQueryId;

    /// <summary>
    /// Gets response query Id.
    /// </summary>
    public long? ResponseQueryId
    {
      get { return responseQueryId; }
    }

    /// <summary>
    /// Gets the received message.
    /// </summary>
    public object Message
    {
      get { return message; }
    }

    /// <summary>
    /// Gets <see cref="Sender"/> to post reply
    /// </summary>
    public Sender Sender
    {
      get { return sender; }
    }

    /// <summary>
    /// Initialize new instance of <see cref="MessageReceivedEventArgs"/> class.
    /// </summary>
    /// <param name="message">Initial <see cref="Message"/> property value.</param>
    /// <param name="sender"><see cref="Sender"/> to reply to the message.</param>
    /// <param name="responseQueryId">Response query id</param>
    public MessageReceivedEventArgs(object message, Sender sender, long? responseQueryId)
    {
      this.message = message;
      this.responseQueryId = responseQueryId;
      this.sender = sender;
    }
  }
}