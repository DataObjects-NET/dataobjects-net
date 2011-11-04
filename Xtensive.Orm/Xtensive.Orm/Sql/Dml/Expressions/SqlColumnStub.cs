// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.09.08

using System;

namespace Xtensive.Sql.Dml
{
  [Serializable]
  public class SqlColumnStub : SqlColumn
  {
    public SqlColumn Column { get; set; }

    internal override object Clone(SqlNodeCloneContext context)
    {
      if (context.NodeMapping.ContainsKey(this))
        return context.NodeMapping[this];

      var clone = new SqlColumnStub(
        SqlTable != null ? (SqlTable) SqlTable.Clone(context) : null, 
        Column);

      context.NodeMapping[this] = clone;
      return clone;
    }

    public override void AcceptVisitor(ISqlVisitor visitor)
    {}

    // Constructors

    internal SqlColumnStub(SqlColumn column)
      : base(column.Name ?? string.Empty)
    {
      Column = column;
    }

    private SqlColumnStub(SqlTable sqlTable, SqlColumn column)
      : base(sqlTable, column.Name ?? string.Empty)
    {
      Column = column;
    }
  }
}