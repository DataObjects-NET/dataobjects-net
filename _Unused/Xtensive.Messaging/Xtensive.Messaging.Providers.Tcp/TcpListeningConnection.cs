// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.06.25

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Xtensive.Core;
using Xtensive.Messaging.Providers.Tcp.Diagnostics;
using Xtensive.Messaging.Providers.Tcp.Resources;

namespace Xtensive.Messaging.Providers.Tcp
{
  /// <summary>
  /// Implements <see cref="IListeningConnection"/> for TCP protocol.
  /// </summary>
  public class TcpListeningConnection : IListeningConnection
  {
    // Private fields
    private readonly List<TcpListener> listeners = new List<TcpListener>();
    private readonly int listeningPort;


    // Events
    /// <summary>
    /// Listening connection accepts new incoming connection.
    /// </summary>
    public event EventHandler Accepted;


    // Unused interfaces' members

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <param name="consumer">Not supported.</param>
    public void AddConsumer(object consumer)
    {
      throw new NotSupportedException(Strings.ExListeningConnectionInvalidMethodOrProperty);
    }


    /// <summary>
    /// Not supported.
    /// </summary>
    public bool HasConsumers
    {
      get { throw new NotSupportedException(Strings.ExListeningConnectionInvalidMethodOrProperty); }
    }

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <param name="consumer">Not supported.</param>
    public void RemoveConsumer(object consumer)
    {
      throw new NotSupportedException(Strings.ExListeningConnectionInvalidMethodOrProperty);
    }

    /// <summary>
    /// Occurs than connection closed.
    /// </summary>
    public event EventHandler Closed;

    // Private members
    /// <summary>
    /// Asynchronous listen ports. Called than new connection incoming.
    /// </summary>
    /// <param name="asyncResult"><see cref="IAsyncResult"/> containing <see cref="WeakReference"/> of this <see cref="TcpListeningConnection"/>.</param>
    private static void BeginAccept(IAsyncResult asyncResult)
    {
      var asyncParameter = (Pair<WeakReference, WeakReference>)asyncResult.AsyncState;
      var listeningConnection = (TcpListeningConnection)asyncParameter.First.Target;
      var listener = (TcpListener)asyncParameter.Second.Target;
      if (!asyncParameter.First.IsAlive || !asyncParameter.Second.IsAlive || listener==null ||
        listeningConnection==null)
        return;

      Socket socket;
      try {
        socket = listener.EndAcceptSocket(asyncResult);
        socket.NoDelay = true;
      }
      catch {
        return;
      }
      finally {
        lock (listener) {
          try {
            if (listener.Server.IsBound)
              listener.BeginAcceptSocket(BeginAccept, asyncResult.AsyncState);
          }
          catch (SocketException) {
            RaiseConnectionClosedEvent(listeningConnection);
          } // Listener may already be closed.
          catch (ObjectDisposedException) {
            RaiseConnectionClosedEvent(listeningConnection);
          } // Listener may already be disposed.
        }
      }
      var connection = new TcpBidirectionalConnection(new SocketAdapter(socket));
      EventHandler connectionAccepted = listeningConnection.Accepted;
      if (connectionAccepted!=null) {
        DebugInfo.IncreaseConnectionCount();
        connectionAccepted(connection, new EventArgs());
      }
      connection.StartRead();
    }

    private static void RaiseConnectionClosedEvent(TcpListeningConnection connection)
    {
      EventHandler connectionClosed = connection.Closed;
      if (connectionClosed!=null) {
        connectionClosed(connection, new EventArgs());
      }
    }


    // Constructors

    /// <summary>
    /// Creates new instance of <see cref="TcpListeningConnection"/>.
    /// </summary>
    /// <param name="addresses">List of local IP addresses to listen on. Provide <see cref="IPAddress.Any"/> to listen on all available local IP addresses.</param>
    /// <param name="listenAtPort">Port to listen on.</param>
    public TcpListeningConnection(IEnumerable<IPAddress> addresses, int listenAtPort)
    {
      if (listenAtPort < 0 || listenAtPort > 65535)
        throw new ArgumentOutOfRangeException("listenAtPort", listenAtPort, Strings.ExPortOutOfRange);
      listeningPort = listenAtPort;
      foreach (IPAddress address in addresses) {
        try {
          // Start to listen TCP port
          var listener = new TcpListener(address, listeningPort);
          listener.Start();
          listener.BeginAcceptSocket(BeginAccept,
            new Pair<WeakReference, WeakReference>(new WeakReference(this),
              new WeakReference(listener)));
          listeners.Add(listener);
        }
        catch (Exception) {
          foreach (TcpListener listener in listeners) {
            listener.Stop();
          }
          listeners.Clear();
          throw;
        }
      }
    }

    // Dispose, Finalize

    ///<summary>
    ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    ///</summary>
    ///<filterpriority>2</filterpriority>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes <see cref="TcpBidirectionalConnection"/> and releases managed and unmanaged resources.
    /// </summary>
    /// <param name="disposing"><see langword="True"/> if called from <see cref="Dispose()"/> method, <see langword="frue"/> if called from finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
      lock (this) {
        foreach (TcpListener listener in listeners) {
          listener.Stop();
        }
      }
    }

    /// <inheritdoc/>
    ~TcpListeningConnection()
    {
      Dispose(false);
    }
  }
}