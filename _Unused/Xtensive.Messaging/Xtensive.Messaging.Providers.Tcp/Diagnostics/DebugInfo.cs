// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2007.07.04

using System.Threading;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Messaging.Diagnostics;

namespace Xtensive.Messaging.Providers.Tcp.Diagnostics
{
  /// <summary>
  /// Provides methods to maintain debug information.
  /// </summary>
  public sealed class DebugInfo : DebugInfoTemplate<DebugInfo>
  {
    // Private fields

    private static long getSocketFromPoolCount;
    private static long sendCount;
    private static long connectionCount;


    /// <summary>
    /// Gets total count of connections.
    /// </summary>
    public static long ConnectionCount
    {
      get
      {
        EnsureOperable();
        return Interlocked.Read(ref connectionCount);
      }
    }


    /// <summary>
    /// Gets count of socket retrieval from pool.
    /// </summary>
    public static long GetSocketFromPoolCount
    {
      get
      {
        EnsureOperable();
        return Interlocked.Read(ref getSocketFromPoolCount);
      }
    }

    /// <summary>
    /// Get count of sends to TCP socket.
    /// </summary>
    public static long SendCount
    {
      get
      {
        EnsureOperable();
        return Interlocked.Read(ref sendCount);
      }
    }

    /// <summary>
    /// Resets all counters.
    /// </summary>
    public static void Reset()
    {
      EnsureOperable();
      lock (SyncRoot) {
        Interlocked.Exchange(ref connectionCount, 0);
        Interlocked.Exchange(ref getSocketFromPoolCount, 0);
        Interlocked.Exchange(ref sendCount, 0);
      }
    }

    #region Internal

    internal static void IncreaseGetSocketFromPoolCount()
    {
      Interlocked.Increment(ref getSocketFromPoolCount);
    }

    internal static void IncreaseSendCount()
    {
      Interlocked.Increment(ref sendCount);
    }

    internal static void IncreaseConnectionCount()
    {
      Interlocked.Increment(ref connectionCount);
    }

    #endregion

    /// <summary>
    /// Goes through socket pool and closes all consumed sockets.
    /// </summary>
    /// <returns>Count of dropped connections.</returns>
    public static int DropAllPolledConnections()
    {
      lock (SyncRoot) {
        int count = 0;
        Pool<Pair<string, int>, SocketAdapter> pool = TcpBidirectionalConnection.GetSocketPool();
        foreach (SocketAdapter socket in pool)
          try {
            if (socket!=null && pool.IsConsumed(socket))
              socket.Close();
            count++;
          }
          catch {
            ;
          }
        return count;
      }
    }
  }
}