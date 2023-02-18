// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;
using Xtensive.Core;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public abstract class SqlColumn : SqlExpression
  {
    private string name;
    private SqlTable sqlTable;

    /// <summary>
    /// Gets or sets the name of this instance.
    /// </summary>
    /// <value>The alias.</value>
    public virtual string Name
    {
      get { return name; }
    }

    /// <summary>
    /// Gets the table reference.
    /// </summary>
    /// <value>The table reference.</value>
    public SqlTable SqlTable
    {
      get { return sqlTable; }
    }

    public override void ReplaceWith(SqlExpression expression)
    {
      var replacingExpression = ArgumentValidator.EnsureArgumentIs<SqlColumn>(expression);
      sqlTable = replacingExpression.SqlTable;
      name = replacingExpression.Name;
    }

    // Constructor

    internal SqlColumn(SqlTable sqlTable, string name) : base(SqlNodeType.Column)
    {
      this.sqlTable = sqlTable;
      this.name = name;
    }

    internal SqlColumn(string name) : base(SqlNodeType.Column)
    {
      this.name = name;
    }

    internal SqlColumn(SqlTable sqlTable) : base(SqlNodeType.Column)
    {
      this.sqlTable = sqlTable;
    }

    internal SqlColumn() : base(SqlNodeType.Column)
    {
    }

  }
}
