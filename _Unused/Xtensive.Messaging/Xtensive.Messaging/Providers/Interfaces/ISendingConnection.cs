// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.06.08

namespace Xtensive.Messaging.Providers
{
  /// <summary>
  /// Describes connection what can send data across the wire.
  /// </summary>
  public interface ISendingConnection: IConnection
  {
    /// <summary>
    /// Sends data across the wire.
    /// </summary>
    /// <param name="message">Array of bytes to send to connection.</param>
    void Send(byte[] message);

    /// <summary>
    /// Sends data across the wire.
    /// </summary>
    /// <param name="message">Array of bytes to send to connection.</param>
    /// <param name="offset">Initial index in <paramref name="message"/> to send from.</param>
    /// <param name="length">Count of bytes to send starting from <paramref name="offset"/>.</param>
    void Send(byte[] message, int offset, int length);

    /// <summary>
    /// Closes the connection.
    /// </summary>
    void Close();
  }
}