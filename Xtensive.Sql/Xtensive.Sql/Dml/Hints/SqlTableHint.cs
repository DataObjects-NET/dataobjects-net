// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public abstract class SqlTableHint : SqlHint
  {
    /// <summary>
    /// Gets the SQL table.
    /// </summary>
    /// <value>The SQL table.</value>
    public SqlTable Table { get; private set; }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="sqlTable">The SQL table.</param>
    protected SqlTableHint(SqlTable sqlTable)
    {
      Table = sqlTable;
    }
  }
}
