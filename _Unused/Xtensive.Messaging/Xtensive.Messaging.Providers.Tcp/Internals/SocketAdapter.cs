// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Gamzov
// Created:    2007.07.13

using System;
using System.Net.Sockets;
using System.Threading;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Messaging.Providers.Tcp
{
  internal delegate void SocketClosed();

  /// <summary>
  /// Cover <see cref="Socket"/> with reading thread.
  /// </summary>
  internal class SocketAdapter : IDisposable
  {
    // Constants

    private static readonly TimeSpan receiveTimeout = TimeSpan.FromMilliseconds(100);
    private static readonly TimeSpan sendTimeout = TimeSpan.FromMilliseconds(100);
    private const int ReceiveBufferSize = 65535;
    private const int SendBufferSize = 65535;

    // Private fields
    private readonly byte[] readBuffer = new byte[ReceiveBufferSize];
    private Thread workerThread;
    private bool stopWorkerThread;
    private bool disposed;
    private readonly Socket socket;
    private AutoResetEvent stopWorkerProcessWaitHandle;
    private AutoResetEvent workerProcessExecutedWaitHandle;


    // Events

    /// <summary>
    /// Raised if socket received new portion of incoming data.
    /// </summary>
    public event SocketDataReceived SocketDataReceived;

    /// <summary>
    /// Raised if socket closed either by remote side or by local timeout.
    /// </summary>
    public event SocketClosed SocketClosed;


    // Properties


    /// <summary>
    /// Gets <see langword="true"/> if socket is connected to remote host, otherwise <see langword="false"/>.
    /// </summary>
    public bool Connected
    {
      get { return socket.Connected; }
    }


    // Methods

    /// <summary>
    /// Sends bytes to socket.
    /// </summary>
    /// <param name="buffer">Array of <see cref="byte"/> to send to socket.</param>
    /// <param name="offset">Position from there read data from buffer.</param>
    /// <param name="size">Count of bytes to send.</param>
    public void Send(byte[] buffer, int offset, int size)
    {
      socket.Send(buffer, offset, size, SocketFlags.None);
    }

    /// <summary>
    /// Closes socket.
    /// </summary>
    public void Close()
    {
      socket.Close();
    }

    /// <summary>
    /// Starts reading process from socket.
    /// </summary>
    public void StartRead()
    {
      if (stopWorkerProcessWaitHandle==null)
        stopWorkerProcessWaitHandle = new AutoResetEvent(false);
      if (workerProcessExecutedWaitHandle==null)
        workerProcessExecutedWaitHandle = new AutoResetEvent(false);
      if (workerThread==null) {
        workerThread = new Thread(SocketReadWorkerProcess);
        workerThread.IsBackground = true;
        workerThread.Start(new WeakReference(this));
        workerProcessExecutedWaitHandle.WaitOne();
      }
    }

    // Private methods

    /// <summary>
    /// Runs read from socket.
    /// </summary>
    /// <param name="workerParameter">A <see cref="WeakReference"/> of this <see cref="SocketAdapter"/>.</param>
    private static void SocketReadWorkerProcess(object workerParameter)
    {
      var reference = (WeakReference)workerParameter;
      var adapter = (SocketAdapter)reference.Target;
      if (adapter==null || !reference.IsAlive)
        return;

      Socket socket = adapter.socket;
      try {
        socket.ReceiveTimeout = (int)receiveTimeout.TotalMilliseconds;
        socket.ReceiveBufferSize = ReceiveBufferSize;
        SocketError errorCode;
        socket.BeginReceive(adapter.readBuffer, 0, adapter.readBuffer.Length, SocketFlags.None, out errorCode,
          BeginReceive,
          new WeakReference(adapter));
        if (errorCode!=SocketError.Success) {
          ThrowSocketClosedEvent(adapter);
          return;
        }
      }
      catch {
        ThrowSocketClosedEvent(adapter);
        return;
      }
      finally {
        adapter.workerProcessExecutedWaitHandle.Set();
      }
      WaitHandle waitHandle = adapter.stopWorkerProcessWaitHandle;
      adapter = null; // To avoid to store strong reference.
      waitHandle.WaitOne();
    }

    private static void ThrowSocketClosedEvent(SocketAdapter adapter)
    {
      SocketClosed socketClosed = adapter.SocketClosed;
      if (socketClosed!=null)
        try {
          socketClosed();
        }
        finally {
          ;
        }
      adapter.socket.Close();
    }

    /// <summary>
    /// Asynchronous read from socket.
    /// </summary>
    /// <param name="receiveParameter"></param>
    private static void BeginReceive(IAsyncResult receiveParameter)
    {
      var parameter = (WeakReference)receiveParameter.AsyncState;
      var adapter = (SocketAdapter)parameter.Target;
      if (!parameter.IsAlive || adapter==null)
        return;
      Socket socket = adapter.socket;
      if (!adapter.stopWorkerThread) {
        int bytesReceived;
        try {
          bytesReceived = socket.EndReceive(receiveParameter);
        }
        catch {
          ThrowSocketClosedEvent(adapter);
          return;
        }
        if (bytesReceived > 0) {
          var receivedData = new byte[bytesReceived];
          Array.Copy(adapter.readBuffer, receivedData, bytesReceived);
          // Raise DataReceived event
          SocketDataReceived dataReceived = adapter.SocketDataReceived;
          if (dataReceived!=null)
            try {
              dataReceived(new SocketDataReceivedEventArgs(receivedData));
            }
            catch (Exception) {
              ;
            }
        }
        try {
          socket.BeginReceive(adapter.readBuffer, 0, adapter.readBuffer.Length, SocketFlags.None, BeginReceive,
            parameter);
        }
        catch (SocketException) {
          ThrowSocketClosedEvent(adapter);
        }
        catch (ObjectDisposedException) {
          ThrowSocketClosedEvent(adapter);
        }
      }
    }


    // Constructors


    /// <summary>
    /// Creates new instance of <see cref="SocketAdapter"/>, initializes it with <paramref name="host"/> and <see paramref="port"/>
    /// and connects to remote host.
    /// </summary>
    /// <param name="host">Host address to send data to.</param>
    /// <param name="port">Port to send data to.</param>
    public SocketAdapter(string host, int port)
      : this(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
    {
      socket.Connect(host, port);
    }

    /// <summary>
    /// Creates new instance of <see cref="SocketAdapter"/> using <see cref="Socket"/>, created outside of this class.
    /// </summary>
    /// <param name="socket">Socket to use in adapter.</param>
    public SocketAdapter(Socket socket)
    {
      this.socket = socket;
      socket.NoDelay = true;
      socket.SendBufferSize = SendBufferSize;
      socket.ReceiveBufferSize = ReceiveBufferSize;
      socket.SendTimeout = (int)sendTimeout.TotalMilliseconds;
      socket.ReceiveTimeout = (int)receiveTimeout.TotalMilliseconds;
    }


    // Dispose and finalize.

    /// <summary>
    /// <see cref="DisposableDocTemplate.Dispose()" copy="true"/>
    /// </summary>
    public void Dispose()
    {
      GC.SuppressFinalize(this);
      Dispose(true);
    }

    /// <summary>
    /// <see cref="DisposableDocTemplate.Dispose(bool)" copy="true"/>
    /// </summary>
    protected void Dispose(bool disposing)
    {
      if (!disposed) {
        disposed = true;
        socket.Close();
        stopWorkerThread = true;
        if (stopWorkerProcessWaitHandle!=null)
          stopWorkerProcessWaitHandle.Set();
        if (workerThread!=null && workerThread.IsAlive)
          workerThread.Join();
        if (disposing) {
          workerProcessExecutedWaitHandle.Close();
          stopWorkerProcessWaitHandle.Close();
        }
      }
    }

    /// <summary>
    /// <see cref="DisposableDocTemplate.Dtor" copy="true"/>
    /// </summary>
    ~SocketAdapter()
    {
      Dispose(false);
    }
  }
}