// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.06.08

namespace Xtensive.Messaging.Providers
{
  /// <summary>
  /// Defines connection creation methods.
  /// </summary>
  public interface IMessagingProvider
  {
    /// <summary>
    /// Creates new <see cref="ISendingConnection"/>.
    /// </summary>
    /// <param name="sendTo">Endpoint of remote side where connect to.</param>
    /// <returns>Initialized and ready to use <see cref="ISendingConnection"/>.</returns>
    ISendingConnection CreateSendingConnection(EndPointInfo sendTo); // Can return IBidirectionalConnection - in this case the answer should arrive to it
    /// <summary>
    /// Creates new <see cref="IListeningConnection"/>.
    /// </summary>
    /// <param name="listenAt">Local endpoint. Specifies port and host name or IP address where to listen for incoming connections.</param>
    /// <returns>Initialized and ready to use <see cref="IListeningConnection"/>.</returns>
    IListeningConnection CreateListeningConnection(EndPointInfo listenAt);
  }
}