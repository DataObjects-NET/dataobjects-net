// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.07.13

using System;

namespace Xtensive.Messaging.Providers.Tcp
{
  internal delegate void SocketDataReceived(SocketDataReceivedEventArgs args);

  internal class SocketDataReceivedEventArgs: EventArgs
  {
    private readonly byte[] data;

    /// <summary>
    /// Creates new instance of <see cref="SocketDataReceivedEventArgs"/>.
    /// </summary>
    /// <param name="data">Array of <see cref="byte"/> received from socket.</param>
    public SocketDataReceivedEventArgs(byte[] data)
    {
      this.data = data;
    }

    /// <summary>
    /// Gets array of <see cref="byte"/> received from socket.
    /// </summary>
    public byte[] Data
    {
      get { return data; }
    }
  }
}