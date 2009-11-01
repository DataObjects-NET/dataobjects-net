// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System.Data.Common;
using Xtensive.Sql.Common;

namespace Xtensive.Sql.Dom
{
  public abstract class SqlConnection : Connection
  {
    /// <inheritdoc/>
    protected override DbCommand CreateDbCommand()
    {
      return new SqlCommand(this);
    }

    /// <summary>
    /// Gets a <see cref="SqlDriver">RDBMS driver</see> the connection is working through.
    /// </summary>
    /// <seealso cref="Driver"/>
    public new SqlDriver Driver
    {
      get { return (SqlDriver)base.Driver; }
    }

    /// <summary>
    /// Creates and returns a <see cref="SqlCommand"></see> object associated with the current connection.
    /// </summary>
    /// <param name="statement">The <see cref="ISqlCompileUnit"/> object.</param>
    /// <returns>A <see cref="SqlCommand"></see> object.</returns>
    public SqlCommand CreateCommand(ISqlCompileUnit statement)
    {
      SqlCommand command = new SqlCommand(this);
      command.Statement = statement;
      return command;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlConnection"/> class.
    /// </summary>
    /// <param name="driver">A <see cref="Driver">driver</see> instance which
    /// will proceed RDBMS interaction.</param>
    /// <param name="realConnection">The real connection.</param>
    /// <param name="connectionInfo">The connection info.</param>
    protected SqlConnection(SqlDriver driver, DbConnection realConnection, ConnectionInfo connectionInfo)
      : base(driver, realConnection, connectionInfo)
    {
    }
  }
}