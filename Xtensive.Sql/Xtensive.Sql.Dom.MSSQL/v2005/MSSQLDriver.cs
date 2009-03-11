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
  [Protocol("mssql")]
  [Protocol("mssql2005")]
  [Protocol("yukon")]
  public class MssqlDriver : SqlDriver
  {
    /// <inheritdoc/>
    protected override SqlTranslator CreateTranslator()
    {
      return new MssqlTranslator(this);
    }

    /// <inheritdoc/>
    protected override Connection CreateDbConnection(ConnectionInfo info)
    {
      return new MssqlSqlConnection(this, info);
    }

    /// <inheritdoc/>
    protected override SqlExtractor CreateExtractor()
    {
      return new MssqlExtractor(this);
    }

    /// <inheritdoc/>
    protected override IServerInfoProvider CreateServerInfoProvider(ConnectionInfo connectionInfo)
    {
      using (Connection connection = CreateConnection(connectionInfo))
      {
        return new MssqlServerInfoProvider(connection);
      }
    }

    /// <inheritdoc/>
    protected override IServerInfoProvider CreateServerInfoProvider(VersionInfo versionInfo)
    {
      return new MssqlServerInfoProvider((MssqlVersionInfo)versionInfo);
    }

    /// <inheritdoc/>
    protected override SqlCompiler CreateCompiler()
    {
      return new MssqlCompiler(this);
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
