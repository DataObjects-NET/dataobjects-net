// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Sql.Common;
using Xtensive.Sql.Common.VistaDB.v3;
using Xtensive.Sql.Dom.Compiler;
using Xtensive.Sql.Dom.Database.Extractor;

namespace Xtensive.Sql.Dom.VistaDB.v3
{
  [Protocol("vistadb")]
  public class VistaDBDriver : SqlDriver
  {
    /// <inheritdoc/>
    protected override SqlCompiler CreateCompiler()
    {
      return new VistaDBCompiler(this);
    }

    /// <inheritdoc/>
    protected override SqlTranslator CreateTranslator()
    {
      return new VistaDBTranslator(this);
    }

    /// <inheritdoc/>
    protected override Connection CreateDbConnection(ConnectionInfo info)
    {
      return new VistaDBSqlConnection(this, info);
    }

    /// <inheritdoc/>
    protected override IServerInfoProvider CreateServerInfoProvider(ConnectionInfo connectionInfo)
    {
      return new VistaDBServerInfoProvider();
    }

    /// <inheritdoc/>
    protected override IServerInfoProvider CreateServerInfoProvider(VersionInfo versionInfo)
    {
      return new VistaDBServerInfoProvider();
    }

    /// <inheritdoc/>
    protected override SqlExtractor CreateExtractor()
    {
      return new VistaDBExtractor(this);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="connectionInfo">The connection info.</param>
    public VistaDBDriver(ConnectionInfo connectionInfo)
      : base(connectionInfo)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate()" copy="true"/>
    /// </summary>
    /// <param name="versionInfo">The version info.</param>
    public VistaDBDriver(VersionInfo versionInfo)
      : base(versionInfo)
    {
    }

  }
}
