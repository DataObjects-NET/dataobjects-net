// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.06.25

using System;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Messaging.Providers.Tcp.Diagnostics;
using Xtensive.Messaging.Providers.Tcp.Resources;

namespace Xtensive.Messaging.Providers.Tcp
{
  /// <summary>
  /// Describes bidirectional TCP connection, that can be used to both send and receive data.
  /// </summary>
  public class TcpBidirectionalConnection : IBidirectionalConnection
  {
    // Private fields

    private SocketAdapter socket;
    private bool isSocketOwner;
    private readonly int port = -1;
    private readonly string host;
    private static readonly Pool<Pair<string, int>, SocketAdapter> socketPool =
      new Pool<Pair<string, int>, SocketAdapter>(1024, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(1));
    private readonly SetSlim<object> consumers = new SetSlim<object>();
    private bool disposed;
    // Properties

    /// <summary>
    /// Gets a count of consumers.
    /// </summary>
    public bool HasConsumers
    {
      get
      {
        lock (this) {
          if (disposed)
            throw new ObjectDisposedException("TcpBidirectionalConnection");
          return consumers.Count > 0;
        }
      }
    }


    // Events

    /// <summary>
    /// Raises than connection closed.
    /// </summary>
    public event EventHandler Closed;

    /// <summary>
    /// Raises than connection received new portion of data.
    /// </summary>
    public event EventHandler<DataReceivedEventArgs> DataReceived;

    // Methods

    /// <summary>
    /// Sends data to TCP socket.
    /// </summary>
    /// <param name="message">Array of <see cref="byte"/> to send to TCP socket.</param>
    public void Send(byte[] message)
    {
      lock (this) {
        if (disposed)
          throw new ObjectDisposedException("TcpBidirectionalConnection");
        Send(message, 0, message.Length);
      }
    }

    /// <summary>
    /// Sends data to TCP socket.
    /// </summary>
    /// <param name="message">Array of <see cref="byte"/> to send to TCP socket.</param>
    /// <param name="offset">Offset in <paramref name="message"/>.</param>
    /// <param name="length">Length of data.</param>
    public void Send(byte[] message, int offset, int length)
    {
      lock (this) {
        if (disposed)
          throw new ObjectDisposedException("TcpBidirectionalConnection");
        ArgumentValidator.EnsureArgumentNotNull(message, "message");
        if (!HasConsumers)
          throw new MessagingProviderException(Strings.ExUseConsumeBeforeSend);
        if (offset < 0 || length <= 0 || offset + length > message.Length)
          throw new ArgumentOutOfRangeException(Strings.ExSendBoundsIncorrect);
        if (message.Length==0)
          throw new MessagingProviderException(Strings.ExMessageIsEmpty);
        if (socket==null || !socket.Connected)
          throw new MessagingProviderException(Strings.ExSocketClosed);
        try {
          socket.Send(message, offset, length);
          DebugInfo.IncreaseSendCount();
        }
        catch (Exception ex) {
          throw new MessagingProviderException(Strings.ExSocketSendError, ex);
        }
      }
    }

    /// <summary>
    /// Closes the connection.
    /// </summary>
    public void Close()
    {
      lock (this) {
        if (socket!=null) {
          socket.Close();
        }
      }
    }

    /// <summary>
    /// Consumes <paramref name="consumer"/>. Starts read data from socket.
    /// </summary>
    /// <param name="consumer">Consumer.</param>
    public void AddConsumer(object consumer)
    {
      if (disposed)
        throw new ObjectDisposedException("TcpBidirectionalConnection");
      ArgumentValidator.EnsureArgumentNotNull(consumer, "consumer");
      lock (this) {
        consumers.Add(consumer);
        if (socket==null || !socket.Connected) {
          if (socket!=null && isSocketOwner) {
            socket.Dispose();
            socketPool.Release(socket);
            socketPool.Remove(socket);
          }
          socket = GetSocket();
          isSocketOwner = true;
          StartRead();
        }
      }
    }

