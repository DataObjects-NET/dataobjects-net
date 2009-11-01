// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using Xtensive.Sql.Common;
using Xtensive.Sql.Common.VistaDB;

namespace Xtensive.Sql.Dom.VistaDB
{
  public class VistaDBSqlConnection : SqlConnection
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlConnection"/> class.
    /// </summary>
    public VistaDBSqlConnection(SqlDriver driver, ConnectionInfo info)
      : base(driver, VistaDBConnection.GetRealConnection(info), info)
    {
    }
  }
}