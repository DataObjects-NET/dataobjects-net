// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using Xtensive.Core;

namespace Xtensive.Sql.Common.Mssql.v2000
{
  #region ProtocolAttributeUsage_MSSQL
  [Protocol("mssql")]
  [Protocol("msde")]
  public class MssqlDriver : Driver
  #endregion
  {
    /// <summary>
    /// Creates the database connection.
    /// </summary>
    /// <param name="info">The connection info.</param>
    /// <returns>
    /// A <see cref="Connection"/> instance that wraps
    /// real connection to a data source.
    /// </returns>
    protected override Connection CreateDbConnection(ConnectionInfo info)
    {
      return new MssqlConnection(info, this);
    }

    /// <inheritdoc/>
    protected override IServerInfoProvider CreateServerInfoProvider(ConnectionInfo connectionInfo)
    {
      using (Connection connection = CreateConnection(connectionInfo)) {
        return new MssqlServerInfoProvider(connection);
      }
    }

    /// <inheritdoc/>
    protected override IServerInfoProvider CreateServerInfoProvider(VersionInfo versionInfo)
    {
      ArgumentValidator.EnsureArgumentIs<MssqlVersionInfo>(versionInfo, "versionInfo");
      return new MssqlServerInfoProvider(versionInfo as MssqlVersionInfo);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MssqlDriver"/> class.
    /// </summary>
    public MssqlDriver(MssqlVersionInfo versionInfo)
      : base(versionInfo)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MssqlDriver"/> class.
    /// </summary>
    /// <param name="connectionInfo">The connection info.</param>
    public MssqlDriver(ConnectionInfo connectionInfo)
      : base(connectionInfo)
    {
    }
  }
}
