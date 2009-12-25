// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.06.08

using System;
using System.Collections.Generic;
using System.Net;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Messaging.Providers.Tcp.Resources;

namespace Xtensive.Messaging.Providers.Tcp
{
  /// <summary>
  /// Creates TCP listening connection or TCP sending connection
  /// </summary>
  [MessagingProvider("tcp")]
  public class TcpProvider: IMessagingProvider
  {
    /// <summary>
    /// Represent "ANY IP Address" value for use in <see cref="TcpListeningConnection"/> to make it listen on all IP addresses available on host.
    /// </summary>
    public const string AnyIPAddress = "$ANY";


    /// <summary>
    /// Creates TCP implementation of <see cref="ISendingConnection"/> that actually <see cref="IBidirectionalConnection"/>.
    /// </summary>
    /// <param name="sendTo"><see cref="EndPointInfo"/> of message receiver.</param>
    /// <returns><see cref="ISendingConnection"/> for TCP protocol.</returns>
    public ISendingConnection CreateSendingConnection(EndPointInfo sendTo)
    {
      ArgumentValidator.EnsureArgumentNotNull(sendTo, "sendTo");
      if (sendTo.Protocol != "tcp")
        throw new ArgumentOutOfRangeException("sendTo", Strings.ExWrongProtocol, sendTo.Protocol);

      return new TcpBidirectionalConnection(sendTo);
    }

    /// <summary>
    /// Creates TCP implementation of <see cref="IListeningConnection"/>.
    /// </summary>
    /// <param name="listenAt"><see cref="EndPointInfo"/> describes where to listen for incoming connections.</param>
    /// <returns><see cref="IListeningConnection"/> for TCP protocol.</returns>
    public IListeningConnection CreateListeningConnection(EndPointInfo listenAt)
    {
      ArgumentValidator.EnsureArgumentNotNull(listenAt, "listenAt");
      if (listenAt.Protocol != "tcp")
        throw new ArgumentException(Strings.ExWrongProtocol, "listenAt");

      // May raise exception if another listener on the same address and port already exists
      IEnumerable<IPAddress> listenAddresses;
      if (listenAt.Host == AnyIPAddress) {
        listenAddresses = new[] {IPAddress.Any};
      }
      else
      {
        IPAddress[] addresses = Dns.GetHostAddresses(listenAt.Host);
        IPAddress[] localAddresses = Dns.GetHostAddresses("");
        ISet<IPAddress> ipSet = new SetSlim<IPAddress>(localAddresses);
        ipSet.Add(new IPAddress(new byte[]{127,0,0,1}));
        ipSet.IntersectWith(addresses);
        if (ipSet.Count == 0)
          throw new MessagingProviderException(Strings.ExIncorrectListeningAddress);
        listenAddresses = ipSet;
      }
      return new TcpListeningConnection(listenAddresses, listenAt.Port);
    }
  }
}