// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

namespace Xtensive.Sql.Common.VistaDB.v3
{
  [Protocol("vistadb")]
  public class VistaDBDriver : Driver
  {
    /// <summary>
    /// Creates the connection.
    /// </summary>
    /// <returns></returns>
    protected override Connection CreateDbConnection(ConnectionInfo info)
    {
      return new VistaDBConnection(this, info);
    }

    /// <summary>
    /// Creates the <see cref="Driver.ServerInfo"/> provider.
    /// </summary>
    protected override IServerInfoProvider CreateServerInfoProvider(ConnectionInfo connectionInfo)
    {
      return new VistaDBServerInfoProvider();
    }

    /// <summary>
    /// Creates the <see cref="Driver.ServerInfo"/> provider.
    /// </summary>
    protected override IServerInfoProvider CreateServerInfoProvider(VersionInfo versionInfo)
    {
      return new VistaDBServerInfoProvider();
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="VistaDBDriver"/> class.
    /// </summary>
    public VistaDBDriver(VersionInfo versionInfo)
      : base(versionInfo)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VistaDBDriver"/> class.
    /// </summary>
    public VistaDBDriver(ConnectionInfo connectionInfo)
      : base(connectionInfo)
    {
    }
  }
}