    /// <summary>
    /// Releases <paramref name="consumer"/>. Stops read data from socket if all no consumers left.
    /// </summary>
    /// <param name="consumer">Consumer.</param>
    public void RemoveConsumer(object consumer)
    {
      lock (this) {
        consumers.Remove(consumer);
      }
    }

    internal void StartRead()
    {
      if (disposed)
        throw new ObjectDisposedException("TcpBidirectionalConnection");
      if (socket!=null)
        socket.StartRead();
    }

    // Private methods

    /// <summary>
    /// Raises than socket expired in pool.
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="e">Arguments.</param>
    private static void PoolSocketExpired(object sender, ItemRemovedEventArgs<SocketAdapter> e)
    {
      e.Item.Dispose();
    }

    /// <summary>
    /// Returns existing socket from pool or creates a new one.
    /// </summary>
    /// <returns></returns>
    private SocketAdapter GetSocket()
    {
      SocketAdapter resultSocket;
      do {
        resultSocket = socketPool.Consume(new Pair<string, int>(host, port),
          () => new SocketAdapter(host, port));
        if (!resultSocket.Connected) {
          resultSocket.Close();
          socketPool.Release(resultSocket);
          socketPool.Remove(resultSocket);
        }
      } while (!resultSocket.Connected);
      resultSocket.SocketDataReceived += SocketDataReceived;
      resultSocket.SocketClosed += SocketClosed;
      DebugInfo.IncreaseGetSocketFromPoolCount();
      return resultSocket;
    }

    private void SocketClosed()
    {
      EventHandler connectionClosed = Closed;
      if (connectionClosed!=null)
        try {
          connectionClosed(this, new EventArgs());
        }
        finally {
          ;
        }
    }

    private void SocketDataReceived(SocketDataReceivedEventArgs args)
    {
      EventHandler<DataReceivedEventArgs> dataReceived = DataReceived;
      if (dataReceived!=null)
        try {
          dataReceived(this, new DataReceivedEventArgs(args.Data, this));
        }
        finally {
          ;
        }
    }

    internal static Pool<Pair<string, int>, SocketAdapter> GetSocketPool()
    {
      return socketPool;
    }

    // Constructors

    /// <summary>
    /// Creates new instance of <see cref="TcpBidirectionalConnection"/>.
    /// </summary>
    /// <param name="sendTo"><see cref="EndPointInfo"/> represents information about remote endpoint.</param>
    public TcpBidirectionalConnection(EndPointInfo sendTo)
    {
      ArgumentValidator.EnsureArgumentNotNull(sendTo, "sendTo");
      if (sendTo.Protocol!="tcp")
        throw new ArgumentOutOfRangeException("sendTo", Strings.ExWrongProtocol);

      port = sendTo.Port;
      host = sendTo.Host;
    }

    /// <summary>
    /// Creates new instance of <see cref="TcpBidirectionalConnection"/> by existing socket. Usually used to send answers back to existing socket.
    /// </summary>
    /// <param name="socket"><see cref="SocketAdapter"/> to read or write data.</param>
    internal TcpBidirectionalConnection(SocketAdapter socket)
    {
      ArgumentValidator.EnsureArgumentNotNull(socket, "socket");
      this.socket = socket;
      this.socket.SocketDataReceived += SocketDataReceived;
      this.socket.SocketClosed += SocketClosed;
    }

    /// <summary>
    /// Initializes socket pool for all <see cref="TcpBidirectionalConnection"/> instances. 
    /// </summary>
    static TcpBidirectionalConnection()
    {
      socketPool.ItemRemoved += PoolSocketExpired;
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
        if (!disposed) {
          disposed = true;
          if (socket!=null) {
            socket.SocketClosed -= SocketClosed;
            socket.SocketDataReceived -= SocketDataReceived;
            if (isSocketOwner)
              socketPool.Release(socket);
            else
              socket.Dispose();
            socket = null;
          }
        }
      }
    }

    /// <inheritdoc/>
    ~TcpBidirectionalConnection()
    {
      Dispose(false);
    }

    // DebugInfo
  }
}