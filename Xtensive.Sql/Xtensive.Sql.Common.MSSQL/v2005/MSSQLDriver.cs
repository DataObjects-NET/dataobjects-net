// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using Xtensive.Core;

namespace Xtensive.Sql.Common.Mssql.v2005
{
  [Protocol("mssql2005")]
  [Protocol("yukon")]
  public class MssqlDriver : v2000.MssqlDriver
  {
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

    public MssqlDriver(ConnectionInfo connectionInfo)
      : base(connectionInfo)
    {
    }

    public MssqlDriver(MssqlVersionInfo versionInfo)
      : base(versionInfo)
    {
    }
  }
}
