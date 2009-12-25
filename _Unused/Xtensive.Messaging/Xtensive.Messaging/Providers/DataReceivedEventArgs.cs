// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: 
// Created:    2007.11.16

using System;
namespace Xtensive.Messaging.Providers
{
  /// <summary>
  /// Contains event data for <see cref="IReceivingConnection.DataReceived"/> event.
  /// </summary>
  public class DataReceivedEventArgs : EventArgs
  {
    readonly byte[] data;
    readonly ISendingConnection replyTo;

    /// <summary>
    /// Gets data received from <see cref="IReceivingConnection"/>.
    /// </summary>
    public byte[] Data
    {
      get { return data; }
    }

    /// <summary>
    /// Gets <see cref="ISendingConnection"/> to send reply to.
    /// </summary>
    public ISendingConnection ReplyTo
    {
      get { return replyTo; }
    }

    /// <summary>
    /// Creates new instance of <see cref="DataReceivedEventArgs"/>.
    /// </summary>
    /// <param name="data">Data received from <see cref="IReceivingConnection"/>.</param>
    /// <param name="replyTo"><see cref="ISendingConnection"/> to send reply to.</param>
    public DataReceivedEventArgs(byte[] data, ISendingConnection replyTo)
    {
      this.data = data;
      this.replyTo = replyTo;
    }

  }
}