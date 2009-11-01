// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Common
{
  /// <summary>
  /// Declares a base functionality of any <see cref="Driver"/>
  /// that can be instantiated by the <see cref="ConnectionProvider"/>.
  /// </summary>
  /// <remarks>
  /// <para>Any driver that is intended to be accessible via <see cref="ConnectionProvider"/>
  /// has to be <see cref="Driver"/> descentant and in addition has to
  /// be marked with <see cref="ProtocolAttribute"/>.</para>
  /// </remarks>
  /// <seealso cref="ConnectionProvider"/>
  /// <seealso cref="ProtocolAttribute"/>
  public abstract class Driver
  {
    private ServerInfo serverInfo;
    private ConnectionInfo connectionInfo;
    private VersionInfo versionInfo;
    private IServerInfoProvider serverInfoProvider;

    public virtual IServerInfoProvider ServerInfoProvider
    {
      get {
        if (serverInfoProvider!=null)
          return serverInfoProvider;
        if (connectionInfo!=null) {
          serverInfoProvider = CreateServerInfoProvider(connectionInfo);
          return serverInfoProvider;
        }
        if (versionInfo!=null) {
          serverInfoProvider = CreateServerInfoProvider(versionInfo);
          return serverInfoProvider;
        }
        throw new InvalidOperationException("Driver instance has not been initialized properly.");
      }
    }

    /// <summary>
    /// Gets an instance that provides complete information
    /// about underlying RDBMS.
    /// </summary>
    /// <value>
    /// <para>An instance that implements <see cref="ServerInfo"/> interface.</para>
    /// <para>The value never can be <see langword="null"/>.</para>
    /// </value>
    /// <seealso cref="ServerInfo"/>
    public virtual ServerInfo ServerInfo
    {
      get {
        if (serverInfo==null)
          serverInfo = ServerInfo.Build(ServerInfoProvider);
        return serverInfo; 
      }
    }

    /// <summary>
    /// Creates <see cref="Driver"/> dependent <see cref="Connection"/> instance.
    /// </summary>
    /// <returns>A <see cref="Connection"/> instance that wraps
    /// real connection to a data source.</returns>
    /// <seealso cref="Connection"/>
    public Connection CreateConnection(ConnectionInfo info)
    {
      Connection connection = CreateDbConnection(info);
      return connection;
    }

    protected abstract IServerInfoProvider CreateServerInfoProvider(VersionInfo versionInfo);

    protected abstract IServerInfoProvider CreateServerInfoProvider(ConnectionInfo connectionInfo);

    /// <summary>
    /// Creates the database connection.
    /// </summary>
    /// <param name="info">The connection info.</param>
    /// <returns>A <see cref="Connection"/> instance that wraps
    /// real connection to a data source.</returns>
    protected abstract Connection CreateDbConnection(ConnectionInfo info);

    protected Driver(ConnectionInfo connectionInfo)
    {
      if (connectionInfo==null)
        throw new ArgumentNullException("connectionInfo");
      this.connectionInfo = connectionInfo;
    }

    protected Driver(VersionInfo versionInfo)
    {
      if (versionInfo==null)
        throw new ArgumentNullException("versionInfo");
      this.versionInfo = versionInfo;
    }
  }
}
