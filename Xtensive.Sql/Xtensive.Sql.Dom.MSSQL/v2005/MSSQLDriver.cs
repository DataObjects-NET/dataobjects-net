// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using Xtensive.Sql.Common;
using Xtensive.Sql.Common.Mssql;
using Xtensive.Sql.Common.Mssql.v2005;
using Xtensive.Sql.Dom.Compiler;
using Xtensive.Sql.Dom.Database.Extractor;

namespace Xtensive.Sql.Dom.Mssql.v2005
{
  [Protocol("mssql2005")]
  [Protocol("yukon")]
  public class MssqlDriver : v2000.MssqlDriver
  {
    /// <summary>
    /// Creates the SQL translator.
    /// </summary>
    protected override SqlTranslator CreateTranslator()
    {
      return new MssqlTranslator(this);
    }

    protected override SqlExtractor CreateExtractor()
    {
      return new MssqlExtractor(this);
    }

    protected override IServerInfoProvider CreateServerInfoProvider(ConnectionInfo connectionInfo)
    {
      using (Connection connection = CreateConnection(connectionInfo)) {
        return new MssqlServerInfoProvider(connection);
      }
    }

    protected override IServerInfoProvider CreateServerInfoProvider(VersionInfo versionInfo)
    {
      return new MssqlServerInfoProvider((MssqlVersionInfo)versionInfo);
    }

    public MssqlDriver(MssqlVersionInfo versionInfo)
      : base(versionInfo)
    {
    }

    public MssqlDriver(ConnectionInfo connectionInfo)
      : base(connectionInfo)
    {
    }
  }
}
