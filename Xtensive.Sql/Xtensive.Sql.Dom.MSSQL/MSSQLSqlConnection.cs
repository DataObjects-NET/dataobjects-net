// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
using Xtensive.Sql.Common;
using Xtensive.Sql.Common.Mssql;

namespace Xtensive.Sql.Dom
{
  public class MssqlSqlConnection : SqlConnection
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlConnection"/> class.
    /// </summary>
    public MssqlSqlConnection(SqlDriver driver, ConnectionInfo info)
      : base(driver, MssqlConnection.GetRealConnection(info), info)
    {
    }
  }
}